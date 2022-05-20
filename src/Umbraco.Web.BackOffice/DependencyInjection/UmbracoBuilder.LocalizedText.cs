using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the Umbraco back office
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds the supplementary localized texxt file sources from the various physical and virtual locations supported.
    /// </summary>
    private static IUmbracoBuilder AddSupplemenataryLocalizedTextFileSources(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient(sp =>
        {
            return GetSupplementaryFileSources(
                sp.GetRequiredService<IWebHostEnvironment>());
        });

        return builder;
    }


    /// <summary>
    ///     Loads the suplimentary localization files from plugins and user config
    /// </summary>
    private static IEnumerable<LocalizedTextServiceSupplementaryFileSource> GetSupplementaryFileSources(
        IWebHostEnvironment webHostEnvironment)
    {
        IFileProvider webFileProvider = webHostEnvironment.WebRootFileProvider;
        IFileProvider contentFileProvider = webHostEnvironment.ContentRootFileProvider;

        // gets all langs files in /app_plugins real or virtual locations
        IEnumerable<LocalizedTextServiceSupplementaryFileSource> pluginLangFileSources =
            GetPluginLanguageFileSources(webFileProvider, Constants.SystemDirectories.AppPlugins, false);

        // user defined langs that overwrite the default, these should not be used by plugin creators
        var userConfigLangFolder = Constants.SystemDirectories.Config
            .TrimStart(Constants.CharArrays.Tilde);

        IEnumerable<LocalizedTextServiceSupplementaryFileSource> userLangFileSources = contentFileProvider
            .GetDirectoryContents(userConfigLangFolder)
            .Where(x => x.IsDirectory && x.Name.InvariantEquals("lang"))
            .Select(x => new DirectoryInfo(x.PhysicalPath))
            .SelectMany(x => x.GetFiles("*.user.xml", SearchOption.TopDirectoryOnly))
            .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, true));

        return pluginLangFileSources
            .Concat(userLangFileSources);
    }


    /// <summary>
    ///     Loads the suplimentary localaization files via the file provider.
    /// </summary>
    /// <remarks>
    ///     locates all *.xml files in the lang folder of any sub folder of the one provided.
    ///     e.g /app_plugins/plugin-name/lang/*.xml
    /// </remarks>
    private static IEnumerable<LocalizedTextServiceSupplementaryFileSource> GetPluginLanguageFileSources(
        IFileProvider fileProvider, string folder, bool overwriteCoreKeys)
    {
        // locate all the *.xml files inside Lang folders inside folders of the main folder
        // e.g. /app_plugins/plugin-name/lang/*.xml
        var fileSources = new List<LocalizedTextServiceSupplementaryFileSource>();

        var pluginFolders = fileProvider.GetDirectoryContents(folder)
            .Where(x => x.IsDirectory).ToList();

        foreach (IFileInfo pluginFolder in pluginFolders)
        {
            // get the full virtual path for the plugin folder
            var pluginFolderPath = WebPath.Combine(folder, pluginFolder.Name);

            // get any lang folders in this plugin
            IEnumerable<IFileInfo> langFolders = fileProvider.GetDirectoryContents(pluginFolderPath)
                .Where(x => x.IsDirectory && x.Name.InvariantEquals("lang"));

            // loop through the lang folder(s)
            //  - there could be multiple on case sensitive file system
            foreach (IFileInfo langFolder in langFolders)
            {
                // get the full 'virtual' path of the lang folder
                var langFolderPath = WebPath.Combine(pluginFolderPath, langFolder.Name);

                // request all the files out of the path, these will have physicalPath set.
                var files = fileProvider.GetDirectoryContents(langFolderPath)
                    .Where(x => x.Name.InvariantEndsWith(".xml") && !string.IsNullOrEmpty(x.PhysicalPath))
                    .Select(x => new FileInfo(x.PhysicalPath))
                    .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, overwriteCoreKeys))
                    .ToList();

                // add any to our results
                if (files.Count > 0)
                {
                    fileSources.AddRange(files);
                }
            }
        }

        return fileSources;
    }
}
