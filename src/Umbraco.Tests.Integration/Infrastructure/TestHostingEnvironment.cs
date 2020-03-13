using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Hosting;

namespace Umbraco.Tests.Integration.Infrastructure
{

    public class TestHostingEnvironment : IHostingEnvironment
    {
        public TestHostingEnvironment()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "umbraco-temp-" + Guid.NewGuid());
            if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
            LocalTempPath = tempPath;
            ApplicationPhysicalPath = tempPath; // same location for now
        }

        public string SiteName => "UmbracoIntegrationTests";

        public string ApplicationId { get; } = Guid.NewGuid().ToString();

        public string ApplicationPhysicalPath { get; private set; }

        public string LocalTempPath { get; private set; }

        public string ApplicationVirtualPath => "/";

        public bool IsDebugMode => true;

        public bool IsHosted => false;

        public Version IISVersion => new Version(0, 0); // TODO not necessary IIS

        public string MapPath(string path) => Path.Combine(ApplicationPhysicalPath, path);

        public string ToAbsolute(string virtualPath, string root) => virtualPath.TrimStart('~');

        public void RegisterObject(IRegisteredObject registeredObject)
        {
        }

        public void UnregisterObject(IRegisteredObject registeredObject)
        {
        }
    }
}
