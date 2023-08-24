﻿using Microsoft.Extensions.Logging;
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
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public MediaEditingService(
        IMediaService contentService,
        IMediaTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ContentEditingServiceBase<IMedia, IMediaType, IMediaService, IMediaTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        ITreeEntitySortingService treeEntitySortingService)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger, scopeProvider, treeEntitySortingService)
    {
        _logger = logger;
        _userIdKeyResolver = userIdKeyResolver;
    }

    public async Task<IMedia?> GetAsync(Guid key)
    {
        IMedia? media = ContentService.GetById(key);
        return await Task.FromResult(media);
    }

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> CreateAsync(MediaCreateModel createModel, Guid userKey)
    {
        Attempt<IMedia?, ContentEditingOperationStatus> result = await MapCreate(createModel, (name, parentId, contentType) => new Models.Media(name, parentId, contentType));
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

    public async Task<Attempt<IMedia, ContentEditingOperationStatus>> UpdateAsync(IMedia content, MediaUpdateModel updateModel, Guid userKey)
    {
        Attempt<ContentEditingOperationStatus> result = await MapUpdate(content, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(result.Result, content);
        }

        var currentUserId = await GetUserIdAsync(userKey);
        ContentEditingOperationStatus operationStatus = Save(content, currentUserId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, content)
            : Attempt.FailWithStatus(operationStatus, content);
    }

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleDeletionAsync(key, media => ContentService.MoveToRecycleBin(media, currentUserId).Result, false);
    }

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleDeletionAsync(key, media => ContentService.Delete(media, currentUserId).Result, true);
    }

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleMoveAsync(key, parentKey, (content, newParentId) => ContentService.Move(content, newParentId, currentUserId).Result);
    }

    public async Task<ContentEditingOperationStatus> SortAsync(Guid? parentKey, IEnumerable<SortingModel> sortingModels, Guid userKey)
    {
        var currentUserId = await GetUserIdAsync(userKey);
        return await HandleSortAsync(
            parentKey,
            sortingModels,
            (int id, int pageIndex, int pageSize, out long total) => ContentService.GetPagedChildren(id, pageIndex, pageSize, out total),
            items =>
            {
                var result = ContentService.Sort(items, currentUserId);
                return result
                    ? ContentEditingOperationStatus.Success
                    : ContentEditingOperationStatus.CancelledByNotification;
            });
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

    private async Task<int> GetUserIdAsync(Guid userKey) => await _userIdKeyResolver.GetAsync(userKey);
}
