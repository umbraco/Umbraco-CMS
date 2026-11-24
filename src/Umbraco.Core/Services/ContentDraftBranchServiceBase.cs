using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Shared implementation of named draft branch management for a specific publishable content type. Concrete
/// subclasses (<see cref="ContentDraftBranchService"/> for <see cref="IContent"/>, <see cref="ElementDraftBranchService"/>
/// for <see cref="IElement"/>) supply the content-type-specific service and repository.
/// </summary>
/// <typeparam name="TContent">The publishable content type (<see cref="IContent"/>, <see cref="IElement"/>, etc.).</typeparam>
/// <typeparam name="TRepository">The repository type for <typeparamref name="TContent"/>.</typeparam>
public abstract class ContentDraftBranchServiceBase<TContent, TRepository> : RepositoryService
    where TContent : class, IPublishableContentBase
    where TRepository : class, IPublishableContentRepository<TContent>
{
    private readonly IContentDraftBranchRepository _contentDraftBranchRepository;
    private readonly IPublishableContentService<TContent> _contentService;
    private readonly TRepository _contentRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IUserService _userService;
    private readonly ILanguageService _languageService;
    private ContentSettings _contentSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentDraftBranchServiceBase{TContent, TRepository}" /> class.
    /// </summary>
    protected ContentDraftBranchServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IContentDraftBranchRepository contentDraftBranchRepository,
        IPublishableContentService<TContent> contentService,
        TRepository contentRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IUserService userService,
        ILanguageService languageService,
        IOptionsMonitor<ContentSettings> contentSettingsMonitor)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _contentDraftBranchRepository = contentDraftBranchRepository;
        _contentService = contentService;
        _contentRepository = contentRepository;
        _userIdKeyResolver = userIdKeyResolver;
        _userService = userService;
        _languageService = languageService;
        _contentSettings = contentSettingsMonitor.CurrentValue;
        contentSettingsMonitor.OnChange(settings => _contentSettings = settings);
    }

    /// <inheritdoc cref="IContentDraftBranchService.CreateAsync" />
    public async Task<Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus>> CreateAsync(Guid contentKey, string name, Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        TContent? content = _contentService.GetById(contentKey);
        if (content is null)
        {
            return Attempt.FailWithStatus<IContentDraftBranch?, ContentDraftBranchOperationStatus>(ContentDraftBranchOperationStatus.ContentNotFound, null);
        }

        if (content.VersionId <= 0)
        {
            return Attempt.FailWithStatus<IContentDraftBranch?, ContentDraftBranchOperationStatus>(ContentDraftBranchOperationStatus.NoDefaultDraft, null);
        }

        var userId = await _userIdKeyResolver.GetAsync(userKey);

        var branch = new ContentDraftBranch(content.Id, content.Key, name, content.VersionId, userId);

        EventMessages eventMessages = EventMessagesFactory.Get();
        var creatingNotification = new ContentDraftBranchCreatingNotification(branch, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(creatingNotification))
        {
            return Attempt.FailWithStatus<IContentDraftBranch?, ContentDraftBranchOperationStatus>(ContentDraftBranchOperationStatus.CancelledByNotification, null);
        }

        _contentDraftBranchRepository.Save(branch);

        // fork the branch's initial draft from the node's current default draft (which may already carry
        // unpublished edits) - not from the published version, since editors only ever see draft content.
        _contentRepository.CreateBranchVersion(content, branch.Id);

        scope.Notifications.Publish(new ContentDraftBranchCreatedNotification(branch, eventMessages).WithStateFrom(creatingNotification));

        scope.Complete();

        return Attempt.SucceedWithStatus<IContentDraftBranch?, ContentDraftBranchOperationStatus>(ContentDraftBranchOperationStatus.Success, branch);
    }

    /// <inheritdoc cref="IContentDraftBranchService.GetByKey" />
    public IContentDraftBranch? GetByKey(Guid contentKey, Guid branchKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return GetValidatedBranch(contentKey, branchKey);
    }

    /// <inheritdoc cref="IContentDraftBranchService.GetByContentKey" />
    public IEnumerable<IContentDraftBranch> GetByContentKey(Guid contentKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        TContent? content = _contentService.GetById(contentKey);
        return content is null
            ? []
            : _contentDraftBranchRepository.GetByNodeId(content.Id);
    }

    /// <inheritdoc cref="IContentDraftBranchService.GetContent" />
    public TContent? GetContent(Guid contentKey, Guid branchKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        IContentDraftBranch? branch = GetValidatedBranch(contentKey, branchKey);
        return branch is null ? null : _contentRepository.GetByBranch(branch.NodeId, branch.Id);
    }

    /// <inheritdoc cref="IContentDraftBranchService.SaveAsync" />
    public async Task<Attempt<ContentDraftBranchOperationStatus>> SaveAsync(Guid contentKey, Guid branchKey, TContent content, Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IContentDraftBranch? branch = GetValidatedBranch(contentKey, branchKey);
        if (branch is null)
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new ContentDraftBranchSavingNotification(branch, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.CancelledByNotification);
        }

        var userId = await _userIdKeyResolver.GetAsync(userKey);

        try
        {
            OperationResult saveResult = _contentService.Save(content, userId);
            if (!saveResult.Success)
            {
                return Attempt.Fail(ContentDraftBranchOperationStatus.SaveFailed);
            }
        }
        catch (InvalidOperationException)
        {
            // the row backing this branch is no longer current - e.g. the branch was promoted or abandoned (and
            // its version row superseded, and the branch itself deleted) by someone else between loading and
            // saving. From the caller's perspective the branch no longer exists.
            return Attempt.Fail(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        scope.Notifications.Publish(new ContentDraftBranchSavedNotification(branch, eventMessages).WithStateFrom(savingNotification));

        scope.Complete();

        return Attempt.Succeed(ContentDraftBranchOperationStatus.Success);
    }

    /// <inheritdoc cref="IContentDraftBranchService.Abandon" />
    public Attempt<ContentDraftBranchOperationStatus> Abandon(Guid contentKey, Guid branchKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IContentDraftBranch? branch = GetValidatedBranch(contentKey, branchKey);
        if (branch is null)
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var abandoningNotification = new ContentDraftBranchAbandoningNotification(branch, eventMessages);
        if (scope.Notifications.PublishCancelable(abandoningNotification))
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.CancelledByNotification);
        }

        // supersede the branch's own version row BEFORE deleting the branch itself: deleting nulls this row's
        // BranchId without touching Current, so if it were still Current=true at that point it would collide
        // with the default draft's own (BranchId=null, Current=true) row - violating "one current row per
        // (NodeId, BranchId)". Superseding first guarantees the row is already Current=false before that FK
        // link is released.
        TContent? branchContent = _contentRepository.GetByBranch(branch.NodeId, branch.Id);
        if (branchContent is not null)
        {
            _contentRepository.MarkBranchVersionSuperseded(branchContent.VersionId);
        }

        // abandoning is an editorial deletion, not a status change: the branch row is gone immediately - it
        // is not retained for the cleanup job to reap later.
        _contentDraftBranchRepository.Delete(branch.Id);

        scope.Notifications.Publish(new ContentDraftBranchAbandonedNotification(branch, eventMessages).WithStateFrom(abandoningNotification));

        scope.Complete();

        return Attempt.Succeed(ContentDraftBranchOperationStatus.Success);
    }

    /// <inheritdoc cref="IContentDraftBranchService.PromoteAsync" />
    public async Task<Attempt<ContentDraftBranchOperationStatus>> PromoteAsync(Guid contentKey, Guid branchKey, ISet<string> cultures, Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IContentDraftBranch? branch = GetValidatedBranch(contentKey, branchKey);
        if (branch is null)
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        TContent? branchContent = _contentRepository.GetByBranch(branch.NodeId, branch.Id);
        if (branchContent is null)
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.BranchVersionNotFound);
        }

        TContent? defaultDraft = _contentService.GetById(branch.ContentKey);
        if (defaultDraft is null)
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.ContentNotFound);
        }

        bool variesByCulture = defaultDraft.ContentType.VariesByCulture();
        ContentDraftBranchOperationStatus? cultureValidationFailure = ValidateCultures(variesByCulture, cultures, branchContent);
        if (cultureValidationFailure is not null)
        {
            return Attempt.Fail(cultureValidationFailure.Value);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var promotingNotification = new ContentDraftBranchPromotingNotification(branch, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(promotingNotification))
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.CancelledByNotification);
        }

        // Copy the branch's edited values onto the node's actual default draft via a plain save - this is the
        // same copy-forward pattern Rollback uses to restore a historical version, applied here to promote a
        // branch instead. Whatever the default draft itself had pending (if it had diverged since the branch
        // was forked) is deliberately overwritten - branches don't merge, they win outright. Deliberately NOT
        // a publish: promoting a branch never triggers SaveAndPublish or fires any publish-related
        // notification. Publishing only ever happens as a separate, ordinary content-service operation -
        // either an editorial publish action, or the document's own scheduled-publish date firing - and by
        // that point it's just an ordinary draft with pending changes, indistinguishable from any other.
        //
        // Even a partial-culture promote still deletes the whole branch below - any branch content for
        // cultures NOT included here is permanently lost, with no retention. This is intentional.
        if (variesByCulture)
        {
            foreach (string culture in cultures)
            {
                defaultDraft.CopyFrom(branchContent, culture, includeInvariant: false);
            }

            if (await ShouldIncludeInvariantPropertiesAsync(userKey))
            {
                defaultDraft.CopyFrom(branchContent, culture: null);
            }
        }
        else
        {
            defaultDraft.CopyFrom(branchContent);
        }

        var userId = await _userIdKeyResolver.GetAsync(userKey);

        OperationResult saveResult = _contentService.Save(defaultDraft, userId);
        if (!saveResult.Success)
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.SaveFailed);
        }

        // the branch's own version row played no further role once its values were copied forward - supersede
        // it BEFORE deleting the branch itself: deleting nulls this row's BranchId without touching Current,
        // so if it were still Current=true at that point it would collide with the default draft's own
        // (BranchId=null, Current=true) row - violating "one current row per (NodeId, BranchId)". Superseding
        // first guarantees the row is already Current=false before that FK link is released.
        _contentRepository.MarkBranchVersionSuperseded(branchContent.VersionId);

        // promoting is a terminal action, same as abandoning: the branch row is gone immediately, mirroring
        // Abandon exactly - there is no lingering "promoted" status for a cleanup job to reap later.
        _contentDraftBranchRepository.Delete(branch.Id);

        scope.Notifications.Publish(new ContentDraftBranchPromotedNotification(branch, eventMessages).WithStateFrom(promotingNotification));

        scope.Complete();

        return Attempt.Succeed(ContentDraftBranchOperationStatus.Success);
    }

    /// <inheritdoc cref="IContentDraftBranchService.GetVersions" />
    public IEnumerable<TContent> GetVersions(Guid contentKey, Guid? branchKey = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        TContent? content = _contentService.GetById(contentKey);
        if (content is null)
        {
            return [];
        }

        if (branchKey is null)
        {
            return _contentRepository.GetAllVersions(content.Id, branchId: null);
        }

        IContentDraftBranch? branch = _contentDraftBranchRepository.Get(branchKey.Value);
        return branch is null ? [] : _contentRepository.GetAllVersions(branch.NodeId, branch.Id);
    }

    /// <inheritdoc cref="IContentDraftBranchService.GetVersionsSlim" />
    public IEnumerable<TContent> GetVersionsSlim(Guid contentKey, int skip, int take, Guid? branchKey = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        TContent? content = _contentService.GetById(contentKey);
        if (content is null)
        {
            return [];
        }

        if (branchKey is null)
        {
            return _contentRepository.GetAllVersionsSlim(content.Id, skip, take, branchId: null);
        }

        IContentDraftBranch? branch = _contentDraftBranchRepository.Get(branchKey.Value);
        return branch is null ? [] : _contentRepository.GetAllVersionsSlim(branch.NodeId, skip, take, branch.Id);
    }

    /// <inheritdoc cref="IContentDraftBranchService.Schedule" />
    public Attempt<ContentDraftBranchOperationStatus> Schedule(Guid contentKey, Guid branchKey, DateTime? publishDate, ISet<string> cultures)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IContentDraftBranch? branch = GetValidatedBranch(contentKey, branchKey);
        if (branch is null)
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        TContent? branchContent = _contentRepository.GetByBranch(branch.NodeId, branch.Id);
        if (branchContent is null)
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.BranchVersionNotFound);
        }

        TContent? defaultDraft = _contentService.GetById(branch.ContentKey);
        if (defaultDraft is null)
        {
            return Attempt.Fail(ContentDraftBranchOperationStatus.ContentNotFound);
        }

        ContentDraftBranchOperationStatus? cultureValidationFailure =
            ValidateCultures(defaultDraft.ContentType.VariesByCulture(), cultures, branchContent);
        if (cultureValidationFailure is not null)
        {
            return Attempt.Fail(cultureValidationFailure.Value);
        }

        branch.ScheduledPublishDate = publishDate?.ToUniversalTime();
        branch.ScheduledCultures = cultures;
        _contentDraftBranchRepository.Save(branch);

        scope.Complete();

        return Attempt.Succeed(ContentDraftBranchOperationStatus.Success);
    }

    /// <summary>
    /// Resolves a branch by its own key, scoping it to the content item it must belong to - a branch key that
    /// resolves to a different content item is treated as not found, rather than leaking that it exists
    /// elsewhere. This is the single place every branch-scoped operation validates that invariant, so no
    /// caller (API or otherwise) can act on a branch by pairing its key with the wrong content key.
    /// </summary>
    private IContentDraftBranch? GetValidatedBranch(Guid contentKey, Guid branchKey)
    {
        IContentDraftBranch? branch = _contentDraftBranchRepository.Get(branchKey);
        return branch is not null && branch.ContentKey == contentKey ? branch : null;
    }

    /// <summary>
    /// Validates a requested culture set for a promote/schedule operation: culture-variant content requires at
    /// least one culture, each of which must exist on the branch's own content; invariant content requires an
    /// empty set. Returns the failure status if validation fails, or <c>null</c> if the set is valid.
    /// </summary>
    private static ContentDraftBranchOperationStatus? ValidateCultures(bool variesByCulture, ISet<string> cultures, TContent branchContent)
    {
        if (variesByCulture)
        {
            if (cultures.Count == 0)
            {
                return ContentDraftBranchOperationStatus.CultureMissing;
            }

            if (!cultures.All(culture => branchContent.AvailableCultures.InvariantContains(culture)))
            {
                return ContentDraftBranchOperationStatus.InvalidCulture;
            }
        }
        else if (cultures.Count != 0)
        {
            return ContentDraftBranchOperationStatus.InvalidCulture;
        }

        return null;
    }

    /// <summary>
    /// Determines whether invariant properties should be carried along with a culture-variant promote/schedule
    /// operation, mirroring the same rule already established for ordinary content editing: a user may touch
    /// invariant properties from a non-default culture only if they have access to the default language, or if
    /// <see cref="ContentSettings.AllowEditInvariantFromNonDefault"/> is enabled tenant-wide.
    /// </summary>
    private async Task<bool> ShouldIncludeInvariantPropertiesAsync(Guid userKey)
    {
        // the tenant-wide setting grants invariant access unconditionally - no need to resolve the user or
        // their allowed languages at all when it's already true.
        if (_contentSettings.AllowEditInvariantFromNonDefault)
        {
            return true;
        }

        IUser? user = await _userService.GetAsync(userKey);
        if (user is null)
        {
            // the caller's user could not be resolved - conservatively exclude invariant properties rather
            // than silently including data the (unresolvable) user may not be allowed to touch.
            return false;
        }

        int[] allowedLanguageIds = await user.CalculateAllowedLanguageIdsAsync(_languageService);
        HashSet<string> allowedCultures = (await _languageService.GetIsoCodesByIdsAsync(allowedLanguageIds)).ToHashSet();
        ILanguage? defaultLanguage = await _languageService.GetDefaultLanguageAsync();

        return allowedCultures.Contains(defaultLanguage?.IsoCode ?? string.Empty);
    }
}
