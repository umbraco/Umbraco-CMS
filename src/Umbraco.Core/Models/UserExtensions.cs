using System;
using System.Globalization;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models
{
    internal static class UserExtensions
    {
        /// <summary>
        /// Checks if the user has access to the content item based on their start noe
        /// </summary>
        /// <param name="user"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        internal static bool HasPathAccess(this IUser user, IContent content)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (content == null) throw new ArgumentNullException("content");
            var formattedPath = "," + content.Path + ",";
            var formattedStartNodeId = "," + user.StartContentId.ToString(CultureInfo.InvariantCulture) + ",";
            var formattedRecycleBinId = "," + Constants.System.RecycleBinContent + ",";
            
            //only users with root access have access to the recycle bin
            if (formattedPath.Contains(formattedRecycleBinId))
            {
                return user.StartContentId == Constants.System.Root;
            }

            return formattedPath.Contains(formattedStartNodeId);
        }

        /// <summary>
        /// Checks if the user has access to the media item based on their start noe
        /// </summary>
        /// <param name="user"></param>
        /// <param name="media"></param>
        /// <returns></returns>
        internal static bool HasPathAccess(this IUser user, IMedia media)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (media == null) throw new ArgumentNullException("media");
            var formattedPath = "," + media.Path + ",";
            var formattedStartNodeId = "," + user.StartContentId.ToString(CultureInfo.InvariantCulture) + ",";
            var formattedRecycleBinId = "," + Constants.System.RecycleBinMedia + ",";

            //only users with root access have access to the recycle bin
            if (formattedPath.Contains(formattedRecycleBinId) && user.StartContentId == Constants.System.Root)
            {
                return true;
            }

            return formattedPath.Contains(formattedStartNodeId);
        }
    }
}