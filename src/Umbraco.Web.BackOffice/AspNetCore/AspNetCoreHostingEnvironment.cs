using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public class AspNetCoreHostingEnvironment : Umbraco.Core.Hosting.IHostingEnvironment
    {
        private readonly ConcurrentDictionary<IRegisteredObject, RegisteredObjectWrapper> _registeredObjects =
            new ConcurrentDictionary<IRegisteredObject, RegisteredObjectWrapper>();

        private readonly IHostingSettings _hostingSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private string _localTempPath;

        public AspNetCoreHostingEnvironment(IHostingSettings hostingSettings, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IHostApplicationLifetime hostHostApplicationLifetime)
        {
            _hostingSettings = hostingSettings ?? throw new ArgumentNullException(nameof(hostingSettings));
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _hostApplicationLifetime = hostHostApplicationLifetime;

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


        public string MapPath(string path) => Path.Combine(_webHostEnvironment.WebRootPath, path);

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

        public void RegisterObject(IRegisteredObject registeredObject)
        {
            var wrapped = new RegisteredObjectWrapper(registeredObject);
            if (!_registeredObjects.TryAdd(registeredObject, wrapped))
            {
                throw new InvalidOperationException("Could not register object");
            }

            var cancellationTokenRegistration = _hostApplicationLifetime.ApplicationStopping.Register(() => wrapped.Stop(true));
            wrapped.CancellationTokenRegistration = cancellationTokenRegistration;
        }

        public void UnregisterObject(IRegisteredObject registeredObject)
        {
            if (_registeredObjects.TryGetValue(registeredObject, out var wrapped))
            {
                wrapped.CancellationTokenRegistration.Unregister();
            }
        }


        private class RegisteredObjectWrapper
        {
            private readonly IRegisteredObject _inner;

            public RegisteredObjectWrapper(IRegisteredObject inner)
            {
                _inner = inner;
            }

            public CancellationTokenRegistration CancellationTokenRegistration { get; set; }

            public void Stop(bool immediate)
            {
                _inner.Stop(immediate);
            }
        }
    }


}
