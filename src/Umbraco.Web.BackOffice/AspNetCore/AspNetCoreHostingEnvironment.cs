
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

        public AspNetCoreHostingEnvironment(IHostingSettings hostingSettings, IWebHostEnvironment webHostEnvironment)
        {
            _hostingSettings = hostingSettings ?? throw new ArgumentNullException(nameof(hostingSettings));
            _webHostEnvironment = webHostEnvironment;

            SiteName = webHostEnvironment.ApplicationName;
            ApplicationPhysicalPath = webHostEnvironment.ContentRootPath;
            IsHosted = true;
            ApplicationVirtualPath = "/"; //TODO how to find this, This is a server thing, not application thing.
            ApplicationId = AppDomain.CurrentDomain.Id.ToString();
            CurrentDomainId = AppDomain.CurrentDomain.Id;
            // IISVersion = HttpRuntime.IISVersion;
        }

        public string SiteName { get; }
        public string ApplicationId { get; }
        public string ApplicationPhysicalPath { get; }
        public string LocalTempPath { get; }
        public string ApplicationVirtualPath { get; }
        public int CurrentDomainId { get; }
        public bool IsDebugMode => _hostingSettings.DebugMode;
        public bool IsHosted { get; }
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
