using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.Models;
using System.IO;
using Umbraco.Core;
using Ganss.XSS;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Filters;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class IconController : UmbracoAuthorizedApiController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IGlobalSettings _globalSettings;

        public IconController(
            IHostingEnvironment hostingEnvironment,
            IGlobalSettings globalSettings)
        {
            _hostingEnvironment = hostingEnvironment;
            _globalSettings = globalSettings;
        }

        /// <summary>
        /// Gets an IconModel containing the icon name and SvgString according to an icon name found at the global icons path
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public IconModel GetIcon(string iconName)
        {
            return string.IsNullOrWhiteSpace(iconName)
                ? null
                : CreateIconModel(iconName.StripFileExtension(),
                    _hostingEnvironment.MapPathWebRoot($"{_globalSettings.IconsPath}/{iconName}.svg"));
        }

        /// <summary>
        /// Gets an IconModel using values from a FileInfo model
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public IconModel GetIcon(FileInfo fileInfo)
        {
            return fileInfo == null || string.IsNullOrWhiteSpace(fileInfo.Name)
                ? null
                : CreateIconModel(fileInfo.Name.StripFileExtension(), fileInfo.FullName);
        }

        /// <summary>
        /// Gets a list of all svg icons found at at the global icons path.
        /// </summary>
        /// <returns></returns>
        public List<IconModel> GetAllIcons()
        {
            var icons = new List<IconModel>();
            var directory = new DirectoryInfo(_hostingEnvironment.MapPathWebRoot($"{_globalSettings.IconsPath}/"));
            var iconNames = directory.GetFiles("*.svg");

            iconNames.OrderBy(f => f.Name).ToList().ForEach(iconInfo =>
            {
                var icon = GetIcon(iconInfo);

                if (icon != null)
                {
                    icons.Add(icon);
                }
            });

            return icons;
        }

        /// <summary>
        /// Gets an IconModel containing the icon name and SvgString
        /// </summary>
        /// <param name="iconName"></param>
        /// <param name="iconPath"></param>
        /// <returns></returns>
        private IconModel CreateIconModel(string iconName, string iconPath)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.UnionWith(Core.Constants.SvgSanitizer.Attributes);
            sanitizer.AllowedCssProperties.UnionWith(Core.Constants.SvgSanitizer.Attributes);
            sanitizer.AllowedTags.UnionWith(Core.Constants.SvgSanitizer.Tags);

            try
            {
                var svgContent = System.IO.File.ReadAllText(iconPath);
                var sanitizedString = sanitizer.Sanitize(svgContent);

                var svg = new IconModel
                {
                    Name = iconName,
                    SvgString = sanitizedString
                };

                return svg;
            }
            catch
            {
                return null;
            }
        }
    }
}
