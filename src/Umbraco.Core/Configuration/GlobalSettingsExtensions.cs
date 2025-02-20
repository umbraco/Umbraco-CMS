using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Extensions;

public static class GlobalSettingsExtensions
{
    /// <summary>
    ///     Returns the absolute path for the Umbraco back office
    /// </summary>
    /// <param name="globalSettings"></param>
    /// <param name="hostingEnvironment"></param>
    /// <returns></returns>
    [Obsolete("The UmbracoPath setting is removed, use IHostingEnvironment.GetBackOfficePath() instead. Scheduled for removal in Umbraco 17.")]
    public static string GetBackOfficePath(this GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
        => hostingEnvironment.GetBackOfficePath();

    /// <summary>
    ///     This returns the string of the MVC Area route.
    /// </summary>
    /// <remarks>
    ///     This will return the MVC area that we will route all custom routes through like surface controllers, etc...
    ///     We will use the 'Path' (default ~/umbraco) to create it but since it cannot contain '/' and people may specify a
    ///     path of ~/asdf/asdf/admin
    ///     we will convert the '/' to '-' and use that as the path. its a bit lame but will work.
    ///     We also make sure that the virtual directory (SystemDirectories.Root) is stripped off first, otherwise we'd end up
    ///     with something
    ///     like "MyVirtualDirectory-Umbraco" instead of just "Umbraco".
    /// </remarks>
    [Obsolete("The UmbracoPath setting is removed, use Constants.System.UmbracoPathSegment as area name instead. Scheduled for removal in Umbraco 17.")]
    public static string GetUmbracoMvcArea(this GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
        => Constants.System.UmbracoPathSegment;
}
