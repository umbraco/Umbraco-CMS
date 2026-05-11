// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="IUser" /> interface.
/// </summary>
public static class UmbracoUserExtensions
{
    /// <summary>
    ///     Gets the permissions for a user on a specific content path.
    /// </summary>
    /// <param name="user">The user to get permissions for.</param>
    /// <param name="path">The content path to check permissions on.</param>
    /// <param name="userService">The user service.</param>
    /// <returns>An enumerable of permission strings.</returns>
    public static IEnumerable<string> GetPermissions(this IUser user, string path, IUserService userService) =>
        userService.GetPermissionsForPath(user, path).GetAllPermissions();

    /// <summary>
    ///     Determines whether the user has access to the specified section (application).
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="app">The section/application alias to check access for.</param>
    /// <returns><c>true</c> if the user has access to the section; otherwise, <c>false</c>.</returns>
    public static bool HasSectionAccess(this IUser user, string app)
    {
        IEnumerable<string> apps = user.AllowedSections;
        return apps.Any(uApp => uApp.InvariantEquals(app));
    }

    /// <summary>
    ///     Determines whether this user is the 'super' user.
    /// </summary>
    public static bool IsSuper(this IUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return user.Id == Constants.Security.SuperUserId;
    }

    /// <summary>
    ///     Determines whether this user belongs to the administrators group.
    /// </summary>
    /// <remarks>The 'super' user does not automatically belongs to the administrators group.</remarks>
    public static bool IsAdmin(this IUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return user.Groups != null && user.Groups.Any(x => x.Alias == Constants.Security.AdminGroupAlias);
    }

    /// <summary>
    ///     Returns the culture info associated with this user, based on the language they're assigned to in the back office
    /// </summary>
    /// <param name="user"></param>
    /// <param name="textService"></param>
    /// <param name="globalSettings"></param>
    /// <returns></returns>
    public static CultureInfo GetUserCulture(this IUser user, ILocalizedTextService textService, GlobalSettings globalSettings)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (textService == null)
        {
            throw new ArgumentNullException(nameof(textService));
        }

        return GetUserCulture(user.Language, textService, globalSettings);
    }

    /// <summary>
    ///     Gets the culture info for a specified language code.
    /// </summary>
    /// <param name="userLanguage">The user's language code.</param>
    /// <param name="textService">The localized text service.</param>
    /// <param name="globalSettings">The global settings.</param>
    /// <returns>The <see cref="CultureInfo" /> for the specified language, or the default UI language if not found.</returns>
    public static CultureInfo GetUserCulture(string? userLanguage, ILocalizedTextService textService, GlobalSettings globalSettings)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(userLanguage!.Replace("_", "-"));

            // TODO: This is a hack because we store the user language as 2 chars instead of the full culture
            // which is actually stored in the language files (which are also named with 2 chars!) so we need to attempt
            // to convert to a supported full culture
            CultureInfo result = textService.ConvertToSupportedCultureWithRegionCode(culture);
            return result;
        }
        catch (CultureNotFoundException)
        {
            // return the default one
            return CultureInfo.GetCultureInfo(globalSettings.DefaultUILanguage);
        }
    }
}
