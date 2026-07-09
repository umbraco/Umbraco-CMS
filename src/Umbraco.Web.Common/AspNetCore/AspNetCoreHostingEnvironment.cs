using Microsoft.AspNetCore.DataProtection.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.Common.AspNetCore;

/// <summary>
/// ASP.NET Core implementation of <see cref="IHostingEnvironment" /> that provides
/// hosting information such as the application URL, physical paths, site name, and debug mode.
/// </summary>
public class AspNetCoreHostingEnvironment : IHostingEnvironment
{
    private readonly IApplicationDiscriminator? _applicationDiscriminator;
    private readonly ConcurrentHashSet<Uri> _applicationUrls = new();
    private readonly IOptionsMonitor<HostingSettings> _hostingSettings;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;

    private readonly UrlMode _urlProviderMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="AspNetCoreHostingEnvironment" /> class
    /// with an <see cref="IApplicationDiscriminator" /> for unique application identification.
    /// </summary>
    /// <param name="hostingSettings">The hosting settings monitor.</param>
    /// <param name="webRoutingSettings">The web routing settings monitor.</param>
    /// <param name="webHostEnvironment">The ASP.NET Core web host environment.</param>
    /// <param name="applicationDiscriminator">The application discriminator used for generating a unique application identifier.</param>
    public AspNetCoreHostingEnvironment(
        IOptionsMonitor<HostingSettings> hostingSettings,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        IWebHostEnvironment webHostEnvironment,
        IApplicationDiscriminator applicationDiscriminator)
        : this(hostingSettings, webRoutingSettings, webHostEnvironment) =>
        _applicationDiscriminator = applicationDiscriminator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AspNetCoreHostingEnvironment" /> class.
    /// </summary>
    /// <param name="hostingSettings">The hosting settings monitor.</param>
    /// <param name="webRoutingSettings">The web routing settings monitor.</param>
    /// <param name="webHostEnvironment">The ASP.NET Core web host environment.</param>
    public AspNetCoreHostingEnvironment(
        IOptionsMonitor<HostingSettings> hostingSettings,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        IWebHostEnvironment webHostEnvironment)
    {
        _hostingSettings = hostingSettings ?? throw new ArgumentNullException(nameof(hostingSettings));
        _webRoutingSettings = webRoutingSettings ?? throw new ArgumentNullException(nameof(webRoutingSettings));
        _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        _urlProviderMode = _webRoutingSettings.CurrentValue.UrlProviderMode;

        SetSiteNameAndDebugMode(hostingSettings.CurrentValue);

        // We have to ensure that the OptionsMonitor is an actual options monitor since we have a hack
        // where we initially use an OptionsMonitorAdapter, which doesn't implement OnChange.
        // See summery of OptionsMonitorAdapter for more information.
        if (hostingSettings is OptionsMonitor<HostingSettings>)
        {
            hostingSettings.OnChange(settings => SetSiteNameAndDebugMode(settings));
        }

        ApplicationPhysicalPath = webHostEnvironment.ContentRootPath;

        if (_webRoutingSettings.CurrentValue.UmbracoApplicationUrl is not null)
        {
            ApplicationMainUrl = new Uri(_webRoutingSettings.CurrentValue.UmbracoApplicationUrl);
        }
    }

    /// <inheritdoc />
    public bool IsHosted { get; } = true;

    private Uri? _applicationMainUrl;

    /// <inheritdoc />
    public Uri ApplicationMainUrl
    {
        get => _applicationMainUrl!;
        private set => _applicationMainUrl = value;
    }

    /// <inheritdoc />
    public string? SiteName { get; private set; }

    /// <inheritdoc />
    public string ApplicationId
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            field = _applicationDiscriminator?.GetApplicationId() ?? _webHostEnvironment.GetTemporaryApplicationId();

            return field;
        }
    }

    /// <inheritdoc />
    public string ApplicationPhysicalPath { get; }

    /// <inheritdoc/>
    public string ApplicationVirtualPath =>
        _hostingSettings.CurrentValue.ApplicationVirtualPath?.EnsureStartsWith('/') ?? "/";

    /// <inheritdoc />
    public bool IsDebugMode { get; private set; }

    /// <inheritdoc/>
    public string LocalTempPath
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            switch (_hostingSettings.CurrentValue.LocalTempStorageLocation)
            {
                case LocalTempStorage.EnvironmentTemp:

                    // environment temp is unique, we need a folder per site

                    // use a hash
                    // combine site name and application id
                    // site name is a Guid on Cloud
                    // application id is eg /LM/W3SVC/123456/ROOT
                    // the combination is unique on one server
                    // and, if a site moves from worker A to B and then back to A...
                    // hopefully it gets a new Guid or new application id?
                    var hashString = SiteName + "::" + ApplicationId;
                    var hash = hashString.GenerateHash();
                    var siteTemp = Path.Combine(Path.GetTempPath(), "UmbracoData", hash);

                    return field = siteTemp;

                default:

                    return field = MapPathContentRoot(Core.Constants.SystemDirectories.TempData);
            }
        }
    }

    /// <inheritdoc />
    public string TemporaryFileUploadPath => _hostingSettings.CurrentValue.TemporaryFileUploadLocation
                                             ?? Path.Combine(MapPathContentRoot(Core.Constants.SystemDirectories.TempData), "TemporaryFile");

    /// <inheritdoc />
    public string MapPathWebRoot(string path) => _webHostEnvironment.MapPathWebRoot(path);

    /// <inheritdoc />
    public string MapPathContentRoot(string path) => _webHostEnvironment.MapPathContentRoot(path);

    /// <inheritdoc />
    public string ToAbsolute(string virtualPath)
    {
        if (!virtualPath.StartsWith("~/") && !virtualPath.StartsWith("/") && _urlProviderMode != UrlMode.Absolute)
        {
            throw new InvalidOperationException(
                $"The value {virtualPath} for parameter {nameof(virtualPath)} must start with ~/ or /");
        }

        // will occur if it starts with "/"
        if (Uri.IsWellFormedUriString(virtualPath, UriKind.Absolute))
        {
            return virtualPath;
        }

        var fullPath = ApplicationVirtualPath.EnsureEndsWith('/') +
                       virtualPath.TrimStart(Core.Constants.CharArrays.TildeForwardSlash);

        return fullPath;
    }

    /// <inheritdoc/>
    public void EnsureApplicationMainUrl(Uri? currentApplicationUrl)
    {
        if (currentApplicationUrl is null)
        {
            return;
        }

        // Explicit configuration always takes precedence.
        if (_webRoutingSettings.CurrentValue.UmbracoApplicationUrl is not null)
        {
            return;
        }

        switch (_webRoutingSettings.CurrentValue.ApplicationUrlDetection)
        {
            case ApplicationUrlDetection.None:
                return;

            case ApplicationUrlDetection.FirstRequest:
                // Atomic: only the first thread to arrive sets the URL.
                // Subsequent calls (even concurrent ones with different hosts) are no-ops.
                Interlocked.CompareExchange(ref _applicationMainUrl, currentApplicationUrl, null);
                break;

            case ApplicationUrlDetection.EveryRequest:
                var change = _applicationUrls.Contains(currentApplicationUrl) is false;
                if (change)
                {
                    if (_applicationUrls.TryAdd(currentApplicationUrl))
                    {
                        ApplicationMainUrl = currentApplicationUrl;
                    }
                }

                break;
        }
    }

    private void SetSiteNameAndDebugMode(HostingSettings hostingSettings)
    {
        SiteName = string.IsNullOrWhiteSpace(hostingSettings.SiteName)
            ? _webHostEnvironment.ApplicationName
            : hostingSettings.SiteName;

        IsDebugMode = hostingSettings.Debug;
    }
}
