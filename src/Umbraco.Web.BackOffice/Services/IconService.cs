using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using File = System.IO.File;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.BackOffice.Services;

public class IconService : IIconService
{
    private readonly IAppPolicyCache _cache;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private GlobalSettings _globalSettings;

    [Obsolete("Use other ctor - Will be removed in Umbraco 12")]
    public IconService(
        IOptionsMonitor<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment,
        AppCaches appCaches)
        : this(
            globalSettings,
            hostingEnvironment,
            appCaches,
            StaticServiceProvider.Instance.GetRequiredService<IWebHostEnvironment>())
    {
    }

    [Obsolete("Use other ctor - Will be removed in Umbraco 12")]
    public IconService(
        IOptionsMonitor<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment,
        AppCaches appCaches,
        IWebHostEnvironment webHostEnvironment)
    {
        _globalSettings = globalSettings.CurrentValue;
        _hostingEnvironment = hostingEnvironment;
        _webHostEnvironment = webHostEnvironment;
        _cache = appCaches.RuntimeCache;

        globalSettings.OnChange(x => _globalSettings = x);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string>? GetIcons() => GetIconDictionary();

    /// <inheritdoc />
    public IconModel? GetIcon(string iconName)
    {
        if (iconName.IsNullOrWhiteSpace())
        {
            return null;
        }

        IReadOnlyDictionary<string, string>? allIconModels = GetIconDictionary();
        if (allIconModels?.ContainsKey(iconName) ?? false)
        {
            return new IconModel { Name = iconName, SvgString = allIconModels[iconName] };
        }

        return null;
    }

    /// <summary>
    ///     Gets an IconModel using values from a FileInfo model
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    private IconModel? GetIcon(FileInfo fileInfo) =>
        fileInfo == null || string.IsNullOrWhiteSpace(fileInfo.Name)
            ? null
            : CreateIconModel(fileInfo.Name.StripFileExtension(), fileInfo.FullName);

    /// <summary>
    ///     Gets an IconModel containing the icon name and SvgString
    /// </summary>
    /// <param name="iconName"></param>
    /// <param name="iconPath"></param>
    /// <returns></returns>
    private IconModel? CreateIconModel(string iconName, string iconPath)
    {
        try
        {
            var svgContent = File.ReadAllText(iconPath);

            var svg = new IconModel { Name = iconName, SvgString = svgContent };

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
        var appPluginsDirectoryPath = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.AppPlugins);
        if (Directory.Exists(appPluginsDirectoryPath))
        {
            var appPlugins = new DirectoryInfo(appPluginsDirectoryPath);

            // iterate sub directories of app plugins
            foreach (DirectoryInfo dir in appPlugins.EnumerateDirectories())
            {
                // AppPluginIcons path was previoulsy the wrong case, so we first check for the prefered directory
                // and then check the legacy directory.
                var iconPath = _hostingEnvironment.MapPathContentRoot(
                    $"{Constants.SystemDirectories.AppPlugins}/{dir.Name}{Constants.SystemDirectories.PluginIcons}");
                var iconPathExists = Directory.Exists(iconPath);

                if (!iconPathExists)
                {
                    iconPath = _hostingEnvironment.MapPathContentRoot(
                        $"{Constants.SystemDirectories.AppPlugins}/{dir.Name}{Constants.SystemDirectories.AppPluginIcons}");
                    iconPathExists = Directory.Exists(iconPath);
                }

                if (iconPathExists)
                {
                    IEnumerable<FileInfo> dirIcons =
                        new DirectoryInfo(iconPath).EnumerateFiles("*.svg", SearchOption.TopDirectoryOnly);
                    icons.UnionWith(dirIcons);
                }
            }
        }

        // Get icons from the web root file provider (both physical and virtual)
        icons.UnionWith(GetIconsFiles(_webHostEnvironment.WebRootFileProvider, Constants.SystemDirectories.AppPlugins));

        IDirectoryContents? iconFolder =
            _webHostEnvironment.WebRootFileProvider.GetDirectoryContents(_globalSettings.IconsPath);

        IEnumerable<FileInfo> coreIcons = iconFolder
            .Where(x => !x.IsDirectory && x.Name.EndsWith(".svg"))
            .Select(x => new FileInfo(x.PhysicalPath));

        icons.UnionWith(coreIcons);

        return icons;
    }


    /// <summary>
    /// Finds all SVG icon files based on the specified <paramref name="fileProvider"/> and <paramref name="path"/>.
    /// The method will find both physical and virtual (eg. from a Razor Class Library) icons.
    /// </summary>
    /// <param name="fileProvider">The file provider to be used.</param>
    /// <param name="path">The sub path to start from - should probably always be <see cref="Constants.SystemDirectories.AppPlugins"/>.</param>
    /// <returns>A collection of <see cref="FileInfo"/> representing the found SVG icon files.</returns>
    private static IEnumerable<FileInfo> GetIconsFiles(IFileProvider fileProvider, string path)
    {
        // Iterate through all plugin folders, this is necessary because on Linux we'll get casing issues when
        // we directly try to access {path}/{pluginDirectory.Name}/{Constants.SystemDirectories.PluginIcons}
        foreach (IFileInfo pluginDirectory in fileProvider.GetDirectoryContents(path))
        {
            // Ideally there shouldn't be any files, but we'd better check to be sure
            if (!pluginDirectory.IsDirectory)
            {
                continue;
            }

            // Iterate through the sub directories of each plugin folder
            foreach (IFileInfo subDir1 in fileProvider.GetDirectoryContents($"{path}/{pluginDirectory.Name}"))
            {
                // Skip files again
                if (!subDir1.IsDirectory)
                {
                    continue;
                }

                // Iterate through second level sub directories
                foreach (IFileInfo subDir2 in fileProvider.GetDirectoryContents($"{path}/{pluginDirectory.Name}/{subDir1.Name}"))
                {
                    // Skip files again
                    if (!subDir2.IsDirectory)
                    {
                        continue;
                    }

                    // Does the directory match the plugin icons folder? (case insensitive for legacy support)
                    if (!$"/{subDir1.Name}/{subDir2.Name}".InvariantEquals(Constants.SystemDirectories.PluginIcons))
                    {
                        continue;
                    }

                    // Iterate though the files of the second level sub directory. This should be where the SVG files are located :D
                    foreach (IFileInfo file in fileProvider.GetDirectoryContents($"{path}/{pluginDirectory.Name}/{subDir1.Name}/{subDir2.Name}"))
                    {
                        if (file.Name.InvariantEndsWith(".svg"))
                        {
                            yield return new FileInfo(file.PhysicalPath);
                        }
                    }
                }
            }
        }
    }

    private IReadOnlyDictionary<string, string>? GetIconDictionary() => _cache.GetCacheItem(
        $"{typeof(IconService).FullName}.{nameof(GetIconDictionary)}",
        () => GetAllIconsFiles()
            .Select(GetIcon)
            .WhereNotNull()
            .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().SvgString, StringComparer.OrdinalIgnoreCase));

    private class CaseInsensitiveFileInfoComparer : IEqualityComparer<FileInfo>
    {
        public bool Equals(FileInfo? one, FileInfo? two) =>
            StringComparer.InvariantCultureIgnoreCase.Equals(one?.Name, two?.Name);

        public int GetHashCode(FileInfo item) => StringComparer.InvariantCultureIgnoreCase.GetHashCode(item.Name);
    }
}
