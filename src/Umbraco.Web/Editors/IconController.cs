using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Core.Logging;
using Umbraco.Web.Models;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace Umbraco.Web.Editors
{
    // TODO - I'm not sure how to correctly set up the authentication so that it only works for people logged into the backoffice 

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
                return _GetIcon(iconName.StripFileExtension(), IOHelper.MapPath($"{GlobalSettings.Path}/assets/icons/{iconName}.svg"));
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
                return _GetIcon(fileInfo.Name.StripFileExtension(), fileInfo.FullName);
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

                if(icon != null)
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
        private IconModel _GetIcon(string iconName, string iconPath)
        {
            // TODO - HtmlSanitizer is a package used for sanitizering HTML and can be extended to sanitize SVGs. This is commented out as I'm not sure if this will/should be used, and if i can be, how to get it set up in the project correctly.

            //var sanitizer = new HtmlSanitizer();
            //sanitizer.AllowedAttributes.UnionWith(Svg.Attributes);
            //sanitizer.AllowedTags.UnionWith(Svg.Tags);

            try
            {
                //var svg = Util.Cache(content.Url, 60 * 60 * 24, delegate
                //{
                var svgContent = File.ReadAllText(iconPath);
                //string sanitizedString = sanitizer.Sanitize(svgContent);

                var svg = new IconModel
                {
                    Name = iconName,
                    SvgString = svgContent
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
