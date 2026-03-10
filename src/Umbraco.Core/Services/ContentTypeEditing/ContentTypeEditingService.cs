using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

// NOTE: this is the implementation for document types. in the code we refer to document types as content types
//       at core level, so it has to be named ContentTypeEditingService instead of DocumentTypeEditingService.

/// <summary>
///     Implementation of <see cref="IContentTypeEditingService"/> for managing document types (content types).
/// </summary>
/// <remarks>
///     In Umbraco Core, document types are referred to as content types. This service handles
///     creating and updating document types including their properties, templates, compositions,
///     and history cleanup settings.
/// </remarks>
internal sealed class ContentTypeEditingService : ContentTypeEditingServiceBase<IContentType, IContentTypeService, ContentTypePropertyTypeModel, ContentTypePropertyContainerModel>, IContentTypeEditingService
{
    private readonly ITemplateService _templateService;
    private readonly IElementSwitchValidator _elementSwitchValidator;
    private readonly IReservedFieldNamesService _reservedFieldNamesService;
    private readonly IContentTypeService _contentTypeService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeEditingService"/> class.
    /// </summary>
    /// <param name="contentTypeService">The content type service for managing content types.</param>
    /// <param name="templateService">The template service for managing templates.</param>
    /// <param name="dataTypeService">The data type service for validating property data types.</param>
    /// <param name="entityService">The entity service for resolving entity relationships.</param>
    /// <param name="shortStringHelper">The helper for generating safe aliases.</param>
    /// <param name="elementSwitchValidator">The validator for element type switching operations.</param>
    /// <param name="reservedFieldNamesService">The service providing reserved field names.</param>
    public ContentTypeEditingService(
        IContentTypeService contentTypeService,
        ITemplateService templateService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper,
        IElementSwitchValidator elementSwitchValidator,
        IReservedFieldNamesService reservedFieldNamesService)
        : base(contentTypeService, contentTypeService, dataTypeService, entityService, shortStringHelper)
    {
        _contentTypeService = contentTypeService;
        _templateService = templateService;
        _elementSwitchValidator = elementSwitchValidator;
        _reservedFieldNamesService = reservedFieldNamesService;
    }

    /// <inheritdoc />
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
        Attempt<ContentTypeOperationStatus> creationAttempt = await _contentTypeService.CreateAsync(contentType, userKey);

        if(creationAttempt.Success is false)
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(creationAttempt.Result, contentType);
        }

        return Attempt.SucceedWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, contentType);
    }

    /// <inheritdoc />
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

        Attempt<ContentTypeOperationStatus> attempt = await _contentTypeService.UpdateAsync(contentType, userKey);

        return attempt.Success
            ? Attempt.SucceedWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, contentType)
            : Attempt.FailWithStatus<IContentType?, ContentTypeOperationStatus>(attempt.Result, null);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositionsAsync(
        Guid? key,
        IEnumerable<Guid> currentCompositeKeys,
        IEnumerable<string> currentPropertyAliases,
        bool isElement) =>
        await FindAvailableCompositionsAsync(key, currentCompositeKeys, currentPropertyAliases, isElement);

    /// <inheritdoc />
    protected override async Task<ContentTypeOperationStatus> AdditionalCreateValidationAsync(
        ContentTypeEditingModelBase<ContentTypePropertyTypeModel, ContentTypePropertyContainerModel> model)
    {
        // validate if the parent documentType (if set) has the same element status as the documentType being created
        return await ValidateCreateParentElementStatusAsync(model);
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

    /// <summary>
    ///     Validates element status changes when updating a content type.
    /// </summary>
    /// <param name="contentType">The existing content type being updated.</param>
    /// <param name="model">The update model containing the new element status.</param>
    /// <returns>
    ///     <see cref="ContentTypeOperationStatus.Success"/> if validation passes;
    ///     otherwise, an error status indicating why the element status change is not allowed.
    /// </returns>
    private async Task<ContentTypeOperationStatus> ValidateElementStatusForUpdateAsync(IContentTypeBase contentType, ContentTypeModelBase model)
    {
        // no change, ignore rest of validation
        if (contentType.IsElement == model.IsElement)
        {
            return ContentTypeOperationStatus.Success;
        }

        // this method should only contain blocking validation, warnings are handled by WarnDocumentTypeElementSwitchNotificationHandler

        // => check whether the element was used in a block structure prior to updating
        if (model.IsElement is false)
        {
            return await _elementSwitchValidator.ElementToDocumentNotUsedInBlockStructuresAsync(contentType)
                ? ContentTypeOperationStatus.Success
                : ContentTypeOperationStatus.InvalidElementFlagElementIsUsedInPropertyEditorConfiguration;
        }

        return await _elementSwitchValidator.DocumentToElementHasNoContentAsync(contentType)
            ? ContentTypeOperationStatus.Success
            : ContentTypeOperationStatus.InvalidElementFlagDocumentHasContent;
    }

    /// <summary>
    /// Should be called after it has been established that the composition list is in a valid state and the (composition) parent exists
    /// </summary>
    private async Task<ContentTypeOperationStatus> ValidateCreateParentElementStatusAsync(
        ContentTypeEditingModelBase<ContentTypePropertyTypeModel, ContentTypePropertyContainerModel> model)
    {
        Guid? parentId = model.Compositions
            .SingleOrDefault(composition => composition.CompositionType == CompositionType.Inheritance)?.Key;
        if (parentId is null)
        {
            return ContentTypeOperationStatus.Success;
        }

        IContentType? parent = await _contentTypeService.GetAsync(parentId.Value);
        return parent!.IsElement == model.IsElement
            ? ContentTypeOperationStatus.Success
            : ContentTypeOperationStatus.InvalidElementFlagComparedToParent;
    }

    /// <inheritdoc />
    protected override IContentType CreateContentType(IShortStringHelper shortStringHelper, int parentId)
        => new ContentType(shortStringHelper, parentId);

    /// <inheritdoc />
    protected override bool SupportsPublishing => true;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ContentTypeObjectType => UmbracoObjectTypes.DocumentType;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.DocumentTypeContainer;

    /// <inheritdoc />
    protected override ISet<string> GetReservedFieldNames() => _reservedFieldNamesService.GetDocumentReservedFieldNames();
}
