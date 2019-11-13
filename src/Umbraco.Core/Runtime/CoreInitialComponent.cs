using Umbraco.Core.Composing;
using Umbraco.Core.IO;

namespace Umbraco.Core.Runtime
{
    public class CoreInitialComponent : IComponent
    {
        private readonly IIOHelper _ioHelper;

        public CoreInitialComponent(IIOHelper ioHelper)
        {
            _ioHelper = ioHelper;
        }

        public void Initialize()
        {
            // ensure we have some essential directories
            // every other component can then initialize safely
            _ioHelper.EnsurePathExists("~/App_Data");
            _ioHelper.EnsurePathExists(SystemDirectories.Media);
            _ioHelper.EnsurePathExists(SystemDirectories.MvcViews);
            _ioHelper.EnsurePathExists(SystemDirectories.MvcViews + "/Partials");
            _ioHelper.EnsurePathExists(SystemDirectories.MvcViews + "/MacroPartials");
        }

        public void Terminate()
        { }
    }
}
