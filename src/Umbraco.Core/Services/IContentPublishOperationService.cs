// src/Umbraco.Core/Services/IContentPublishOperationService.cs
using System.ComponentModel;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for content publishing operations (publish, unpublish, scheduled publishing, branch publishing).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Implementation Note:</strong> Do not implement this interface directly.
/// Instead, inherit from <see cref="ContentServiceBase"/> which provides required
/// infrastructure (scoping, repository access, auditing). Direct implementation
/// without this base class will result in missing functionality.
/// </para>
/// <para>
/// This interface is part of the ContentService refactoring initiative (Phase 5).
/// It extracts publishing operations into a focused, testable service.
/// </para>
/// <para>
/// <strong>Note:</strong> This interface is named IContentPublishOperationService to avoid
/// collision with the existing IContentPublishingService which is an API-layer orchestrator.
/// </para>
/// <para>
/// <strong>Versioning Policy:</strong> This interface follows additive-only changes.
/// New methods may be added with default implementations. Existing methods will not
/// be removed or have signatures changed without a 2 major version deprecation period.
/// </para>
/// </remarks>
public interface IContentPublishOperationService : IService
{
    #region Publishing

    /// <summary>
    /// Publishes a document.
    /// </summary>
    /// <param name="content">The document to publish.</param>
    /// <param name="cultures">The cultures to publish. Use "*" for all cultures or specific culture codes.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The publish result indicating success or failure.</returns>
    /// <remarks>
    /// <para>When a culture is being published, it includes all varying values along with all invariant values.</para>
    /// <para>Wildcards (*) can be used as culture identifier to publish all cultures.</para>
    /// <para>An empty array (or a wildcard) can be passed for culture invariant content.</para>
    /// <para>Fires ContentPublishingNotification (cancellable) before publish and ContentPublishedNotification after.</para>
    /// </remarks>
    PublishResult Publish(IContent content, string[] cultures, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Publishes a document branch.
    /// </summary>
    /// <param name="content">The root document of the branch.</param>
    /// <param name="publishBranchFilter">Options for force publishing unpublished or re-publishing unchanged content.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <returns>Results for each document in the branch.</returns>
    /// <remarks>The root of the branch is always published, regardless of <paramref name="publishBranchFilter"/>.</remarks>
    IEnumerable<PublishResult> PublishBranch(IContent content, PublishBranchFilter publishBranchFilter, string[] cultures, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Unpublishing

    /// <summary>
    /// Unpublishes a document.
    /// </summary>
    /// <param name="content">The document to unpublish.</param>
    /// <param name="culture">The culture to unpublish, or "*" for all cultures.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The publish result indicating success or failure.</returns>
    /// <remarks>
    /// <para>By default, unpublishes the document as a whole, but it is possible to specify a culture.</para>
    /// <para>If the content type is variant, culture can be either '*' or an actual culture.</para>
    /// <para>If the content type is invariant, culture can be either '*' or null or empty.</para>
    /// <para>Fires ContentUnpublishingNotification (cancellable) before and ContentUnpublishedNotification after.</para>
    /// </remarks>
    PublishResult Unpublish(IContent content, string? culture = "*", int userId = Constants.Security.SuperUserId);

    #endregion

    #region Document Changes (Advanced API)

    /// <summary>
    /// Commits pending document publishing/unpublishing changes.
    /// </summary>
    /// <param name="content">The document with pending publish state changes.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <param name="notificationState">Optional state dictionary for notification propagation across orchestrated operations.</param>
    /// <returns>The publish result indicating success or failure.</returns>
    /// <remarks>
    /// <para>
    /// <strong>This is an advanced API.</strong> Most consumers should use <see cref="Publish"/> or
    /// <see cref="Unpublish"/> instead.
    /// </para>
    /// <para>
    /// Call this after setting <see cref="IContent.PublishedState"/> to
    /// <see cref="PublishedState.Publishing"/> or <see cref="PublishedState.Unpublishing"/>.
    /// </para>
    /// <para>
    /// This method is exposed for orchestration scenarios where publish/unpublish must be coordinated
    /// with other operations (e.g., MoveToRecycleBin unpublishes before moving).
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    PublishResult CommitDocumentChanges(IContent content, int userId = Constants.Security.SuperUserId, IDictionary<string, object?>? notificationState = null);

    #endregion

    #region Scheduled Publishing

    /// <summary>
    /// Publishes and unpublishes scheduled documents.
    /// </summary>
    /// <param name="date">The date to check schedules against.</param>
    /// <returns>Results for each processed document.</returns>
    IEnumerable<PublishResult> PerformScheduledPublish(DateTime date);

    /// <summary>
    /// Gets documents having an expiration date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <param name="date">The date to check against.</param>
    /// <returns>Documents scheduled for expiration.</returns>
    IEnumerable<IContent> GetContentForExpiration(DateTime date);

    /// <summary>
    /// Gets documents having a release date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <param name="date">The date to check against.</param>
    /// <returns>Documents scheduled for release.</returns>
    IEnumerable<IContent> GetContentForRelease(DateTime date);

    #endregion

    #region Schedule Management

    /// <summary>
    /// Gets publish/unpublish schedule for a content node by integer id.
    /// </summary>
    /// <param name="contentId">Id of the content to load schedule for.</param>
    /// <returns>The content schedule collection.</returns>
    ContentScheduleCollection GetContentScheduleByContentId(int contentId);

    /// <summary>
    /// Gets publish/unpublish schedule for a content node by GUID.
    /// </summary>
    /// <param name="contentId">Key of the content to load schedule for.</param>
    /// <returns>The content schedule collection.</returns>
    ContentScheduleCollection GetContentScheduleByContentId(Guid contentId);

    /// <summary>
    /// Persists publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="contentSchedule">The schedule to persist.</param>
    void PersistContentSchedule(IContent content, ContentScheduleCollection contentSchedule);

    /// <summary>
    /// Gets a dictionary of content Ids and their matching content schedules.
    /// </summary>
    /// <param name="keys">The content keys.</param>
    /// <returns>A dictionary with nodeId and an IEnumerable of matching ContentSchedules.</returns>
    IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(Guid[] keys);

    #endregion

    #region Path Checks

    /// <summary>
    /// Gets a value indicating whether a document is path-publishable.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns>True if all ancestors are published.</returns>
    /// <remarks>A document is path-publishable when all its ancestors are published.</remarks>
    bool IsPathPublishable(IContent content);

    /// <summary>
    /// Gets a value indicating whether a document is path-published.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns>True if all ancestors and the document itself are published.</returns>
    /// <remarks>A document is path-published when all its ancestors, and the document itself, are published.</remarks>
    bool IsPathPublished(IContent? content);

    #endregion

    #region Workflow

    /// <summary>
    /// Saves a document and raises the "sent to publication" events.
    /// </summary>
    /// <param name="content">The content to send to publication.</param>
    /// <param name="userId">The identifier of the user issuing the send to publication.</param>
    /// <returns>True if sending publication was successful otherwise false.</returns>
    /// <remarks>
    /// Fires ContentSendingToPublishNotification (cancellable) before and ContentSentToPublishNotification after.
    /// </remarks>
    bool SendToPublication(IContent? content, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Published Content Queries

    /// <summary>
    /// Gets published children of a parent content item.
    /// </summary>
    /// <param name="id">Id of the parent to retrieve children from.</param>
    /// <returns>Published child content items, ordered by sort order.</returns>
    IEnumerable<IContent> GetPublishedChildren(int id);

    #endregion
}
