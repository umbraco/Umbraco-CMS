using Microsoft.Extensions.Options;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;

namespace Umbraco.Core.Runtime
{
    public class CoreInitialComponent : IComponent
    {
        private readonly IIOHelper _ioHelper;
        private readonly GlobalSettings _globalSettings;

        public CoreInitialComponent(IIOHelper ioHelper, IOptions<GlobalSettings> globalSettings)
        {
            _ioHelper = ioHelper;
            _globalSettings = globalSettings.Value;
        }

        public void Initialize()
        {
            // ensure we have some essential directories
            // every other component can then initialize safely
            _ioHelper.EnsurePathExists(Constants.SystemDirectories.Data);
            _ioHelper.EnsurePathExists(_globalSettings.UmbracoMediaPath);
            _ioHelper.EnsurePathExists(Constants.SystemDirectories.MvcViews);
            _ioHelper.EnsurePathExists(Constants.SystemDirectories.PartialViews);
            _ioHelper.EnsurePathExists(Constants.SystemDirectories.MacroPartials);
        }

        public void Terminate()
        { }
    }
}
