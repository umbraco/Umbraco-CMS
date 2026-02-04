using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Runtime;

/// <summary>
/// Handles the creation of essential directories during application startup.
/// </summary>
/// <remarks>
/// This notification handler ensures that required directories (Data, Media, Views, PartialViews)
/// exist before other components initialize, providing a safe environment for the application to run.
/// </remarks>
public class EssentialDirectoryCreator : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IIOHelper _ioHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="EssentialDirectoryCreator"/> class.
    /// </summary>
    /// <param name="ioHelper">The I/O helper for directory operations.</param>
    /// <param name="hostingEnvironment">The hosting environment for path resolution.</param>
    /// <param name="globalSettings">The global settings options.</param>
    public EssentialDirectoryCreator(IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, IOptions<GlobalSettings> globalSettings)
    {
        _ioHelper = ioHelper;
        _hostingEnvironment = hostingEnvironment;
        _globalSettings = globalSettings.Value;
    }

    /// <inheritdoc />
    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        // ensure we have some essential directories
        // every other component can then initialize safely
        _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Data));
        _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathWebRoot(_globalSettings.UmbracoMediaPhysicalRootPath));
        _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.MvcViews));
        _ioHelper.EnsurePathExists(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.PartialViews));
    }
}
