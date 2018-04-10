using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IDocumentRepository : IContentRepository<int, IContent>, IReadRepository<Guid, IContent>
    {
        /// <summary>
        /// Get the count of published items
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We require this on the repo because the IQuery{IContent} cannot supply the 'newest' parameter
        /// </remarks>
        int CountPublished(string contentTypeAlias = null);

        bool IsPathPublished(IContent content);

        /// <summary>
        /// Used to bulk update the permissions set for a content item. This will replace all permissions
        /// assigned to an entity with a list of user id & permission pairs.
        /// </summary>
        /// <param name="permissionSet"></param>
        void ReplaceContentPermissions(EntityPermissionSet permissionSet);

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
        
        /// <summary>
        /// Used to add/update a permission for a content item
        /// </summary>
        /// <param name="permission"></param>
        void AddOrUpdatePermissions(ContentPermissionSet permission);
    }
}
