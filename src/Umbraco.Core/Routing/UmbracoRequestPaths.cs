using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Utility for checking paths
/// </summary>
public class UmbracoRequestPaths
{
    private readonly string _apiMvcPath;
    private readonly string _appPath;
    private readonly string _backOfficeMvcPath;
    private readonly string _backOfficePath;
    private readonly List<string> _defaultUmbPaths;
    private readonly string _installPath;
    private readonly string _mvcArea;
    private readonly string _previewMvcPath;
    private readonly string _surfaceMvcPath;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoRequestPaths" /> class.
    /// </summary>
    public UmbracoRequestPaths(IOptions<GlobalSettings> globalSettings, IHostingEnvironment hostingEnvironment)
    {
        var applicationPath = hostingEnvironment.ApplicationVirtualPath;
        _appPath = applicationPath.TrimStart(Constants.CharArrays.ForwardSlash);

        _backOfficePath = globalSettings.Value.GetBackOfficePath(hostingEnvironment)
            .EnsureStartsWith('/').TrimStart(_appPath.EnsureStartsWith('/')).EnsureStartsWith('/');

        _mvcArea = globalSettings.Value.GetUmbracoMvcArea(hostingEnvironment);
        _defaultUmbPaths = new List<string> { "/" + _mvcArea, "/" + _mvcArea + "/" };
        _backOfficeMvcPath = "/" + _mvcArea + "/BackOffice/";
        _previewMvcPath = "/" + _mvcArea + "/Preview/";
        _surfaceMvcPath = "/" + _mvcArea + "/Surface/";
        _apiMvcPath = "/" + _mvcArea + "/Api/";
        _installPath = hostingEnvironment.ToAbsolute(Constants.SystemDirectories.Install);
    }

    /// <summary>
    ///     Checks if the current uri is a back office request
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         There are some special routes we need to check to properly determine this:
    ///     </para>
    ///     <para>
    ///         These are def back office:
    ///         /Umbraco/BackOffice     = back office
    ///         /Umbraco/Preview        = back office
    ///     </para>
    ///     <para>
    ///         If it's not any of the above then we cannot determine if it's back office or front-end
    ///         so we can only assume that it is not back office. This will occur if people use an UmbracoApiController for the
    ///         backoffice
    ///         but do not inherit from UmbracoAuthorizedApiController and do not use [IsBackOffice] attribute.
    ///     </para>
    ///     <para>
    ///         These are def front-end:
    ///         /Umbraco/Surface        = front-end
    ///         /Umbraco/Api            = front-end
    ///         But if we've got this far we'll just have to assume it's front-end anyways.
    ///     </para>
    /// </remarks>
    public bool IsBackOfficeRequest(string absPath)
    {
        var fullUrlPath = absPath.TrimStart(Constants.CharArrays.ForwardSlash);
        var urlPath = fullUrlPath.TrimStart(_appPath).EnsureStartsWith('/');

        // check if this is in the umbraco back office
        var isUmbracoPath = urlPath.InvariantStartsWith(_backOfficePath);

        // if not, then def not back office
        if (isUmbracoPath == false)
        {
            return false;
        }

        // if its the normal /umbraco path
        if (_defaultUmbPaths.Any(x => urlPath.InvariantEquals(x)))
        {
            return true;
        }

        // check for special back office paths
        if (urlPath.InvariantStartsWith(_backOfficeMvcPath)
            || urlPath.InvariantStartsWith(_previewMvcPath))
        {
            return true;
        }

        // check for special front-end paths
        if (urlPath.InvariantStartsWith(_surfaceMvcPath)
            || urlPath.InvariantStartsWith(_apiMvcPath))
        {
            return false;
        }

        // if its none of the above, we will have to try to detect if it's a PluginController route, we can detect this by
        // checking how many parts the route has, for example, all PluginController routes will be routed like
        // Umbraco/MYPLUGINAREA/MYCONTROLLERNAME/{action}/{id}
        // so if the path contains at a minimum 3 parts: Umbraco + MYPLUGINAREA + MYCONTROLLERNAME then we will have to assume it is a
        // plugin controller for the front-end.
        if (urlPath.Split(Constants.CharArrays.ForwardSlash, StringSplitOptions.RemoveEmptyEntries).Length >= 3)
        {
            return false;
        }

        // if its anything else we can assume it's back office
        return true;
    }

    /// <summary>
    ///     Checks if the current uri is an install request
    /// </summary>
    public bool IsInstallerRequest(string absPath) => absPath.InvariantStartsWith(_installPath);

    /// <summary>
    ///     Rudimentary check to see if it's not a server side request
    /// </summary>
    public bool IsClientSideRequest(string absPath)
    {
        var ext = Path.GetExtension(absPath);
        return !ext.IsNullOrWhiteSpace();
    }
}
