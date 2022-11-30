using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController("UmbracoApi")]
[IsBackOffice]
[UmbracoRequireHttps]
[MiddlewareFilter(typeof(UnhandledExceptionLoggerFilter))]
public class IconController : UmbracoApiController
{
    private readonly IIconService _iconService;

    public IconController(IIconService iconService) => _iconService = iconService;

    /// <summary>
    ///     Gets an IconModel containing the icon name and SvgString according to an icon name found at the global icons path
    /// </summary>
    /// <param name="iconName"></param>
    /// <returns></returns>
    public IconModel? GetIcon(string iconName) => _iconService.GetIcon(iconName);


    /// <summary>
    ///     Gets a list of all svg icons found at at the global icons path.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<string, string>? GetIcons() => _iconService.GetIcons();
}
