using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Manages named draft branches — independently editable, in-progress drafts that exist alongside a content
/// item's default draft, so multiple future states of the same node can be prepared in parallel.
/// </summary>
public interface IContentDraftBranchService
{
    /// <summary>
    /// Creates a new named draft branch for a content item, forked from the node's current default draft
    /// (which may already carry unpublished edits) rather than from its published version.
    /// </summary>
    /// <param name="contentKey">The key of the content item to branch.</param>
    /// <param name="name">The editor-facing name of the new branch (e.g. "Black Friday").</param>
    /// <param name="userKey">The key of the user creating the branch.</param>
    Task<Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus>> CreateAsync(Guid contentKey, string name, Guid userKey);

    /// <summary>
    /// Gets a draft branch by its key, scoped to the content item it must belong to.
    /// </summary>
    /// <param name="contentKey">The key of the content item the branch is expected to belong to.</param>
    /// <param name="branchKey">The key of the branch to retrieve.</param>
    /// <returns>The branch, or <c>null</c> if it doesn't exist or belongs to a different content item.</returns>
    IContentDraftBranch? GetByKey(Guid contentKey, Guid branchKey);

    /// <summary>
    /// Gets all draft branches for a content item, in creation order.
    /// </summary>
    IEnumerable<IContentDraftBranch> GetByContentKey(Guid contentKey);

    /// <summary>
    /// Gets a draft branch's content for editing, scoped to the content item it must belong to. The returned
    /// instance's version id points at the branch's own version row - saving it back via <see cref="SaveAsync"/>
    /// persists changes into that branch alone, leaving the node's default draft (and any other branches) untouched.
    /// </summary>
    /// <param name="contentKey">The key of the content item the branch is expected to belong to.</param>
    /// <param name="branchKey">The key of the branch to load content for.</param>
    /// <returns>The branch's current content, or <c>null</c> if the branch doesn't exist or belongs to a different content item.</returns>
    IContent? GetContent(Guid contentKey, Guid branchKey);

    /// <summary>
    /// Saves edits to a draft branch's content, as loaded via <see cref="GetContent"/>. This is a plain save
    /// only - it never publishes; promoting a branch is <see cref="PromoteAsync"/>, which also never publishes.
    /// </summary>
    /// <param name="contentKey">The key of the content item the branch is expected to belong to.</param>
    /// <param name="branchKey">The key of the branch being edited.</param>
    /// <param name="content">The branch's content, with in-memory changes applied.</param>
    /// <param name="userKey">The key of the user saving the changes.</param>
    Task<Attempt<ContentDraftBranchOperationStatus>> SaveAsync(Guid contentKey, Guid branchKey, IContent content, Guid userKey);

    /// <summary>
    /// Abandons an active or stale draft branch. This is an immediate, permanent deletion of the branch row -
    /// not a status change. There is no terminal "abandoned" status; once abandoned, the branch simply no
    /// longer exists, and there is no retention window during which it could be recovered.
    /// </summary>
    /// <param name="contentKey">The key of the content item the branch is expected to belong to.</param>
    /// <param name="branchKey">The key of the branch to abandon.</param>
    Attempt<ContentDraftBranchOperationStatus> Abandon(Guid contentKey, Guid branchKey);

    /// <summary>
    /// Promotes a draft branch: its edited values are copied forward onto the node's actual default draft via
    /// a plain save. This never publishes anything - the default draft is left exactly as published or
    /// unpublished as it already was. Publishing is always a separate, ordinary content-service operation,
    /// triggered either by an explicit editorial publish action on the document, or by the document's own
    /// scheduled-publish date firing - never as a side effect of promoting a branch. Promoting is an immediate,
    /// permanent deletion of the branch row, mirroring <see cref="Abandon"/> exactly - there is no terminal
    /// "promoted" status and no retention window; once promoted, the branch simply no longer exists. Promoting
    /// one branch does not affect any other branch on the same node.
    /// </summary>
    /// <remarks>
    /// <paramref name="cultures"/> may name only a subset of the branch's cultures - the branch row is still
    /// deleted in full afterward, regardless of how many cultures were included. Any branch content for
    /// cultures NOT named here is therefore permanently lost, with no retention window to recover it. Callers
    /// are responsible for warning users about this before promoting only part of a branch. Whether invariant
    /// properties are carried along is decided independently of <paramref name="cultures"/>, based on whether
    /// the promoting user has access to the default language (or <c>AllowEditInvariantFromNonDefault</c> is
    /// enabled) - invariant is never itself an entry in <paramref name="cultures"/>.
    /// </remarks>
    /// <param name="contentKey">The key of the content item the branch is expected to belong to.</param>
    /// <param name="branchKey">The key of the branch to promote.</param>
    /// <param name="cultures">
    /// The cultures to promote. For culture-variant content, must be non-empty and every entry must exist on
    /// the branch's own content. For invariant content, must be empty.
    /// </param>
    /// <param name="userKey">The key of the user promoting the branch.</param>
    Task<Attempt<ContentDraftBranchOperationStatus>> PromoteAsync(Guid contentKey, Guid branchKey, ISet<string> cultures, Guid userKey);

    /// <summary>
    /// Gets a content item's version history, restricted to either its default (unbranched) draft or a
    /// single named draft branch.
    /// </summary>
    /// <param name="contentKey">The key of the content item.</param>
    /// <param name="branchKey">
    /// When <c>null</c> (the default), returns only the node's own (unbranched) history, excluding any named
    /// draft branch's version rows. When specified, returns only that branch's own version row(s).
    /// </param>
    /// <returns>The matching versions, newest first, or an empty collection if the content item or branch don't exist.</returns>
    IEnumerable<IContent> GetVersions(Guid contentKey, Guid? branchKey = null);

    /// <summary>
    /// Gets a page of a content item's version history, restricted to either its default (unbranched) draft
    /// or a single named draft branch, loading the bare minimum of data needed to support rollback with
    /// variants.
    /// </summary>
    /// <param name="contentKey">The key of the content item.</param>
    /// <param name="skip">The number of versions to skip.</param>
    /// <param name="take">The number of versions to take.</param>
    /// <param name="branchKey">
    /// When <c>null</c> (the default), returns only the node's own (unbranched) history, excluding any named
    /// draft branch's version rows. When specified, returns only that branch's own version row(s).
    /// </param>
    /// <returns>The matching versions, newest first, or an empty collection if the content item or branch don't exist.</returns>
    IEnumerable<IContent> GetVersionsSlim(Guid contentKey, int skip, int take, Guid? branchKey = null);

    /// <summary>
    /// Sets or clears a draft branch's scheduled publish date and the cultures to promote/publish when it
    /// fires. When set, once the date passes, the background scheduled-publishing job promotes the branch for
    /// exactly these cultures (copying its content forward onto the node's default draft, exactly as
    /// <see cref="PromoteAsync"/> would) and then queues a scheduled release on the default draft dated to
    /// this value, so the ordinary content scheduled-publishing job publishes it on its own next run - the
    /// same publish path used for any other scheduled publish, with no branch-specific notification involved.
    /// </summary>
    /// <remarks>
    /// As with <see cref="PromoteAsync"/>, <paramref name="cultures"/> may name only a subset of the branch's
    /// cultures - when the schedule fires, the branch row is still deleted in full, so any branch content for
    /// cultures NOT named here is permanently lost once the scheduled promotion runs.
    /// </remarks>
    /// <param name="contentKey">The key of the content item the branch is expected to belong to.</param>
    /// <param name="branchKey">The key of the branch to schedule.</param>
    /// <param name="publishDate">The date and time (UTC) to publish the branch, or <c>null</c> to clear an existing schedule.</param>
    /// <param name="cultures">
    /// The cultures to promote/publish when the schedule fires. For culture-variant content, must be non-empty
    /// and every entry must exist on the branch's own content. For invariant content, must be empty.
    /// </param>
    /// <returns>At attempt describing the result status of the operation.</returns>
    Attempt<ContentDraftBranchOperationStatus> Schedule(Guid contentKey, Guid branchKey, DateTime? publishDate, ISet<string> cultures);
}
