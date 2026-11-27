using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements the <see cref="IMediaEditingService"/> for creating, updating, deleting,
///     and managing <see cref="IMedia"/> items through the editing API.
/// </summary>
internal sealed class MediaEditingService
    : AsyncContentEditingServiceWithSortingBase<IMedia, IMediaType, IMediaService, IMediaTypeService>, IMediaEditingService
{
    private readonly ILogger<AsyncContentEditingServiceBase<IMedia, IMediaType, IMediaService, IMediaTypeService>> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaEditingService"/> class.
    /// </summary>
    /// <param name="contentService">The <see cref="IMediaService"/> for media operations.</param>
    /// <param name="contentTypeService">The <see cref="IMediaTypeService"/> for media type operations.</param>
    /// <param name="propertyEditorCollection">The collection of property editors.</param>
    /// <param name="dataTypeService">The <see cref="IDataTypeService"/> for data type operations.</param>
    /// <param name="logger">The logger for this service.</param>
    /// <param name="scopeProvider">The <see cref="ICoreScopeProvider"/> for database scope management.</param>
    /// <param name="userIdKeyResolver">The <see cref="IUserIdKeyResolver"/> for resolving user IDs.</param>
    /// <param name="treeEntitySortingService">The <see cref="ITreeEntitySortingService"/> for sorting operations.</param>
    /// <param name="mediaValidationService">The <see cref="IMediaValidationService"/> for media validation.</param>
    /// <param name="optionsMonitor">The options monitor for <see cref="ContentSettings"/>.</param>
    /// <param name="relationService">The <see cref="IRelationService"/> for relation operations.</param>
    /// <param name="contentTypeFilters">The collection of content type filters.</param>
    public MediaEditingService(
        IMediaService contentService,
        IMediaTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<AsyncContentEditingServiceBase<IMedia, IMediaType, IMediaService, IMediaTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        ITreeEntitySortingService treeEntitySortingService,
        IMediaValidationService mediaValidationService,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        ContentTypeFilterCollection contentTypeFilters,
        ILanguageService languageService,
        IUserService userService)
        : base(
            contentService,
            contentTypeService,
            propertyEditorCollection,
            dataTypeService,
            logger,
            scopeProvider,
            userIdKeyResolver,
            mediaValidationService,
            treeEntitySortingService,
            optionsMonitor,
            relationService,
            contentTypeFilters,
            languageService,
            userService)
        => _logger = logger;

    /// <inheritdoc/>
    protected override string? RelateParentOnDeleteAlias => Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;

    /// <inheritdoc />
    public override async Task<IMedia?> GetAsync(Guid key)
        => await ContentService.GetByIdAsync(key, CancellationToken.None);

    /// <inheritdoc />
    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, MediaUpdateModel updateModel)
    {
        IMedia? media = await ContentService.GetByIdAsync(key, CancellationToken.None);
        return media is not null
            ? await ValidatePropertiesAsync(updateModel, media.ContentType.Key)
            : Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ContentValidationResult());
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(MediaCreateModel createModel)
    {
        ContentEditingOperationStatus creationAllowedStatus = await ValidateCreationAllowedAsync(createModel);
        if (creationAllowedStatus != ContentEditingOperationStatus.Success)
        {
            return Attempt.FailWithStatus(creationAllowedStatus, new ContentValidationResult());
        }

        return await ValidatePropertiesAsync(createModel, createModel.ContentTypeKey);
    }

    /// <inheritdoc />
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

        ContentEditingOperationStatus operationStatus = await SaveAsync(media, userKey);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new MediaCreateResult { Content = media })
            : Attempt.FailWithStatus(operationStatus, new MediaCreateResult { Content = media });
    }

    /// <inheritdoc />
    public async Task<Attempt<MediaUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, MediaUpdateModel updateModel, Guid userKey)
    {
        IMedia? media = await ContentService.GetByIdAsync(key, CancellationToken.None);
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

        ContentEditingOperationStatus operationStatus = await SaveAsync(media, userKey);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new MediaUpdateResult { Content = media, ValidationResult = validationResult })
            : Attempt.FailWithStatus(operationStatus, new MediaUpdateResult { Content = media });
    }

    /// <inheritdoc />
    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey)
        => await HandleMoveToRecycleBinAsync(key, userKey);

    /// <inheritdoc />
    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey,false);

    /// <inheritdoc />
    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey, true);

    /// <inheritdoc />
    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey)
        => await HandleMoveAsync(key, parentKey, userKey);

    /// <inheritdoc />
    public async Task<Attempt<IMedia?, ContentEditingOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey, bool includeDescendants)
        => await HandleMoveAsync(key, parentKey, userKey, true, includeDescendants);

    /// <inheritdoc />
    public async Task<ContentEditingOperationStatus> SortAsync(Guid? parentKey, IEnumerable<SortingModel> sortingModels, Guid userKey)
        => await HandleSortAsync(parentKey, sortingModels, userKey);

    /// <inheritdoc />
    public async Task<ContentEditingOperationStatus> SortByFieldAsync(Guid? parentKey, ContentSortField field, Direction direction, Guid userKey)

        // Media never varies by culture, so children are always ordered by the invariant name.
        => await HandleSortByFieldAsync(parentKey, field, direction, culture: null, userKey);


    /// <inheritdoc />
    protected override IMedia New(string name, int parentId, IMediaType mediaType)
        => new Models.Media(name, parentId, mediaType);

    /// <inheritdoc />
    protected override async Task<ContentEditingOperationStatus> MoveAsync(IMedia media, Guid? parentKey, bool includeDescendants, Guid userKey)
    {
        Attempt<ContentMoveOperationStatus> result = await ContentService.MoveAsync(media, parentKey, includeDescendants, userKey, CancellationToken.None);
        return result.Result switch
        {
            ContentMoveOperationStatus.Success => ContentEditingOperationStatus.Success,
            ContentMoveOperationStatus.CancelledByNotification => ContentEditingOperationStatus.CancelledByNotification,
            ContentMoveOperationStatus.ParentNotFound => ContentEditingOperationStatus.ParentNotFound,
            ContentMoveOperationStatus.ParentTrashed => ContentEditingOperationStatus.InTrash,
            _ => ContentEditingOperationStatus.Unknown,
        };
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Copy is not supported for media items.</exception>
    protected override Task<IMedia?> CopyAsync(IMedia media, Guid? parentKey, bool relateToOriginal, bool includeDescendants, Guid userKey)
        => throw new NotSupportedException("Copy is not supported for media");

    /// <inheritdoc />
    protected override async Task<OperationResult?> MoveToRecycleBinAsync(IMedia media, Guid userKey)
    {
        Attempt<ContentMoveToRecycleBinOperationStatus> result = await ContentService.MoveToRecycleBinAsync(media, userKey, CancellationToken.None);
        return result.Success
            ? OperationResult.Succeed(new EventMessages())
            : OperationResult.Cancel(new EventMessages());
    }

    /// <inheritdoc />
    protected override async Task<OperationResult?> DeleteAsync(IMedia media, Guid userKey)
    {
        Attempt<ContentDeleteOperationStatus> result = await ContentService.DeleteAsync(media, userKey, CancellationToken.None);
        return result.Success ? OperationResult.Succeed(new EventMessages()) : OperationResult.Cancel(new EventMessages());
    }

    /// <inheritdoc />
    protected override Task<PagedModel<IMedia>> GetPagedChildrenAsync(Guid? parentKey, int pageIndex, int pageSize, Ordering? ordering)
        => ContentService.GetChildrenAsync(parentKey, pageIndex * pageSize, pageSize, propertyAliases: null, ordering, CancellationToken.None);

    /// <inheritdoc />
    protected override async Task<ContentEditingOperationStatus> SortAsync(IReadOnlyList<Guid> orderedKeys, Guid userKey, CancellationToken cancellationToken)
    {
        Attempt<ContentSortOperationStatus> result = await ContentService.SortAsync(orderedKeys, userKey, cancellationToken);
        OperationResult operationResult = result.Result switch
        {
            ContentSortOperationStatus.Success => OperationResult.Succeed(new EventMessages()),
            ContentSortOperationStatus.NoOperation => new OperationResult(OperationResultType.NoOperation, new EventMessages()),
            _ => OperationResult.Cancel(new EventMessages()),
        };
        return OperationResultToOperationStatus(operationResult);
    }

    /// <inheritdoc />
    protected override async Task<ContentEditingOperationStatus> SortChildrenInBulkAsync(Guid? parentKey, IReadOnlyList<Guid> orderedChildKeys, Guid userKey)
    {
        Attempt<ContentSortChildrenOperationStatus> result = await ContentService.SortChildrenAsync(parentKey, orderedChildKeys, userKey, CancellationToken.None);
        OperationResult operationResult = result.Result switch
        {
            ContentSortChildrenOperationStatus.Success => OperationResult.Succeed(new EventMessages()),
            ContentSortChildrenOperationStatus.NoOperation => new OperationResult(OperationResultType.NoOperation, new EventMessages()),
            _ => OperationResult.Cancel(new EventMessages()),
        };
        return OperationResultToOperationStatus(operationResult);
    }

    private async Task<ContentEditingOperationStatus> SaveAsync(IMedia media, Guid userKey)
    {
        try
        {
            Attempt<ContentSaveOperationStatus> saveResult = await ContentService.SaveAsync(media, userKey, CancellationToken.None);
            return saveResult.Result switch
            {
                ContentSaveOperationStatus.Success => ContentEditingOperationStatus.Success,
                ContentSaveOperationStatus.CancelledByNotification => ContentEditingOperationStatus.CancelledByNotification,
                ContentSaveOperationStatus.InvalidName => ContentEditingOperationStatus.InvalidName,
                ContentSaveOperationStatus.DuplicateKey => ContentEditingOperationStatus.DuplicateKey,
                ContentSaveOperationStatus.InvalidKey => ContentEditingOperationStatus.InvalidKey,
                _ => ContentEditingOperationStatus.Unknown,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Media save operation failed");
            return ContentEditingOperationStatus.Unknown;
        }
    }
}
