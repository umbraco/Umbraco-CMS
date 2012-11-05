using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Web.Services
{
    /// <summary>
    /// Defines the ContentService, which is an easy access to operations involving <see cref="IContent"/>
    /// </summary>
    public interface IContentService : IService
    {
        /// <summary>
        /// Creates an <see cref="IContent"/> object using the alias of the <see cref="IContentType"/>
        /// that this Content is based on.
        /// </summary>
        /// <param name="parentId">Id of Parent for content</param>
        /// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional id of the user creating the content</param>
        /// <returns><see cref="IContent"/></returns>
        IContent CreateContent(int parentId, string contentTypeAlias, int userId = -1);

        //TODO Add CreateNewVersion method? Its currently used in the Document API when Publishing - latest version is published, 
        //but then a new version is created so latest version is not published.
        //IContent CreateNewVersion(int id);

        /// <summary>
        /// Gets an <see cref="IContent"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        IContent GetById(int id);

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by the Id of the <see cref="IContentType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetContentOfContentType(int id);

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Level
        /// </summary>
        /// <param name="level">The level to retrieve Content from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetByLevel(int level);

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetChildren(int id);

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects versions by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetVersions(int id);

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetRootContent();

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has an expiration date greater then today
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetContentForExpiration();

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has a release date greater then today
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetContentForRelease();

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetContentInRecycleBin();

        /// <summary>
        /// Re-Publishes all Content
        /// </summary>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        bool RePublishAll(int userId = -1);

        /// <summary>
        /// Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        bool Publish(IContent content, int userId = -1);

        /// <summary>
        /// Publishes a <see cref="IContent"/> object and all its children
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish along with its children</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        bool PublishWithChildren(IContent content, int userId = -1);

        /// <summary>
        /// UnPublishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if unpublishing succeeded, otherwise False</returns>
        bool UnPublish(IContent content, int userId = -1);

        /// <summary>
        /// Saves and Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save and publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        bool SaveAndPublish(IContent content, int userId = -1);

        /// <summary>
        /// Saves a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        void Save(IContent content, int userId = -1);

        /// <summary>
        /// Saves a collection of <see cref="IContent"/> objects
        /// </summary>
        /// <param name="contents">Collection of <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        void Save(IEnumerable<IContent> contents, int userId = -1);

        /// <summary>
        /// Deletes all content of specified type. All children of deleted content is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="contentTypeId">Id of the <see cref="IContentType"/></param>
        void DeleteContentOfType(int contentTypeId);

        /// <summary>
        /// Permanently deletes an <see cref="IContent"/> object
        /// </summary>
        /// <remarks>Please note that this method will completely remove the Content from the database</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        void Delete(IContent content, int userId = -1);

        /// <summary>
        /// Deletes an <see cref="IContent"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <remarks>Move an item to the Recycle Bin will result in the item being unpublished</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        void MoveToRecycleBin(IContent content, int userId = -1);

        /// <summary>
        /// Moves an <see cref="IContent"/> object to a new location
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to move</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="userId">Optional Id of the User moving the Content</param>
        void Move(IContent content, int parentId, int userId = -1);

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IContent"/> that resides in the bin
        /// </summary>
        void EmptyRecycleBin();

        /// <summary>
        /// Copies an <see cref="IContent"/> object by creating a new Content object of the same type and copies all data from the current 
        /// to the new copy which is returned.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to copy</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="userId">Optional Id of the User copying the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        IContent Copy(IContent content, int parentId, int userId = -1);

        /// <summary>
        /// Sends an <see cref="IContent"/> to Publication, which executes handlers and events for the 'Send to Publication' action.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to send to publication</param>
        /// <param name="userId">Optional Id of the User issueing the send to publication</param>
        /// <returns>True if sending publication was succesfull otherwise false</returns>
        bool SendToPublication(IContent content, int userId = -1);

        /// <summary>
        /// Rollback an <see cref="IContent"/> object to a previous version.
        /// This will create a new version, which is a copy of all the old data.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/>being rolled back</param>
        /// <param name="versionId">Id of the version to rollback to</param>
        /// <param name="userId">Optional Id of the User issueing the rollback of the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        IContent Rollback(int id, Guid versionId, int userId = -1);
    }
}