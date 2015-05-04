using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the Media Service, which is an easy access to operations involving <see cref="IMedia"/>
    /// </summary>
    public interface IMediaService : IService
    {
        /// <summary>
        /// Rebuilds all xml content in the cmsContentXml table for all media
        /// </summary>
        /// <param name="contentTypeIds">
        /// Only rebuild the xml structures for the content type ids passed in, if none then rebuilds the structures
        /// for all media
        /// </param>
        void RebuildXmlStructures(params int[] contentTypeIds);

        int Count(string contentTypeAlias = null);
        int CountChildren(int parentId, string contentTypeAlias = null);
        int CountDescendants(int parentId, string contentTypeAlias = null);

        IEnumerable<IMedia> GetByIds(IEnumerable<int> ids);

        /// <summary>
        /// Creates an <see cref="IMedia"/> object using the alias of the <see cref="IMediaType"/>
        /// that this Media should based on.
        /// </summary>
        /// <remarks>
        /// Note that using this method will simply return a new IMedia without any identity
        /// as it has not yet been persisted. It is intended as a shortcut to creating new media objects
        /// that does not invoke a save operation against the database.
        /// </remarks>
        /// <param name="name">Name of the Media object</param>
        /// <param name="parentId">Id of Parent for the new Media item</param>
        /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user creating the media item</param>
        /// <returns><see cref="IMedia"/></returns>
        IMedia CreateMedia(string name, int parentId, string mediaTypeAlias, int userId = 0);

        /// <summary>
        /// Creates an <see cref="IMedia"/> object using the alias of the <see cref="IMediaType"/>
        /// that this Media should based on.
        /// </summary>
        /// <remarks>
        /// Note that using this method will simply return a new IMedia without any identity
        /// as it has not yet been persisted. It is intended as a shortcut to creating new media objects
        /// that does not invoke a save operation against the database.
        /// </remarks>
        /// <param name="name">Name of the Media object</param>
        /// <param name="parent">Parent <see cref="IMedia"/> for the new Media item</param>
        /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user creating the media item</param>
        /// <returns><see cref="IMedia"/></returns>
        IMedia CreateMedia(string name, IMedia parent, string mediaTypeAlias, int userId = 0);

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Content to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        IMedia GetById(int id);

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetChildren(int id);

        [Obsolete("Use the overload with 'long' parameter types instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerable<IMedia> GetPagedChildren(int id, int pageIndex, int pageSize, out int totalRecords,
            string orderBy = "SortOrder", Direction orderDirection = Direction.Ascending, string filter = "");

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IMedia> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "SortOrder", Direction orderDirection = Direction.Ascending, string filter = "");

        [Obsolete("Use the overload with 'long' parameter types instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerable<IMedia> GetPagedDescendants(int id, int pageIndex, int pageSize, out int totalRecords,
            string orderBy = "Path", Direction orderDirection = Direction.Ascending, string filter = "");

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Descendants from</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IMedia> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "Path", Direction orderDirection = Direction.Ascending, string filter = "");

        /// <summary>
        /// Gets descendants of a <see cref="IMedia"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve descendants from</param>
        /// <returns>An Enumerable flat list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetDescendants(int id);

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by the Id of the <see cref="IContentType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetMediaOfMediaType(int id);

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetRootMedia();

        /// <summary>
        /// Gets a collection of an <see cref="IMedia"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetMediaInRecycleBin();

        /// <summary>
        /// Moves an <see cref="IMedia"/> object to a new location
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to move</param>
        /// <param name="parentId">Id of the Media's new Parent</param>
        /// <param name="userId">Id of the User moving the Media</param>
        void Move(IMedia media, int parentId, int userId = 0);

        /// <summary>
        /// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        void MoveToRecycleBin(IMedia media, int userId = 0);

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IMedia"/> that resides in the bin
        /// </summary>
        void EmptyRecycleBin();

        /// <summary>
        /// Deletes all media of specified type. All children of deleted media is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="mediaTypeId">Id of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional Id of the user deleting Media</param>
        void DeleteMediaOfType(int mediaTypeId, int userId = 0);

        /// <summary>
        /// Permanently deletes an <see cref="IMedia"/> object
        /// </summary>
        /// <remarks>
        /// Please note that this method will completely remove the Media from the database,
        /// but current not from the file system.
        /// </remarks>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        void Delete(IMedia media, int userId = 0);

        /// <summary>
        /// Saves a single <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Media</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        void Save(IMedia media, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Saves a collection of <see cref="IMedia"/> objects
        /// </summary>
        /// <param name="medias">Collection of <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Media</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        void Save(IEnumerable<IMedia> medias, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by its 'UniqueId'
        /// </summary>
        /// <param name="key">Guid key of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        IMedia GetById(Guid key);

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Level
        /// </summary>
        /// <param name="level">The level to retrieve Media from</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetByLevel(int level);

        /// <summary>
        /// Gets a specific version of an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="IMedia"/> item</returns>
        IMedia GetByVersion(Guid versionId);

        /// <summary>
        /// Gets a collection of an <see cref="IMedia"/> objects versions by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetVersions(int id);

        /// <summary>
        /// Checks whether an <see cref="IMedia"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/></param>
        /// <returns>True if the media has any children otherwise False</returns>
        bool HasChildren(int id);

        /// <summary>
        /// Permanently deletes versions from an <see cref="IMedia"/> object prior to a specific date.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> object to delete versions from</param>
        /// <param name="versionDate">Latest version date</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        void DeleteVersions(int id, DateTime versionDate, int userId = 0);

        /// <summary>
        /// Permanently deletes specific version(s) from an <see cref="IMedia"/> object.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        void DeleteVersion(int id, Guid versionId, bool deletePriorVersions, int userId = 0);

        /// <summary>
        /// Gets an <see cref="IMedia"/> object from the path stored in the 'umbracoFile' property.
        /// </summary>
        /// <param name="mediaPath">Path of the media item to retrieve (for example: /media/1024/koala_403x328.jpg)</param>
        /// <returns><see cref="IMedia"/></returns>
        IMedia GetMediaByPath(string mediaPath);

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which are ancestors of the current media.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> to retrieve ancestors for</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetAncestors(int id);

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which are ancestors of the current media.
        /// </summary>
        /// <param name="media"><see cref="IMedia"/> to retrieve ancestors for</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetAncestors(IMedia media);

        /// <summary>
        /// Gets descendants of a <see cref="IMedia"/> object by its Id
        /// </summary>
        /// <param name="media">The Parent <see cref="IMedia"/> object to retrieve descendants from</param>
        /// <returns>An Enumerable flat list of <see cref="IMedia"/> objects</returns>
        IEnumerable<IMedia> GetDescendants(IMedia media);

        /// <summary>
        /// Gets the parent of the current media as an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> to retrieve the parent from</param>
        /// <returns>Parent <see cref="IMedia"/> object</returns>
        IMedia GetParent(int id);

        /// <summary>
        /// Gets the parent of the current media as an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="media"><see cref="IMedia"/> to retrieve the parent from</param>
        /// <returns>Parent <see cref="IMedia"/> object</returns>
        IMedia GetParent(IMedia media);

        /// <summary>
        /// Sorts a collection of <see cref="IMedia"/> objects by updating the SortOrder according
        /// to the ordering of items in the passed in <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="userId"></param>
        /// <param name="raiseEvents"></param>
        /// <returns>True if sorting succeeded, otherwise False</returns>
        bool Sort(IEnumerable<IMedia> items, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Creates an <see cref="IMedia"/> object using the alias of the <see cref="IMediaType"/>
        /// that this Media should based on.
        /// </summary>
        /// <remarks>
        /// This method returns an <see cref="IMedia"/> object that has been persisted to the database
        /// and therefor has an identity.
        /// </remarks>
        /// <param name="name">Name of the Media object</param>
        /// <param name="parent">Parent <see cref="IMedia"/> for the new Media item</param>
        /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user creating the media item</param>
        /// <returns><see cref="IMedia"/></returns>
        IMedia CreateMediaWithIdentity(string name, IMedia parent, string mediaTypeAlias, int userId = 0);

        /// <summary>
        /// Creates an <see cref="IMedia"/> object using the alias of the <see cref="IMediaType"/>
        /// that this Media should based on.
        /// </summary>
        /// <remarks>
        /// This method returns an <see cref="IMedia"/> object that has been persisted to the database
        /// and therefor has an identity.
        /// </remarks>
        /// <param name="name">Name of the Media object</param>
        /// <param name="parentId">Id of Parent for the new Media item</param>
        /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user creating the media item</param>
        /// <returns><see cref="IMedia"/></returns>
        IMedia CreateMediaWithIdentity(string name, int parentId, string mediaTypeAlias, int userId = 0);
    }
}
