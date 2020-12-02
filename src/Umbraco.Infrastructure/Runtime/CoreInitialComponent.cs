using Microsoft.Extensions.Options;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;

namespace Umbraco.Core.Runtime
{
    public class CoreInitialComponent : IComponent
    {
        private readonly IIOHelper _ioHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly GlobalSettings _globalSettings;

        public CoreInitialComponent(IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, IOptions<GlobalSettings> globalSettings)
        {
            _ioHelper = ioHelper;
            _hostingEnvironment = hostingEnvironment;
            _globalSettings = globalSettings.Value;
        }

        public void Initialize()
        {
            // ensure we have some essential directories
            // every other component can then initialize safely
            _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Data));
            _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathWebRoot(_globalSettings.UmbracoMediaPath));
            _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.MvcViews));
            _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.PartialViews));
            _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.MacroPartials));
        }

        public void Terminate()
        { }
    }
}
