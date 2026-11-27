using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the Media Service, which is an easy access to operations involving <see cref="IMedia" />.
/// </summary>
/// <remarks>
///     Every persistence member is asynchronous, takes Guid keys and reports expected failures through an
///     <see cref="Attempt{TStatus}" />; only the four file-system members, which wrap the media file system rather than
///     persistence, are synchronous.
/// </remarks>
public interface IMediaService : IAsyncContentServiceBase<IMedia>
{
    /// <summary>
    ///     Gets media items by their unique identifiers.
    /// </summary>
    /// <param name="keys">The unique identifiers of the media items.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media items, in the order requested.</returns>
    Task<IEnumerable<IMedia>> GetByIdsAsync(IEnumerable<Guid> keys, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets media items by their unique identifiers, loading only the requested properties.
    /// </summary>
    /// <param name="keys">The unique identifiers of the media items.</param>
    /// <param name="propertyAliases">
    ///     The property aliases to load. If <c>null</c>, all properties are loaded.
    ///     If empty, no custom properties are loaded (only system properties).
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media items, in the order requested.</returns>
    Task<IEnumerable<IMedia>> GetByIdsAsync(IEnumerable<Guid> keys, string[]? propertyAliases, CancellationToken cancellationToken);

    /// <summary>
    ///     Counts media items of a given media type.
    /// </summary>
    /// <param name="mediaTypeAlias">The media type alias, or <c>null</c> for all types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media item count.</returns>
    Task<int> CountAsync(string? mediaTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Counts child media items of a given parent, of a given media type.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the media tree.</param>
    /// <param name="mediaTypeAlias">The media type alias, or <c>null</c> for all types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The child media item count.</returns>
    Task<int> CountChildrenAsync(Guid? parentKey, string? mediaTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Counts descendant media items of a given parent, of a given media type.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the media tree.</param>
    /// <param name="mediaTypeAlias">The media type alias, or <c>null</c> for all types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The descendant media item count.</returns>
    Task<int> CountDescendantsAsync(Guid? parentKey, string? mediaTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a value indicating whether a media item has children.
    /// </summary>
    /// <param name="key">The Guid key of the media item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the media item has children; otherwise, <c>false</c>.</returns>
    Task<bool> HasChildrenAsync(Guid key, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of direct children of a media node.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent node, or <c>null</c> for the root of the media tree.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="propertyAliases">
    ///     Optional array of property aliases to load. If <c>null</c>, all properties are loaded.
    ///     If empty, no custom properties are loaded (only system properties).
    /// </param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by sort order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching children.</returns>
    Task<PagedModel<IMedia>> GetChildrenAsync(Guid? parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of all descendants of a media node.
    /// </summary>
    /// <param name="ancestorKey">The Guid key of the ancestor node.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by path (ancestors before their own descendants).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="includeTrashed">Whether to include descendants that are currently in the recycle bin. Default is <c>true</c>.</param>
    /// <returns>A paged result containing the matching descendants.</returns>
    Task<PagedModel<IMedia>> GetDescendantsAsync(Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken, bool includeTrashed = true);

    /// <summary>
    ///     Gets a paged list of media items of a given media type.
    /// </summary>
    /// <param name="mediaTypeKey">The Guid key of the media type.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by sort order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching media items.</returns>
    Task<PagedModel<IMedia>> GetPagedOfTypeAsync(Guid mediaTypeKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of media items whose media type is one of the specified media types.
    /// </summary>
    /// <param name="mediaTypeKeys">The Guid keys of the media types.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by sort order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching media items.</returns>
    Task<PagedModel<IMedia>> GetPagedOfTypesAsync(Guid[] mediaTypeKeys, int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets root-level media items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The root-level media items.</returns>
    Task<IEnumerable<IMedia>> GetRootMediaAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of media items in the recycle bin.
    /// </summary>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> to order by path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching recycle bin items.</returns>
    Task<PagedModel<IMedia>> GetPagedMediaInRecycleBinAsync(int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of media items at a given level.
    /// </summary>
    /// <remarks>
    ///     Contrary to most methods, this method filters out trashed media items.
    /// </remarks>
    /// <param name="level">The level.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification. Must not be <c>null</c>: items at the same level may belong to unrelated parents, so there is no single ordering that applies by default.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching, non-trashed media items.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ordering" /> is <c>null</c>.</exception>
    Task<PagedModel<IMedia>> GetByLevelAsync(int level, int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of ancestor media items of a media item, ordered root-first.
    /// </summary>
    /// <param name="key">The Guid key of the media item to retrieve ancestors for.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the media item's ancestors, root-first.</returns>
    Task<PagedModel<IMedia>> GetAncestorsAsync(Guid key, int skip, int take, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of ancestor media items of a media item, ordered root-first.
    /// </summary>
    /// <param name="media">The media item to retrieve ancestors for.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the media item's ancestors, root-first.</returns>
    Task<PagedModel<IMedia>> GetAncestorsAsync(IMedia media, int skip, int take, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the parent of a media item.
    /// </summary>
    /// <param name="key">The Guid key of the media item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parent media item, or <c>null</c> if not found.</returns>
    Task<IMedia?> GetParentAsync(Guid key, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the parent of a media item.
    /// </summary>
    /// <param name="media">The media item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parent media item, or <c>null</c> if not found.</returns>
    Task<IMedia?> GetParentAsync(IMedia? media, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a media item.
    /// </summary>
    /// <remarks>
    ///     The media item is not persisted; it has no identity until it is saved.
    /// </remarks>
    /// <param name="name">The name of the media item.</param>
    /// <param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the media tree.</param>
    /// <param name="mediaTypeAlias">The media type alias.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created media item.</returns>
    Task<IMedia> CreateAsync(string name, Guid? parentKey, string mediaTypeAlias, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a media item.
    /// </summary>
    /// <remarks>
    ///     The media item is not persisted; it has no identity until it is saved.
    /// </remarks>
    /// <param name="name">The name of the media item.</param>
    /// <param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the media tree.</param>
    /// <param name="mediaType">The media type.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created media item.</returns>
    Task<IMedia> CreateAsync(string name, Guid? parentKey, IMediaType mediaType, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a media item.
    /// </summary>
    /// <remarks>
    ///     The media item is not persisted; it has no identity until it is saved.
    /// </remarks>
    /// <param name="name">The name of the media item.</param>
    /// <param name="parent">The parent media item.</param>
    /// <param name="mediaTypeAlias">The media type alias.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created media item.</returns>
    Task<IMedia> CreateAsync(string name, IMedia parent, string mediaTypeAlias, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates and saves a media item.
    /// </summary>
    /// <param name="name">The name of the media item.</param>
    /// <param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the media tree.</param>
    /// <param name="mediaTypeAlias">The media type alias.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created and saved media item.</returns>
    /// <exception cref="InvalidOperationException">The media item cannot be saved because its name or key is invalid.</exception>
    /// <remarks>
    ///     When a saving notification cancels the save, the media item is returned unsaved (without an identity).
    /// </remarks>
    Task<IMedia> CreateAndSaveAsync(string name, Guid? parentKey, string mediaTypeAlias, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates and saves a media item.
    /// </summary>
    /// <param name="name">The name of the media item.</param>
    /// <param name="parent">The parent media item.</param>
    /// <param name="mediaTypeAlias">The media type alias.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created and saved media item.</returns>
    /// <exception cref="InvalidOperationException">The media item cannot be saved because its name or key is invalid.</exception>
    /// <remarks>
    ///     When a saving notification cancels the save, the media item is returned unsaved (without an identity).
    /// </remarks>
    Task<IMedia> CreateAndSaveAsync(string name, IMedia parent, string mediaTypeAlias, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Saves a media item.
    /// </summary>
    /// <param name="media">The media item to save.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     An attempt carrying the operation status: <see cref="ContentSaveOperationStatus.InvalidName" /> for an empty
    ///     or over-long name, <see cref="ContentSaveOperationStatus.InvalidKey" /> when the item's key is a version 7
    ///     Guid the registered media path scheme cannot use, <see cref="ContentSaveOperationStatus.DuplicateKey" />
    ///     when a new item's key already exists, and <see cref="ContentSaveOperationStatus.CancelledByNotification" />
    ///     when a saving notification handler cancelled the save.
    /// </returns>
    Task<Attempt<ContentSaveOperationStatus>> SaveAsync(IMedia media, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a media item.
    /// </summary>
    /// <param name="media">The media item to delete.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    /// <remarks>
    ///     <para>This method will also delete the media item's descendants.</para>
    ///     <para>This method entirely clears the media item from the database; the media files are deleted by the notification handlers.</para>
    /// </remarks>
    Task<Attempt<ContentDeleteOperationStatus>> DeleteAsync(IMedia media, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes all media of a given media type.
    /// </summary>
    /// <param name="mediaTypeKey">The Guid key of the media type.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    ///     <para>Every descendant of the deleted media that is not of the deleted type is moved to the recycle bin.</para>
    ///     <para>This operation is potentially dangerous and expensive.</para>
    /// </remarks>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentDeleteOfTypesOperationStatus>> DeleteOfTypeAsync(Guid mediaTypeKey, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes all media of the given media types.
    /// </summary>
    /// <param name="mediaTypeKeys">The Guid keys of the media types.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    ///     <para>Every descendant of the deleted media that is not of the deleted types is moved to the recycle bin.</para>
    ///     <para>This operation is potentially dangerous and expensive.</para>
    /// </remarks>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentDeleteOfTypesOperationStatus>> DeleteOfTypesAsync(IEnumerable<Guid> mediaTypeKeys, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Moves a media item under a new parent, optionally leaving its descendants behind.
    /// </summary>
    /// <param name="media">The media item to move.</param>
    /// <param name="parentKey">The Guid key of the new parent, or <c>null</c> for the root of the media tree.</param>
    /// <param name="includeDescendants">
    ///     Whether to move the descendants of the media item along with it. When restoring an item out of the recycle
    ///     bin this can be set to <c>false</c> to restore only the item itself, leaving its descendants in the
    ///     recycle bin as top-level bin items.
    /// </param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentMoveOperationStatus>> MoveAsync(IMedia media, Guid? parentKey, bool includeDescendants, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Moves a media item to the recycle bin.
    /// </summary>
    /// <param name="media">The media item to move to the recycle bin.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentMoveToRecycleBinOperationStatus>> MoveToRecycleBinAsync(IMedia media, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Empties the recycle bin by deleting every media item that resides in it.
    /// </summary>
    /// <param name="userKey">The Guid key of the user emptying the recycle bin.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentEmptyRecycleBinOperationStatus>> EmptyRecycleBinAsync(Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a value indicating whether there is any media in the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if there is media in the recycle bin; otherwise, <c>false</c>.</returns>
    Task<bool> RecycleBinSmellsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Sorts media items by persisting the supplied (already ordered) keys as the new sort order,
    ///     firing the cancelable saving notification (flagged as a sort), one saved notification for the batch, and
    ///     an audit record for every item whose position actually changes.
    /// </summary>
    /// <param name="orderedKeys">The Guid keys of the media items, in the desired order.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    /// <remarks>
    ///     Unlike <see cref="SortChildrenAsync" />, this reloads each item, fires the cancelable
    ///     saving notification, and only persists/audits items whose sort position changes.
    /// </remarks>
    Task<Attempt<ContentSortOperationStatus>> SortAsync(IReadOnlyList<Guid> orderedKeys, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Sorts the children of a parent by persisting the supplied (already ordered) child keys as the
    ///     new sort order, in a single batched operation.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the media tree.</param>
    /// <param name="orderedChildKeys">The Guid keys of the children, in the desired order.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    /// <remarks>
    ///     Unlike <see cref="SortAsync" />, this does not load the children or fire per-item
    ///     save notifications; it persists the order directly and refreshes the affected cache branch.
    /// </remarks>
    Task<Attempt<ContentSortChildrenOperationStatus>> SortChildrenAsync(Guid? parentKey, IReadOnlyList<Guid> orderedChildKeys, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a version of a media item.
    /// </summary>
    /// <remarks>
    ///     Int-keyed rather than Guid-keyed: unlike media items, versions have no Guid key resolvable
    ///     from a caller-facing identifier today, so this takes the version's raw database id directly.
    /// </remarks>
    /// <param name="versionId">The version identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media version, or <c>null</c> if not found.</returns>
    Task<IMedia?> GetVersionAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets all versions of a media item.
    /// </summary>
    /// <param name="key">The Guid key of the media item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media versions.</returns>
    /// <remarks>Versions are ordered with current first, then most recent first. A media item keeps one version.</remarks>
    Task<IEnumerable<IMedia>> GetVersionsAsync(Guid key, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes versions of a media item prior to a given date.
    /// </summary>
    /// <param name="key">The Guid key of the media item.</param>
    /// <param name="versionDate">The date before which versions should be deleted.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentVersionOperationStatus>> DeleteVersionsAsync(Guid key, DateTime versionDate, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a version of a media item.
    /// </summary>
    /// <param name="key">The Guid key of the media item.</param>
    /// <param name="versionId">The version identifier to delete.</param>
    /// <param name="deletePriorVersions">Whether to also delete versions prior to the specified version.</param>
    /// <param name="userKey">The Guid key of the user performing the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An attempt carrying the operation status.</returns>
    Task<Attempt<ContentVersionOperationStatus>> DeleteVersionAsync(Guid key, int versionId, bool deletePriorVersions, Guid userKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a media item from the path stored in its file property.
    /// </summary>
    /// <param name="mediaPath">The path of the media item to retrieve (for example <c>/media/1024/koala_403x328.jpg</c>).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media item, or <c>null</c> if no media item stores that path.</returns>
    Task<IMedia?> GetMediaByPathAsync(string mediaPath, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the content of a media file as a stream.
    /// </summary>
    /// <param name="filepath">The filesystem path to the media file.</param>
    /// <returns>The content of the media file.</returns>
    Stream GetMediaFileContentStream(string filepath);

    /// <summary>
    ///     Sets the content of a media file.
    /// </summary>
    /// <param name="filepath">The filesystem path to the media file.</param>
    /// <param name="content">The content of the media file.</param>
    void SetMediaFileContent(string filepath, Stream content);

    /// <summary>
    ///     Deletes a media file.
    /// </summary>
    /// <param name="filepath">The filesystem path to the media file.</param>
    void DeleteMediaFile(string filepath);

    /// <summary>
    ///     Gets the size of a media file.
    /// </summary>
    /// <param name="filepath">The filesystem path to the media file.</param>
    /// <returns>The size of the media file.</returns>
    long GetMediaFileSize(string filepath);
}
