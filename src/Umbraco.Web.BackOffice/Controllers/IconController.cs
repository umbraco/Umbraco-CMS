using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Common.Attributes;

namespace Umbraco.Web.BackOffice.Controllers
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
        [DetermineAmbiguousActionByPassingParameters]
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
    }
}
