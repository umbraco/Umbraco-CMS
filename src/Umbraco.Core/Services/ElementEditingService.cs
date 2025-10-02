using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal sealed class ElementEditingService
    : ContentEditingServiceBase<IElement, IContentType, IElementService, IContentTypeService>, IElementEditingService
{
    private readonly ILogger<ElementEditingService> _logger;
    private readonly IElementContainerService _containerService;

    public ElementEditingService(
        IElementService elementService,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ElementEditingService> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        IElementValidationService validationService,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        IElementContainerService containerService,
        ContentTypeFilterCollection contentTypeFilters)
        : base(
            elementService,
            contentTypeService,
            propertyEditorCollection,
            dataTypeService,
            logger,
            scopeProvider,
            userIdKeyResolver,
            validationService,
            optionsMonitor,
            relationService,
            contentTypeFilters)
    {
        _logger = logger;
        _containerService = containerService;
    }

    public Task<IElement?> GetAsync(Guid key)
    {
        IElement? element = ContentService.GetById(key);
        return Task.FromResult(element);
    }

    // TODO ELEMENTS: implement validation here
    public Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(ElementCreateModel createModel, Guid userKey)
        => Task.FromResult(Attempt<ContentValidationResult, ContentEditingOperationStatus>.Succeed(ContentEditingOperationStatus.Success, new ()));

    // TODO ELEMENTS: implement validation here
    public Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, ValidateElementUpdateModel updateModel, Guid userKey)
        => Task.FromResult(Attempt<ContentValidationResult, ContentEditingOperationStatus>.Succeed(ContentEditingOperationStatus.Success, new ()));

    public async Task<Attempt<ElementCreateResult, ContentEditingOperationStatus>> CreateAsync(ElementCreateModel createModel, Guid userKey)
    {
        if (await ValidateCulturesAsync(createModel) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.InvalidCulture, new ElementCreateResult());
        }

        Attempt<ElementCreateResult, ContentEditingOperationStatus> result = await MapCreate<ElementCreateResult>(createModel);
        if (result.Success == false)
        {
            return result;
        }

        // the create mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        // TODO ELEMENTS: we need a fix for this; see ContentEditingService
        IElement element = result.Result.Content!;
        // IElement element = await EnsureOnlyAllowedFieldsAreUpdated(result.Result.Content!, userKey);

        ContentEditingOperationStatus saveStatus = await Save(element, userKey);
        return saveStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new ElementCreateResult { Content = element, ValidationResult = validationResult })
            : Attempt.FailWithStatus(saveStatus, new ElementCreateResult { Content = element });
    }

    public async Task<Attempt<ElementUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ElementUpdateModel updateModel, Guid userKey)
    {
        IElement? element = ContentService.GetById(key);
        if (element is null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ElementUpdateResult());
        }

        if (await ValidateCulturesAsync(updateModel) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.InvalidCulture, new ElementUpdateResult { Content = element });
        }

        Attempt<ElementUpdateResult, ContentEditingOperationStatus> result = await MapUpdate<ElementUpdateResult>(element, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(result.Status, result.Result);
        }

        // the update mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        // TODO ELEMENTS: we need a fix for this; see ContentEditingService
        // element = await EnsureOnlyAllowedFieldsAreUpdated(element, userKey);

        ContentEditingOperationStatus saveStatus = await Save(element, userKey);
        return saveStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new ElementUpdateResult { Content = element, ValidationResult = validationResult })
            : Attempt.FailWithStatus(saveStatus, new ElementUpdateResult { Content = element });
    }

    public async Task<Attempt<IElement?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey, false);

    protected override IElement New(string? name, int parentId, IContentType contentType)
        => new Element(name, parentId, contentType);

    protected override async Task<(int? ParentId, ContentEditingOperationStatus OperationStatus)> TryGetAndValidateParentIdAsync(Guid? parentKey, IContentType contentType)
    {
        if (parentKey.HasValue is false)
        {
            return (Constants.System.Root, ContentEditingOperationStatus.Success);
        }

        EntityContainer? container = await _containerService.GetAsync(parentKey.Value);
        return container is not null
            ? (container.Id, ContentEditingOperationStatus.Success)
            : (null, ContentEditingOperationStatus.ParentNotFound);
    }

    protected override OperationResult? Move(IElement element, int newParentId, int userId)
        => throw new NotImplementedException("TODO ELEMENTS: implement move");

    protected override IElement? Copy(IElement element, int newParentId, bool relateToOriginal, bool includeDescendants, int userId)
        => throw new NotImplementedException("TODO ELEMENTS: implement copy");

    protected override OperationResult? MoveToRecycleBin(IElement element, int userId)
        => throw new NotImplementedException("TODO ELEMENTS: implement recycle bin");

    protected override OperationResult? Delete(IElement element, int userId)
        => ContentService.Delete(element, userId);

    private async Task<ContentEditingOperationStatus> Save(IElement content, Guid userKey)
    {
        try
        {
            var currentUserId = await GetUserIdAsync(userKey);
            OperationResult saveResult = ContentService.Save(content, currentUserId);
            return saveResult.Result switch
            {
                // these are the only result states currently expected from Save
                OperationResultType.Success => ContentEditingOperationStatus.Success,
                OperationResultType.FailedCancelledByEvent => ContentEditingOperationStatus.CancelledByNotification,

                // for any other state we'll return "unknown" so we know that we need to amend this
                _ => ContentEditingOperationStatus.Unknown
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Content save operation failed");
            return ContentEditingOperationStatus.Unknown;
        }
    }
}
