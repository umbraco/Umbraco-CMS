using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

internal sealed class MediaListViewService : AsyncContentListViewServiceBase<IMedia, IMediaType, IMediaTypeService>, IMediaListViewService
{
    private readonly IMediaService _mediaService;
    private readonly IMediaPermissionAuthorizer _mediaPermissionAuthorizer;

    protected override Guid DefaultListViewKey => Constants.DataTypes.Guids.ListViewMediaGuid;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Services.Implement.MediaListViewService"/> class.
    /// </summary>
    /// <param name="mediaService">Service for managing media items.</param>
    /// <param name="mediaTypeService">Service for managing media types.</param>
    /// <param name="dataTypeService">Service for managing data types associated with media.</param>
    /// <param name="mediaSearchService">Service for searching media items.</param>
    /// <param name="mediaPermissionAuthorizer">Service for authorizing media permissions.</param>
    public MediaListViewService(
        IMediaService mediaService,
        IMediaTypeService mediaTypeService,
        IDataTypeService dataTypeService,
        IMediaSearchService mediaSearchService,
        IMediaPermissionAuthorizer mediaPermissionAuthorizer)
        : base(mediaTypeService, dataTypeService, mediaSearchService)
    {
        _mediaService = mediaService;
        _mediaPermissionAuthorizer = mediaPermissionAuthorizer;
    }

    /// <summary>
    /// Asynchronously retrieves a paged list of media items, filtered and ordered according to the specified parameters.
    /// </summary>
    /// <param name="user">The user requesting the media list view items.</param>
    /// <param name="key">An optional key identifying the parent media item to filter by. If null, retrieves from the root.</param>
    /// <param name="dataTypeKey">An optional data type key to further filter the media items.</param>
    /// <param name="orderBy">The property name to order the results by.</param>
    /// <param name="orderDirection">The direction to order the results (ascending or descending).</param>
    /// <param name="filter">An optional filter string to apply to the media items.</param>
    /// <param name="skip">The number of items to skip for paging.</param>
    /// <param name="take">The number of items to take for paging.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an <see cref="Attempt{T, TStatus}"/> with a paged list view model of media items if successful, or a <see cref="ContentCollectionOperationStatus"/> indicating the outcome.
    /// </returns>
    public async Task<Attempt<ListViewPagedModel<IMedia>?, ContentCollectionOperationStatus>> GetListViewItemsByKeyAsync(
        IUser user,
        Guid? key,
        Guid? dataTypeKey,
        string orderBy,
        Direction orderDirection,
        string? filter,
        int skip,
        int take)
    {
        IMedia? media = key.HasValue
            ? _mediaService.GetById(key.Value)
            : null;

        if (key.HasValue && media is null)
        {
            return Attempt.FailWithStatus<ListViewPagedModel<IMedia>?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.ContentNotFound, null);
        }

        return await GetListViewResultAsync(user, media, dataTypeKey, orderBy, null, orderDirection, filter, skip, take);
    }

    /// <inheritdoc/>
    protected override async Task<ISet<Guid>> FilterAuthorizedKeysAsync(IUser user, IEnumerable<Guid> keys) =>
        await _mediaPermissionAuthorizer.FilterAuthorizedAsync(user, keys);
}
