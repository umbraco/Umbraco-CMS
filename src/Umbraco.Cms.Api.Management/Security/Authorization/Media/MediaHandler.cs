using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Media;

/// <summary>
///     Authorizes that the current user has the correct permission access to the media item(s) specified in the request.
/// </summary>
public class MediaHandler : MustSatisfyRequirementAuthorizationHandler<MediaRequirement, IEnumerable<Guid>>
{
    private readonly IMediaAuthorizer _mediaAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaHandler" /> class.
    /// </summary>
    /// <param name="mediaAuthorizer">Authorizer for media access.</param>
    public MediaHandler(IMediaAuthorizer mediaAuthorizer)
        => _mediaAuthorizer = mediaAuthorizer;

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        MediaRequirement requirement,
        IEnumerable<Guid> resource) =>
        await _mediaAuthorizer.IsAuthorizedAsync(context.User, resource);
}
