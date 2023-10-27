using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

// NOTE: this is the implementation for document types. in the code we refer to document types as content types
//       at core level, so it has to be named ContentTypeEditingService instead of DocumentTypeEditingService.
internal sealed class ContentTypeEditingService : ContentTypeEditingServiceBase<IContentType, IContentTypeService, ContentTypePropertyTypeModel, ContentTypePropertyContainerModel>, IContentTypeEditingService
{
    private readonly ITemplateService _templateService;
    private readonly IContentTypeService _contentTypeService;

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
        Attempt<IContentType?, ContentTypeOperationStatus> result = await ValidateAndMapForUpdateAsync(contentType, model);
        if (result.Success is false)
        {
            return result;
        }

        contentType = result.Result ?? throw new InvalidOperationException($"{nameof(ValidateAndMapForUpdateAsync)} succeeded but did not yield any result");

        UpdateHistoryCleanup(contentType, model);
        UpdateTemplates(contentType, model);

        await SaveAsync(contentType, userKey);

        return Attempt.SucceedWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, contentType);
    }

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

    private async Task SaveAsync(IContentType contentType, Guid userKey)
        => await _contentTypeService.SaveAsync(contentType, userKey);

    protected override Guid[] GetAvailableCompositionKeys(IContentTypeComposition? source, IContentTypeComposition[] allContentTypes, bool isElement)
        => _contentTypeService.GetAvailableCompositeContentTypes(source, allContentTypes, isElement: isElement)
            .Results
            .Where(x => x.Allowed)
            .Select(x => x.Composition.Key)
            .ToArray();

    protected override IContentType CreateContentType(IShortStringHelper shortStringHelper, int parentId)
        => new ContentType(shortStringHelper, parentId);

    protected override bool SupportsPublishing => true;

    protected override UmbracoObjectTypes ContentTypeObjectType => UmbracoObjectTypes.DocumentType;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.DocumentTypeContainer;
}
