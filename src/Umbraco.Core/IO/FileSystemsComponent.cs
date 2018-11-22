using LightInject;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.IO.MediaPathSchemes;

namespace Umbraco.Core.IO
{
    public sealed class FileSystemsComponent : UmbracoComponentBase
    {
        public override void Compose(Composition composition)
        {
            var container = composition.Container;

            // register filesystems
            container.RegisterSingleton<FileSystems>();
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MediaFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().ScriptsFileSystem, Constants.Composing.FileSystems.ScriptFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().PartialViewsFileSystem, Constants.Composing.FileSystems.PartialViewFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MacroPartialsFileSystem, Constants.Composing.FileSystems.PartialViewMacroFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().StylesheetsFileSystem, Constants.Composing.FileSystems.StylesheetFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MasterPagesFileSystem, Constants.Composing.FileSystems.MasterpageFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MvcViewsFileSystem, Constants.Composing.FileSystems.ViewFileSystem);

            container.RegisterSingleton<IMediaPathScheme, TwoGuidsMediaPathScheme>();
        }
    }
}
