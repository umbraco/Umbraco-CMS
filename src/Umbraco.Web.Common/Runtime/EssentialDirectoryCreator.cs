using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Runtime;

public class EssentialDirectoryCreator : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly GlobalSettings _globalSettings;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IIOHelper _ioHelper;

    public EssentialDirectoryCreator(
        IIOHelper ioHelper,
        IHostEnvironment hostEnvironment,
        IWebHostEnvironment webHostEnvironment,
        IOptions<GlobalSettings> globalSettings)
    {
        _ioHelper = ioHelper;
        _hostEnvironment = hostEnvironment;
        _webHostEnvironment = webHostEnvironment;
        _globalSettings = globalSettings.Value;
    }

    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        // ensure we have some essential directories
        // every other component can then initialize safely
        _ioHelper.EnsurePathExists(_hostEnvironment.MapPathContentRoot(Core.Constants.SystemDirectories.Data));
        _ioHelper.EnsurePathExists(_webHostEnvironment.MapPathWebRoot(_globalSettings.UmbracoMediaPhysicalRootPath));
        _ioHelper.EnsurePathExists(_hostEnvironment.MapPathContentRoot(Core.Constants.SystemDirectories.MvcViews));
        _ioHelper.EnsurePathExists(_hostEnvironment.MapPathContentRoot(Core.Constants.SystemDirectories.PartialViews));
    }
}
