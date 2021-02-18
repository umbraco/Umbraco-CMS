using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Core.Security
{
    internal class ContentPermissionsHelper
    {
        public enum ContentAccess
        {
            Granted,
            Denied,
            NotFound
        }

        public static ContentAccess CheckPermissions(
            IContent content,
            IUser user,
            IUserService userService,
            IEntityService entityService,
            AppCaches appCaches,
            params char[] permissionsToCheck)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (userService == null) throw new ArgumentNullException("userService");
            if (entityService == null) throw new ArgumentNullException("entityService");

            if (content == null) return ContentAccess.NotFound;

            var hasPathAccess = user.HasPathAccess(content, entityService, appCaches);

            if (hasPathAccess == false)
                return ContentAccess.Denied;

            if (permissionsToCheck == null || permissionsToCheck.Length == 0)
                return ContentAccess.Granted;

            //get the implicit/inherited permissions for the user for this path
            return CheckPermissionsPath(content.Path, user, userService, permissionsToCheck)
                ? ContentAccess.Granted
                : ContentAccess.Denied;
        }

        public static ContentAccess CheckPermissions(
            IUmbracoEntity entity,
            IUser user,
            IUserService userService,
            IEntityService entityService,
            AppCaches appCaches,
            params char[] permissionsToCheck)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (userService == null) throw new ArgumentNullException("userService");
            if (entityService == null) throw new ArgumentNullException("entityService");

            if (entity == null) return ContentAccess.NotFound;

            var hasPathAccess = user.HasContentPathAccess(entity, entityService, appCaches);

            if (hasPathAccess == false)
                return ContentAccess.Denied;

            if (permissionsToCheck == null || permissionsToCheck.Length == 0)
                return ContentAccess.Granted;

            //get the implicit/inherited permissions for the user for this path
            return CheckPermissionsPath(entity.Path, user, userService, permissionsToCheck)
                ? ContentAccess.Granted
                : ContentAccess.Denied;
        }

        /// <summary>
        /// Checks if the user has access to the specified node and permissions set
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="user"></param>
        /// <param name="userService"></param>
        /// <param name="entityService"></param>
        /// <param name="entity">The <see cref="IUmbracoEntity"/> item resolved if one was found for the id</param>
        /// <param name="permissionsToCheck"></param>
        /// <returns></returns>
        public static ContentAccess CheckPermissions(
            int nodeId,
            IUser user,
            IUserService userService,
            IEntityService entityService,
            AppCaches appCaches,
            out IUmbracoEntity entity,
            params char[] permissionsToCheck)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (userService == null) throw new ArgumentNullException("userService");
            if (entityService == null) throw new ArgumentNullException("entityService");

            bool? hasPathAccess = null;
            entity = null;

            if (nodeId == Constants.System.Root)
                hasPathAccess = user.HasContentRootAccess(entityService, appCaches);
            else if (nodeId == Constants.System.RecycleBinContent)
                hasPathAccess = user.HasContentBinAccess(entityService, appCaches);

            if (hasPathAccess.HasValue)
                return hasPathAccess.Value ? ContentAccess.Granted : ContentAccess.Denied;

            entity = entityService.Get(nodeId, UmbracoObjectTypes.Document);
            if (entity == null) return ContentAccess.NotFound;
            hasPathAccess = user.HasContentPathAccess(entity, entityService, appCaches);

            if (hasPathAccess == false)
                return ContentAccess.Denied;

            if (permissionsToCheck == null || permissionsToCheck.Length == 0)
                return ContentAccess.Granted;

            //get the implicit/inherited permissions for the user for this path
            return CheckPermissionsPath(entity.Path, user, userService, permissionsToCheck)
                ? ContentAccess.Granted
                : ContentAccess.Denied;
        }

        /// <summary>
        /// Checks if the user has access to the specified node and permissions set
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="user"></param>
        /// <param name="userService"></param>
        /// <param name="contentService"></param>
        /// <param name="entityService"></param>
        /// <param name="contentItem">The <see cref="IContent"/> item resolved if one was found for the id</param>
        /// <param name="permissionsToCheck"></param>
        /// <returns></returns>
        public static ContentAccess CheckPermissions(
            int nodeId,
            IUser user,
            IUserService userService,
            IContentService contentService,
            IEntityService entityService,
            AppCaches appCaches,
            out IContent contentItem,
            params char[] permissionsToCheck)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (userService == null) throw new ArgumentNullException("userService");
            if (contentService == null) throw new ArgumentNullException("contentService");
            if (entityService == null) throw new ArgumentNullException("entityService");

            bool? hasPathAccess = null;
            contentItem = null;

            if (nodeId == Constants.System.Root)
                hasPathAccess = user.HasContentRootAccess(entityService, appCaches);
            else if (nodeId == Constants.System.RecycleBinContent)
                hasPathAccess = user.HasContentBinAccess(entityService, appCaches);

            if (hasPathAccess.HasValue)
                return hasPathAccess.Value ? ContentAccess.Granted : ContentAccess.Denied;

            contentItem = contentService.GetById(nodeId);
            if (contentItem == null) return ContentAccess.NotFound;
            hasPathAccess = user.HasPathAccess(contentItem, entityService, appCaches);

            if (hasPathAccess == false)
                return ContentAccess.Denied;

            if (permissionsToCheck == null || permissionsToCheck.Length == 0)
                return ContentAccess.Granted;

            //get the implicit/inherited permissions for the user for this path
            return CheckPermissionsPath(contentItem.Path, user, userService, permissionsToCheck)
                ? ContentAccess.Granted
                : ContentAccess.Denied;
        }

        private static bool CheckPermissionsPath(string path, IUser user, IUserService userService, params char[] permissionsToCheck)
        {
            //get the implicit/inherited permissions for the user for this path,
            //if there is no content item for this id, than just use the id as the path (i.e. -1 or -20)
            var permission = userService.GetPermissionsForPath(user, path);

            var allowed = true;
            foreach (var p in permissionsToCheck)
            {
                if (permission == null
                    || permission.GetAllPermissions().Contains(p.ToString(CultureInfo.InvariantCulture)) == false)
                {
                    allowed = false;
                }
            }
            return allowed;
        }

        public static bool HasPathAccess(string path, int[] startNodeIds, int recycleBinId)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));

            // check for no access
            if (startNodeIds.Length == 0)
                return false;

            // check for root access
            if (startNodeIds.Contains(Constants.System.Root))
                return true;

            var formattedPath = string.Concat(",", path, ",");

            // only users with root access have access to the recycle bin,
            // if the above check didn't pass then access is denied
            if (formattedPath.Contains(string.Concat(",", recycleBinId, ",")))
                return false;

            // check for a start node in the path
            return startNodeIds.Any(x => formattedPath.Contains(string.Concat(",", x, ",")));
        }

        internal static bool IsInBranchOfStartNode(string path, int[] startNodeIds, string[] startNodePaths, out bool hasPathAccess)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));

            hasPathAccess = false;

            // check for no access
            if (startNodeIds.Length == 0)
                return false;

            // check for root access
            if (startNodeIds.Contains(Constants.System.Root))
            {
                hasPathAccess = true;
                return true;
            }

            //is it self?
            var self = startNodePaths.Any(x => x == path);
            if (self)
            {
                hasPathAccess = true;
                return true;
            }

            //is it ancestor?
            var ancestor = startNodePaths.Any(x => x.StartsWith(path));
            if (ancestor)
            {
                //hasPathAccess = false;
                return true;
            }

            //is it descendant?
            var descendant = startNodePaths.Any(x => path.StartsWith(x));
            if (descendant)
            {
                hasPathAccess = true;
                return true;
            }

            return false;
        }
    }
}
