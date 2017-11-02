using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{

    public interface IContentRepository : IRepositoryVersionable<int, IContent>, IRecycleBinRepository<IContent>, IReadRepository<Guid, IContent>, IDeleteMediaFilesRepository
    {
        /// <summary>
        /// This builds the Xml document used for the XML cache
        /// </summary>
        /// <returns></returns>
        XmlDocument BuildXmlCache();

        /// <summary>
        /// Get the count of published items
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We require this on the repo because the IQuery{IContent} cannot supply the 'newest' parameter
        /// </remarks>
        int CountPublished(string contentTypeAlias = null);

        /// <summary>
        /// Used to bulk update the permissions set for a content item. This will replace all permissions
        /// assigned to an entity with a list of user id & permission pairs.
        /// </summary>
        /// <param name="permissionSet"></param>
        void ReplaceContentPermissions(EntityPermissionSet permissionSet);

        /// <summary>
        /// Clears the published flag for a content.
        /// </summary>
        /// <param name="content"></param>
        void ClearPublished(IContent content);

        /// <summary>
        /// Gets all published Content by the specified query
        /// </summary>
        /// <param name="query">Query to execute against published versions</param>
        /// <returns>An enumerable list of <see cref="IContent"/></returns>
        IEnumerable<IContent> GetByPublishedVersion(IQuery<IContent> query);

        /// <summary>
        /// Assigns a single permission to the current content item for the specified user group ids
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permission"></param>
        /// <param name="groupIds"></param>
        void AssignEntityPermission(IContent entity, char permission, IEnumerable<int> groupIds);

        /// <summary>
        /// Gets the explicit list of permissions for the content item
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        EntityPermissionCollection GetPermissionsForEntity(int entityId);

        ///// <summary>
        ///// Gets the implicit/inherited list of permissions for the content item
        ///// </summary>
        ///// <param name="path"></param>
        ///// <returns></returns>
        //IEnumerable<EntityPermission> GetPermissionsForPath(string path);

        /// <summary>
        /// Used to add/update published xml for the content item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        void AddOrUpdateContentXml(IContent content, Func<IContent, XElement> xml);

        /// <summary>
        /// Used to add/update a permission for a content item
        /// </summary>
        /// <param name="permission"></param>
        void AddOrUpdatePermissions(ContentPermissionSet permission);

        /// <summary>
        /// Used to remove the content xml for a content item
        /// </summary>
        /// <param name="content"></param>
        void DeleteContentXml(IContent content);

        /// <summary>
        /// Used to add/update preview xml for the content item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        void AddOrUpdatePreviewXml(IContent content, Func<IContent, XElement> xml);
    }
}