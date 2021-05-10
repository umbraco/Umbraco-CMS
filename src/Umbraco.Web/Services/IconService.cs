using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public IReadOnlyDictionary<string, string> GetIcons() => GetIconDictionary();

        /// <inheritdoc />
        public IList<IconModel> GetAllIcons() =>
            GetIconDictionary()
                .Select(x => new IconModel { Name = x.Key, SvgString = x.Value })
                .ToList();

        /// <inheritdoc />
        public IconModel GetIcon(string iconName)
        {
            if (iconName.IsNullOrWhiteSpace())
            {
                return null;
            }

            var allIconModels = GetIconDictionary();
            if (allIconModels.ContainsKey(iconName))
            {
                return new IconModel
                {
                    Name = iconName,
                    SvgString = allIconModels[iconName]
                };
            }

            return null;
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
            try
            {
                var svgContent = System.IO.File.ReadAllText(iconPath);

                var svg = new IconModel
                {
                    Name = iconName,
                    SvgString = svgContent
                };

                return svg;
            }
            catch
            {
                return null;
            }
        }

        private IEnumerable<FileInfo> GetAllIconsFiles()
        {
            var icons = new HashSet<FileInfo>(new CaseInsensitiveFileInfoComparer());

            // add icons from plugins
            var appPluginsDirectoryPath = IOHelper.MapPath(SystemDirectories.AppPlugins);
            if (Directory.Exists(appPluginsDirectoryPath))
            {
                var appPlugins = new DirectoryInfo(appPluginsDirectoryPath);

                // iterate sub directories of app plugins
                foreach (var dir in appPlugins.EnumerateDirectories())
                {
                    var iconPath = IOHelper.MapPath($"{SystemDirectories.AppPlugins}/{dir.Name}{SystemDirectories.AppPluginIcons}");
                    if (Directory.Exists(iconPath))
                    {
                        var dirIcons = new DirectoryInfo(iconPath).EnumerateFiles("*.svg", SearchOption.TopDirectoryOnly);
                        icons.UnionWith(dirIcons);
                    }
                }
            }

            // add icons from IconsPath if not already added from plugins
            var coreIconsDirectory = new DirectoryInfo(IOHelper.MapPath($"{_globalSettings.IconsPath}/"));
            var coreIcons = coreIconsDirectory.GetFiles("*.svg");

            icons.UnionWith(coreIcons);

            return icons;
        }

        private class CaseInsensitiveFileInfoComparer : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo one, FileInfo two) => StringComparer.InvariantCultureIgnoreCase.Equals(one.Name, two.Name);

            public int GetHashCode(FileInfo item) => StringComparer.InvariantCultureIgnoreCase.GetHashCode(item.Name);
        }

        private IReadOnlyDictionary<string, string> GetIconDictionary() => _cache.GetCacheItem(
            $"{typeof(IconService).FullName}.{nameof(GetIconDictionary)}",
            () => GetAllIconsFiles()
                .Select(GetIcon)
                .Where(i => i != null)
                .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().SvgString, StringComparer.OrdinalIgnoreCase)
        );
    }
}
