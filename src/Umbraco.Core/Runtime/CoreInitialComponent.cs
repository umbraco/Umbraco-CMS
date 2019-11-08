using Umbraco.Core.Composing;
using Umbraco.Core.IO;

namespace Umbraco.Core.Runtime
{
    public class CoreInitialComponent : IComponent
    {
        public void Initialize()
        {
            // ensure we have some essential directories
            // every other component can then initialize safely
            Current.IOHelper.EnsurePathExists("~/App_Data");
            Current.IOHelper.EnsurePathExists(SystemDirectories.Media);
            Current.IOHelper.EnsurePathExists(SystemDirectories.MvcViews);
            Current.IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/Partials");
            Current.IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/MacroPartials");
        }

        public void Terminate()
        { }
    }
}
