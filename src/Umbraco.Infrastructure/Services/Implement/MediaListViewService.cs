using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

internal sealed class MediaListViewService : ContentListViewServiceBase<IMedia, IMediaType, IMediaTypeService>, IMediaListViewService
{
    private readonly IMediaService _mediaService;
    private readonly IMediaPermissionAuthorizer _mediaPermissionAuthorizer;

    protected override Guid DefaultListViewKey => Constants.DataTypes.Guids.ListViewMediaGuid;

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

    // We can use an authorizer here, as it already handles all the necessary checks for this filtering.
    // However, we cannot pass in all the items; we want only the ones that comply, as opposed to
    // a general response whether the user has access to all nodes.
    protected override async Task<bool> HasAccessToListViewItemAsync(IUser user, Guid key)
    {
        var isDenied = await _mediaPermissionAuthorizer.IsDeniedAsync(
            user,
            key);

        return isDenied is false;
    }
}
