using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Core.Logging;
using Umbraco.Web.Models;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;
using Ganss.XSS;

namespace Umbraco.Web.Editors
{
    // TODO: I'm not sure how to correctly set up the authentication so that it only works for people logged into the backoffice. Can this be verified please.

    [PluginController("UmbracoApi")]
    [IsBackOffice]
    [WebApi.UmbracoAuthorize]
    public class IconController : UmbracoApiController
    {

        /// <summary>
        /// Gets an IconModel containing the icon name and SvgString according to an icon name found at the global icons path
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        public IconModel GetIcon(string iconName)
        {
            if (string.IsNullOrWhiteSpace(iconName) == false)
            {
                return CreateIconModel(iconName.StripFileExtension(), IOHelper.MapPath($"{GlobalSettings.Path}/assets/icons/{iconName}.svg"));
            }

            return null;
        }

        /// <summary>
        /// Gets an IconModel using values from a FileInfo model
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        public IconModel GetIcon(FileInfo fileInfo)
        {
            if (fileInfo != null && string.IsNullOrWhiteSpace(fileInfo.Name) == false)
            {
                return CreateIconModel(fileInfo.Name.StripFileExtension(), fileInfo.FullName);
            }

            return null;
        }

        /// <summary>
        /// Gets a list of all svg icons found at at the global icons path.
        /// </summary>
        /// <returns></returns>
        public List<IconModel> GetAllIcons()
        {
            var icons = new List<IconModel>();
            var directory = new DirectoryInfo(IOHelper.MapPath($"{GlobalSettings.Path}/assets/icons"));
            var iconNames = directory.GetFiles("*.svg");

            iconNames.ToList().ForEach(iconInfo =>
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
        /// <returns></returns>
        private IconModel CreateIconModel(string iconName, string iconPath)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.UnionWith(Core.Constants.SvgSanitizer.Attributes);
            sanitizer.AllowedCssProperties.UnionWith(Core.Constants.SvgSanitizer.Attributes);
            sanitizer.AllowedTags.UnionWith(Core.Constants.SvgSanitizer.Tags);

            try
            {
                //var svg = Util.Cache(content.Url, 60 * 60 * 24, delegate
                //{
                var svgContent = File.ReadAllText(iconPath);
                var sanitizedString = sanitizer.Sanitize(svgContent);

                var svg = new IconModel
                {
                    Name = iconName,
                    SvgString = sanitizedString
                };

                return svg;
                //});
            }
            catch (Exception ex)
            {
                Logger.Error<IconController>(ex, $"Could not load {iconName}.svg");
                return null;
            }
        }
    }
}
