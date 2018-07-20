using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Composing.Composers
{
    public static class FileSystemsComposer
    {
        public static IContainer ComposeFileSystems(this IContainer container)
        {
            // register FileSystems, which manages all filesystems
            container.RegisterSingleton<FileSystems>();

            // register IFileSystems, which gives access too all filesystems
            container.RegisterSingleton<IFileSystems>(factory => factory.GetInstance<FileSystems>());

            // register MediaFileSystem, which can be injected directly
            container.RegisterSingleton(factory => factory.GetInstance<IFileSystems>().MediaFileSystem);

            // register MediaFileSystem, so that FileSystems can create it
            container.Register<IFileSystem, MediaFileSystem>((f, wrappedFileSystem)
                => new MediaFileSystem(wrappedFileSystem, f.GetInstance<IContentSection>(), f.GetInstance<IMediaPathScheme>(), f.GetInstance<ILogger>()));

            return container;
        }
    }
}
