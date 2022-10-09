using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.IO.MediaPathSchemes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

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
        builder.Services.AddSingleton<FileSystems>();

        // register the scheme for media paths
        builder.Services.AddUnique<IMediaPathScheme, UniqueMediaPathScheme>();

        builder.Services.AddUnique<IViewHelper, ViewHelper>();
        builder.Services.AddUnique<IDefaultViewContentProvider, DefaultViewContentProvider>();

        builder.SetMediaFileSystem(factory =>
        {
            IIOHelper ioHelper = factory.GetRequiredService<IIOHelper>();
            IHostingEnvironment hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();
            ILogger<PhysicalFileSystem> logger = factory.GetRequiredService<ILogger<PhysicalFileSystem>>();
            GlobalSettings globalSettings = factory.GetRequiredService<IOptions<GlobalSettings>>().Value;

            var rootPath = Path.IsPathRooted(globalSettings.UmbracoMediaPhysicalRootPath)
                ? globalSettings.UmbracoMediaPhysicalRootPath
                : hostingEnvironment.MapPathWebRoot(globalSettings.UmbracoMediaPhysicalRootPath);
            var rootUrl = hostingEnvironment.ToAbsolute(globalSettings.UmbracoMediaPath);
            return new PhysicalFileSystem(ioHelper, hostingEnvironment, logger, rootPath, rootUrl);
        });

        return builder;
    }
}
