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
/// Grants the preview user the unpublished version of a document or element only when they have access to the
/// section it is managed in, it is within their start nodes for that tree, and they hold the browse permission for it.
/// </summary>
internal sealed class PreviewAccessEvaluator : IPreviewAccessEvaluator
{
    private const string PreviewUserCacheKey = "PreviewAccessEvaluator:PreviewUser";

    private static readonly PreviewTree _documents = new(
        Constants.Applications.Content,
        Constants.System.RecycleBinContent,
        ActionBrowse.ActionLetter,
        (user, entityService, appCaches) => user.CalculateContentStartNodeIds(entityService, appCaches));

    private static readonly PreviewTree _elements = new(
        Constants.Applications.Library,
        Constants.System.RecycleBinElement,
        ActionElementBrowse.ActionLetter,
        (user, entityService, appCaches) => user.CalculateElementStartNodeIds(entityService, appCaches));

    private readonly IPreviewService _previewService;
    private readonly IUserService _userService;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewAccessEvaluator"/> class.
    /// </summary>
    /// <param name="previewService">The service that resolves the user the current preview session belongs to.</param>
    /// <param name="userService">The service used to load the preview user and their permissions.</param>
    /// <param name="entityService">The service used to calculate the preview user's start nodes and to resolve element paths.</param>
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
        return previewUser.User is null || CanPreview(previewUser, _documents, content.Path);
    }

    /// <inheritdoc />
    public bool CanPreviewElement(IPublishedElement element)
    {
        PreviewUser previewUser = GetPreviewUser();
        if (previewUser.User is null)
        {
            return true;
        }

        // Published elements don't carry their tree path, so it is looked up (once per element per request).
        string? path = previewUser.ElementPaths.GetOrAdd(
            element.Key,
            key => _entityService.GetAllPaths(UmbracoObjectTypes.Element, key).FirstOrDefault()?.Path);

        return CanPreview(previewUser, _elements, path);
    }

    private bool CanPreview(PreviewUser previewUser, PreviewTree tree, string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || previewUser.User!.HasSectionAccess(tree.Section) is false)
        {
            return false;
        }

        return previewUser.PathAccess.GetOrAdd((tree, path), entry => HasBrowseAccess(previewUser.User!, entry.Tree, entry.Path));
    }

    private bool HasBrowseAccess(IUser user, PreviewTree tree, string path)
        => ContentPermissions.HasPathAccess(path, tree.GetStartNodeIds(user, _entityService, _appCaches), tree.RecycleBinId)
           && _userService.GetPermissionsForPath(user, path).GetAllPermissions().Contains(tree.BrowsePermission);

    private PreviewUser GetPreviewUser()
        => (PreviewUser)_appCaches.RequestCache.Get(PreviewUserCacheKey, ResolvePreviewUser)!;

    private PreviewUser ResolvePreviewUser()
    {
        Attempt<ClaimsIdentity> identityAttempt = _previewService.TryGetPreviewClaimsIdentityAsync().GetAwaiter().GetResult();
        Guid? userKey = identityAttempt.Success ? identityAttempt.Result?.GetUserKey() : null;
        IUser? user = userKey.HasValue ? _userService.GetAsync(userKey.Value).GetAwaiter().GetResult() : null;

        return user is null ? PreviewUser.None : new PreviewUser(user);
    }

    private sealed record PreviewTree(
        string Section,
        int RecycleBinId,
        string BrowsePermission,
        Func<IUser, IEntityService, AppCaches, int[]?> GetStartNodeIds);

    private sealed class PreviewUser
    {
        // No preview user means no verified website preview session (no or invalid cookie, a Delivery API request,
        // or no HTTP request at all), so a draft was requested explicitly by server-side code rather than by a
        // previewing user, and is allowed.
        public static readonly PreviewUser None = new(null);

        public PreviewUser(IUser? user) => User = user;

        public IUser? User { get; }

        public ConcurrentDictionary<(PreviewTree Tree, string Path), bool> PathAccess { get; } = new();

        public ConcurrentDictionary<Guid, string?> ElementPaths { get; } = new();
    }
}
