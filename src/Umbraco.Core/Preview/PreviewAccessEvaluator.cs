using System.Collections.Concurrent;
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

    private readonly IPreviewSessionService _previewSessionService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewAccessEvaluator"/> class.
    /// </summary>
    /// <param name="previewSessionService">The service that reports whether the current request is in a preview session.</param>
    /// <param name="backOfficeSecurityAccessor">The accessor for the back-office user the preview session belongs to.</param>
    /// <param name="userService">The service used to load the preview user's permissions.</param>
    /// <param name="entityService">The service used to calculate the preview user's start nodes and to resolve element paths.</param>
    /// <param name="appCaches">The caches holding the per-request preview user and the user's start nodes.</param>
    public PreviewAccessEvaluator(
        IPreviewSessionService previewSessionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IEntityService entityService,
        AppCaches appCaches)
    {
        _previewSessionService = previewSessionService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <inheritdoc />
    public bool CanPreviewDocument(IPublishedContent content)
    {
        PreviewUser previewUser = GetPreviewUser();
        if (previewUser.IsInSession is false)
        {
            return true;
        }

        return previewUser.User is not null && CanPreview(previewUser, _documents, content.Path);
    }

    /// <inheritdoc />
    public bool CanPreviewElement(IPublishedElement element)
    {
        PreviewUser previewUser = GetPreviewUser();
        if (previewUser.IsInSession is false)
        {
            return true;
        }

        if (previewUser.User is null)
        {
            return false;
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
        if (_previewSessionService.IsActive() is false)
        {
            return PreviewUser.NoSession;
        }

        // The session rides on the back-office authentication cookie, which disabling or locking out a user does not
        // invalidate, so the user's state is checked here.
        IUser? user = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        return new PreviewUser(true, user is { IsApproved: true, IsLockedOut: false } ? user : null);
    }

    private sealed record PreviewTree(
        string Section,
        int RecycleBinId,
        string BrowsePermission,
        Func<IUser, IEntityService, AppCaches, int[]?> GetStartNodeIds);

    private sealed class PreviewUser
    {
        // Outside a preview session a draft was requested explicitly by server-side code (for example a webhook or
        // Delivery API preview) rather than by a previewing user, so it is allowed.
        public static readonly PreviewUser NoSession = new(false, null);

        public PreviewUser(bool isInSession, IUser? user)
        {
            IsInSession = isInSession;
            User = user;
        }

        public bool IsInSession { get; }

        // Null in a session whose user can't be resolved, or is disabled or locked out, which is refused every draft.
        public IUser? User { get; }

        public ConcurrentDictionary<(PreviewTree Tree, string Path), bool> PathAccess { get; } = new();

        public ConcurrentDictionary<Guid, string?> ElementPaths { get; } = new();
    }
}
