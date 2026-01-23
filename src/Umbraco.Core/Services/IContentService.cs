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
    /// <param name="id">The identifier of the blueprint.</param>
    /// <returns>The blueprint, or null if not found.</returns>
    IContent? GetBlueprintById(int id);

    /// <summary>
    ///     Gets a blueprint.
    /// </summary>
    /// <param name="id">The unique identifier of the blueprint.</param>
    /// <returns>The blueprint, or null if not found.</returns>
    IContent? GetBlueprintById(Guid id);

    /// <summary>
    ///     Gets blueprints for a content type.
    /// </summary>
    /// <param name="documentTypeId">The document type identifiers.</param>
    /// <returns>The blueprints.</returns>
    IEnumerable<IContent> GetBlueprintsForContentTypes(params int[] documentTypeId);

    /// <summary>
    ///     Saves a blueprint.
    /// </summary>
    /// <param name="content">The blueprint to save.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    [Obsolete("Please use the method taking all parameters. Scheduled for removal in Umbraco 18.")]
    void SaveBlueprint(IContent content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Saves a blueprint.
    /// </summary>
    /// <param name="content">The blueprint to save.</param>
    /// <param name="createdFromContent">The content from which the blueprint was created.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    void SaveBlueprint(IContent content, IContent? createdFromContent, int userId = Constants.Security.SuperUserId)
#pragma warning disable CS0618 // Type or member is obsolete
        => SaveBlueprint(content, userId);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    ///     Deletes a blueprint.
    /// </summary>
    /// <param name="content">The blueprint to delete.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    void DeleteBlueprint(IContent content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a blueprint from a content item.
    /// </summary>
    /// <param name="blueprint">The content item to create a blueprint from.</param>
    /// <param name="name">The name for the new blueprint.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The created blueprint.</returns>
    // TODO: Remove the default implementation when CreateContentFromBlueprint is removed.
    IContent CreateBlueprintFromContent(IContent blueprint, string name, int userId = Constants.Security.SuperUserId)
        => throw new NotImplementedException();

    /// <summary>
    ///     (Deprecated) Creates a new content item from a blueprint.
    /// </summary>
    /// <param name="blueprint">The blueprint to create content from.</param>
    /// <param name="name">The name for the new content.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The created content.</returns>
    /// <remarks>If creating content from a blueprint, use <see cref="IContentBlueprintEditingService.GetScaffoldedAsync"/>
    /// instead. If creating a blueprint from content use <see cref="CreateBlueprintFromContent"/> instead.</remarks>
    [Obsolete("Use IContentBlueprintEditingService.GetScaffoldedAsync() instead. Scheduled for removal in V18.")]
    IContent CreateContentFromBlueprint(IContent blueprint, string name, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes blueprints for a content type.
    /// </summary>
    /// <param name="contentTypeId">The content type identifier.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    void DeleteBlueprintsOfType(int contentTypeId, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes blueprints for content types.
    /// </summary>
    /// <param name="contentTypeIds">The content type identifiers.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    void DeleteBlueprintsOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Get, Count Documents

    /// <summary>
    ///     Gets a document.
    /// </summary>
    /// <param name="id">The identifier of the document.</param>
    /// <returns>The document, or null if not found.</returns>
    IContent? GetById(int id);

    /// <summary>
    ///     Gets a document.
    /// </summary>
    /// <param name="key">The unique identifier of the document.</param>
    /// <returns>The document, or null if not found.</returns>
    // TODO (V18): This is already declared on the base type, so for the next major, when we can allow a binary breaking change, we should remove it from here.
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    IContent? GetById(Guid key);
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

    /// <summary>
    ///     Gets publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="contentId">The identifier of the content to load schedule for.</param>
    /// <returns>The <see cref="ContentScheduleCollection" />.</returns>
    ContentScheduleCollection GetContentScheduleByContentId(int contentId);

    /// <summary>
    ///     Persists publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="content">The content to persist the schedule for.</param>
    /// <param name="contentSchedule">The content schedule collection.</param>
    void PersistContentSchedule(IContent content, ContentScheduleCollection contentSchedule);

    /// <summary>
    ///     Gets documents.
    /// </summary>
    /// <param name="ids">The identifiers of the documents.</param>
    /// <returns>The documents.</returns>
    IEnumerable<IContent> GetByIds(IEnumerable<int> ids);

    /// <summary>
    ///     Gets documents.
    /// </summary>
    /// <param name="ids">The unique identifiers of the documents.</param>
    /// <returns>The documents.</returns>
    IEnumerable<IContent> GetByIds(IEnumerable<Guid> ids);

    /// <summary>
    ///     Gets documents at a given level.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <returns>The documents at the specified level.</returns>
    IEnumerable<IContent> GetByLevel(int level);

    /// <summary>
    ///     Gets the parent of a document.
    /// </summary>
    /// <param name="id">The identifier of the document.</param>
    /// <returns>The parent document, or null if not found.</returns>
    IContent? GetParent(int id);

    /// <summary>
    ///     Gets the parent of a document.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <returns>The parent document, or null if not found.</returns>
    IContent? GetParent(IContent content);

    /// <summary>
    ///     Gets ancestor documents of a document.
    /// </summary>
    /// <param name="id">The identifier of the document.</param>
    /// <returns>The ancestor documents.</returns>
    IEnumerable<IContent> GetAncestors(int id);

    /// <summary>
    ///     Gets ancestor documents of a document.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <returns>The ancestor documents.</returns>
    IEnumerable<IContent> GetAncestors(IContent content);

    /// <summary>
    ///     Gets all versions of a document.
    /// </summary>
    /// <param name="id">The identifier of the document.</param>
    /// <returns>The document versions.</returns>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    IEnumerable<IContent> GetVersions(int id);

    /// <summary>
    ///     Gets all versions of a document.
    /// </summary>
    /// <param name="id">The identifier of the document.</param>
    /// <param name="skip">The number of versions to skip.</param>
    /// <param name="take">The number of versions to take.</param>
    /// <returns>The document versions.</returns>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take);

    /// <summary>
    ///     Gets top versions of a document.
    /// </summary>
    /// <param name="id">The identifier of the document.</param>
    /// <param name="topRows">The number of top versions to get.</param>
    /// <returns>The version identifiers.</returns>
    /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
    IEnumerable<int> GetVersionIds(int id, int topRows);

    /// <summary>
    ///     Gets a version of a document.
    /// </summary>
    /// <param name="versionId">The version identifier.</param>
    /// <returns>The document version, or null if not found.</returns>
    IContent? GetVersion(int versionId);

    /// <summary>
    ///     Gets root-level documents.
    /// </summary>
    /// <returns>The root-level documents.</returns>
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
    ///     Gets paged documents of a content type.
    /// </summary>
    /// <param name="contentTypeId">The content type identifier.</param>
    /// <param name="pageIndex">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Total number of documents.</param>
    /// <param name="filter">Query filter.</param>
    /// <param name="ordering">Ordering infos.</param>
    /// <returns>The paged documents.</returns>
    IEnumerable<IContent> GetPagedOfType(int contentTypeId, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent> filter, Ordering? ordering = null);

    /// <summary>
    ///     Gets paged documents for specified content types.
    /// </summary>
    /// <param name="contentTypeIds">The content type identifiers.</param>
    /// <param name="pageIndex">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Total number of documents.</param>
    /// <param name="filter">Query filter.</param>
    /// <param name="ordering">Ordering infos.</param>
    /// <returns>The paged documents.</returns>
    IEnumerable<IContent> GetPagedOfTypes(int[] contentTypeIds, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter, Ordering? ordering = null);

    /// <summary>
    ///     Counts documents of a given document type.
    /// </summary>
    /// <param name="documentTypeAlias">The document type alias, or null for all types.</param>
    /// <returns>The document count.</returns>
    int Count(string? documentTypeAlias = null);

    /// <summary>
    ///     Counts published documents of a given document type.
    /// </summary>
    /// <param name="documentTypeAlias">The document type alias, or null for all types.</param>
    /// <returns>The published document count.</returns>
    int CountPublished(string? documentTypeAlias = null);

    /// <summary>
    ///     Counts child documents of a given parent, of a given document type.
    /// </summary>
    /// <param name="parentId">The parent identifier.</param>
    /// <param name="documentTypeAlias">The document type alias, or null for all types.</param>
    /// <returns>The child document count.</returns>
    int CountChildren(int parentId, string? documentTypeAlias = null);

    /// <summary>
    ///     Counts descendant documents of a given parent, of a given document type.
    /// </summary>
    /// <param name="parentId">The parent identifier.</param>
    /// <param name="documentTypeAlias">The document type alias, or null for all types.</param>
    /// <returns>The descendant document count.</returns>
    int CountDescendants(int parentId, string? documentTypeAlias = null);

    /// <summary>
    ///     Gets a value indicating whether a document has children.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <returns><c>true</c> if the document has children; otherwise, <c>false</c>.</returns>
    bool HasChildren(int id);

    /// <summary>
    ///     Gets a dictionary of content Ids and their matching content schedules.
    /// </summary>
    /// <param name="keys">The content keys.</param>
    /// <returns>A dictionary with a node Id and an IEnumerable of matching ContentSchedules.</returns>
    IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(Guid[] keys) => ImmutableDictionary<int, IEnumerable<ContentSchedule>>.Empty;


    #endregion

    #region Save, Delete Document

    /// <summary>
    ///     Saves a document.
    /// </summary>
    /// <param name="content">The document to save.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <param name="contentSchedule">The content schedule collection.</param>
    /// <returns>The operation result.</returns>
    OperationResult Save(IContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null);

    /// <summary>
    ///     Saves documents.
    /// </summary>
    /// <param name="contents">The documents to save.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The operation result.</returns>
    // TODO: why only 1 result not 1 per content?!
    new OperationResult Save(IEnumerable<IContent> contents, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a document.
    /// </summary>
    /// <param name="content">The document to delete.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    ///     <para>This method will also delete associated media files, child content and possibly associated domains.</para>
    ///     <para>This method entirely clears the content from the database.</para>
    /// </remarks>
    OperationResult Delete(IContent content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes all documents of a given document type.
    /// </summary>
    /// <param name="documentTypeId">The document type identifier.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <remarks>
    ///     <para>All non-deleted descendants of the deleted documents are moved to the recycle bin.</para>
    ///     <para>This operation is potentially dangerous and expensive.</para>
    /// </remarks>
    void DeleteOfType(int documentTypeId, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes all documents of given document types.
    /// </summary>
    /// <param name="contentTypeIds">The content type identifiers.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <remarks>
    ///     <para>All non-deleted descendants of the deleted documents are moved to the recycle bin.</para>
    ///     <para>This operation is potentially dangerous and expensive.</para>
    /// </remarks>
    void DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes versions of a document prior to a given date.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="date">The date before which versions should be deleted.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    void DeleteVersions(int id, DateTime date, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a version of a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="versionId">The version identifier to delete.</param>
    /// <param name="deletePriorVersions">Whether to also delete versions prior to the specified version.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Move, Copy, Sort Document

    /// <summary>
    ///     Moves a document under a new parent.
    /// </summary>
    /// <param name="content">The document to move.</param>
    /// <param name="parentId">The identifier of the new parent.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The operation result.</returns>
    OperationResult Move(IContent content, int parentId, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Copies a document.
    /// </summary>
    /// <param name="content">The document to copy.</param>
    /// <param name="parentId">The identifier of the new parent.</param>
    /// <param name="relateToOriginal">Whether to relate the copy to the original.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The copied document, or null if the copy failed.</returns>
    /// <remarks>
    ///     <para>Recursively copies all children.</para>
    /// </remarks>
    IContent? Copy(IContent content, int parentId, bool relateToOriginal, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Copies a document.
    /// </summary>
    /// <param name="content">The document to copy.</param>
    /// <param name="parentId">The identifier of the new parent.</param>
    /// <param name="relateToOriginal">Whether to relate the copy to the original.</param>
    /// <param name="recursive">Whether to recursively copy all children.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The copied document, or null if the copy failed.</returns>
    /// <remarks>
    ///     <para>Optionally recursively copies all children.</para>
    /// </remarks>
    IContent? Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Moves a document to the recycle bin.
    /// </summary>
    /// <param name="content">The document to move to the recycle bin.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The operation result.</returns>
    OperationResult MoveToRecycleBin(IContent content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Empties the Recycle Bin by deleting all <see cref="IContent" /> that resides in the bin.
    /// </summary>
    /// <param name="userId">Optional identifier of the user emptying the Recycle Bin.</param>
    /// <returns>The operation result.</returns>
    OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Returns true if there is any content in the recycle bin.
    /// </summary>
    /// <returns><c>true</c> if there is content in the recycle bin; otherwise, <c>false</c>.</returns>
    bool RecycleBinSmells();

    /// <summary>
    ///     Sorts documents.
    /// </summary>
    /// <param name="items">The documents to sort, in the desired order.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The operation result.</returns>
    OperationResult Sort(IEnumerable<IContent> items, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Sorts documents.
    /// </summary>
    /// <param name="ids">The document identifiers, in the desired order.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The operation result.</returns>
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
    /// <param name="content">The document.</param>
    /// <returns><c>true</c> if the document is path-publishable; otherwise, <c>false</c>.</returns>
    /// <remarks>A document is path-publishable when all its ancestors are published.</remarks>
    bool IsPathPublishable(IContent content);

    /// <summary>
    ///     Gets a value indicating whether a document is path-published.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <returns><c>true</c> if the document is path-published; otherwise, <c>false</c>.</returns>
    /// <remarks>A document is path-published when all its ancestors, and the document itself, are published.</remarks>
    bool IsPathPublished(IContent content);

    /// <summary>
    ///     Saves a document and raises the "sent to publication" events.
    /// </summary>
    /// <param name="content">The document to send to publication.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns><c>true</c> if the document was sent to publication; otherwise, <c>false</c>.</returns>
    bool SendToPublication(IContent? content, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Publishes and unpublishes scheduled documents.
    /// </summary>
    /// <param name="date">The date to use for determining scheduled actions.</param>
    /// <returns>The publish results.</returns>
    IEnumerable<PublishResult> PerformScheduledPublish(DateTime date);

    #endregion

    #region Permissions

    /// <summary>
    ///     Gets permissions assigned to a document.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <returns>The permissions assigned to the document.</returns>
    EntityPermissionCollection GetPermissions(IContent content);

    /// <summary>
    ///     Sets the permission of a document.
    /// </summary>
    /// <param name="permissionSet">The permission set to apply.</param>
    /// <remarks>Replaces all permissions with the new set of permissions.</remarks>
    void SetPermissions(EntityPermissionSet permissionSet);

    /// <summary>
    ///     Assigns a permission to a document.
    /// </summary>
    /// <param name="entity">The document entity.</param>
    /// <param name="permission">The permission to assign.</param>
    /// <param name="groupIds">The group identifiers to assign the permission to.</param>
    /// <remarks>Adds the permission to existing permissions.</remarks>
    void SetPermission(IContent entity, string permission, IEnumerable<int> groupIds);

    #endregion

    #region Create

    /// <summary>
    ///     Creates a document.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="parentId">The unique identifier of the parent.</param>
    /// <param name="documentTypeAlias">The document type alias.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The created document.</returns>
    IContent Create(string name, Guid parentId, string documentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a document.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="parentId">The identifier of the parent.</param>
    /// <param name="documentTypeAlias">The document type alias.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The created document.</returns>
    IContent Create(string name, int parentId, string documentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a document.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="parentId">The identifier of the parent.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The created document.</returns>
    IContent Create(string name, int parentId, IContentType contentType, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a document.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="parent">The parent document.</param>
    /// <param name="documentTypeAlias">The document type alias.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The created document.</returns>
    IContent Create(string name, IContent? parent, string documentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates and saves a document.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="parentId">The identifier of the parent.</param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The created and saved document.</returns>
    IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates and saves a document.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="parent">The parent document.</param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The created and saved document.</returns>
    IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    #endregion

    /// <summary>
    ///     Empties the Recycle Bin asynchronously by deleting all <see cref="IContent" /> that resides in the bin.
    /// </summary>
    /// <param name="userId">The unique identifier of the user emptying the Recycle Bin.</param>
    /// <returns>A task representing the asynchronous operation with the operation result.</returns>
    Task<OperationResult> EmptyRecycleBinAsync(Guid userId);

    /// <summary>
    ///     Gets publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="contentId">The unique identifier of the content to load schedule for.</param>
    /// <returns>The <see cref="ContentScheduleCollection" />.</returns>
    ContentScheduleCollection GetContentScheduleByContentId(Guid contentId) => StaticServiceProvider.Instance
        .GetRequiredService<ContentService>().GetContentScheduleByContentId(contentId);
}
