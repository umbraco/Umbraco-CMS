using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the ContentService, which is an easy access to operations involving <see cref="IContent"/>
    /// </summary>
    public interface IContentService : IContentServiceBase
    {
        #region Blueprints

        /// <summary>
        /// Gets a blueprint.
        /// </summary>
        IContent GetBlueprintById(int id);

        /// <summary>
        /// Gets a blueprint.
        /// </summary>
        IContent GetBlueprintById(Guid id);

        /// <summary>
        /// Gets blueprints for a content type.
        /// </summary>
        IEnumerable<IContent> GetBlueprintsForContentTypes(params int[] documentTypeId);

        /// <summary>
        /// Saves a blueprint.
        /// </summary>
        void SaveBlueprint(IContent content, int userId = -1);

        /// <summary>
        /// Deletes a blueprint.
        /// </summary>
        void DeleteBlueprint(IContent content, int userId = -1);

        /// <summary>
        /// Creates a new content item from a blueprint.
        /// </summary>
        IContent CreateContentFromBlueprint(IContent blueprint, string name, int userId = -1);

        /// <summary>
        /// Deletes blueprints for a content type.
        /// </summary>
        void DeleteBlueprintsOfType(int contentTypeId, int userId = -1);

        /// <summary>
        /// Deletes blueprints for content types.
        /// </summary>
        void DeleteBlueprintsOfTypes(IEnumerable<int> contentTypeIds, int userId = -1);

        #endregion

        #region Get, Count Documents

        /// <summary>
        /// Gets a document.
        /// </summary>
        IContent GetById(int id);

        /// <summary>
        /// Gets a document.
        /// </summary>
        IContent GetById(Guid key);

        /// <summary>
        /// Gets documents.
        /// </summary>
        IEnumerable<IContent> GetByIds(IEnumerable<int> ids);

        /// <summary>
        /// Gets documents.
        /// </summary>
        IEnumerable<IContent> GetByIds(IEnumerable<Guid> ids);

        /// <summary>
        /// Gets documents of a given document type.
        /// </summary>
        IEnumerable<IContent> GetByType(int documentTypeId);

        /// <summary>
        /// Gets documents at a given level.
        /// </summary>
        IEnumerable<IContent> GetByLevel(int level);

        /// <summary>
        /// Gets child documents of a given parent.
        /// </summary>
        IEnumerable<IContent> GetChildren(int parentId);

        /// <summary>
        /// Gets child documents of a document, (partially) matching a name.
        /// </summary>
        IEnumerable<IContent> GetChildren(int parentId, string name);

        /// <summary>
        /// Gets the parent of a document.
        /// </summary>
        IContent GetParent(int id);

        /// <summary>
        /// Gets the parent of a document.
        /// </summary>
        IContent GetParent(IContent content);

        /// <summary>
        /// Gets ancestor documents of a document.
        /// </summary>
        IEnumerable<IContent> GetAncestors(int id);

        /// <summary>
        /// Gets ancestor documents of a document.
        /// </summary>
        IEnumerable<IContent> GetAncestors(IContent content);

        /// <summary>
        /// Gets descendant documents of a document.
        /// </summary>
        IEnumerable<IContent> GetDescendants(int id);

        /// <summary>
        /// Gets descendant documents of a document.
        /// </summary>
        IEnumerable<IContent> GetDescendants(IContent content);

        /// <summary>
        /// Gets all versions of a document.
        /// </summary>
        /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
        IEnumerable<IContent> GetVersions(int id);

        /// <summary>
        /// Gets top versions of a document.
        /// </summary>
        /// <remarks>Versions are ordered with current first, then most recent first.</remarks>
        IEnumerable<int> GetVersionIds(int id, int topRows);

        /// <summary>
        /// Gets a version of a document.
        /// </summary>
        IContent GetVersion(int versionId);

        /// <summary>
        /// Gets root-level documents.
        /// </summary>
        IEnumerable<IContent> GetRootContent();

        /// <summary>
        /// Gets documents with an expiration date greater then today.
        /// </summary>
        IEnumerable<IContent> GetContentForExpiration();

        /// <summary>
        /// Gets documents with a release date greater then today.
        /// </summary>
        IEnumerable<IContent> GetContentForRelease();

        /// <summary>
        /// Gets documents in the recycle bin.
        /// </summary>
        IEnumerable<IContent> GetContentInRecycleBin();

        /// <summary>
        /// Gets child documents of a given parent.
        /// </summary>
        /// <param name="id">The parent identifier.</param>
        /// <param name="pageIndex">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="totalRecords">Total number of documents.</param>
        /// <param name="orderBy">A field to order by.</param>
        /// <param name="orderDirection">The ordering direction.</param>
        /// <param name="filter">Search text filter.</param>
        IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "SortOrder", Direction orderDirection = Direction.Ascending, string filter = "");

        /// <summary>
        /// Gets child documents of a given parent.
        /// </summary>
        /// <param name="id">The parent identifier.</param>
        /// <param name="pageIndex">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="totalRecords">Total number of documents.</param>
        /// <param name="orderBy">A field to order by.</param>
        /// <param name="orderDirection">The ordering direction.</param>
        /// <param name="orderBySystemField">A flag indicating whether the ordering field is a system field.</param>
        /// <param name="filter">Query filter.</param>
        IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IContent> filter);

        /// <summary>
        /// Gets descendant documents of a given parent.
        /// </summary>
        /// <param name="id">The parent identifier.</param>
        /// <param name="pageIndex">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="totalRecords">Total number of documents.</param>
        /// <param name="orderBy">A field to order by.</param>
        /// <param name="orderDirection">The ordering direction.</param>
        /// <param name="filter">Search text filter.</param>
        IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "path", Direction orderDirection = Direction.Ascending, string filter = "");

        /// <summary>
        /// Gets descendant documents of a given parent.
        /// </summary>
        /// <param name="id">The parent identifier.</param>
        /// <param name="pageIndex">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="totalRecords">Total number of documents.</param>
        /// <param name="orderBy">A field to order by.</param>
        /// <param name="orderDirection">The ordering direction.</param>
        /// <param name="orderBySystemField">A flag indicating whether the ordering field is a system field.</param>
        /// <param name="filter">Query filter.</param>
        IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IContent> filter);

        /// <summary>
        /// Counts documents of a given document type.
        /// </summary>
        int Count(string documentTypeAlias = null);

        /// <summary>
        /// Counts published documents of a given document type.
        /// </summary>
        int CountPublished(string documentTypeAlias = null);

        /// <summary>
        /// Counts child documents of a given parent, of a given document type.
        /// </summary>
        int CountChildren(int parentId, string documentTypeAlias = null);

        /// <summary>
        /// Counts descendant documents of a given parent, of a given document type.
        /// </summary>
        int CountDescendants(int parentId, string documentTypeAlias = null);

        /// <summary>
        /// Gets a value indicating whether a document has children.
        /// </summary>
        bool HasChildren(int id);

        #endregion

        #region Save, Delete Document

        /// <summary>
        /// Saves a document.
        /// </summary>
        OperationResult Save(IContent content, int userId = -1, bool raiseEvents = true);

        /// <summary>
        /// Saves documents.
        /// </summary>
        // fixme why only 1 result not 1 per content?!
        OperationResult Save(IEnumerable<IContent> contents, int userId = -1, bool raiseEvents = true);

        /// <summary>
        /// Deletes a document.
        /// </summary>
        /// <remarks>
        /// <para>This method will also delete associated media files, child content and possibly associated domains.</para>
        /// <para>This method entirely clears the content from the database.</para>
        /// </remarks>
        OperationResult Delete(IContent content, int userId = -1);

        /// <summary>
        /// Deletes all documents of a given document type.
        /// </summary>
        /// <remarks>
        /// <para>All non-deleted descendants of the deleted documents are moved to the recycle bin.</para>
        /// <para>This operation is potentially dangerous and expensive.</para>
        /// </remarks>
        void DeleteOfType(int documentTypeId, int userId = -1);

        /// <summary>
        /// Deletes all documents of given document types.
        /// </summary>
        /// <remarks>
        /// <para>All non-deleted descendants of the deleted documents are moved to the recycle bin.</para>
        /// <para>This operation is potentially dangerous and expensive.</para>
        /// </remarks>
        void DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId = -1);

        /// <summary>
        /// Deletes versions of a document prior to a given date.
        /// </summary>
        void DeleteVersions(int id, DateTime date, int userId = -1);

        /// <summary>
        /// Deletes a version of a document.
        /// </summary>
        void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = -1);

        #endregion

        #region Move, Copy, Sort Document

        /// <summary>
        /// Moves a document under a new parent.
        /// </summary>
        void Move(IContent content, int parentId, int userId = -1);

        /// <summary>
        /// Copies a document.
        /// </summary>
        /// <remarks>
        /// <para>Recursively copies all children.</para>
        /// </remarks>
        IContent Copy(IContent content, int parentId, bool relateToOriginal, int userId = -1);

        /// <summary>
        /// Copies a document.
        /// </summary>
        /// <remarks>
        /// <para>Optionaly recursively copies all children.</para>
        /// </remarks>
        IContent Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId = -1);

        /// <summary>
        /// Moves a document to the recycle bin.
        /// </summary>
        OperationResult MoveToRecycleBin(IContent content, int userId = -1);

        /// <summary>
        /// Empties the recycle bin.
        /// </summary>
        OperationResult EmptyRecycleBin();

        /// <summary>
        /// Sorts documents.
        /// </summary>
        bool Sort(IEnumerable<IContent> items, int userId = -1, bool raiseEvents = true);

        /// <summary>
        /// Sorts documents.
        /// </summary>
        bool Sort(IEnumerable<int> ids, int userId = -1, bool raiseEvents = true);

        #endregion

        #region Publish Document

        /// <summary>
        /// Saves and publishes a document.
        /// </summary>
        /// <remarks>Property values must first be published at document level.</remarks>
        PublishResult SaveAndPublish(IContent content, int userId = -1, bool raiseEvents = true);

        /// <summary>
        /// Saves and publishes a document branch.
        /// </summary>
        IEnumerable<PublishResult> SaveAndPublishBranch(IContent content, bool force, string culture = null, string segment = null, int userId = -1);

        /// <summary>
        /// Saves and publishes a document branch.
        /// </summary>
        IEnumerable<PublishResult> SaveAndPublishBranch(IContent content, bool force, Func<IContent, bool> editing, Func<IContent, bool> publishValues, int userId = -1);

        /// <summary>
        /// Unpublishes a document or optionally unpublishes a culture and/or segment for the document.
        /// </summary>
        UnpublishResult Unpublish(IContent content, string culture = null, string segment = null, int userId = -1);

        /// <summary>
        /// Gets a value indicating whether a document is path-publishable.
        /// </summary>
        /// <remarks>A document is path-publishable when all its ancestors are published.</remarks>
        bool IsPathPublishable(IContent content);

        /// <summary>
        /// Gets a value indicating whether a document is path-published.
        /// </summary>
        /// <remarks>A document is path-published when all its ancestors, and the document itself, are published.</remarks>
        bool IsPathPublished(IContent content);

        /// <summary>
        /// Saves a document and raises the "sent to publication" events.
        /// </summary>
        bool SendToPublication(IContent content, int userId = -1);

        /// <summary>
        /// Publishes and unpublishes scheduled documents.
        /// </summary>
        IEnumerable<PublishResult> PerformScheduledPublish();

        #endregion

        #region Permissions

        /// <summary>
        /// Gets permissions assigned to a document.
        /// </summary>
        EntityPermissionCollection GetPermissions(IContent content);

        /// <summary>
        /// Sets the permission of a document.
        /// </summary>
        /// <remarks>Replaces all permissions with the new set of permissions.</remarks>
        void SetPermissions(EntityPermissionSet permissionSet);

        /// <summary>
        /// Assigns a permission to a document.
        /// </summary>
        /// <remarks>Adds the permission to existing permissions.</remarks>
        void SetPermission(IContent entity, char permission, IEnumerable<int> groupIds);

        #endregion

        #region Create

        /// <summary>
        /// Creates a document.
        /// </summary>
        IContent Create(string name, Guid parentId, string documentTypeAlias, int userId = -1);

        /// <summary>
        /// Creates a document.
        /// </summary>
        IContent Create(string name, int parentId, string documentTypeAlias, int userId = -1);

        /// <summary>
        /// Creates a document.
        /// </summary>
        IContent Create(string name, IContent parent, string documentTypeAlias, int userId = -1);

        /// <summary>
        /// Creates and saves a document.
        /// </summary>
        IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = -1);

        /// <summary>
        /// Creates and saves a document.
        /// </summary>
        IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = -1);

        #endregion
    }
}
