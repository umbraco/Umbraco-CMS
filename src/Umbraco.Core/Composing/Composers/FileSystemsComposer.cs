using Umbraco.Core.Components;
using Umbraco.Core.IO;
using Umbraco.Core.IO.MediaPathSchemes;

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
         *   composition.Container.RegisterFileSystem<IMediaFileSystem, MediaFileSystem>(
         *       factory => new PhysicalFileSystem("~/somewhere"));
         *
         * return whatever supporting filesystem you like.
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
         * The ctor can have more parameters that will be resolved by the container.
         *
         * Register your filesystem, in a component:
         *
         *    composition.Container.RegisterFileSystem<MyFileSystem>(
         *        factory => new PhysicalFileSystem("~/my"));
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
         *    composition.Container.RegisterFileSystem<IMyFileSystem, MyFileSystem>(
         *        factory => new PhysicalFileSystem("~/my"));
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
            composition.RegisterSingleton(factory => factory.CreateInstance<FileSystems>(factory));

            // register IFileSystems, which gives access too all filesystems
            composition.RegisterSingleton<IFileSystems>(factory => factory.GetInstance<FileSystems>());

            // register the scheme for media paths
            composition.RegisterSingleton<IMediaPathScheme, TwoGuidsMediaPathScheme>();

            // register the IMediaFileSystem implementation with a supporting filesystem
            composition.RegisterFileSystem<IMediaFileSystem, MediaFileSystem>(
                factory => new PhysicalFileSystem("~/media"));

            return composition;
        }
    }
}
