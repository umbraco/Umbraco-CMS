using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class IconController : UmbracoAuthorizedApiController
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
        /// Gets a list of all svg icons found at the global icons path.
        /// </summary>
        /// <returns></returns>
        public IList<IconModel> GetAllIcons()
        {
            return _iconService.GetAllIcons();
        }

        /// <summary>
        /// Gets a paged result of svg icons found at the global icons path.
        /// </summary>
        /// <returns></returns>
        public PagedResult<IconModel> GetPagedIcons(
            int pageNumber = 1,
            int pageSize = 100,
            string filter = "")
        {
            if (pageSize <= 0 || pageNumber <= 0)
            {
                return new PagedResult<IconModel>(0, pageNumber, pageSize);
            }

            var icons = _iconService.GetPagedIcons(pageNumber - 1, pageSize, out long totalRecords, filter);

            return new PagedResult<IconModel>(totalRecords, pageNumber, pageSize)
            {
                Items = icons
            };
        }
    }
}
