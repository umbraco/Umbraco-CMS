using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

internal sealed class MediaListViewService : ContentListViewServiceBase<IMedia, IMediaType, IMediaTypeService>, IMediaListViewService
{
    private readonly IMediaService _mediaService;
    private readonly IMediaPermissionService _mediaPermissionService;

    public MediaListViewService(
        IMediaService mediaService,
        IMediaTypeService mediaTypeService,
        IMediaPermissionService mediaPermissionService,
        IDataTypeService dataTypeService,
        ISqlContext sqlContext)
        : base(mediaTypeService, dataTypeService, sqlContext)
    {
        _mediaService = mediaService;
        _mediaPermissionService = mediaPermissionService;
    }

    protected override Guid DefaultListViewKey => Constants.DataTypes.Guids.ListViewMediaGuid;

    protected override IEnumerable<IMedia> GetPagedChildren(
        int id,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IMedia>? filter,
        Ordering? ordering) =>
        _mediaService.GetPagedChildren(
            id,
            pageIndex,
            pageSize,
            out totalRecords,
            filter,
            ordering);

    // We can use an authorizer here, as it already handles all the necessary checks for this filtering.
    // However, we cannot pass in all the items; we want only the ones that comply, as opposed to
    // a general response whether the user has access to all nodes.
    protected override async Task<bool> HasAccessToListViewItemAsync(IUser user, Guid key)
    {
        // TODO: Consider if it is better to use IMediaPermissionAuthorizer here as people will be able to apply their external authorization
        MediaAuthorizationStatus accessStatus = await _mediaPermissionService.AuthorizeAccessAsync(
            user,
            key);

        // var isAuthorized = await _mediaPermissionAuthorizer.IsAuthorizedAsync(
        //     user, //IPrincipal
        //     item.Key);
        //
        // return isAuthorized;

        return accessStatus == MediaAuthorizationStatus.Success;
    }

    public async Task<Attempt<ListViewPagedModel<IMedia>?, ContentCollectionOperationStatus>> GetListViewItemsByKeyAsync(
        IUser user,
        Guid? key,
        Guid? dataTypeKey,
        string orderBy,
        string? orderByCulture,
        Direction orderDirection,
        string? filter,
        int skip,
        int take)
    {
        IMedia? media = null;

        if (key.HasValue)
        {
            media = _mediaService.GetById(key.Value);

            if (media == null)
            {
                return Attempt.FailWithStatus<ListViewPagedModel<IMedia>?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.ContentNotFound, null);
            }
        }

        return await GetListViewResultAsync(user, media, dataTypeKey, orderBy, orderByCulture, orderDirection, filter, skip, take);
    }
}
