using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Runtime;

public class EssentialDirectoryCreator : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IIOHelper _ioHelper;

    public EssentialDirectoryCreator(IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, IOptions<GlobalSettings> globalSettings)
    {
        _ioHelper = ioHelper;
        _hostingEnvironment = hostingEnvironment;
        _globalSettings = globalSettings.Value;
    }

    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        // ensure we have some essential directories
        // every other component can then initialize safely
        _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Data));
        _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathWebRoot(_globalSettings.UmbracoMediaPhysicalRootPath));
        _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.MvcViews));
        _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.PartialViews));
        _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.MacroPartials));
    }
}
