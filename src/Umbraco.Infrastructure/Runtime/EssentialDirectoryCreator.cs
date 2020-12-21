using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;

namespace Umbraco.Infrastructure.Runtime
{
    public class EssentialDirectoryCreator : INotificationHandler<UmbracoApplicationStarting>
    {
        private readonly IIOHelper _ioHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly GlobalSettings _globalSettings;

        public EssentialDirectoryCreator(IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, IOptions<GlobalSettings> globalSettings)
        {
            _ioHelper = ioHelper;
            _hostingEnvironment = hostingEnvironment;
            _globalSettings = globalSettings.Value;
        }

        public Task HandleAsync(UmbracoApplicationStarting notification, CancellationToken cancellationToken)
        {
            // ensure we have some essential directories
            // every other component can then initialize safely
            _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Data));
            _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathWebRoot(_globalSettings.UmbracoMediaPath));
            _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.MvcViews));
            _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.PartialViews));
            _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.MacroPartials));

            return Task.CompletedTask;
        }
    }
}
