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
    public override Task<IMedia?> GetAsync(Guid key)
    {
        IMedia? media = ContentService.GetById(key);
        return Task.FromResult(media);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, MediaUpdateModel updateModel)
    {
        IMedia? media = ContentService.GetById(key);
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

        var currentUserId = await GetUserIdAsync(userKey);
        ContentEditingOperationStatus operationStatus = Save(media, currentUserId);
        return operationStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new MediaCreateResult { Content = media })
            : Attempt.FailWithStatus(operationStatus, new MediaCreateResult { Content = media });
    }

    /// <inheritdoc />
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
        var userId = await GetUserIdAsync(userKey);
        Attempt<OperationResult?> result = ContentService.Move(media, ResolveParentId(parentKey), includeDescendants, userId);
        return OperationResultToOperationStatus(result.Result);
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Copy is not supported for media items.</exception>
    protected override Task<IMedia?> CopyAsync(IMedia media, Guid? parentKey, bool relateToOriginal, bool includeDescendants, Guid userKey)
        => throw new NotSupportedException("Copy is not supported for media");

    /// <inheritdoc />
    protected override async Task<OperationResult?> MoveToRecycleBinAsync(IMedia media, Guid userKey)
    {
        var userId = await GetUserIdAsync(userKey);
        return ContentService.MoveToRecycleBin(media, userId).Result;
    }

    /// <inheritdoc />
    protected override async Task<OperationResult?> DeleteAsync(IMedia media, Guid userKey)
    {
        var userId = await GetUserIdAsync(userKey);
        return ContentService.Delete(media, userId).Result;
    }

    /// <inheritdoc />
    protected override Task<PagedModel<IMedia>> GetPagedChildrenAsync(Guid? parentKey, int pageIndex, int pageSize, Ordering? ordering)
    {
        IEnumerable<IMedia> pagedChildren = ContentService.GetPagedChildren(ResolveParentId(parentKey), pageIndex, pageSize, out long total, filter: null, ordering: ordering);
        return Task.FromResult(new PagedModel<IMedia>(total, pagedChildren));
    }

    /// <inheritdoc />
    protected override async Task<ContentEditingOperationStatus> SortAsync(IReadOnlyList<Guid> orderedKeys, Guid userKey, CancellationToken cancellationToken)
    {
        var userId = await GetUserIdAsync(userKey);
        Dictionary<Guid, IMedia> itemsByKey = ContentService.GetByIds(orderedKeys).ToDictionary(item => item.Key);
        IEnumerable<IMedia> orderedItems = orderedKeys.Where(itemsByKey.ContainsKey).Select(key => itemsByKey[key]);

        return ContentService.Sort(orderedItems, userId)
            ? ContentEditingOperationStatus.Success
            : ContentEditingOperationStatus.CancelledByNotification;
    }

    /// <inheritdoc />
    protected override async Task<ContentEditingOperationStatus> SortChildrenInBulkAsync(Guid? parentKey, IReadOnlyList<Guid> orderedChildKeys, Guid userKey)
    {
        var userId = await GetUserIdAsync(userKey);
        Dictionary<Guid, int> idsByKey = ContentService.GetByIds(orderedChildKeys).ToDictionary(child => child.Key, child => child.Id);
        List<int> orderedChildIds = orderedChildKeys.Where(idsByKey.ContainsKey).Select(key => idsByKey[key]).ToList();

        OperationResult result = ContentService.SortChildren(ResolveParentId(parentKey), orderedChildIds, userId);
        return OperationResultToOperationStatus(result);
    }

    // The media service still identifies parents by id; the recycle bin and the root have fixed ids and every
    // other parent is looked up by its key.
    private int ResolveParentId(Guid? parentKey)
    {
        if (parentKey is null)
        {
            return Constants.System.Root;
        }

        if (parentKey == Constants.System.RecycleBinMediaKey)
        {
            return Constants.System.RecycleBinMedia;
        }

        return ContentService.GetById(parentKey.Value)?.Id ?? Constants.System.Root;
    }

    /// <summary>
    ///     Saves a media item to the repository.
    /// </summary>
    /// <param name="media">The <see cref="IMedia"/> item to save.</param>
    /// <param name="userId">The identifier of the user performing the save operation.</param>
    /// <returns>A <see cref="ContentEditingOperationStatus"/> indicating the outcome of the save operation.</returns>
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
                OperationResultType.FailedInvalidKey => ContentEditingOperationStatus.InvalidKey,

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
