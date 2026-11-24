using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a named draft branch — an independently editable, in-progress draft that exists alongside a content
/// item's default draft, so multiple future states of the same node can be prepared in parallel (e.g. a
/// "Black Friday" draft and a "Summer Sale" draft on the same node), with exactly one eventually published.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.Models.Entities.IEntity" />
public interface IContentDraftBranch : IEntity, IRememberBeingDirty
{
    /// <summary>
    /// Gets the id of the node this draft branch belongs to.
    /// </summary>
    int NodeId { get; }

    /// <summary>
    /// Gets the key of the content item this draft branch belongs to (as distinct from <see cref="IEntity.Key"/>,
    /// the branch's own key). Lets the branch be resolved back to its content via the generic, entity-agnostic
    /// <c>GetById(Guid)</c> surface shared by Documents, Elements, etc.
    /// </summary>
    Guid ContentKey { get; }

    /// <summary>
    /// Gets or sets the editor-facing name of the draft branch (e.g. "Black Friday").
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the id of the user who created this draft branch.
    /// </summary>
    int? CreatedByUserId { get; }

    /// <summary>
    /// Gets the id of the content version that was current on the node's default draft at the moment this branch
    /// was forked.
    /// </summary>
    int? BasedOnVersionId { get; }

    /// <summary>
    /// Gets or sets the date and time (UTC) at which this branch should automatically be published, or
    /// <c>null</c> if it has no scheduled publish date. When it fires, the branch is first promoted the same
    /// way a manual <c>PromoteAsync</c> call would (its content copied forward onto the node's default
    /// draft), and the default draft is then given a scheduled release entry dated to this value, so the
    /// ordinary scheduled-publishing job picks it up and publishes it on its own next run - the same path
    /// used for any other scheduled publish.
    /// </summary>
    DateTime? ScheduledPublishDate { get; set; }

    /// <summary>
    /// Gets or sets the cultures to promote/publish when <see cref="ScheduledPublishDate"/> fires. Empty for
    /// invariant content. Chosen and permission-checked at the time the schedule is set, since the background
    /// job that later acts on it has no user context of its own to check against.
    /// </summary>
    ISet<string> ScheduledCultures { get; set; }
}
