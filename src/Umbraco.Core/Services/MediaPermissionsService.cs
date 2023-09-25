using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class MediaPermissionsService : IMediaPermissionsService
{
    private readonly IMediaService _mediaService;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    public MediaPermissionsService(
        IMediaService mediaService,
        IEntityService entityService,
        AppCaches appCaches)
    {
        _mediaService = mediaService;
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <inheritdoc/>
    public async Task<MediaAuthorizationStatus> AuthorizeAccessAsync(IUser performingUser, IEnumerable<Guid> mediaKeys)
    {
        IEnumerable<IMedia> mediaItems = _mediaService.GetByIds(mediaKeys);

        // Check if the user has access to all media items
        var hasAccessToAll = mediaItems.All(mediaItem => performingUser.HasPathAccess(mediaItem, _entityService, _appCaches));

        return hasAccessToAll
            ? MediaAuthorizationStatus.Success
            : MediaAuthorizationStatus.UnauthorizedMissingPathAccess;
    }

    /// <inheritdoc/>
    public async Task<MediaAuthorizationStatus> AuthorizeRootAccessAsync(IUser performingUser)
        => performingUser.HasMediaRootAccess(_entityService, _appCaches)
            ? MediaAuthorizationStatus.Success
            : MediaAuthorizationStatus.UnauthorizedMissingRootAccess;

    /// <inheritdoc/>
    public async Task<MediaAuthorizationStatus> AuthorizeBinAccessAsync(IUser performingUser)
        => performingUser.HasMediaBinAccess(_entityService, _appCaches)
            ? MediaAuthorizationStatus.Success
            : MediaAuthorizationStatus.UnauthorizedMissingBinAccess;
}
