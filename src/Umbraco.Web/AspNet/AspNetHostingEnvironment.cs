using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;

namespace Umbraco.Web.Hosting
{
    // TODO: This has been migrated to netcore
    public class AspNetHostingEnvironment : IHostingEnvironment
    {

        private readonly HostingSettings _hostingSettings;
        private string _localTempPath;


        public AspNetHostingEnvironment(IOptions<HostingSettings> hostingSettings)
        {
            _hostingSettings = hostingSettings.Value ?? throw new ArgumentNullException(nameof(hostingSettings));
            SiteName = HostingEnvironment.SiteName;
            ApplicationId = HostingEnvironment.ApplicationID;
            // when we are not hosted (i.e. unit test or otherwise) we'll need to get the root path from the executing assembly
            ApplicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath ?? Assembly.GetExecutingAssembly().GetRootDirectorySafe();
            ApplicationVirtualPath = _hostingSettings.ApplicationVirtualPath?.EnsureStartsWith('/')
                                     ?? HostingEnvironment.ApplicationVirtualPath?.EnsureStartsWith("/")
                                     ?? "/";
            IISVersion = HttpRuntime.IISVersion;
        }

        public string SiteName { get; }
        public string ApplicationId { get; }
        public string ApplicationPhysicalPath { get; }

        public string ApplicationVirtualPath { get; }

        public bool IsDebugMode => HttpContext.Current?.IsDebuggingEnabled ?? _hostingSettings.Debug;
        /// <inheritdoc/>
        public bool IsHosted => (HttpContext.Current != null || HostingEnvironment.IsHosted);

        public Version IISVersion { get; }

        public string MapPathWebRoot(string path)
        {
            if (HostingEnvironment.IsHosted)
                return HostingEnvironment.MapPath(path);

            // this will be the case in unit tests, we'll manually map the path
            var newPath = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            return newPath.StartsWith(ApplicationPhysicalPath) ? newPath : Path.Combine(ApplicationPhysicalPath, newPath.TrimStart('~', '/'));
        }

        public string MapPathContentRoot(string path) => MapPathWebRoot(path);

        public string ToAbsolute(string virtualPath) => VirtualPathUtility.ToAbsolute(virtualPath, ApplicationVirtualPath);


        public string LocalTempPath
        {
            get
            {
                if (_localTempPath != null)
                    return _localTempPath;

                switch (_hostingSettings.LocalTempStorageLocation)
                {
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

                    default:
                        return _localTempPath = MapPathContentRoot(Constants.SystemDirectories.TempData);
                }
            }
        }

    }


}
