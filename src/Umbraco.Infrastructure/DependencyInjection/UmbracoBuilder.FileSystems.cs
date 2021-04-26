using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.IO.MediaPathSchemes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection
{
    public static partial class UmbracoBuilderExtensions
    {
        /*
         * HOW TO REPLACE THE MEDIA UNDERLYING FILESYSTEM
         * ----------------------------------------------
         *
         * Create an implementation of IFileSystem and register it as the underlying filesystem for
         * MediaFileSystem with the following extension on composition.
         *
         * builder.SetMediaFileSystem(factory => FactoryMethodToReturnYourImplementation())
         *
         * WHAT IS SHADOWING
         * -----------------
         *
         * Shadowing is the technology used for Deploy to implement some sort of
         * transaction-management on top of filesystems. The plumbing explained above,
         * compared to creating your own physical filesystem, ensures that your filesystem
         * would participate into such transactions.
         *
         */

        internal static IUmbracoBuilder AddFileSystems(this IUmbracoBuilder builder)
        {
            // register FileSystems, which manages all filesystems
            // Takes a factory method for creating the stylesheet filesystem to allow it to be replaced.
            builder.AddFileSystems(factory =>
            {
                IIOHelper ioHelper = factory.GetRequiredService<IIOHelper>();
                IHostingEnvironment hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();
                ILogger<PhysicalFileSystem> logger = factory.GetRequiredService<ILogger<PhysicalFileSystem>>();
                GlobalSettings settings = factory.GetRequiredService<IOptions<GlobalSettings>>().Value;

                var path = settings.UmbracoCssPath;
                var rootPath = hostingEnvironment.MapPathWebRoot(path);
                var rootUrl = hostingEnvironment.ToAbsolute(path);
                return new PhysicalFileSystem(ioHelper, hostingEnvironment, logger, rootPath, rootUrl);
            });

            // register the scheme for media paths
            builder.Services.AddUnique<IMediaPathScheme, UniqueMediaPathScheme>();

            builder.SetMediaFileSystem(factory =>
            {
                IIOHelper ioHelper = factory.GetRequiredService<IIOHelper>();
                IHostingEnvironment hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();
                ILogger<PhysicalFileSystem> logger = factory.GetRequiredService<ILogger<PhysicalFileSystem>>();
                GlobalSettings globalSettings = factory.GetRequiredService<IOptions<GlobalSettings>>().Value;

                var rootPath = hostingEnvironment.MapPathWebRoot(globalSettings.UmbracoMediaPath);
                var rootUrl = hostingEnvironment.ToAbsolute(globalSettings.UmbracoMediaPath);
                return new PhysicalFileSystem(ioHelper, hostingEnvironment, logger, rootPath, rootUrl);
            });

            return builder;
        }
    }
}
