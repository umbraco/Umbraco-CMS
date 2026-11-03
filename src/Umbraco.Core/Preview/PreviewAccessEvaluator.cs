using System.Collections.Concurrent;
using System.Security.Claims;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Preview;

/// <summary>
/// Grants the preview user the unpublished version of a document only when they have access to the content section,
/// the document is within their content start nodes, and they hold the browse permission for it.
/// </summary>
internal sealed class PreviewAccessEvaluator : IPreviewAccessEvaluator
{
    private const string PreviewUserCacheKey = "PreviewAccessEvaluator:PreviewUser";

    private readonly IPreviewService _previewService;
    private readonly IUserService _userService;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewAccessEvaluator"/> class.
    /// </summary>
    /// <param name="previewService">The service that resolves the user the current preview session belongs to.</param>
    /// <param name="userService">The service used to load the preview user and their permissions.</param>
    /// <param name="entityService">The service used to calculate the preview user's start nodes.</param>
    /// <param name="appCaches">The caches holding the per-request preview user and the user's start nodes.</param>
    public PreviewAccessEvaluator(
        IPreviewService previewService,
        IUserService userService,
        IEntityService entityService,
        AppCaches appCaches)
    {
        _previewService = previewService;
        _userService = userService;
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <inheritdoc />
    public bool CanPreviewDocument(IPublishedContent content)
    {
        PreviewUser previewUser = GetPreviewUser();

        // No preview user means no verified website preview session (no or invalid cookie, a Delivery API request,
        // or no HTTP request at all), so the draft was requested explicitly by server-side code rather than by a
        // previewing user.
        if (previewUser.User is null)
        {
            return true;
        }

        if (previewUser.HasContentSectionAccess is false || string.IsNullOrWhiteSpace(content.Path))
        {
            return false;
        }

        return previewUser.PathAccess.GetOrAdd(content.Path, path => HasBrowseAccess(previewUser, path));
    }

    private bool HasBrowseAccess(PreviewUser previewUser, string path)
        => ContentPermissions.HasPathAccess(path, previewUser.StartNodeIds, Constants.System.RecycleBinContent)
           && _userService.GetPermissionsForPath(previewUser.User!, path).GetAllPermissions().Contains(ActionBrowse.ActionLetter);

    private PreviewUser GetPreviewUser()
        => (PreviewUser)_appCaches.RequestCache.Get(PreviewUserCacheKey, ResolvePreviewUser)!;

    private PreviewUser ResolvePreviewUser()
    {
        Attempt<ClaimsIdentity> identityAttempt = _previewService.TryGetPreviewClaimsIdentityAsync().GetAwaiter().GetResult();
        Guid? userKey = identityAttempt.Success ? identityAttempt.Result?.GetUserKey() : null;
        IUser? user = userKey.HasValue ? _userService.GetAsync(userKey.Value).GetAwaiter().GetResult() : null;

        return user is null
            ? PreviewUser.None
            : new PreviewUser(
                user,
                user.HasSectionAccess(Constants.Applications.Content),
                user.CalculateContentStartNodeIds(_entityService, _appCaches));
    }

    private sealed class PreviewUser
    {
        public static readonly PreviewUser None = new(null, false, null);

        public PreviewUser(IUser? user, bool hasContentSectionAccess, int[]? startNodeIds)
        {
            User = user;
            HasContentSectionAccess = hasContentSectionAccess;
            StartNodeIds = startNodeIds;
        }

        public IUser? User { get; }

        public bool HasContentSectionAccess { get; }

        public int[]? StartNodeIds { get; }

        public ConcurrentDictionary<string, bool> PathAccess { get; } = new();
    }
}
