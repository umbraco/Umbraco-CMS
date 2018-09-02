using Umbraco.Core.IO;

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


            // fixme - review registering mediafilesystem. it seems to create cyclic dependencies for castle.
            // let's try naming it so the default is overwritten...

            // register MediaFileSystem, which can be injected directly
            // note: the actual MediaFileSystem implementation is created by FileSystems directly,
            //       without being registered in the container - this just gives access to it
            container.Register(f => f.GetInstance<FileSystems>().MediaFileSystem);


            return container;
        }
    }
}
