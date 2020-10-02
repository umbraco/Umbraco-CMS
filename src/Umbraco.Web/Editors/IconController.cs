using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class IconController : UmbracoAuthorizedApiController
    {
        private readonly IIconService _iconService;

        public IconController(
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IRuntimeState runtimeState,
            UmbracoMapper umbracoMapper,
            IPublishedUrlProvider publishedUrlProvider,
            IIconService iconService) : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoMapper, publishedUrlProvider)
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
    }
}
