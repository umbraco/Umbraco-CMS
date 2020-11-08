using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ganss.XSS;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Services
{
    public class IconService : IIconService
    {
        private readonly IGlobalSettings _globalSettings;

        public IconService(IGlobalSettings globalSettings)
        {
            _globalSettings = globalSettings;
        }


        /// <inheritdoc />
        public IList<IconModel> GetAllIcons()
        {
            var icons = new List<IconModel>();
            var iconNames = GetIconNames();

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
        public IList<IconModel> GetPagedIcons(int pageNumber, int pageSize, string filter = "")
        {
            var icons = new List<IconModel>();
            var iconNames = GetIconNames();

            var filtered = !string.IsNullOrEmpty(filter)
                ? iconNames.Where(x => x.Name.InvariantContains(filter))
                : iconNames;

            filtered.OrderBy(f => f.Name).Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToList().ForEach(iconInfo =>
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
                : CreateIconModel(iconName.StripFileExtension(), IOHelper.MapPath($"{_globalSettings.IconsPath}/{iconName}.svg"));
        }

        /// <summary>
        /// Gets svg icon names from directory
        /// </summary>
        /// <returns></returns>
        private FileInfo[] GetIconNames()
        {
            var directory = new DirectoryInfo(IOHelper.MapPath($"{_globalSettings.IconsPath}/"));
            var iconNames = directory.GetFiles("*.svg");

            return iconNames;
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
            sanitizer.AllowedAttributes.UnionWith(Constants.SvgSanitizer.Attributes);
            sanitizer.AllowedCssProperties.UnionWith(Constants.SvgSanitizer.Attributes);
            sanitizer.AllowedTags.UnionWith(Constants.SvgSanitizer.Tags);

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
