using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ganss.XSS;
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
        public IList<IconModel> GetAllIcons() => GetIconModels().Values.OrderBy(i => i.Name).ToList();

        /// <inheritdoc />
        public IconModel GetIcon(string iconName)
        {
            if (iconName.IsNullOrWhiteSpace())
            {
                return null;
            }

            var allIconModels = GetIconModels();
            return allIconModels.ContainsKey(iconName)
                ? allIconModels[iconName]
                : null;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FileInfo> GetAllIconNames()
        {
            // add icons from plugins
            var appPlugins = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.AppPlugins));
            var pluginIcons = new List<FileInfo>();

            // iterate sub directories of app plugins
            foreach (var dir in appPlugins.EnumerateDirectories())
            {
                var iconPath = dir.FullName + SystemDirectories.AppPluginIcons;
                if (Directory.Exists(iconPath))
                {
                    var dirIcons = new DirectoryInfo(iconPath).EnumerateFiles("*.svg", SearchOption.TopDirectoryOnly);
                    pluginIcons.AddRange(dirIcons);
                }
            }

            // add icons from IconsPath if not already added from plugins
            var directory = new DirectoryInfo(IOHelper.MapPath($"{_globalSettings.IconsPath}/"));
            var iconNames = directory.GetFiles("*.svg")
                .Where(x => pluginIcons.Any(i => i.Name == x.Name) == false)
                .ToList();

            iconNames.AddRange(pluginIcons);

            return iconNames;
        }

        private Dictionary<string, IconModel> GetIconModels() => _cache.GetCacheItem(
            "Umbraco.Web.Services.IconService::AllIconModels",
            () => GetAllIconNames()
                    .Select(GetIcon)
                    .Where(i => i != null)
                    .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase)
        );
    }
}
