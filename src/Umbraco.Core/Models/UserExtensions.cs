using System;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    public static class UserExtensions
    {
        /// <summary>
        /// Returns the culture info associated with this user, based on the language they're assigned to in the back office
        /// </summary>
        /// <param name="user"></param>
        /// <param name="textService"></param>
        /// <returns></returns>      
        public static CultureInfo GetUserCulture(this IUser user, ILocalizedTextService textService)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (textService == null) throw new ArgumentNullException("textService");
            return GetUserCulture(user.Language, textService);
        }

        internal static CultureInfo GetUserCulture(string userLanguage, ILocalizedTextService textService)
        {
            return textService.GetSupportedCultures()
                .FirstOrDefault(culture =>
                    //match on full name first
                    culture.Name.InvariantEquals(userLanguage.Replace("_", "-")) ||
                        //then match on the 2 letter name
                    culture.TwoLetterISOLanguageName.InvariantEquals(userLanguage));
        } 

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