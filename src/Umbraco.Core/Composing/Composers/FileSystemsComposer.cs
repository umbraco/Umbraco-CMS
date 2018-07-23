using System.Linq;
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
            container.Register/*Singleton*/(factory => factory.GetInstance<IFileSystems>().MediaFileSystem);

            return container;
        }
    }
}
