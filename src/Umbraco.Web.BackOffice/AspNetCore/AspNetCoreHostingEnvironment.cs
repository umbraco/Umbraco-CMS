using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public class AspNetCoreHostingEnvironment : Umbraco.Core.Hosting.IHostingEnvironment
    {
        

        private readonly IHostingSettings _hostingSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        private string _localTempPath;

        public AspNetCoreHostingEnvironment(IHostingSettings hostingSettings, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _hostingSettings = hostingSettings ?? throw new ArgumentNullException(nameof(hostingSettings));
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;

            SiteName = webHostEnvironment.ApplicationName;
            ApplicationId = AppDomain.CurrentDomain.Id.ToString();
            ApplicationPhysicalPath = webHostEnvironment.ContentRootPath;

            ApplicationVirtualPath = "/"; //TODO how to find this, This is a server thing, not application thing.
            IISVersion = new Version(0, 0); // TODO not necessary IIS
            IsDebugMode =  _hostingSettings.DebugMode;
        }

        public bool IsHosted { get; } = true;
        public string SiteName { get; }
        public string ApplicationId { get; }
        public string ApplicationPhysicalPath { get; }

        public string ApplicationVirtualPath { get; }
        public bool IsDebugMode { get; }

        public Version IISVersion { get; }
        public string LocalTempPath
        {
            get
            {
                if (_localTempPath != null)
                    return _localTempPath;

                switch (_hostingSettings.LocalTempStorageLocation)
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
                        return _localTempPath = MapPath("~/App_Data/TEMP");
                }
            }
        }

        public string MapPath(string path)
        {
            var newPath = path.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_webHostEnvironment.WebRootPath, newPath);
        }

        // TODO: Need to take into account 'root' here
        public string ToAbsolute(string virtualPath, string root)
        {
            if (Uri.TryCreate(virtualPath, UriKind.Absolute, out _))
            {
                return virtualPath;
            }

            var segment = new PathString(virtualPath.Substring(1));
            var applicationPath = _httpContextAccessor.HttpContext.Request.PathBase;

            return applicationPath.Add(segment).Value;
        }

        
    }


}
