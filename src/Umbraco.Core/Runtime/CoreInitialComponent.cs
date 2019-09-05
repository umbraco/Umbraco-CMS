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
            IOHelper.EnsurePathExists("~/App_Data");
            IOHelper.EnsurePathExists(SystemDirectories.Media);
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews);
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/Partials");
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/MacroPartials");
        }

        public void Terminate()
        { }
    }
}
