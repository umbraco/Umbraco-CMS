using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the ContentService, which is an easy access to operations involving <see cref="IContent" />
/// </summary>
public interface IContentService : IContentServiceBase<IContent>
{
    #region Rollback

    /// <summary>
    ///     Rolls back the content to a specific version.
    /// </summary>
    /// <param name="id">The id of the content node.</param>
    /// <param name="versionId">The version id to roll back to.</param>
    /// <param name="culture">An optional culture to roll back.</param>
    /// <param name="userId">The identifier of the user who is performing the roll back.</param>
    /// <remarks>
    ///     <para>When no culture is specified, all cultures are rolled back.</para>
    /// </remarks>
    OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId);

    #endregion

    #region Blueprints

    /// <summary>
    ///     Gets a blueprint.
    /// </summary>
    IContent? GetBlueprintById(int id);

    /// <summary>
    ///     Gets a blueprint.
    /// </summary>
    IContent? GetBlueprintById(Guid id);

    /// <summary>
    ///     Gets blueprints for a content type.
    /// </summary>
    IEnumerable<IContent> GetBlueprintsForContentTypes(params int[] documentTypeId);

    /// <summary>
    ///     Saves a blueprint.
    /// </summary>
    [Obsolete("Please use the method taking all parameters. Scheduled for removal in Umbraco 18.")]
    void SaveBlueprint(IContent content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Saves a blueprint.
    /// </summary>
    void SaveBlueprint(IContent content, IContent? createdFromContent, int userId = Constants.Security.SuperUserId)
#pragma warning disable CS0618 // Type or member is obsolete
        => SaveBlueprint(content, userId);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    ///     Deletes a blueprint.
    /// </summary>
    void DeleteBlueprint(IContent content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a blueprint from a content item.
    /// </summary>
    // TODO: Remove the default implementation when CreateContentFromBlueprint is removed.
    IContent CreateBlueprintFromContent(IContent blueprint, string name, int userId = Constants.Security.SuperUserId)
        => throw new NotImplementedException();

    /// <summary>
    ///     (Deprecated) Creates a new content item from a blueprint.
    /// </summary>
    /// <remarks>If creating content from a blueprint, use <see cref="IContentBlueprintEditingService.GetScaffoldedAsync"/>
    /// instead. If creating a blueprint from content use <see cref="CreateBlueprintFromContent"/> instead.</remarks>
    [Obsolete("Use IContentBlueprintEditingService.GetScaffoldedAsync() instead. Scheduled for removal in V18.")]
    IContent CreateContentFromBlueprint(IContent blueprint, string name, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes blueprints for a content type.
    /// </summary>
    void DeleteBlueprintsOfType(int contentTypeId, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes blueprints for content types.
    /// </summary>
    void DeleteBlueprintsOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Get, Count Documents

    /// <summary>
    ///     Gets a document.
    /// </summary>
    IContent? GetById(int id);

    /// <summary>
    ///     Gets a document.
    /// </summary>
    new IContent? GetById(Guid key);

    /// <summary>
    ///     Gets publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="contentId">Id of the Content to load schedule for</param>
    /// <returns>
    ///     <see cref="ContentScheduleCollection" />
    /// </returns>
    ContentScheduleCollection GetContentScheduleByContentId(int contentId);

    /// <summary>
    ///     Persists publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentSchedule"></param>
    void PersistContentSchedule(IContent content, ContentScheduleCollection contentSchedule);

    /// <summary>
    ///     Gets documents.
    /// </summary>
    IEnumerable<IContent> GetByIds(IEnumerable<int> ids);

    /// <summary>
    ///     Gets documents.
    /// </summary>
    IEnumerable<IContent> GetByIds(IEnumerable<Guid> ids);

    /// <summary>
    ///     Gets documents at a given level.
    /// </summary>
    IEnumerable<IContent> GetByLevel(int level);

    /// <summary>
    ///     Gets the parent of a document.
    /// </summary>
    IContent? GetParent(int id);

    /// <summary>
    ///     Gets the parent of a document.
    /// </summary>
    IContent? GetParent(IContent content);

    /// <summary>
    ///     Gets ancestor documents of a document.
    /// </summary>
    IEnumerable<IContent> GetAncestors(int id);

    /// <summary>
    ///     Gets ancestor documents of a document.
    /// </summary>
    IEnumerable<IContent> GetAncestors(IContent content);

    /// <summary>
    ///     Gets all versions of a document.
    /// </summary>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    IEnumerable<IContent> GetVersions(int id);

    /// <summary>
    ///     Gets all versions of a document.
    /// </summary>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take);

    /// <summary>
    ///     Gets top versions of a document.
    /// </summary>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    IEnumerable<int> GetVersionIds(int id, int topRows);

    /// <summary>
    ///     Gets a version of a document.
    /// </summary>
    IContent? GetVersion(int versionId);

    /// <summary>
    ///     Gets root-level documents.
    /// </summary>
    IEnumerable<IContent> GetRootContent();

    /// <summary>
    ///     Gets documents having an expiration date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    /// <remarks>
    ///     The content returned from this method may be culture variant, in which case you can use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus(IContent, ContentScheduleCollection, string?)" /> to get the status for a specific culture.
    /// </remarks>
    IEnumerable<IContent> GetContentForExpiration(DateTime date);

    /// <summary>
    ///     Gets documents having a release date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    /// <remarks>
    ///     The content returned from this method may be culture variant, in which case you can use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus(IContent, ContentScheduleCollection, string?)" /> to get the status for a specific culture.
    /// </remarks>
    IEnumerable<IContent> GetContentForRelease(DateTime date);

    /// <summary>
    ///     Gets documents in the recycle bin.
    /// </summary>
    IEnumerable<IContent> GetPagedContentInRecycleBin(long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter = null, Ordering? ordering = null);

    /// <summary>
    ///     Gets child documents of a parent.
    /// </summary>
    /// <param name="id">The parent identifier.</param>
    /// <param name="pageIndex">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Total number of documents.</param>
    /// <param name="filter">Query filter.</param>
    /// <param name="ordering">Ordering infos.</param>
    IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter = null, Ordering? ordering = null);

    /// <summary>
    ///     Gets descendant documents of a given parent.
    /// </summary>
    /// <param name="id">The parent identifier.</param>
    /// <param name="pageIndex">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Total number of documents.</param>
    /// <param name="filter">Query filter.</param>
    /// <param name="ordering">Ordering infos.</param>
    IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter = null, Ordering? ordering = null);

    /// <summary>
    ///     Gets paged documents of a content
    /// </summary>
    /// <param name="contentTypeId">The page number.</param>
    /// <param name="pageIndex">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Total number of documents.</param>
    /// <param name="filter">Search text filter.</param>
    /// <param name="ordering">Ordering infos.</param>
    IEnumerable<IContent> GetPagedOfType(int contentTypeId, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent> filter, Ordering? ordering = null);

    /// <summary>
    ///     Gets paged documents for specified content types
    /// </summary>
    /// <param name="contentTypeIds">The page number.</param>
    /// <param name="pageIndex">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Total number of documents.</param>
    /// <param name="filter">Search text filter.</param>
    /// <param name="ordering">Ordering infos.</param>
    IEnumerable<IContent> GetPagedOfTypes(int[] contentTypeIds, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter, Ordering? ordering = null);

    /// <summary>
    ///     Counts documents of a given document type.
    /// </summary>
    int Count(string? documentTypeAlias = null);

    /// <summary>
    ///     Counts published documents of a given document type.
    /// </summary>
    int CountPublished(string? documentTypeAlias = null);

    /// <summary>
    ///     Counts child documents of a given parent, of a given document type.
    /// </summary>
    int CountChildren(int parentId, string? documentTypeAlias = null);

    /// <summary>
    ///     Counts descendant documents of a given parent, of a given document type.
    /// </summary>
    int CountDescendants(int parentId, string? documentTypeAlias = null);

    /// <summary>
    ///     Gets a value indicating whether a document has children.
    /// </summary>
    bool HasChildren(int id);

    /// <summary>
    ///     Gets a dictionary of content Ids and their matching content schedules.
    /// </summary>
    /// <param name="keys">The content keys.</param>
    /// <returns>
    ///     A dictionary with a nodeId and an IEnumerable of matching ContentSchedules.
    /// </returns>
    IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(Guid[] keys) => ImmutableDictionary<int, IEnumerable<ContentSchedule>>.Empty;


    #endregion

    #region Save, Delete Document

    /// <summary>
    ///     Saves a document.
    /// </summary>
    OperationResult Save(IContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null);

    /// <summary>
    ///     Saves documents.
    /// </summary>
    // TODO: why only 1 result not 1 per content?!
    OperationResult Save(IEnumerable<IContent> contents, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a document.
    /// </summary>
    /// <remarks>
    ///     <para>This method will also delete associated media files, child content and possibly associated domains.</para>
    ///     <para>This method entirely clears the content from the database.</para>
    /// </remarks>
    OperationResult Delete(IContent content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes all documents of a given document type.
    /// </summary>
    /// <remarks>
    ///     <para>All non-deleted descendants of the deleted documents are moved to the recycle bin.</para>
    ///     <para>This operation is potentially dangerous and expensive.</para>
    /// </remarks>
    void DeleteOfType(int documentTypeId, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes all documents of given document types.
    /// </summary>
    /// <remarks>
    ///     <para>All non-deleted descendants of the deleted documents are moved to the recycle bin.</para>
    ///     <para>This operation is potentially dangerous and expensive.</para>
    /// </remarks>
    void DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes versions of a document prior to a given date.
    /// </summary>
    void DeleteVersions(int id, DateTime date, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a version of a document.
    /// </summary>
    void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Move, Copy, Sort Document

    /// <summary>
    ///     Moves a document under a new parent.
    /// </summary>
    OperationResult Move(IContent content, int parentId, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Copies a document.
    /// </summary>
    /// <remarks>
    ///     <para>Recursively copies all children.</para>
    /// </remarks>
    IContent? Copy(IContent content, int parentId, bool relateToOriginal, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Copies a document.
    /// </summary>
    /// <remarks>
    ///     <para>Optionally recursively copies all children.</para>
    /// </remarks>
    IContent? Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Moves a document to the recycle bin.
    /// </summary>
    OperationResult MoveToRecycleBin(IContent content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Empties the Recycle Bin by deleting all <see cref="IContent" /> that resides in the bin
    /// </summary>
    /// <param name="userId">Optional Id of the User emptying the Recycle Bin</param>
    OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Returns true if there is any content in the recycle bin
    /// </summary>
    bool RecycleBinSmells();

    /// <summary>
    ///     Sorts documents.
    /// </summary>
    OperationResult Sort(IEnumerable<IContent> items, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Sorts documents.
    /// </summary>
    OperationResult Sort(IEnumerable<int>? ids, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Publish Document

    /// <summary>
    ///     Publishes a document.
    /// </summary>
    /// <remarks>
    ///     <para>When a culture is being published, it includes all varying values along with all invariant values.</para>
    ///     <para>Wildcards (*) can be used as culture identifier to publish all cultures.</para>
    ///     <para>An empty array (or a wildcard) can be passed for culture invariant content.</para>
    /// </remarks>
    /// <param name="content">The document to publish.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    PublishResult Publish(IContent content, string[] cultures, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Publishes a document branch.
    /// </summary>
    /// <param name="content">The root document.</param>
    /// <param name="force">A value indicating whether to force-publish documents that are not already published.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <remarks>
    ///     <para>
    ///         The <paramref name="force" /> parameter determines which documents are published. When <c>false</c>,
    ///         only those documents that are already published, are republished. When <c>true</c>, all documents are
    ///         published. The root of the branch is always published, regardless of <paramref name="force" />.
    ///     </para>
    /// </remarks>
    [Obsolete("This method is not longer used as the 'force' parameter has been extended into options for publishing unpublished and re-publishing changed content. Please use the overload containing the parameter for those options instead. Will be removed in V17.")]
    IEnumerable<PublishResult> PublishBranch(IContent content, bool force, string[] cultures, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Publishes a document branch.
    /// </summary>
    /// <param name="content">The root document.</param>
    /// <param name="publishBranchFilter">A value indicating options for force publishing unpublished or re-publishing unchanged content.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <remarks>
    ///     <para>
    ///         The root of the branch is always published, regardless of <paramref name="publishBranchFilter" />.
    ///     </para>
    /// </remarks>
    IEnumerable<PublishResult> PublishBranch(IContent content, PublishBranchFilter publishBranchFilter, string[] cultures, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Unpublishes a document.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, unpublishes the document as a whole, but it is possible to specify a culture to be
    ///         unpublished. Depending on whether that culture is mandatory, and other cultures remain published,
    ///         the document as a whole may or may not remain published.
    ///     </para>
    ///     <para>
    ///         If the content type is variant, then culture can be either '*' or an actual culture, but neither null nor
    ///         empty. If the content type is invariant, then culture can be either '*' or null or empty.
    ///     </para>
    /// </remarks>
    PublishResult Unpublish(IContent content, string? culture = "*", int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets a value indicating whether a document is path-publishable.
    /// </summary>
    /// <remarks>A document is path-publishable when all its ancestors are published.</remarks>
    bool IsPathPublishable(IContent content);

    /// <summary>
    ///     Gets a value indicating whether a document is path-published.
    /// </summary>
    /// <remarks>A document is path-published when all its ancestors, and the document itself, are published.</remarks>
    bool IsPathPublished(IContent content);

    /// <summary>
    ///     Saves a document and raises the "sent to publication" events.
    /// </summary>
    bool SendToPublication(IContent? content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Publishes and unpublishes scheduled documents.
    /// </summary>
    IEnumerable<PublishResult> PerformScheduledPublish(DateTime date);

    #endregion

    #region Permissions

    /// <summary>
    ///     Gets permissions assigned to a document.
    /// </summary>
    EntityPermissionCollection GetPermissions(IContent content);

    /// <summary>
    ///     Sets the permission of a document.
    /// </summary>
    /// <remarks>Replaces all permissions with the new set of permissions.</remarks>
    void SetPermissions(EntityPermissionSet permissionSet);

    /// <summary>
    ///     Assigns a permission to a document.
    /// </summary>
    /// <remarks>Adds the permission to existing permissions.</remarks>
    void SetPermission(IContent entity, string permission, IEnumerable<int> groupIds);

    #endregion

    #region Create

    /// <summary>
    ///     Creates a document.
    /// </summary>
    IContent Create(string name, Guid parentId, string documentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a document.
    /// </summary>
    IContent Create(string name, int parentId, string documentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a document
    /// </summary>
    IContent Create(string name, int parentId, IContentType contentType, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a document.
    /// </summary>
    IContent Create(string name, IContent? parent, string documentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates and saves a document.
    /// </summary>
    IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates and saves a document.
    /// </summary>
    IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    #endregion

    Task<OperationResult> EmptyRecycleBinAsync(Guid userId);

ContentScheduleCollection GetContentScheduleByContentId(Guid contentId) => StaticServiceProvider.Instance
    .GetRequiredService<ContentService>().GetContentScheduleByContentId(contentId);
}
