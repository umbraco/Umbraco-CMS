
using System;
using Microsoft.AspNetCore.Hosting;
using Umbraco.Core;
using Umbraco.Core.Configuration;


namespace Umbraco.Web.BackOffice.AspNetCore
{
    public class AspNetCoreHostingEnvironment : Umbraco.Core.Hosting.IHostingEnvironment
    {
        private readonly IHostingSettings _hostingSettings;

        public AspNetCoreHostingEnvironment(IHostingSettings hostingSettings, IHostingEnvironment hostingEnvironment)
        {
            // _hostingSettings = hostingSettings ?? throw new ArgumentNullException(nameof(hostingSettings));
            // SiteName = hostingEnvironment.ApplicationName;
            // ApplicationId = hostingEnvironment.ApplicationID;
            // ApplicationPhysicalPath = hostingEnvironment.WebRootPath;
            // ApplicationVirtualPath = hostingEnvironment.ApplicationVirtualPath;
            // CurrentDomainId = AppDomain.CurrentDomain.Id;
            // IISVersion = HttpRuntime.IISVersion;
        }

        public string SiteName { get; }
        public string ApplicationId { get; }
        public string ApplicationPhysicalPath { get; }
        public string LocalTempPath { get; }
        public string ApplicationVirtualPath { get; }
        public int CurrentDomainId { get; }
        public bool IsDebugMode { get; }
        public bool IsHosted { get; }
        public Version IISVersion { get; }
        public string MapPath(string path) => throw new NotImplementedException();

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
