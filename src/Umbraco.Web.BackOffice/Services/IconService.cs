using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ganss.XSS;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using File = System.IO.File;

namespace Umbraco.Web.BackOffice.Services
{
    public class IconService : IIconService
    {
        private readonly IOptions<GlobalSettings> _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

        public IconService(IOptions<GlobalSettings> globalSettings, IHostingEnvironment hostingEnvironment)
        {
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
        }


        /// <inheritdoc />
        public IList<IconModel> GetAllIcons()
        {
            var icons = new List<IconModel>();
            var directory = new DirectoryInfo(_hostingEnvironment.MapPathWebRoot($"{_globalSettings.Value.IconsPath}/"));
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

        /// <inheritdoc />
        public IconModel GetIcon(string iconName)
        {
            return string.IsNullOrWhiteSpace(iconName)
                ? null
                : CreateIconModel(iconName.StripFileExtension(), _hostingEnvironment.MapPathWebRoot($"{_globalSettings.Value.IconsPath}/{iconName}.svg"));
        }

        /// <summary>
        /// Gets an IconModel using values from a FileInfo model
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        private IconModel GetIcon(FileInfo fileInfo)
        {
            return fileInfo == null || string.IsNullOrWhiteSpace(fileInfo.Name)
                ? null
                : CreateIconModel(fileInfo.Name.StripFileExtension(), fileInfo.FullName);
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
                var svgContent = File.ReadAllText(iconPath);
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
