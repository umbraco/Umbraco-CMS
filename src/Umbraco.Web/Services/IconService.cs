using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ganss.XSS;
using Microsoft.CodeAnalysis.Operations;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Services
{
    public class IconService : IIconService
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IAppPolicyCache _cache;

        public IconService(IGlobalSettings globalSettings, AppCaches appCaches)
        {
            _globalSettings = globalSettings;
            _cache = appCaches.RuntimeCache;
        }

        /// <inheritdoc />
        public IList<IconModel> GetAllIcons() => _cache.GetCacheItem<IList<IconModel>>("Umbraco.Web.Services.IconService::AllIconModels", () =>
        {
            var icons = new List<IconModel>();
            var iconNames = GetAllIconNames();

            iconNames.OrderBy(f => f.Name).ToList().ForEach(iconInfo =>
            {
                var icon = GetIcon(iconInfo);

                if (icon != null)
                {
                    icons.Add(icon);
                }
            });

            return icons;
        });

        /// <inheritdoc />
        public IconModel GetIcon(string iconName)
        {
            return string.IsNullOrWhiteSpace(iconName)
                ? null
                : CreateIconModel(iconName.StripFileExtension());
        }

        /// <summary>
        /// Gets an IconModel using values from a FileInfo model
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns><see cref="IconModel"/></returns>
        private IconModel GetIcon(FileSystemInfo fileInfo)
        {
            return fileInfo == null || string.IsNullOrWhiteSpace(fileInfo.Name)
                ? null
                : CreateIconModel(fileInfo.Name.StripFileExtension(), fileInfo.FullName);
        }

        /// <summary>
        /// Gets an IconModel containing the icon name and SvgString
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns><see cref="IconModel"/></returns>
        private IconModel CreateIconModel(string iconName)
        {
            if (string.IsNullOrWhiteSpace(iconName))
                return null;

            return GetAllIcons().FirstOrDefault(i => i.Name == iconName);
        }

        /// <summary>
        /// Gets an IconModel containing the icon name and SvgString
        /// </summary>
        /// <param name="iconName"></param>
        /// <param name="iconPath"></param>
        /// <returns><see cref="IconModel"/></returns>
        private static IconModel CreateIconModel(string iconName, string iconPath)
        {
            if (string.IsNullOrWhiteSpace(iconPath))
                return null;

            try
            {
                var sanitizer = new HtmlSanitizer();
                sanitizer.AllowedAttributes.UnionWith(Constants.SvgSanitizer.Attributes);
                sanitizer.AllowedCssProperties.UnionWith(Constants.SvgSanitizer.Attributes);
                sanitizer.AllowedTags.UnionWith(Constants.SvgSanitizer.Tags);

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

        private IEnumerable<FileInfo> GetAllIconNames()
        {
            // add icons from plugins
            var appPlugins = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.AppPlugins));
            var pluginIcons = appPlugins.Exists == false
                ? new List<FileInfo>()
                : appPlugins.GetDirectories()
                    // Find all directories in App_Plugins that are named "Icons" and get a list of SVGs from them
                    .SelectMany(x => x.GetDirectories("Icons", SearchOption.AllDirectories))
                    .SelectMany(x => x.GetFiles("*.svg", SearchOption.TopDirectoryOnly));

            // add icons from IconsPath if not already added from plugins
            var directory = new DirectoryInfo(IOHelper.MapPath($"{_globalSettings.IconsPath}/"));
            var iconNames = directory.GetFiles("*.svg")
                .Where(x => pluginIcons.Any(i => i.Name == x.Name) == false);

            iconNames = iconNames.Concat(pluginIcons).ToList();

            return iconNames;
        }
    }
}
