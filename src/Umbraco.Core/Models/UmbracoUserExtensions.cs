﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    public static class UmbracoUserExtensions
    {
        public static IEnumerable<string> GetPermissions(this IUser user, string path, IUserService userService)
        {
            return userService.GetPermissionsForPath(user, path).GetAllPermissions();
        }

        public static bool HasSectionAccess(this IUser user, string app)
        {
            var apps = user.AllowedSections;
            return apps.Any(uApp => uApp.InvariantEquals(app));
        }

        /// <summary>
        /// Determines whether this user is the 'super' user.
        /// </summary>
        public static bool IsSuper(this IUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return user.Id == Constants.Security.SuperUserId;
        }

        /// <summary>
        /// Determines whether this user belongs to the administrators group.
        /// </summary>
        /// <remarks>The 'super' user does not automatically belongs to the administrators group.</remarks>
        public static bool IsAdmin(this IUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return user.Groups != null && user.Groups.Any(x => x.Alias == Constants.Security.AdminGroupAlias);
        }

        /// <summary>
        /// Returns the culture info associated with this user, based on the language they're assigned to in the back office
        /// </summary>
        /// <param name="user"></param>
        /// <param name="textService"></param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        public static CultureInfo GetUserCulture(this IUser user, ILocalizedTextService textService, GlobalSettings globalSettings)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (textService == null) throw new ArgumentNullException(nameof(textService));
            return GetUserCulture(user.Language, textService, globalSettings);
        }

        public static CultureInfo GetUserCulture(string userLanguage, ILocalizedTextService textService, GlobalSettings globalSettings)
        {
            try
            {
                var culture = CultureInfo.GetCultureInfo(userLanguage.Replace("_", "-"));
                // TODO: This is a hack because we store the user language as 2 chars instead of the full culture
                // which is actually stored in the language files (which are also named with 2 chars!) so we need to attempt
                // to convert to a supported full culture
                var result = textService.ConvertToSupportedCultureWithRegionCode(culture);
                return result;
            }
            catch (CultureNotFoundException)
            {
                //return the default one
                return CultureInfo.GetCultureInfo(globalSettings.DefaultUILanguage);
            }
        }
    }
}
