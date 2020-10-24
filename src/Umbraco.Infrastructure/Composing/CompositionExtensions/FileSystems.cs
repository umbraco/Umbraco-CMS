using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.IO.MediaPathSchemes;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Composing.CompositionExtensions
{
    internal static class FileSystems
    {
        /*
         * HOW TO REPLACE THE MEDIA UNDERLYING FILESYSTEM
         * ----------------------------------------------
         *
         *   composition.RegisterUnique<IMediaFileSystem>(factoryMethod);
         *
         *   composition.RegisterUnique<IMediaFileSystem, TImplementation>();
         *
         *
         * WHAT IS SHADOWING
         * -----------------
         *
         * Shadowing is the technology used for Deploy to implement some sort of
         * transaction-management on top of filesystems. The plumbing explained above,
         * compared to creating your own physical filesystem, ensures that your filesystem
         * would participate into such transactions.
         *
         *
         */

        public static Composition ComposeFileSystems(this Composition composition)
        {
            // register FileSystems, which manages all filesystems
            // it needs to be registered (not only the interface) because it provides additional
            // functionality eg for scoping, and is injected in the scope provider - whereas the
            // interface is really for end-users to get access to filesystems.
            composition.RegisterUnique(factory => factory.CreateInstance<Core.IO.FileSystems>(factory));

            // register IFileSystems, which gives access too all filesystems
            composition.RegisterUnique<IFileSystems>(factory => factory.GetInstance<Core.IO.FileSystems>());

            // register the scheme for media paths
            composition.RegisterUnique<IMediaPathScheme, UniqueMediaPathScheme>();

            // register the default IMediaFileSystem implementation
            composition.RegisterUnique<IMediaFileSystem>(factory =>
            {
                var ioHelper = factory.GetInstance<IIOHelper>();
                var hostingEnvironment = factory.GetInstance<IHostingEnvironment>();
                var logger = factory.GetInstance<ILogger<PhysicalFileSystem>>();
                var globalSettings = factory.GetInstance<IOptions<GlobalSettings>>().Value;

                var rootPath = hostingEnvironment.MapPathWebRoot(globalSettings.UmbracoMediaPath);
                var rootUrl = hostingEnvironment.ToAbsolute(globalSettings.UmbracoMediaPath);
                var defaultInnerFileSystem = new PhysicalFileSystem(ioHelper, hostingEnvironment, logger, rootPath, rootUrl);

                return new MediaFileSystem(
                    defaultInnerFileSystem,
                    factory.GetInstance<IMediaPathScheme>(),
                    factory.GetInstance<ILogger<MediaFileSystem>>(),
                    factory.GetInstance<IShortStringHelper>()
                );
            });

            return composition;
        }
    }
}
