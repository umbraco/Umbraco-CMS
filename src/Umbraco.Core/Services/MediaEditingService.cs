using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal sealed class MediaEditingService
    : ContentEditingServiceBase<IMedia, IMediaType, IMediaService, IMediaTypeService>, IMediaEditingService
{
    private readonly ILogger<ContentEditingServiceBase<IMedia, IMediaType, IMediaService, IMediaTypeService>> _logger;
    private readonly IUserService _userService;

    public MediaEditingService(
        IMediaService contentService,
        IMediaTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ContentEditingServiceBase<IMedia, IMediaType, IMediaService, IMediaTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserService userService)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger, scopeProvider)
    {
        _logger = logger;
        _userService = userService;
    }

    public async Task<IMedia?> GetAsync(Guid id)
    {
        IMedia? media = ContentService.GetById(id);
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

        var currentUserId = _userService.GetAsync(userKey).Result?.Id ?? Constants.Security.SuperUserId;
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

        var currentUserId = _userService.GetAsync(userKey).Result?.Id ?? Constants.Security.SuperUserId;
        ContentEditingOperationStatus operationStatus = Save(content, currentUserId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, content)
            : Attempt.FailWithStatus(operationStatus, content);
    }

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid id, Guid userKey)
    {
        var currentUserId = _userService.GetAsync(userKey).Result?.Id ?? Constants.Security.SuperUserId;
        return await HandleDeletionAsync(id, media => ContentService.MoveToRecycleBin(media, currentUserId).Result, false);
    }

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteAsync(Guid id, Guid userKey)
    {
        var currentUserId = _userService.GetAsync(userKey).Result?.Id ?? Constants.Security.SuperUserId;
        return await HandleDeletionAsync(id, media => ContentService.Delete(media, currentUserId).Result, true);
    }

    protected override IMedia Create(string? name, int parentId, IMediaType contentType)
        => new Models.Media(name, parentId, contentType);

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
