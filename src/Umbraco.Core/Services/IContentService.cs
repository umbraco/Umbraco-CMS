using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the ContentService, which is an easy access to operations involving <see cref="IContent" />
/// </summary>
public interface IContentService : IPublishableContentService<IContent>, IAsyncPublishableContentService<IContent>
{
    // IPublishableContentService<IContent> and IAsyncPublishableContentService<IContent> both derive from a
    // base interface declaring this member with an identical signature (IContentServiceBase and
    // IAsyncContentServiceBase respectively), so without redeclaring it here every call site that holds an
    // IContentService reference and invokes it directly is ambiguous (CS0121). A single implementation still
    // satisfies both interface members implicitly.
    new ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options);

    #region Blueprints

    /// <summary>
    ///     Gets a blueprint.
    /// </summary>
    /// <param name="key">The Guid key of the blueprint.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The blueprint, or null if not found.</returns>
    Task<IContent?> GetBlueprintByIdAsync(Guid key, CancellationToken cancellationToken);

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
    /// <param name="createdFromContent">The content from which the blueprint was created.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    void SaveBlueprint(IContent content, IContent? createdFromContent, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Moves a blueprint.
    /// </summary>
    /// <param name="content">The blueprint to move.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    // TODO (V19): Remove the default implementation from this
    void MoveBlueprint(IContent content, int userId = Constants.Security.SuperUserId) => throw new NotImplementedException();

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
    ///     Gets a paged list of documents at a given level.
    /// </summary>
    /// <remarks>
    ///     Contrary to most methods, this method filters out trashed content items.
    /// </remarks>
    /// <param name="level">The level.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification. Must not be <c>null</c> — items at the same level may belong to unrelated parents, so there is no single ordering that applies by default.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching, non-trashed documents.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ordering" /> is <c>null</c>.</exception>
    Task<PagedModel<IContent>> GetByLevelAsync(int level, int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the parent of a document.
    /// </summary>
    /// <param name="key">The Guid key of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parent document, or null if not found.</returns>
    Task<IContent?> GetParentAsync(Guid key, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the parent of a document.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parent document, or null if not found.</returns>
    Task<IContent?> GetParentAsync(IContent? content, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets ancestor documents of a document.
    /// </summary>
    /// <param name="key">The Guid key of the document.</param>
    /// <returns>The ancestor documents.</returns>
    [Obsolete("Use GetAncestorsAsync(Guid, int, int, CancellationToken) instead. Scheduled for removal in Umbraco 21.")]
    IEnumerable<IContent> GetAncestors(Guid key);

    /// <summary>
    ///     Gets ancestor documents of a document.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <returns>The ancestor documents.</returns>
    [Obsolete("Use GetAncestorsAsync(IContent, int, int, CancellationToken) instead. Scheduled for removal in Umbraco 21.")]
    IEnumerable<IContent> GetAncestors(IContent content);

    /// <summary>
    ///     Gets a paged list of ancestor documents of a document, ordered root-first.
    /// </summary>
    /// <param name="key">The Guid key of the document to retrieve ancestors for.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the document's ancestors, root-first.</returns>
    Task<PagedModel<IContent>> GetAncestorsAsync(Guid key, int skip, int take, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of ancestor documents of a document, ordered root-first.
    /// </summary>
    /// <param name="content">The document to retrieve ancestors for.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the document's ancestors, root-first.</returns>
    Task<PagedModel<IContent>> GetAncestorsAsync(IContent content, int skip, int take, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets root-level documents.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The root-level documents.</returns>
    Task<IEnumerable<IContent>> GetRootContentAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets documents having an expiration date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <param name="date">The date to check expiration against.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    /// <remarks>
    ///     The content returned from this method may be culture variant, in which case you can use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus(IContent, ContentScheduleCollection, string?)" /> to get the status for a specific culture.
    /// </remarks>
    Task<IEnumerable<IContent>> GetContentForExpirationAsync(DateTime date, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets documents having a release date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <param name="date">The date to check release against.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    /// <remarks>
    ///     The content returned from this method may be culture variant, in which case you can use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus(IContent, ContentScheduleCollection, string?)" /> to get the status for a specific culture.
    /// </remarks>
    Task<IEnumerable<IContent>> GetContentForReleaseAsync(DateTime date, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of documents in the recycle bin.
    /// </summary>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching recycle bin items.</returns>
    Task<PagedModel<IContent>> GetPagedContentInRecycleBinAsync(int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of direct children of a document node.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent node, or <c>null</c> for the root of the content tree.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="propertyAliases">
    ///     Optional array of property aliases to load. If <c>null</c>, all properties are loaded.
    ///     If empty, no custom properties are loaded (only system properties).
    /// </param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by sort order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching children.</returns>
    Task<PagedModel<IContent>> GetChildrenAsync(Guid? parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of direct children of a document node, without loading template information.
    /// </summary>
    /// <remarks>
    ///     Use this overload when template IDs are not required (e.g. collection/list views) to avoid the
    ///     template-existence validation round-trip against the template repository.
    /// </remarks>
    /// <param name="parentKey">The Guid key of the parent node, or <c>null</c> for the root of the content tree.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="propertyAliases">
    ///     Optional array of property aliases to load. If <c>null</c>, all properties are loaded.
    ///     If empty, no custom properties are loaded (only system properties).
    /// </param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by sort order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching children with <c>null</c> template IDs.</returns>
    Task<PagedModel<IContent>> GetChildrenWithoutTemplatesAsync(Guid? parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of all descendants of a document node.
    /// </summary>
    /// <param name="ancestorKey">The Guid key of the ancestor node.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by path (ancestors before their own descendants).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="includeTrashed">Whether to include descendants that are currently in the recycle bin. Default is <c>true</c>.</param>
    /// <returns>A paged result containing the matching descendants.</returns>
    Task<PagedModel<IContent>> GetDescendantsAsync(Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken, bool includeTrashed = true);

    /// <summary>
    ///     Gets a paged list of all descendants of a document node, without loading template information.
    /// </summary>
    /// <remarks>
    ///     Use this overload when template IDs are not required (e.g. collection/list views) to avoid the
    ///     template-existence validation round-trip against the template repository.
    /// </remarks>
    /// <param name="ancestorKey">The Guid key of the ancestor node.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by path (ancestors before their own descendants).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="includeTrashed">Whether to include descendants that are currently in the recycle bin. Default is <c>true</c>.</param>
    /// <returns>A paged result containing the matching descendants with <c>null</c> template IDs.</returns>
    Task<PagedModel<IContent>> GetDescendantsWithoutTemplatesAsync(Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken, bool includeTrashed = true);

    /// <summary>
    ///     Gets a paged list of documents of a given content type.
    /// </summary>
    /// <param name="contentTypeKey">The Guid key of the content type.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by sort order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching documents.</returns>
    Task<PagedModel<IContent>> GetPagedOfTypeAsync(Guid contentTypeKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of documents whose content type is one of the specified content types.
    /// </summary>
    /// <param name="contentTypeKeys">The Guid keys of the content types.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by sort order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching documents.</returns>
    Task<PagedModel<IContent>> GetPagedOfTypesAsync(Guid[] contentTypeKeys, int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Counts documents of a given document type.
    /// </summary>
    /// <param name="contentTypeAlias">The document type alias, or null for all types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The document count.</returns>
    Task<int> CountAsync(string? contentTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Counts child documents of a given parent, of a given document type.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent.</param>
    /// <param name="contentTypeAlias">The document type alias, or null for all types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The child document count.</returns>
    Task<int> CountChildrenAsync(Guid parentKey, string? contentTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Counts descendant documents of a given parent, of a given document type.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent.</param>
    /// <param name="contentTypeAlias">The document type alias, or null for all types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The descendant document count.</returns>
    Task<int> CountDescendantsAsync(Guid parentKey, string? contentTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a value indicating whether a document has children.
    /// </summary>
    /// <param name="key">The Guid key of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the document has children; otherwise, <c>false</c>.</returns>
    Task<bool> HasChildrenAsync(Guid key, CancellationToken cancellationToken);

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
    ///     Deletes versions of a document prior to a given date.
    /// </summary>
    /// <param name="key">The Guid key of the document.</param>
    /// <param name="versionDate">The date before which versions should be deleted.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteVersionsAsync(Guid key, DateTime versionDate, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a version of a document.
    /// </summary>
    /// <param name="key">The Guid key of the document.</param>
    /// <param name="versionId">The version identifier to delete.</param>
    /// <param name="deletePriorVersions">Whether to also delete versions prior to the specified version.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteVersionAsync(Guid key, int versionId, bool deletePriorVersions, Guid userKey, CancellationToken cancellationToken);

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
    ///     Moves a document under a new parent, optionally leaving its descendants behind.
    /// </summary>
    /// <param name="content">The document to move.</param>
    /// <param name="parentId">The identifier of the new parent.</param>
    /// <param name="includeDescendants">
    ///     Whether to move the descendants of the document along with it. When restoring a document out of the recycle
    ///     bin this can be set to <c>false</c> to restore only the document itself, leaving its descendants in the
    ///     recycle bin as top-level bin items.
    /// </param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The operation result.</returns>
#pragma warning disable CS0618 // Type or member is obsolete - the int-userId overloads still default to SuperUserId; there is no non-obsolete int equivalent until it is removed in v18
    OperationResult Move(IContent content, int parentId, bool includeDescendants, int userId = Constants.Security.SuperUserId)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        // Only the whole-tree move can be satisfied by delegating to the existing method; there is no way to honour
        // includeDescendants: false without the concrete implementation, so fail fast rather than silently move
        // the descendants after all.
        if (includeDescendants is false)
        {
            throw new NotImplementedException("This IContentService implementation does not support moving without descendants. Override the Move overload that takes an includeDescendants parameter to support it.");
        }

        return Move(content, parentId, userId);
    }

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
    ///     Gets a value indicating whether there is any content in the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if there is content in the recycle bin; otherwise, <c>false</c>.</returns>
    Task<bool> RecycleBinSmellsAsync(CancellationToken cancellationToken);

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

    /// <summary>
    ///     Sorts the children of a parent by persisting the supplied (already ordered) child keys as the
    ///     new sort order, in a single batched operation.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the content tree.</param>
    /// <param name="orderedChildKeys">The Guid keys of the children, in the desired order.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    ///     Unlike <see cref="Sort(IEnumerable{int}?, int)" />, this does not load the children or fire per-item
    ///     save/sort notifications; it persists the order directly and refreshes the affected cache branch.
    /// </remarks>
    Task<OperationResult> SortChildrenAsync(Guid? parentKey, IReadOnlyList<Guid> orderedChildKeys, Guid userKey, CancellationToken cancellationToken);

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
    ///     Gets a value indicating whether a document is path-publishable.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the document is path-publishable; otherwise, <c>false</c>.</returns>
    /// <remarks>A document is path-publishable when all its ancestors are published.</remarks>
    Task<bool> IsPathPublishableAsync(IContent content, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a value indicating whether a document is path-published.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the document is path-published; otherwise, <c>false</c>.</returns>
    /// <remarks>A document is path-published when all its ancestors, and the document itself, are published.</remarks>
    Task<bool> IsPathPublishedAsync(IContent content, CancellationToken cancellationToken);

    /// <summary>
    ///     Saves a document and raises the "sent to publication" events.
    /// </summary>
    /// <param name="content">The document to send to publication.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns><c>true</c> if the document was sent to publication; otherwise, <c>false</c>.</returns>
    bool SendToPublication(IContent? content, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Permissions

    /// <summary>
    ///     Gets permissions assigned to a document.
    /// </summary>
    /// <param name="contentKey">The Guid key of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The permissions assigned to the document.</returns>
    Task<EntityPermissionCollection> GetPermissionsAsync(Guid contentKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Sets the permission of a document.
    /// </summary>
    /// <param name="permissionSet">The permission set to apply.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>Replaces all permissions with the new set of permissions.</remarks>
    [Obsolete("Use IUserGroup.GranularPermissions (persisted via IUserGroupService) to manage document permissions instead. Scheduled for removal in Umbraco 21.")]
    Task SetPermissionsAsync(EntityPermissionSet permissionSet, CancellationToken cancellationToken);

    /// <summary>
    ///     Assigns a permission to a document.
    /// </summary>
    /// <param name="entity">The document entity.</param>
    /// <param name="permission">The permission to assign.</param>
    /// <param name="groupKeys">The Guid keys of the groups to assign the permission to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>Adds the permission to existing permissions.</remarks>
    [Obsolete("Use IUserGroup.GranularPermissions (persisted via IUserGroupService) to manage document permissions instead. Scheduled for removal in Umbraco 21.")]
    Task SetPermissionAsync(IContent entity, string permission, IEnumerable<Guid> groupKeys, CancellationToken cancellationToken);

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
}
