using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Web.Common.DependencyInjection;
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
    private readonly string _defaultUmbPath;
    private readonly string _defaultUmbPathWithSlash;
    private readonly string _installPath;
    private readonly string _previewMvcPath;
    private readonly string _surfaceMvcPath;
    private readonly IOptions<UmbracoRequestPathsOptions> _umbracoRequestPathsOptions;

    [Obsolete("Use constructor that takes IOptions<UmbracoRequestPathsOptions> - Will be removed in Umbraco 13")]
    public UmbracoRequestPaths(IOptions<GlobalSettings> globalSettings, IHostingEnvironment hostingEnvironment)
        : this(globalSettings, hostingEnvironment, StaticServiceProvider.Instance.GetRequiredService<IOptions<UmbracoRequestPathsOptions>>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoRequestPaths" /> class.
    /// </summary>
    public UmbracoRequestPaths(IOptions<GlobalSettings> globalSettings, IHostingEnvironment hostingEnvironment, IOptions<UmbracoRequestPathsOptions> umbracoRequestPathsOptions)
    {
        _appPath = hostingEnvironment.ApplicationVirtualPath;

        _backOfficePath = globalSettings.Value.GetBackOfficePath(hostingEnvironment)
            .EnsureStartsWith('/').TrimStart(_appPath).EnsureStartsWith('/');

        string mvcArea = globalSettings.Value.GetUmbracoMvcArea(hostingEnvironment);

        _defaultUmbPath = "/" + mvcArea;
        _defaultUmbPathWithSlash = "/" + mvcArea + "/";
        _backOfficeMvcPath = "/" + mvcArea + "/BackOffice/";
        _previewMvcPath = "/" + mvcArea + "/Preview/";
        _surfaceMvcPath = "/" + mvcArea + "/Surface/";
        _apiMvcPath = "/" + mvcArea + "/Api/";
        _installPath = hostingEnvironment.ToAbsolute(Constants.SystemDirectories.Install);
        _umbracoRequestPathsOptions = umbracoRequestPathsOptions;
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
    ///         /Umbraco/Management/Api = back office
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
        string urlPath = absPath.TrimStart(_appPath).EnsureStartsWith('/');

        // check if this is in the umbraco back office
        if (!urlPath.InvariantStartsWith(_backOfficePath))
        {
            return false;
        }

        // if its the normal /umbraco path
        if (urlPath.InvariantEquals(_defaultUmbPath) || urlPath.InvariantEquals(_defaultUmbPathWithSlash))
        {
            return true;
        }

        // check for special back office paths
        if (urlPath.InvariantStartsWith(_backOfficeMvcPath) || urlPath.InvariantStartsWith(_previewMvcPath))
        {
            return true;
        }

        // check for special front-end paths
        if (urlPath.InvariantStartsWith(_surfaceMvcPath) || urlPath.InvariantStartsWith(_apiMvcPath))
        {
            return false;
        }

        if (_umbracoRequestPathsOptions.Value.IsBackOfficeRequest(urlPath))
        {
            return true;
        }

        // if its none of the above, we will have to try to detect if it's a PluginController route
        return !IsPluginControllerRoute(urlPath);
    }

    /// <summary>
    /// Checks if the path is from a PluginController route.
    /// </summary>
    private static bool IsPluginControllerRoute(string path)
    {
        // Detect this by checking how many parts the route has, for example, all PluginController routes will be routed like
        // Umbraco/MYPLUGINAREA/MYCONTROLLERNAME/{action}/{id}
        // so if the path contains at a minimum 3 parts: Umbraco + MYPLUGINAREA + MYCONTROLLERNAME then we will have to assume it is a plugin controller for the front-end.

        int count = 0;

        for (int i = 0; i < path.Length; i++)
        {
            char chr = path[i];

            if (chr == '/')
            {
                count++;
                continue;
            }

            // Check last char so we can properly determine the number of parts, e.g. /url/path/ has two parts, /url/path/test has three.
            if (count == 3)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Checks if the current uri is an install request
    /// </summary>
    public bool IsInstallerRequest(string absPath) =>
        absPath.InvariantEquals(_installPath)
        || absPath.InvariantStartsWith(_installPath.EnsureEndsWith('/'))
        || absPath.InvariantStartsWith(_installPath.EnsureEndsWith('?'));

    /// <summary>
    ///     Rudimentary check to see if it's not a server side request
    /// </summary>
    public bool IsClientSideRequest(string absPath)
    {
        var ext = Path.GetExtension(absPath);
        return !ext.IsNullOrWhiteSpace();
    }
}
