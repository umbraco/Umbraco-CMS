// src/Umbraco.Core/Services/IContentCrudService.cs
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for content CRUD (Create, Read, Update, Delete) operations.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Implementation Note:</strong> Do not implement this interface directly.
/// Instead, inherit from <see cref="ContentServiceBase"/> which provides required
/// infrastructure (scoping, repository access, auditing). Direct implementation
/// without this base class will result in missing functionality.
/// </para>
/// <para>
/// This interface is part of the ContentService refactoring initiative.
/// It extracts core CRUD operations into a focused, testable service.
/// </para>
/// <para>
/// <strong>Versioning Policy:</strong> This interface follows additive-only changes.
/// New methods may be added with default implementations. Existing methods will not
/// be removed or have signatures changed without a 2 major version deprecation period.
/// </para>
/// <para>
/// <strong>Version History:</strong>
/// <list type="bullet">
///   <item><description>v1.0 (Phase 1): Initial interface with Create, Read, Save, Delete operations</description></item>
/// </list>
/// </para>
/// </remarks>
/// <since>1.0</since>
public interface IContentCrudService : IService
{
    #region Create

    /// <summary>
    /// Creates a document without persisting it.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parentId">Id of the parent, or -1 for root.</param>
    /// <param name="contentTypeAlias">Alias of the content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The new document.</returns>
    IContent Create(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Creates a document without persisting it.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parentId">Guid key of the parent.</param>
    /// <param name="contentTypeAlias">Alias of the content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The new document.</returns>
    IContent Create(string name, Guid parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Creates a document without persisting it.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parentId">Id of the parent, or -1 for root.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The new document.</returns>
    IContent Create(string name, int parentId, IContentType contentType, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Creates a document without persisting it.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parent">The parent document.</param>
    /// <param name="contentTypeAlias">Alias of the content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The new document.</returns>
    IContent Create(string name, IContent? parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Creates and persists a document.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parentId">Id of the parent, or -1 for root.</param>
    /// <param name="contentTypeAlias">Alias of the content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The persisted document.</returns>
    IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Creates and persists a document.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parent">The parent document.</param>
    /// <param name="contentTypeAlias">Alias of the content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The persisted document.</returns>
    IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Read

    /// <summary>
    /// Gets a document by id.
    /// </summary>
    /// <param name="id">The document id.</param>
    /// <returns>The document, or null if not found.</returns>
    IContent? GetById(int id);

    /// <summary>
    /// Gets a document by key.
    /// </summary>
    /// <param name="key">The document key.</param>
    /// <returns>The document, or null if not found.</returns>
    IContent? GetById(Guid key);

    /// <summary>
    /// Gets documents by ids.
    /// </summary>
    /// <param name="ids">The document ids.</param>
    /// <returns>The documents.</returns>
    IEnumerable<IContent> GetByIds(IEnumerable<int> ids);

    /// <summary>
    /// Gets documents by keys.
    /// </summary>
    /// <param name="ids">The document keys.</param>
    /// <returns>The documents.</returns>
    IEnumerable<IContent> GetByIds(IEnumerable<Guid> ids);

    /// <summary>
    /// Gets root-level documents.
    /// </summary>
    /// <returns>The root documents.</returns>
    IEnumerable<IContent> GetRootContent();

    /// <summary>
    /// Gets the parent of a document.
    /// </summary>
    /// <param name="id">Id of the document.</param>
    /// <returns>The parent document, or null if at root.</returns>
    IContent? GetParent(int id);

    /// <summary>
    /// Gets the parent of a document.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <returns>The parent document, or null if at root.</returns>
    IContent? GetParent(IContent? content);

    #endregion

    #region Read (Tree Traversal)

    /// <summary>
    /// Gets ancestors of a document.
    /// </summary>
    /// <param name="id">Id of the document.</param>
    /// <returns>The ancestor documents, from root to parent (closest to root first).</returns>
    IEnumerable<IContent> GetAncestors(int id);

    /// <summary>
    /// Gets ancestors of a document.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <returns>The ancestor documents, from root to parent (closest to root first).</returns>
    IEnumerable<IContent> GetAncestors(IContent content);

    /// <summary>
    /// Gets paged children of a document.
    /// </summary>
    /// <param name="id">Id of the parent document.</param>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="totalChildren">Total number of children.</param>
    /// <param name="filter">Optional filter query.</param>
    /// <param name="ordering">Optional ordering.</param>
    /// <returns>The child documents.</returns>
    IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null);

    /// <summary>
    /// Gets paged descendants of a document.
    /// </summary>
    /// <param name="id">Id of the ancestor document.</param>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="totalChildren">Total number of descendants.</param>
    /// <param name="filter">Optional filter query.</param>
    /// <param name="ordering">Optional ordering.</param>
    /// <returns>The descendant documents.</returns>
    IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null);

    /// <summary>
    /// Checks whether a document has children.
    /// </summary>
    /// <param name="id">The document id.</param>
    /// <returns>True if the document has children; otherwise false.</returns>
    bool HasChildren(int id);

    /// <summary>
    /// Checks whether a document with the specified id exists.
    /// </summary>
    /// <param name="id">The document id.</param>
    /// <returns>True if the document exists; otherwise false.</returns>
    bool Exists(int id);

    /// <summary>
    /// Checks whether a document with the specified key exists.
    /// </summary>
    /// <param name="key">The document key.</param>
    /// <returns>True if the document exists; otherwise false.</returns>
    bool Exists(Guid key);

    #endregion

    #region Save

    /// <summary>
    /// Saves a document.
    /// </summary>
    /// <param name="content">The document to save.</param>
    /// <param name="userId">Optional id of the user saving the content.</param>
    /// <param name="contentSchedule">Optional content schedule.</param>
    /// <returns>The operation result.</returns>
    OperationResult Save(IContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null);

    /// <summary>
    /// Saves multiple documents.
    /// </summary>
    /// <param name="contents">The documents to save.</param>
    /// <param name="userId">Optional id of the user saving the content.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    /// This method does not support content schedules. To save content with schedules,
    /// use the single-item <see cref="Save(IContent, int?, ContentScheduleCollection?)"/> overload.
    /// </remarks>
    OperationResult Save(IEnumerable<IContent> contents, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Delete

    /// <summary>
    /// Permanently deletes a document and all its descendants.
    /// </summary>
    /// <param name="content">The document to delete.</param>
    /// <param name="userId">Optional id of the user deleting the content.</param>
    /// <returns>The operation result.</returns>
    OperationResult Delete(IContent content, int userId = Constants.Security.SuperUserId);

    #endregion
}
