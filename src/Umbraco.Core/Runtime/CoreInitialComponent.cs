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
            Current.IOHelper.EnsurePathExists(Current.SystemDirectories.Media);
            Current.IOHelper.EnsurePathExists(Current.SystemDirectories.MvcViews);
            Current.IOHelper.EnsurePathExists(Current.SystemDirectories.MvcViews + "/Partials");
            Current.IOHelper.EnsurePathExists(Current.SystemDirectories.MvcViews + "/MacroPartials");
        }

        public void Terminate()
        { }
    }
}
