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
            return HasPathAccess(content.Path, user.StartContentId, Constants.System.RecycleBinContent);
        }

        internal static bool HasPathAccess(string path, int startNodeId, int recycleBinId)
        {
            Mandate.ParameterNotNullOrEmpty(path, "path");

            var formattedPath = "," + path + ",";
            var formattedStartNodeId = "," + startNodeId.ToInvariantString() + ",";
            var formattedRecycleBinId = "," + recycleBinId.ToInvariantString() + ",";

            //only users with root access have access to the recycle bin
            if (formattedPath.Contains(formattedRecycleBinId))
            {
                return startNodeId == Constants.System.Root;
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
            return HasPathAccess(media.Path, user.StartMediaId, Constants.System.RecycleBinMedia);
        }
    }
}