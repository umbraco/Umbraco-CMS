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
            _ioHelper.EnsurePathExists(Constants.SystemDirectories.Data);
            _ioHelper.EnsurePathExists(Current.Configs.Global().UmbracoMediaPath);
            _ioHelper.EnsurePathExists(Constants.SystemDirectories.MvcViews);
            _ioHelper.EnsurePathExists(Constants.SystemDirectories.PartialViews);
            _ioHelper.EnsurePathExists(Constants.SystemDirectories.MacroPartials);
        }

        public void Terminate()
        { }
    }
}
