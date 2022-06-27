using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Extensions;

public static class FriendlyUrlHelperExtensions
{
    private static IUmbracoContext UmbracoContext =>
        StaticServiceProvider.Instance.GetRequiredService<IUmbracoContextAccessor>().GetRequiredUmbracoContext();

    private static IDataProtectionProvider DataProtectionProvider { get; } =
        StaticServiceProvider.Instance.GetRequiredService<IDataProtectionProvider>();

    /// <summary>
    ///     Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified
    ///     SurfaceController
    /// </summary>
    /// <param name="url"></param>
    /// <param name="action"></param>
    /// <param name="controllerName"></param>
    /// <returns></returns>
    public static string SurfaceAction(this IUrlHelper url, string action, string controllerName)
        => url.SurfaceAction(UmbracoContext, DataProtectionProvider, action, controllerName);

    /// <summary>
    ///     Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified
    ///     SurfaceController
    /// </summary>
    /// <param name="url"></param>
    /// <param name="action"></param>
    /// <param name="controllerName"></param>
    /// <param name="additionalRouteVals"></param>
    /// <returns></returns>
    public static string SurfaceAction(this IUrlHelper url, string action, string controllerName, object additionalRouteVals)
        => url.SurfaceAction(UmbracoContext, DataProtectionProvider, action, controllerName, additionalRouteVals);

    /// <summary>
    ///     Generates a URL based on the current Umbraco URL with a custom query string that will route to the specified
    ///     SurfaceController
    /// </summary>
    /// <param name="url"></param>
    /// <param name="action"></param>
    /// <param name="controllerName"></param>
    /// <param name="area"></param>
    /// <param name="additionalRouteVals"></param>
    /// <returns></returns>
    public static string SurfaceAction(this IUrlHelper url, string action, string controllerName, string area, object additionalRouteVals)
        => url.SurfaceAction(UmbracoContext, DataProtectionProvider, action, controllerName, area, additionalRouteVals);
}
