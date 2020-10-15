using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.IO.MediaPathSchemes;

namespace Umbraco.Core.Composing.CompositionExtensions
{
    internal static class FileSystems
    {
        /*
         * HOW TO REPLACE THE MEDIA UNDERLYING FILESYSTEM
         * ----------------------------------------------
         *
         * Create a component and use it to modify the composition by adding something like:
         *
         *   composition.RegisterUniqueFor<IFileSystem, IMediaFileSystem>(...);
         *
         * and register whatever supporting filesystem you like.
         *
         *
         * HOW TO IMPLEMENT MY OWN FILESYSTEM
         * ----------------------------------
         *
         * Create your filesystem class:
         *
         *   public class MyFileSystem : FileSystemWrapper
         *   {
         *       public MyFileSystem(IFileSystem innerFileSystem)
         *           : base(innerFileSystem)
         *       { }
         *   }
         *
         * The ctor can have more parameters, that will be resolved by the container.
         *
         * Register your filesystem, in a component:
         *
         *   composition.RegisterFileSystem<MyFileSystem>();
         *
         * Register the underlying filesystem:
         *
         *   composition.RegisterUniqueFor<IFileSystem, MyFileSystem>(...);
         *
         * And that's it, you can inject MyFileSystem wherever it's needed.
         *
         *
         * You can also declare a filesystem interface:
         *
         *   public interface IMyFileSystem : IFileSystem
         *   { }
         *
         * Make the class implement the interface, then
         * register your filesystem, in a component:
         *
         *   composition.RegisterFileSystem<IMyFileSystem, MyFileSystem>();
         *   composition.RegisterUniqueFor<IFileSystem, IMyFileSystem>(...);
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

            // register the IMediaFileSystem implementation
            composition.RegisterFileSystem<IMediaFileSystem, MediaFileSystem>();

            // register the supporting filesystems provider
            composition.Register(factory => new SupportingFileSystems(factory), Lifetime.Singleton);

            // register the IFileSystem supporting the IMediaFileSystem
            // THIS IS THE ONLY THING THAT NEEDS TO CHANGE, IN ORDER TO REPLACE THE UNDERLYING FILESYSTEM
            // and, SupportingFileSystem.For<IMediaFileSystem>() returns the underlying filesystem
            composition.SetMediaFileSystem(factory =>
            {
                var ioHelper = factory.GetInstance<IIOHelper>();
                var hostingEnvironment = factory.GetInstance<IHostingEnvironment>();
                var logger = factory.GetInstance<ILogger<PhysicalFileSystem>>();
                var globalSettings = factory.GetInstance<IOptions<GlobalSettings>>().Value;

                var rootPath = hostingEnvironment.MapPathWebRoot(globalSettings.UmbracoMediaPath);
                var rootUrl = hostingEnvironment.ToAbsolute(globalSettings.UmbracoMediaPath);
                return new PhysicalFileSystem(ioHelper, hostingEnvironment, logger, rootPath, rootUrl);
            });

            return composition;
        }
    }
}
