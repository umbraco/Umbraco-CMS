using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Extensions;

public static class GlobalSettingsExtensions
{
    private static string? _mvcArea;
    private static string? _backOfficePath;

    /// <summary>
    ///     Returns the absolute path for the Umbraco back office
    /// </summary>
    /// <param name="globalSettings"></param>
    /// <param name="hostingEnvironment"></param>
    /// <returns></returns>
    public static string GetBackOfficePath(this GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
    {
        if (_backOfficePath != null)
        {
            return _backOfficePath;
        }

        _backOfficePath = hostingEnvironment.ToAbsolute(globalSettings.UmbracoPath);
        return _backOfficePath;
    }

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
    public static string GetUmbracoMvcArea(this GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
    {
        if (_mvcArea != null)
        {
            return _mvcArea;
        }

        _mvcArea = globalSettings.GetUmbracoMvcAreaNoCache(hostingEnvironment);

        return _mvcArea;
    }

    internal static string GetUmbracoMvcAreaNoCache(
        this GlobalSettings globalSettings,
        IHostingEnvironment hostingEnvironment)
    {
        var path = string.IsNullOrEmpty(globalSettings.UmbracoPath)
            ? string.Empty
            : hostingEnvironment.ToAbsolute(globalSettings.UmbracoPath);

        if (path.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("Cannot create an MVC Area path without the umbracoPath specified");
        }

        // beware of TrimStart, see U4-2518
        if (path.StartsWith(hostingEnvironment.ApplicationVirtualPath))
        {
            path = path[hostingEnvironment.ApplicationVirtualPath.Length..];
        }

        return path.TrimStart(Constants.CharArrays.Tilde).TrimStart(Constants.CharArrays.ForwardSlash).Replace('/', '-')
            .Trim().ToLower();
    }
}
