using System;

namespace Umbraco.Core.Hosting
{
    public interface IHostingEnvironment
    {
        string SiteName { get; }
        string ApplicationId { get; }
        string ApplicationPhysicalPath { get; }

        string LocalTempPath { get; }
        string ApplicationVirtualPath { get; }

        int CurrentDomainId { get; }

        bool IsDebugMode { get; }
        /// <summary>
        /// Gets a value indicating whether Umbraco is hosted.
        /// </summary>
        bool IsHosted { get; }
        Version IISVersion { get; }
        string MapPath(string path);
        string ToAbsolute(string virtualPath, string root);

        /// <summary>
        /// Terminates the current application. The application restarts the next time a request is received for it.
        /// </summary>
        void LazyRestartApplication();

        void RegisterObject(IRegisteredObject registeredObject);
        void UnregisterObject(IRegisteredObject registeredObject);
    }
}
