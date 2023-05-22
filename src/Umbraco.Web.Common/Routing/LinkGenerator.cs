using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Web.Common.Routing;

/// <summary>
/// Umbraco LinkGenerator. Generates Links with path, when no path is given.
/// </summary>
public class LinkGenerator
{
    private readonly Microsoft.AspNetCore.Routing.LinkGenerator _linkGenerator;
    private readonly string _pathBase;

    /// <summary>
    /// Initialize Umbraco LinkGenerator
    /// </summary>
    /// <param name="linkGenerator"></param>
    /// <param name="hostingEnvironment"></param>
    public LinkGenerator(Microsoft.AspNetCore.Routing.LinkGenerator linkGenerator, IHostingEnvironment hostingEnvironment)
    {
        _linkGenerator = linkGenerator;
        _pathBase = hostingEnvironment.ApplicationVirtualPath;
    }

    public string? GetPathByAddress<TAddress>(
        HttpContext httpContext,
        TAddress address,
        RouteValueDictionary values,
        RouteValueDictionary? ambientValues = null,
        PathString? pathBase = null,
        FragmentString fragment = new FragmentString(),
        LinkOptions? options = null)
    {
        pathBase ??= _pathBase;
        return _linkGenerator.GetPathByAddress(httpContext, address, values, ambientValues, pathBase, fragment, options);
    }

    public string? GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values,
        PathString pathBase = new PathString(), FragmentString fragment = new FragmentString(),
        LinkOptions? options = null)
    {
        pathBase = string.IsNullOrWhiteSpace(pathBase.Value) ? new PathString(_pathBase) : pathBase;
        return _linkGenerator.GetPathByAddress(address, values, pathBase, fragment, options);
    }

    public string? GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values,
        RouteValueDictionary? ambientValues = null, string? scheme = null, HostString? host = null,
        PathString? pathBase = null, FragmentString fragment = new FragmentString(), LinkOptions? options = null)
    {
        pathBase ??= _pathBase;
        return _linkGenerator.GetUriByAddress(httpContext, address, values, ambientValues, scheme, host, pathBase, fragment, options);
    }

    public string? GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string? scheme, HostString host,
        PathString pathBase = new PathString(), FragmentString fragment = new FragmentString(),
        LinkOptions? options = null)
    {
        pathBase = string.IsNullOrWhiteSpace(pathBase.Value) ? new PathString(_pathBase) : pathBase;
        return _linkGenerator.GetUriByAddress(address, values, scheme, host, pathBase, fragment, options);
    }

    public string? GetPathByAction(string action, string controllerName, object values) => _linkGenerator.GetPathByAction(action, controllerName, values, _pathBase);
}
