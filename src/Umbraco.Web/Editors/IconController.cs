﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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
        public IList<IconModel> GetAllIcons()
        {
            return _iconService.GetAllIcons();
        }

        /// <summary>
        /// Gets JSON blob containing all the known icons
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public JsonNetResult GetIcons()
        {
            var dictionary = _iconService.GetAllIcons()
                .ToDictionary(i => i.Name, i => i.SvgString);      

            return new JsonNetResult { Data = dictionary, Formatting = Formatting.None };
        }
    }
}
