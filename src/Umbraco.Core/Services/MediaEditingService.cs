using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

// FIXME: add granular permissions check (for inspiration, check how the old MediaController utilizes IAuthorizationService)
internal sealed class MediaEditingService
    : ContentEditingServiceBase<IMedia, IMediaType, IMediaService, IMediaTypeService>, IMediaEditingService
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
        ITreeEntitySortingService treeEntitySortingService)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger, scopeProvider, userIdKeyResolver, treeEntitySortingService)
        => _logger = logger;

    public async Task<IMedia?> GetAsync(Guid key)
    {
        IMedia? media = ContentService.GetById(key);
        return await Task.FromResult(media);
    }

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> CreateAsync(MediaCreateModel createModel, Guid userKey)
    {
        Attempt<IMedia?, ContentEditingOperationStatus> result = await MapCreate(createModel);
        if (result.Success == false)
        {
            return result;
        }

        IMedia media = result.Result!;

        var currentUserId = await GetUserIdAsync(userKey);
        ContentEditingOperationStatus operationStatus = Save(media, currentUserId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus<IMedia?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, media)
            : Attempt.FailWithStatus<IMedia?, ContentEditingOperationStatus>(operationStatus, media);
    }

    public async Task<Attempt<IMedia, ContentEditingOperationStatus>> UpdateAsync(IMedia media, MediaUpdateModel updateModel, Guid userKey)
    {
        Attempt<ContentEditingOperationStatus> result = await MapUpdate(media, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(result.Result, media);
        }

        var currentUserId = await GetUserIdAsync(userKey);
        ContentEditingOperationStatus operationStatus = Save(media, currentUserId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, media)
            : Attempt.FailWithStatus(operationStatus, media);
    }

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey)
        => await HandleMoveToRecycleBinAsync(key, userKey);

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey);

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey)
        => await HandleMoveAsync(key, parentKey, userKey);

    public async Task<ContentEditingOperationStatus> SortAsync(Guid? parentKey, IEnumerable<SortingModel> sortingModels, Guid userKey)
        => await HandleSortAsync(parentKey, sortingModels, userKey);

    protected override IMedia New(string? name, int parentId, IMediaType mediaType)
        => new Models.Media(name, parentId, mediaType);

    protected override OperationResult? Move(IMedia media, int newParentId, int userId)
        => ContentService.Move(media, newParentId, userId).Result;

    protected override IMedia? Copy(IMedia media, int newParentId, bool relateToOriginal, bool includeDescendants, int userId)
        => throw new NotImplementedException("Copy is not supported for media");

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
