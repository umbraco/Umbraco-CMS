using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        ///  Add the SupplementaryLocalizedTextFilesSources 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
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
            var webFileProvider = webHostEnvironment.WebRootFileProvider;
            var contentFileProvider = webHostEnvironment.ContentRootFileProvider;

            // plugins in /app_plugins
            var pluginLangFolders = GetPluginLanguageFileSources(contentFileProvider, Cms.Core.Constants.SystemDirectories.AppPlugins, false);

            // files in /wwwroot/app_plugins (or any virtual location that maps to this)
            var razorPluginFolders = GetPluginLanguageFileSources(webFileProvider, Cms.Core.Constants.SystemDirectories.AppPlugins, false);

            // user defined langs that overwrite the default, these should not be used by plugin creators
            var userConfigLangFolder = Cms.Core.Constants.SystemDirectories.Config
                                            .TrimStart(Cms.Core.Constants.CharArrays.Tilde);

            var userLangFolders = contentFileProvider.GetDirectoryContents(userConfigLangFolder)
                    .Where(x => x.IsDirectory && x.Name.InvariantEquals("lang"))
                    .Select(x => new DirectoryInfo(x.PhysicalPath))
                    .SelectMany(x => x.GetFiles("*.user.xml", SearchOption.TopDirectoryOnly))
                    .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, true));

            return pluginLangFolders
                .Concat(razorPluginFolders)
                .Concat(userLangFolders);
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
            // locate all the *.xml files inside Lang folders inside folders of the main folder
            // e.g. /app_plugins/plugin-name/lang/*.xml

            return fileProvider.GetDirectoryContents(folder)
                .Where(x => x.IsDirectory)
                .SelectMany(x => fileProvider.GetDirectoryContents(WebPath.Combine(folder, x.Name)))
                .Where(x => x.IsDirectory && x.Name.InvariantEquals("lang"))
                .Select(x => new DirectoryInfo(x.PhysicalPath))
                .SelectMany(x => x.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
                .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, overwriteCoreKeys));
        }
    }
}
