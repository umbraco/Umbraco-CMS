using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the Umbraco back office
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {

        /// <summary>
        /// Adds the supplementary localized texxt file sources from the various physical and virtual locations supported.
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
        ///  Loads the suplimentary localization files from plugins and user config
        /// </summary>
        private static IEnumerable<LocalizedTextServiceSupplementaryFileSource> GetSupplementaryFileSources(
            IWebHostEnvironment webHostEnvironment)
        {
            IFileProvider webFileProvider = webHostEnvironment.WebRootFileProvider;
            IFileProvider contentFileProvider = webHostEnvironment.ContentRootFileProvider;

            IEnumerable<LocalizedTextServiceSupplementaryFileSource> localPluginFileSources = GetPluginLanguageFileSources(contentFileProvider, Cms.Core.Constants.SystemDirectories.AppPlugins, false);

            // gets all langs files in /app_plugins real or virtual locations
            IEnumerable<LocalizedTextServiceSupplementaryFileSource> pluginLangFileSources = GetPluginLanguageFileSources(webFileProvider, Cms.Core.Constants.SystemDirectories.AppPlugins, false);

            // user defined langs that overwrite the default, these should not be used by plugin creators
            var userConfigLangFolder = Cms.Core.Constants.SystemDirectories.Config
                                            .TrimStart(Cms.Core.Constants.CharArrays.Tilde);

            IEnumerable<LocalizedTextServiceSupplementaryFileSource> userLangFileSources = contentFileProvider.GetDirectoryContents(userConfigLangFolder)
                    .Where(x => x.IsDirectory && x.Name.InvariantEquals("lang"))
                    .Select(x => new DirectoryInfo(x.PhysicalPath))
                    .SelectMany(x => x.GetFiles("*.user.xml", SearchOption.TopDirectoryOnly))
                    .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, true));

            return
                localPluginFileSources
                .Concat(pluginLangFileSources)
                .Concat(userLangFileSources);
        }


        /// <summary>
        ///  Loads the suplimentary localaization files via the file provider.
        /// </summary>
        /// <remarks>
        ///  locates all *.xml files in the lang folder of any sub folder of the one provided.
        ///  e.g /app_plugins/plugin-name/lang/*.xml
        /// </remarks>
        private static IEnumerable<LocalizedTextServiceSupplementaryFileSource> GetPluginLanguageFileSources(
            IFileProvider fileProvider, string folder, bool overwriteCoreKeys)
        {
            IEnumerable<IFileInfo> pluginFolders = fileProvider
                .GetDirectoryContents(folder)
                .Where(x => x.IsDirectory);

            foreach (IFileInfo pluginFolder in pluginFolders)
            {
                // get the full virtual path for the plugin folder
                var pluginFolderPath = WebPath.Combine(folder, pluginFolder.Name);

                // loop through the lang folder(s)
                //  - there could be multiple on case sensitive file system
                foreach (var langFolder in GetLangFolderPaths(fileProvider, pluginFolderPath))
                {
                    // request all the files out of the path, these will have physicalPath set.
                    IEnumerable<FileInfo> localizationFiles = fileProvider
                        .GetDirectoryContents(langFolder)
                        .Where(x => !string.IsNullOrEmpty(x.PhysicalPath))
                        .Where(x => x.Name.InvariantEndsWith(".xml"))
                        .Select(x => new FileInfo(x.PhysicalPath));

                    foreach (FileInfo file in localizationFiles)
                    {
                        yield return new LocalizedTextServiceSupplementaryFileSource(file, overwriteCoreKeys);
                    }
                }
            }
        }

        private static IEnumerable<string> GetLangFolderPaths(IFileProvider fileProvider, string path)
        {
            IEnumerable<IFileInfo> directories = fileProvider.GetDirectoryContents(path).Where(x => x.IsDirectory);

            foreach (IFileInfo directory in directories)
            {
                var virtualPath = WebPath.Combine(path, directory.Name);

                if (directory.Name.InvariantEquals("lang"))
                {
                    yield return virtualPath;
                }

                foreach (var nested in GetLangFolderPaths(fileProvider, virtualPath))
                {
                    yield return nested;
                }
            }
        }
    }
}
