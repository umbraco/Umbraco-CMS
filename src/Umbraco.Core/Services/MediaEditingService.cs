using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal sealed class MediaEditingService
    : ContentEditingServiceWithSortingBase<IMedia, IMediaType, IMediaService, IMediaTypeService>, IMediaEditingService
{
    private readonly ILogger<ContentEditingServiceBase<IMedia, IMediaType, IMediaService, IMediaTypeService>> _logger;

    public MediaEditingService(
        IMediaService contentService,
        IMediaTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ContentEditingServiceBase<IMedia, IMediaType, IMediaService, IMediaTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        ITreeEntitySortingService treeEntitySortingService,
        IMediaValidationService mediaValidationService)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger, scopeProvider, userIdKeyResolver, mediaValidationService, treeEntitySortingService)
        => _logger = logger;

    public Task<IMedia?> GetAsync(Guid key)
    {
        IMedia? media = ContentService.GetById(key);
        return Task.FromResult(media);
    }

    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, MediaUpdateModel updateModel)
    {
        IMedia? media = ContentService.GetById(key);
        return media is not null
            ? await ValidatePropertiesAsync(updateModel, media.ContentType.Key)
            : Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ContentValidationResult());
    }

    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(MediaCreateModel createModel)
        => await ValidatePropertiesAsync(createModel, createModel.ContentTypeKey);

    public async Task<Attempt<MediaCreateResult, ContentEditingOperationStatus>> CreateAsync(MediaCreateModel createModel, Guid userKey)
    {
        Attempt<MediaCreateResult, ContentEditingOperationStatus> result = await MapCreate<MediaCreateResult>(createModel);
        if (result.Success == false)
        {
            return result;
        }

        // the create mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        // If we have property validation errors, don't allow saving, as media only supports "published" status.
        if (result.Status == ContentEditingOperationStatus.PropertyValidationError)
        {
            return Attempt.FailWithStatus(validationStatus, new MediaCreateResult { ValidationResult = validationResult });
        }

        IMedia media = result.Result.Content!;

        var currentUserId = await GetUserIdAsync(userKey);
        ContentEditingOperationStatus operationStatus = Save(media, currentUserId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new MediaCreateResult { Content = media })
            : Attempt.FailWithStatus(operationStatus, new MediaCreateResult { Content = media });
    }

    public async Task<Attempt<MediaUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, MediaUpdateModel updateModel, Guid userKey)
    {
        IMedia? media = ContentService.GetById(key);
        if (media is null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new MediaUpdateResult());
        }

        Attempt<MediaUpdateResult, ContentEditingOperationStatus> result = await MapUpdate<MediaUpdateResult>(media, updateModel);
        if (result.Success == false)
        {
            return result;
        }

        // the update mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        var currentUserId = await GetUserIdAsync(userKey);
        ContentEditingOperationStatus operationStatus = Save(media, currentUserId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new MediaUpdateResult { Content = media, ValidationResult = validationResult })
            : Attempt.FailWithStatus(operationStatus, new MediaUpdateResult { Content = media });
    }

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey)
        => await HandleMoveToRecycleBinAsync(key, userKey);

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey,false);

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey, true);

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey)
        => await HandleMoveAsync(key, parentKey, userKey);

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey)
        => await HandleMoveAsync(key, parentKey, userKey, true);

    public async Task<ContentEditingOperationStatus> SortAsync(Guid? parentKey, IEnumerable<SortingModel> sortingModels, Guid userKey)
        => await HandleSortAsync(parentKey, sortingModels, userKey);

    protected override IMedia New(string? name, int parentId, IMediaType mediaType)
        => new Models.Media(name, parentId, mediaType);

    protected override OperationResult? Move(IMedia media, int newParentId, int userId)
        => ContentService.Move(media, newParentId, userId).Result;

    protected override IMedia? Copy(IMedia media, int newParentId, bool relateToOriginal, bool includeDescendants, int userId)
        => throw new NotSupportedException("Copy is not supported for media");

    protected override OperationResult? MoveToRecycleBin(IMedia media, int userId)
        => ContentService.MoveToRecycleBin(media, userId).Result;

    protected override OperationResult? Delete(IMedia media, int userId)
        => ContentService.Delete(media, userId).Result;

    protected override IEnumerable<IMedia> GetPagedChildren(int parentId, int pageIndex, int pageSize, out long total)
        => ContentService.GetPagedChildren(parentId, pageIndex, pageSize, out total);

    protected override ContentEditingOperationStatus Sort(IEnumerable<IMedia> items, int userId)
    {
        bool result = ContentService.Sort(items, userId);
        return result
            ? ContentEditingOperationStatus.Success
            : ContentEditingOperationStatus.CancelledByNotification;
    }

    private ContentEditingOperationStatus Save(IMedia media, int userId)
    {
        try
        {
            Attempt<OperationResult?> saveResult = ContentService.Save(media, userId);
            return saveResult.Result?.Result switch
            {
                // these are the only result states currently expected from Save
                OperationResultType.Success => ContentEditingOperationStatus.Success,
                OperationResultType.FailedCancelledByEvent => ContentEditingOperationStatus.CancelledByNotification,
                OperationResultType.FailedDuplicateKey => ContentEditingOperationStatus.DuplicateKey,

                // for any other state we'll return "unknown" so we know that we need to amend this
                _ => ContentEditingOperationStatus.Unknown
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Media save operation failed");
            return ContentEditingOperationStatus.Unknown;
        }
    }
}
