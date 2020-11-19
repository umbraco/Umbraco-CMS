using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetCoreHostingEnvironment : Core.Hosting.IHostingEnvironment
    {
        private IOptionsMonitor<HostingSettings>  _hostingSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private string _localTempPath;

        public AspNetCoreHostingEnvironment(IOptionsMonitor<HostingSettings> hostingSettings, IWebHostEnvironment webHostEnvironment)
        {
            _hostingSettings = hostingSettings ?? throw new ArgumentNullException(nameof(hostingSettings));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));

            SiteName = webHostEnvironment.ApplicationName;
            ApplicationId = AppDomain.CurrentDomain.Id.ToString();
            ApplicationPhysicalPath = webHostEnvironment.ContentRootPath;

            IISVersion = new Version(0, 0); // TODO not necessary IIS
        }

        public bool IsHosted { get; } = true;
        public string SiteName { get; }
        public string ApplicationId { get; }
        public string ApplicationPhysicalPath { get; }
        public string ApplicationServerAddress { get; }

        //TODO how to find this, This is a server thing, not application thing.
        public string ApplicationVirtualPath => _hostingSettings.CurrentValue.ApplicationVirtualPath?.EnsureStartsWith('/') ?? "/";
        public bool IsDebugMode => _hostingSettings.CurrentValue.Debug;

        public Version IISVersion { get; }
        public string LocalTempPath
        {
            get
            {
                if (_localTempPath != null)
                    return _localTempPath;

                switch (_hostingSettings.CurrentValue.LocalTempStorageLocation)
                {
                    case LocalTempStorage.AspNetTemp:

                        // TODO: I don't think this is correct? but also we probably can remove AspNetTemp as an option entirely
                        // since this is legacy and we shouldn't use it
                         return _localTempPath = System.IO.Path.Combine(Path.GetTempPath(), ApplicationId, "UmbracoData");

                    case LocalTempStorage.EnvironmentTemp:

                        // environment temp is unique, we need a folder per site

                        // use a hash
                        // combine site name and application id
                        //  site name is a Guid on Cloud
                        //  application id is eg /LM/W3SVC/123456/ROOT
                        // the combination is unique on one server
                        // and, if a site moves from worker A to B and then back to A...
                        //  hopefully it gets a new Guid or new application id?

                        var hashString = SiteName + "::" + ApplicationId;
                        var hash = hashString.GenerateHash();
                        var siteTemp = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "UmbracoData", hash);

                        return _localTempPath = siteTemp;

                    //case LocalTempStorage.Default:
                    //case LocalTempStorage.Unknown:
                    default:
                        return _localTempPath = MapPathContentRoot("~/App_Data/TEMP");
                }
            }
        }

        public string MapPathWebRoot(string path) => MapPath(_webHostEnvironment.WebRootPath, path);
        public string MapPathContentRoot(string path) => MapPath(_webHostEnvironment.ContentRootPath, path);

        private string MapPath(string root, string path)
        {
            var newPath = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

            // TODO: This is a temporary error because we switched from IOHelper.MapPath to HostingEnvironment.MapPathXXX
            // IOHelper would check if the path passed in started with the root, and not prepend the root again if it did,
            // however if you are requesting a path be mapped, it should always assume the path is relative to the root, not
            // absolute in the file system.  This error will help us find and fix improper uses, and should be removed once
            // all those uses have been found and fixed
            if (newPath.StartsWith(root)) throw new ArgumentException("The path appears to already be fully qualified.  Please remove the call to MapPath");

            return Path.Combine(root, newPath.TrimStart('~', '/'));
        }

        public string ToAbsolute(string virtualPath)
        {
            if (!virtualPath.StartsWith("~/") && !virtualPath.StartsWith("/"))
                throw new InvalidOperationException($"The value {virtualPath} for parameter {nameof(virtualPath)} must start with ~/ or /");

            // will occur if it starts with "/"
            if (Uri.IsWellFormedUriString(virtualPath, UriKind.Absolute))
                return virtualPath;

            var fullPath = ApplicationVirtualPath.EnsureEndsWith('/') + virtualPath.TrimStart('~', '/');

            return fullPath;
        }
    }


}
