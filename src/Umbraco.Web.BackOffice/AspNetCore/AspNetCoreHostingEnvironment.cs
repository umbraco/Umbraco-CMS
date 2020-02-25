using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Umbraco.Core;
using Umbraco.Core.Configuration;


namespace Umbraco.Web.BackOffice.AspNetCore
{
    public class AspNetCoreHostingEnvironment : Umbraco.Core.Hosting.IHostingEnvironment
    {
        private readonly IHostingSettings _hostingSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private string _localTempPath;

        public AspNetCoreHostingEnvironment(IHostingSettings hostingSettings, IWebHostEnvironment webHostEnvironment)
        {
            _hostingSettings = hostingSettings ?? throw new ArgumentNullException(nameof(hostingSettings));
            _webHostEnvironment = webHostEnvironment;

            SiteName = webHostEnvironment.ApplicationName;
            ApplicationId = AppDomain.CurrentDomain.Id.ToString();
            ApplicationPhysicalPath = webHostEnvironment.ContentRootPath;

            ApplicationVirtualPath = "/"; //TODO how to find this, This is a server thing, not application thing.
            // IISVersion = HttpRuntime.IISVersion;
            IsDebugMode =  _hostingSettings.DebugMode;
        }

        public string SiteName { get; }
        public string ApplicationId { get; }
        public string ApplicationPhysicalPath { get; }
        public string LocalTempPath
        {
            get
            {
                if (_localTempPath != null)
                    return _localTempPath;

                switch (_hostingSettings.LocalTempStorageLocation)
                {
                    case LocalTempStorage.AspNetTemp:
                         return _localTempPath = System.IO.Path.Combine(Path.GetTempPath(),ApplicationId,  "UmbracoData");

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
        public string ApplicationVirtualPath { get; }
        public bool IsDebugMode { get; }
        public bool IsHosted { get; } = true;
        public Version IISVersion { get; }
        public string MapPath(string path) => Path.Combine(_webHostEnvironment.WebRootPath, path);

        public string ToAbsolute(string virtualPath, string root) => throw new NotImplementedException();

        public void LazyRestartApplication()
        {
            throw new NotImplementedException();
        }

        public void RegisterObject(IRegisteredObject registeredObject)
        {
            throw new NotImplementedException();
        }

        public void UnregisterObject(IRegisteredObject registeredObject)
        {
            throw new NotImplementedException();
        }
    }


}
