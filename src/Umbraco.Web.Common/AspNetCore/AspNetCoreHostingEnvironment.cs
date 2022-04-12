using System;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Extensions;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.Common.AspNetCore
{
    public class AspNetCoreHostingEnvironment : IHostingEnvironment
    {
        private readonly ConcurrentHashSet<Uri> _applicationUrls = new ConcurrentHashSet<Uri>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsMonitor<HostingSettings> _hostingSettings;
        private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private string _applicationId;
        private string _localTempPath;
        private UrlMode _urlProviderMode;

        public AspNetCoreHostingEnvironment(
            IServiceProvider serviceProvider,
            IOptionsMonitor<HostingSettings> hostingSettings,
            IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
            IWebHostEnvironment webHostEnvironment)
        {
            _serviceProvider = serviceProvider;
            _hostingSettings = hostingSettings ?? throw new ArgumentNullException(nameof(hostingSettings));
            _webRoutingSettings = webRoutingSettings ?? throw new ArgumentNullException(nameof(webRoutingSettings));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _urlProviderMode = _webRoutingSettings.CurrentValue.UrlProviderMode;

            SetSiteName(hostingSettings.CurrentValue.SiteName);

            // We have to ensure that the OptionsMonitor is an actual options monitor since we have a hack
            // where we initially use an OptionsMonitorAdapter, which doesn't implement OnChange.
            // See summery of OptionsMonitorAdapter for more information.
            if (hostingSettings is OptionsMonitor<HostingSettings>)
            {
                hostingSettings.OnChange(settings => SetSiteName(settings.SiteName));
            }

            ApplicationPhysicalPath = webHostEnvironment.ContentRootPath;

            if (_webRoutingSettings.CurrentValue.UmbracoApplicationUrl is not null)
            {
                ApplicationMainUrl = new Uri(_webRoutingSettings.CurrentValue.UmbracoApplicationUrl);
            }
        }

        /// <inheritdoc/>
        public bool IsHosted { get; } = true;

        /// <inheritdoc/>
        public Uri ApplicationMainUrl { get; private set; }

        /// <inheritdoc/>
        public string SiteName { get; private set; }

        /// <inheritdoc/>
        public string ApplicationId
        {
            get
            {
                if (_applicationId != null)
                {
                    return _applicationId;
                }

                var appId = _serviceProvider.GetApplicationUniqueIdentifier();
                if (appId == null)
                {
                    throw new InvalidOperationException("Could not acquire an ApplicationId, ensure DataProtection services and an IHostEnvironment are registered");
                }

                // Hash this value because it can really be anything. By default this will be the application's path.
                // TODO: Test on IIS, hopefully this would be equivalent to the IIS unique ID.
                // This could also contain sensitive information (i.e. like the physical path) which we don't want to expose in logs.
                _applicationId = appId.GenerateHash();

                return _applicationId;
            }
        }

        /// <inheritdoc/>
        public string ApplicationPhysicalPath { get; }

        // TODO how to find this, This is a server thing, not application thing.
        public string ApplicationVirtualPath => _hostingSettings.CurrentValue.ApplicationVirtualPath?.EnsureStartsWith('/') ?? "/";

        /// <inheritdoc/>
        public bool IsDebugMode => _hostingSettings.CurrentValue.Debug;

        public Version IISVersion { get; }

        public string LocalTempPath
        {
            get
            {
                if (_localTempPath != null)
                {
                    return _localTempPath;
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
                        string hashString = SiteName + "::" + ApplicationId;
                        string hash = hashString.GenerateHash();
                        string siteTemp = Path.Combine(Path.GetTempPath(), "UmbracoData", hash);

                        return _localTempPath = siteTemp;

                    default:

                        return _localTempPath = MapPathContentRoot(Cms.Core.Constants.SystemDirectories.TempData);
                }
            }
        }

        /// <inheritdoc/>
        public string MapPathWebRoot(string path) => _webHostEnvironment.MapPathWebRoot(path);

        /// <inheritdoc/>
        public string MapPathContentRoot(string path) => _webHostEnvironment.MapPathContentRoot(path);

        /// <inheritdoc/>
        public string ToAbsolute(string virtualPath)
        {
            if (!virtualPath.StartsWith("~/") && !virtualPath.StartsWith("/") && _urlProviderMode != UrlMode.Absolute)
            {
                throw new InvalidOperationException($"The value {virtualPath} for parameter {nameof(virtualPath)} must start with ~/ or /");
            }

            // will occur if it starts with "/"
            if (Uri.IsWellFormedUriString(virtualPath, UriKind.Absolute))
            {
                return virtualPath;
            }

            string fullPath = ApplicationVirtualPath.EnsureEndsWith('/') + virtualPath.TrimStart(Core.Constants.CharArrays.TildeForwardSlash);

            return fullPath;
        }

        public void EnsureApplicationMainUrl(Uri currentApplicationUrl)
        {
            // Fixme: This causes problems with site swap on azure because azure pre-warms a site by calling into `localhost` and when it does that
            // it changes the URL to `localhost:80` which actually doesn't work for pinging itself, it only works internally in Azure. The ironic part
            // about this is that this is here specifically for the slot swap scenario https://issues.umbraco.org/issue/U4-10626

            // see U4-10626 - in some cases we want to reset the application url
            // (this is a simplified version of what was in 7.x)
            // note: should this be optional? is it expensive?


            if (currentApplicationUrl is null)
            {
                return;
            }

            if (_webRoutingSettings.CurrentValue.UmbracoApplicationUrl is not null)
            {
                return;
            }

            var change = !_applicationUrls.Contains(currentApplicationUrl);
            if (change)
            {
                if (_applicationUrls.TryAdd(currentApplicationUrl))
                {
                    ApplicationMainUrl = currentApplicationUrl;
                }
            }
        }

        private void SetSiteName(string siteName) =>
            SiteName = string.IsNullOrWhiteSpace(siteName)
                ? _webHostEnvironment.ApplicationName
                : siteName;
    }
}
