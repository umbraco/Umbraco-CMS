using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

// NOTE: this is the implementation for document types. in the code we refer to document types as content types
//       at core level, so it has to be named ContentTypeEditingService instead of DocumentTypeEditingService.
internal sealed class ContentTypeEditingService : ContentTypeEditingServiceBase<IContentType, IContentTypeService, ContentTypePropertyTypeModel, ContentTypePropertyContainerModel>, IContentTypeEditingService
{
    private readonly ITemplateService _templateService;
    private readonly IDataTypeService _dataTypeService;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IContentTypeService _contentTypeService;

    public ContentTypeEditingService(
        IContentTypeService contentTypeService,
        ITemplateService templateService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper,
        PropertyEditorCollection propertyEditorCollection)
        : base(contentTypeService, contentTypeService, dataTypeService, entityService, shortStringHelper)
    {
        _contentTypeService = contentTypeService;
        _templateService = templateService;
        _dataTypeService = dataTypeService;
        _propertyEditorCollection = propertyEditorCollection;
    }

    [Obsolete("Use the constructor that is not marked obsolete, will be removed in v16")]
    public ContentTypeEditingService(
        IContentTypeService contentTypeService,
        ITemplateService templateService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper)
        : base(contentTypeService, contentTypeService, dataTypeService, entityService, shortStringHelper)
    {
        _contentTypeService = contentTypeService;
        _templateService = templateService;
        _dataTypeService = dataTypeService;
        _propertyEditorCollection = StaticServiceProvider.Instance.GetRequiredService<PropertyEditorCollection>();
    }

    public async Task<Attempt<IContentType?, ContentTypeOperationStatus>> CreateAsync(ContentTypeCreateModel model, Guid userKey)
    {
        Attempt<IContentType?, ContentTypeOperationStatus> result = await ValidateAndMapForCreationAsync(model, model.Key, model.ContainerKey);
        if (result.Success is false)
        {
            return result;
        }

        IContentType contentType = result.Result ?? throw new InvalidOperationException($"{nameof(ValidateAndMapForCreationAsync)} succeeded but did not yield any result");

        UpdateHistoryCleanup(contentType, model);
        UpdateTemplates(contentType, model);

        // save content type
        await SaveAsync(contentType, userKey);

        return Attempt.SucceedWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, contentType);
    }

    public async Task<Attempt<IContentType?, ContentTypeOperationStatus>> UpdateAsync(IContentType contentType, ContentTypeUpdateModel model, Guid userKey)
    {
        // this needs to happen before the base call as that one is not a pure function
        ContentTypeOperationStatus elementValidationStatus = await ValidateElementStatusForUpdateAsync(contentType, model);
        if (elementValidationStatus is not ContentTypeOperationStatus.Success)
        {
            return Attempt<IContentType?, ContentTypeOperationStatus>.Fail(elementValidationStatus);
        }

        Attempt<IContentType?, ContentTypeOperationStatus> baseValidationAttempt = await ValidateAndMapForUpdateAsync(contentType, model);
        if (baseValidationAttempt.Success is false)
        {
            return baseValidationAttempt;
        }

        contentType = baseValidationAttempt.Result ?? throw new InvalidOperationException($"{nameof(ValidateAndMapForUpdateAsync)} succeeded but did not yield any result");

        UpdateHistoryCleanup(contentType, model);
        UpdateTemplates(contentType, model);

        await SaveAsync(contentType, userKey);

        return Attempt.SucceedWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, contentType);
    }

    public async Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositionsAsync(
        Guid? key,
        IEnumerable<Guid> currentCompositeKeys,
        IEnumerable<string> currentPropertyAliases,
        bool isElement) =>
        await FindAvailableCompositionsAsync(key, currentCompositeKeys, currentPropertyAliases, isElement);

    // update content type history clean-up
    private void UpdateHistoryCleanup(IContentType contentType, ContentTypeModelBase model)
    {
        contentType.HistoryCleanup ??= new HistoryCleanup();
        contentType.HistoryCleanup.PreventCleanup = model.Cleanup.PreventCleanup;
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = model.Cleanup.KeepAllVersionsNewerThanDays;
        contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = model.Cleanup.KeepLatestVersionPerDayForDays;
    }

    // update allowed templates and assign default template
    private void UpdateTemplates(IContentType contentType, ContentTypeModelBase model)
    {
        ITemplate[] allowedTemplates = model.AllowedTemplateKeys
            .Select(async templateId => await _templateService.GetAsync(templateId))
            .Select(t => t.Result)
            .WhereNotNull()
            .ToArray();
        contentType.AllowedTemplates = allowedTemplates;
        // NOTE: incidentally this also covers removing the default template; when model.DefaultTemplateId is null,
        //       contentType.SetDefaultTemplate() will be called with a null value, which will reset the default template.
        contentType.SetDefaultTemplate(allowedTemplates.FirstOrDefault(t => t.Key == model.DefaultTemplateKey));
    }

    private async Task<ContentTypeOperationStatus> ValidateElementStatusForUpdateAsync(IContentTypeBase contentType, ContentTypeModelBase model)
    {
        // no change, ignore rest of validation
        if (contentType.IsElement == model.IsElement)
        {
            return ContentTypeOperationStatus.Success;
        }

        // this method should only contain blocking validation, warnings are handles by ...

        // => check whether the element was used in a block structure prior to updating
        if (model.IsElement is false)
        {
            return await ValidateElementToDocumentNotUsedInBlockStructuresAsync(contentType);
        }

        return ValidateDocumentToElementHasNoContent(contentType);
    }

    private async Task<ContentTypeOperationStatus> ValidateElementToDocumentNotUsedInBlockStructuresAsync(IContentTypeBase contentType)
    {
        // get all propertyEditors that support block usage
        var editors = _propertyEditorCollection.Where(pe => pe.SupportsConfigurableElements).ToArray();
        var blockEditorAliases = editors.Select(pe => pe.Alias).ToArray();

        // get all dataTypes that are based on those propertyEditors
        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetByEditorAliasAsync(blockEditorAliases);

        // check whether any of the configurations on those dataTypes have this element selected as a possible block
        return dataTypes.Any(dataType =>
            editors.First(editor => editor.Alias == dataType.EditorAlias)
                .GetValueEditor(dataType.ConfigurationObject)
                .ConfiguredElementTypeKeys().Contains(contentType.Key))
            ? ContentTypeOperationStatus.InvalidElementFlagElementIsUsedInPropertyEditorConfiguration
            : ContentTypeOperationStatus.Success;
    }

    private ContentTypeOperationStatus ValidateDocumentToElementHasNoContent(IContentTypeBase contentType) =>
        _contentTypeService.HasContentNodes(contentType.Id)
            ? ContentTypeOperationStatus.InvalidElementFlagDocumentHasContent
            : ContentTypeOperationStatus.Success;

    private async Task SaveAsync(IContentType contentType, Guid userKey)
        => await _contentTypeService.SaveAsync(contentType, userKey);

    protected override IContentType CreateContentType(IShortStringHelper shortStringHelper, int parentId)
        => new ContentType(shortStringHelper, parentId);

    protected override bool SupportsPublishing => true;

    protected override UmbracoObjectTypes ContentTypeObjectType => UmbracoObjectTypes.DocumentType;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.DocumentTypeContainer;
}
