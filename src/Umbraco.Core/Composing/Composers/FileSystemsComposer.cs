using System;
using System.Configuration;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Core.Composing.Composers
{
    public static class FileSystemsComposer
    {
        /*
         * HOW TO REPLACE THE MEDIA UNDERLYING FILESYSTEM
         * ----------------------------------------------
         *
         * Create a component and use it to modify the composition by adding something like:
         *
         *   composition.Container.RegisterSingleton<IFileSystem>("media", factory => ...);
         *
         * where the ... part returns the new underlying filesystem, as an IFileSystem.
         *
         *
         * HOW TO IMPLEMENT MY OWN FILESYSTEM
         * ----------------------------------
         *
         * Declare your filesystem interface:
         *
         *   public interface IMyFileSystem : IFileSystem
         *   { }
         *
         * Create your filesystem class:
         *
         *   [FileSystem("my")]
         *   public class MyFileSystem : FileSystemWrapper, IFormsFileSystem
         *   {
         *       public FormsFileSystem(IFileSystem innerFileSystem)
         *           : base(innerFileSystem)
         *       { }
         *   }
         *
         *  Register both the underlying filesystem, and your filesystem, in a component:
         *
         *    composition.Container.RegisterSingleton<IFileSystem>("my", factory => ...);
         *    composition.Container.RegisterSingleton<IMyFileSystem>(factory =>
         *        factory.GetInstance<FileSystems>().GetFileSystem<MyFileSystem>();
         *
         * And that's it, you can inject IMyFileSystem wherever it's needed.
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
         * Also note that in order for things to work correctly, all filesystems should
         * be instantiated before shadowing - so if registering a new filesystem in a
         * component, it's a good idea to initialize it. This would be enough (in the
         * component):
         *
         *   public void Initialize(IMyFileSystem fs)
         *   { }
         *
         *
         */

        public static IContainer ComposeFileSystems(this IContainer container)
        {
            // register FileSystems, which manages all filesystems
            // it needs to be registered (not only the interface) because it provides additional
            // functionality eg for scoping, and is injected in the scope provider - whereas the
            // interface is really for end-users to get access to filesystems.
            //container.RegisterSingleton<FileSystems>();
            container.RegisterSingleton(factory => factory.CreateInstance<FileSystems>(new { container} ));

            // register IFileSystems, which gives access too all filesystems
            container.RegisterSingleton<IFileSystems>(factory => factory.GetInstance<FileSystems>());

            // register IMediaFileSystem
            var virtualRoot = GetMediaFileSystemVirtualRoot();
            container.RegisterSingleton<IFileSystem>("media", factory => new PhysicalFileSystem(virtualRoot));
            container.RegisterSingleton<IMediaFileSystem>(factory => factory.GetInstance<FileSystems>().GetFileSystem<MediaFileSystem>());

            return container;
        }

        private static string GetMediaFileSystemVirtualRoot()
        {
            // for the time being, we still use the FileSystemProvider config file
            // but, detect if ppl are trying to use it to change the "provider"

            var virtualRoot = "~/media";

            var config = (FileSystemProvidersSection)ConfigurationManager.GetSection("umbracoConfiguration/FileSystemProviders");
            var p = config?.Providers["media"];
            if (p == null) return virtualRoot;

            if (!string.IsNullOrWhiteSpace(p.Type) && p.Type != "Umbraco.Core.IO.PhysicalFileSystem, Umbraco.Core")
                throw new InvalidOperationException("Setting a provider type in FileSystemProviders.config is not supported anymore, see FileSystemsComposer for help.");

            virtualRoot = p?.Parameters["virtualRoot"]?.Value ?? "~/media";
            return virtualRoot;
        }
    }
}
