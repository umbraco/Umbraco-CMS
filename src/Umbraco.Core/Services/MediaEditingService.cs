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
    private readonly ICoreScopeProvider _scopeProvider;

    public MediaEditingService(
        IMediaService contentService,
        IMediaTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ContentEditingServiceBase<IMedia, IMediaType, IMediaService, IMediaTypeService>> logger,
        ICoreScopeProvider scopeProvider)
        : base(contentService, contentTypeService, propertyEditorCollection, dataTypeService, logger)
    {
        _logger = logger;
        _scopeProvider = scopeProvider;
    }

    public async Task<IMedia?> GetAsync(Guid id)
    {
        IMedia? media = ContentService.GetById(id);
        return await Task.FromResult(media);
    }

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> CreateAsync(MediaCreateModel createModel, int userId = Constants.Security.SuperUserId)
    {
        Attempt<IMedia?, ContentEditingOperationStatus> result = await MapCreate(createModel);
        if (result.Success == false)
        {
            return result;
        }

        IMedia media = result.Result!;

        ContentEditingOperationStatus operationStatus = Save(media, userId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus<IMedia?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, media)
            : Attempt.FailWithStatus<IMedia?, ContentEditingOperationStatus>(operationStatus, media);
    }

    public async Task<Attempt<IMedia, ContentEditingOperationStatus>> UpdateAsync(IMedia content, MediaUpdateModel updateModel, int userId = Constants.Security.SuperUserId)
    {
        Attempt<ContentEditingOperationStatus> result = await MapUpdate(content, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(result.Result, content);
        }

        ContentEditingOperationStatus operationStatus = Save(content, userId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(ContentEditingOperationStatus.Success, content)
            : Attempt.FailWithStatus(operationStatus, content);
    }

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid id, int userId = Constants.Security.SuperUserId)
        => await HandleDeletionAsync(id, content => ContentService.MoveToRecycleBin(content, userId));

    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteAsync(Guid id, int userId = Constants.Security.SuperUserId)
        => await HandleDeletionAsync(id, content => ContentService.Delete(content, userId));

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

    // helper method to perform move-to-recycle-bin and delete for content as they are very much handled in the same way
    private async Task<Attempt<IMedia?, ContentEditingOperationStatus>> HandleDeletionAsync(Guid id, Func<IMedia, Attempt<OperationResult?>> performDelete)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete:true);
        IMedia? media = ContentService.GetById(id);
        if (media == null)
        {
            return await Task.FromResult(Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, media));
        }

        Attempt<OperationResult?> deleteResult = performDelete(media);
        return deleteResult.Result?.Result switch
        {
            // these are the only result states currently expected from Delete
            OperationResultType.Success => Attempt.SucceedWithStatus<IMedia?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Success, media),
            OperationResultType.FailedCancelledByEvent => Attempt.FailWithStatus<IMedia?, ContentEditingOperationStatus>(ContentEditingOperationStatus.CancelledByNotification, media),

            // for any other state we'll return "unknown" so we know that we need to amend this
            _ => Attempt.FailWithStatus<IMedia?, ContentEditingOperationStatus>(ContentEditingOperationStatus.Unknown, media)
        };
    }
}
