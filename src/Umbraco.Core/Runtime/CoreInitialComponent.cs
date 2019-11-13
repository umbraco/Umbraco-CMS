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
            Current.IOHelper.EnsurePathExists(Constants.SystemDirectories.Data);
            Current.IOHelper.EnsurePathExists(Current.IOHelper.Media);
            Current.IOHelper.EnsurePathExists(Constants.SystemDirectories.MvcViews);
            Current.IOHelper.EnsurePathExists(Constants.SystemDirectories.PartialViews);
            Current.IOHelper.EnsurePathExists(Constants.SystemDirectories.MacroPartials);
        }

        public void Terminate()
        { }
    }
}
