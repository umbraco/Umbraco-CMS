using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the Media Service, which is an easy access to operations involving <see cref="IMedia" />
/// </summary>
public interface IMediaService : IContentServiceBase<IMedia>
{
    int CountNotTrashed(string? contentTypeAlias = null);

    int Count(string? mediaTypeAlias = null);

    int CountChildren(int parentId, string? mediaTypeAlias = null);

    int CountDescendants(int parentId, string? mediaTypeAlias = null);

    IEnumerable<IMedia> GetByIds(IEnumerable<int> ids);

    IEnumerable<IMedia> GetByIds(IEnumerable<Guid> ids);

    /// <summary>
    ///     Creates an <see cref="IMedia" /> object using the alias of the <see cref="IMediaType" />
    ///     that this Media should based on.
    /// </summary>
    /// <remarks>
    ///     Note that using this method will simply return a new IMedia without any identity
    ///     as it has not yet been persisted. It is intended as a shortcut to creating new media objects
    ///     that does not invoke a save operation against the database.
    /// </remarks>
    /// <param name="name">Name of the Media object</param>
    /// <param name="parentId">Id of Parent for the new Media item</param>
    /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType" /></param>
    /// <param name="userId">Optional id of the user creating the media item</param>
    /// <returns>
    ///     <see cref="IMedia" />
    /// </returns>
    IMedia CreateMedia(string name, Guid parentId, string mediaTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates an <see cref="IMedia" /> object using the alias of the <see cref="IMediaType" />
    ///     that this Media should based on.
    /// </summary>
    /// <remarks>
    ///     Note that using this method will simply return a new IMedia without any identity
    ///     as it has not yet been persisted. It is intended as a shortcut to creating new media objects
    ///     that does not invoke a save operation against the database.
    /// </remarks>
    /// <param name="name">Name of the Media object</param>
    /// <param name="parentId">Id of Parent for the new Media item</param>
    /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType" /></param>
    /// <param name="userId">Optional id of the user creating the media item</param>
    /// <returns>
    ///     <see cref="IMedia" />
    /// </returns>
    IMedia CreateMedia(string? name, int parentId, string mediaTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates an <see cref="IMedia" /> object using the alias of the <see cref="IMediaType" />
    ///     that this Media should based on.
    /// </summary>
    /// <remarks>
    ///     Note that using this method will simply return a new IMedia without any identity
    ///     as it has not yet been persisted. It is intended as a shortcut to creating new media objects
    ///     that does not invoke a save operation against the database.
    /// </remarks>
    /// <param name="name">Name of the Media object</param>
    /// <param name="parent">Parent <see cref="IMedia" /> for the new Media item</param>
    /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType" /></param>
    /// <param name="userId">Optional id of the user creating the media item</param>
    /// <returns>
    ///     <see cref="IMedia" />
    /// </returns>
    IMedia CreateMedia(string name, IMedia? parent, string mediaTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets an <see cref="IMedia" /> object by Id
    /// </summary>
    /// <param name="id">Id of the Content to retrieve</param>
    /// <returns>
    ///     <see cref="IMedia" />
    /// </returns>
    IMedia? GetById(int id);

    /// <summary>
    ///     Gets a collection of <see cref="IMedia" /> objects by Parent Id
    /// </summary>
    /// <param name="id">Id of the Parent to retrieve Children from</param>
    /// <param name="pageIndex">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="totalRecords">Total records query would return without paging</param>
    /// <param name="orderBy">Field to order by</param>
    /// <param name="orderDirection">Direction to order by</param>
    /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
    /// <param name="filter"></param>
    /// <param name="ordering"></param>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    IEnumerable<IMedia> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalRecords, IQuery<IMedia>? filter = null, Ordering? ordering = null);

    /// <summary>
    ///     Gets a collection of <see cref="IMedia" /> objects by Parent Id
    /// </summary>
    /// <param name="id">Id of the Parent to retrieve Descendants from</param>
    /// <param name="pageIndex">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="totalRecords">Total records query would return without paging</param>
    /// <param name="ordering"></param>
    /// <param name="filter"></param>
    /// <returns>An Enumerable list of <see cref="IContent" /> objects</returns>
    IEnumerable<IMedia> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalRecords, IQuery<IMedia>? filter = null, Ordering? ordering = null);

    /// <summary>
    ///     Gets paged documents of a content
    /// </summary>
    /// <param name="contentTypeId">The page number.</param>
    /// <param name="pageIndex">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Total number of documents.</param>
    /// <param name="filter">Search text filter.</param>
    /// <param name="ordering">Ordering infos.</param>
    IEnumerable<IMedia> GetPagedOfType(int contentTypeId, long pageIndex, int pageSize, out long totalRecords, IQuery<IMedia>? filter = null, Ordering? ordering = null);

    /// <summary>
    ///     Gets paged documents for specified content types
    /// </summary>
    /// <param name="contentTypeIds">The page number.</param>
    /// <param name="pageIndex">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Total number of documents.</param>
    /// <param name="filter">Search text filter.</param>
    /// <param name="ordering">Ordering infos.</param>
    IEnumerable<IMedia> GetPagedOfTypes(
        int[] contentTypeIds,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IMedia>? filter = null,
        Ordering? ordering = null);

    /// <summary>
    ///     Gets a collection of <see cref="IMedia" /> objects, which reside at the first level / root
    /// </summary>
    /// <returns>An Enumerable list of <see cref="IMedia" /> objects</returns>
    IEnumerable<IMedia> GetRootMedia();

    /// <summary>
    ///     Gets a collection of an <see cref="IMedia" /> objects, which resides in the Recycle Bin
    /// </summary>
    /// <returns>An Enumerable list of <see cref="IMedia" /> objects</returns>
    IEnumerable<IMedia> GetPagedMediaInRecycleBin(
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IMedia>? filter = null,
        Ordering? ordering = null);

    /// <summary>
    ///     Moves an <see cref="IMedia" /> object to a new location
    /// </summary>
    /// <param name="media">The <see cref="IMedia" /> to move</param>
    /// <param name="parentId">Id of the Media's new Parent</param>
    /// <param name="userId">Id of the User moving the Media</param>
    /// <returns>True if moving succeeded, otherwise False</returns>
    Attempt<OperationResult?> Move(IMedia media, int parentId, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes an <see cref="IMedia" /> object by moving it to the Recycle Bin
    /// </summary>
    /// <param name="media">The <see cref="IMedia" /> to delete</param>
    /// <param name="userId">Id of the User deleting the Media</param>
    Attempt<OperationResult?> MoveToRecycleBin(IMedia media, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Empties the Recycle Bin by deleting all <see cref="IMedia" /> that resides in the bin
    /// </summary>
    /// <param name="userId">Optional Id of the User emptying the Recycle Bin</param>
    OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Returns true if there is any media in the recycle bin
    /// </summary>
    bool RecycleBinSmells();

    /// <summary>
    ///     Deletes all media of specified type. All children of deleted media is moved to Recycle Bin.
    /// </summary>
    /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
    /// <param name="mediaTypeId">Id of the <see cref="IMediaType" /></param>
    /// <param name="userId">Optional Id of the user deleting Media</param>
    void DeleteMediaOfType(int mediaTypeId, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes all media of the specified types. All Descendants of deleted media that is not of these types is moved to
    ///     Recycle Bin.
    /// </summary>
    /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
    /// <param name="mediaTypeIds">Ids of the <see cref="IMediaType" />s</param>
    /// <param name="userId">Optional Id of the user issuing the delete operation</param>
    void DeleteMediaOfTypes(IEnumerable<int> mediaTypeIds, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Permanently deletes an <see cref="IMedia" /> object
    /// </summary>
    /// <remarks>
    ///     Please note that this method will completely remove the Media from the database,
    ///     but current not from the file system.
    /// </remarks>
    /// <param name="media">The <see cref="IMedia" /> to delete</param>
    /// <param name="userId">Id of the User deleting the Media</param>
    Attempt<OperationResult?> Delete(IMedia media, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Saves a single <see cref="IMedia" /> object
    /// </summary>
    /// <param name="media">The <see cref="IMedia" /> to save</param>
    /// <param name="userId">Id of the User saving the Media</param>
    Attempt<OperationResult?> Save(IMedia media, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Saves a collection of <see cref="IMedia" /> objects
    /// </summary>
    /// <param name="medias">Collection of <see cref="IMedia" /> to save</param>
    /// <param name="userId">Id of the User saving the Media</param>
    Attempt<OperationResult?> Save(IEnumerable<IMedia> medias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets an <see cref="IMedia" /> object by its 'UniqueId'
    /// </summary>
    /// <param name="key">Guid key of the Media to retrieve</param>
    /// <returns>
    ///     <see cref="IMedia" />
    /// </returns>
    IMedia? GetById(Guid key);

    /// <summary>
    ///     Gets a collection of <see cref="IMedia" /> objects by Level
    /// </summary>
    /// <param name="level">The level to retrieve Media from</param>
    /// <returns>An Enumerable list of <see cref="IMedia" /> objects</returns>
    IEnumerable<IMedia>? GetByLevel(int level);

    /// <summary>
    ///     Gets a specific version of an <see cref="IMedia" /> item.
    /// </summary>
    /// <param name="versionId">Id of the version to retrieve</param>
    /// <returns>An <see cref="IMedia" /> item</returns>
    IMedia? GetVersion(int versionId);

    /// <summary>
    ///     Gets a collection of an <see cref="IMedia" /> objects versions by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>An Enumerable list of <see cref="IMedia" /> objects</returns>
    IEnumerable<IMedia> GetVersions(int id);

    /// <summary>
    ///     Checks whether an <see cref="IMedia" /> item has any children
    /// </summary>
    /// <param name="id">Id of the <see cref="IMedia" /></param>
    /// <returns>True if the media has any children otherwise False</returns>
    bool HasChildren(int id);

    /// <summary>
    ///     Permanently deletes versions from an <see cref="IMedia" /> object prior to a specific date.
    /// </summary>
    /// <param name="id">Id of the <see cref="IMedia" /> object to delete versions from</param>
    /// <param name="versionDate">Latest version date</param>
    /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
    void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Permanently deletes specific version(s) from an <see cref="IMedia" /> object.
    /// </summary>
    /// <param name="id">Id of the <see cref="IMedia" /> object to delete a version from</param>
    /// <param name="versionId">Id of the version to delete</param>
    /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
    /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
    void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets an <see cref="IMedia" /> object from the path stored in the 'umbracoFile' property.
    /// </summary>
    /// <param name="mediaPath">Path of the media item to retrieve (for example: /media/1024/koala_403x328.jpg)</param>
    /// <returns>
    ///     <see cref="IMedia" />
    /// </returns>
    IMedia? GetMediaByPath(string mediaPath);

    /// <summary>
    ///     Gets a collection of <see cref="IMedia" /> objects, which are ancestors of the current media.
    /// </summary>
    /// <param name="id">Id of the <see cref="IMedia" /> to retrieve ancestors for</param>
    /// <returns>An Enumerable list of <see cref="IMedia" /> objects</returns>
    IEnumerable<IMedia> GetAncestors(int id);

    /// <summary>
    ///     Gets a collection of <see cref="IMedia" /> objects, which are ancestors of the current media.
    /// </summary>
    /// <param name="media"><see cref="IMedia" /> to retrieve ancestors for</param>
    /// <returns>An Enumerable list of <see cref="IMedia" /> objects</returns>
    IEnumerable<IMedia> GetAncestors(IMedia media);

    /// <summary>
    ///     Gets the parent of the current media as an <see cref="IMedia" /> item.
    /// </summary>
    /// <param name="id">Id of the <see cref="IMedia" /> to retrieve the parent from</param>
    /// <returns>Parent <see cref="IMedia" /> object</returns>
    IMedia? GetParent(int id);

    /// <summary>
    ///     Gets the parent of the current media as an <see cref="IMedia" /> item.
    /// </summary>
    /// <param name="media"><see cref="IMedia" /> to retrieve the parent from</param>
    /// <returns>Parent <see cref="IMedia" /> object</returns>
    IMedia? GetParent(IMedia media);

    /// <summary>
    ///     Sorts a collection of <see cref="IMedia" /> objects by updating the SortOrder according
    ///     to the ordering of items in the passed in <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <param name="items"></param>
    /// <param name="userId"></param>
    /// <returns>True if sorting succeeded, otherwise False</returns>
    bool Sort(IEnumerable<IMedia> items, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates an <see cref="IMedia" /> object using the alias of the <see cref="IMediaType" />
    ///     that this Media should based on.
    /// </summary>
    /// <remarks>
    ///     This method returns an <see cref="IMedia" /> object that has been persisted to the database
    ///     and therefor has an identity.
    /// </remarks>
    /// <param name="name">Name of the Media object</param>
    /// <param name="parent">Parent <see cref="IMedia" /> for the new Media item</param>
    /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType" /></param>
    /// <param name="userId">Optional id of the user creating the media item</param>
    /// <returns>
    ///     <see cref="IMedia" />
    /// </returns>
    IMedia CreateMediaWithIdentity(string name, IMedia parent, string mediaTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates an <see cref="IMedia" /> object using the alias of the <see cref="IMediaType" />
    ///     that this Media should based on.
    /// </summary>
    /// <remarks>
    ///     This method returns an <see cref="IMedia" /> object that has been persisted to the database
    ///     and therefor has an identity.
    /// </remarks>
    /// <param name="name">Name of the Media object</param>
    /// <param name="parentId">Id of Parent for the new Media item</param>
    /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType" /></param>
    /// <param name="userId">Optional id of the user creating the media item</param>
    /// <returns>
    ///     <see cref="IMedia" />
    /// </returns>
    IMedia CreateMediaWithIdentity(string name, int parentId, string mediaTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets the content of a media as a stream.
    /// </summary>
    /// <param name="filepath">The filesystem path to the media.</param>
    /// <returns>The content of the media.</returns>
    Stream GetMediaFileContentStream(string filepath);

    /// <summary>
    ///     Sets the content of a media.
    /// </summary>
    /// <param name="filepath">The filesystem path to the media.</param>
    /// <param name="content">The content of the media.</param>
    void SetMediaFileContent(string filepath, Stream content);

    /// <summary>
    ///     Deletes a media file.
    /// </summary>
    /// <param name="filepath">The filesystem path to the media.</param>
    void DeleteMediaFile(string filepath);

    /// <summary>
    ///     Gets the size of a media.
    /// </summary>
    /// <param name="filepath">The filesystem path to the media.</param>
    /// <returns>The size of the media.</returns>
    long GetMediaFileSize(string filepath);
}
