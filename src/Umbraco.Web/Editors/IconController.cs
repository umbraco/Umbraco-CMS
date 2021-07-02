using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [IsBackOffice]
    [UmbracoWebApiRequireHttps]
    [UnhandedExceptionLoggerConfiguration]
    [EnableDetailedErrors]
    public class IconController : UmbracoApiController
    {
        private readonly IIconService _iconService;

        public IconController(IIconService iconService)
        {
            _iconService = iconService;
        }

        /// <summary>
        /// Gets an IconModel containing the icon name and SvgString according to an icon name found at the global icons path
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        public IconModel GetIcon(string iconName)
        {
            return _iconService.GetIcon(iconName);
        }

        /// <summary>
        /// Gets a list of all svg icons found at at the global icons path.
        /// </summary>
        /// <returns></returns>
        [Obsolete("This method should not be used - use GetIcons instead")]
        public IList<IconModel> GetAllIcons()
        {
            return _iconService.GetAllIcons();
        }

        /// <summary>
        /// Gets a list of all svg icons found at at the global icons path.
        /// </summary>
        /// <returns></returns>
        public JsonNetResult GetIcons()
        {
            return new JsonNetResult(JsonNetResult.DefaultJsonSerializerSettings) {
                Data = _iconService.GetIcons(),
                Formatting = Formatting.None
            };
        }
    }
}
