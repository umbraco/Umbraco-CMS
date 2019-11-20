using System;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;

namespace Umbraco.Web.Hosting
{
    public class AspNetHostingEnvironment : IHostingEnvironment
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IIOHelper _ioHelper;
        private string _localTempPath;

        public AspNetHostingEnvironment(IGlobalSettings globalSettings, IIOHelper ioHelper)
        {
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
            _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));

            SiteName = HostingEnvironment.SiteName;
            ApplicationId = HostingEnvironment.ApplicationID;
            ApplicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;
            ApplicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;

            IsDebugMode = HttpContext.Current?.IsDebuggingEnabled ?? globalSettings.DebugMode;
        }

        public string SiteName { get; }
        public string ApplicationId { get; }
        public string ApplicationPhysicalPath { get; }

        public string ApplicationVirtualPath { get; }
        public bool IsDebugMode { get; }
        public bool IsHosted => HostingEnvironment.IsHosted;
        public string MapPath(string path) => HostingEnvironment.MapPath(path);

        public string LocalTempPath
        {
            get
            {
                if (_localTempPath != null)
                    return _localTempPath;

                switch (_globalSettings.LocalTempStorageLocation)
                {
                    case LocalTempStorage.AspNetTemp:
                        return _localTempPath = System.IO.Path.Combine(HttpRuntime.CodegenDir, "UmbracoData");

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
                        return _localTempPath = _ioHelper.MapPath("~/App_Data/TEMP");
                }
            }
        }

    }
}
