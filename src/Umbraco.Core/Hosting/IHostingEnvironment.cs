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

        bool IsDebugMode { get; }
        /// <summary>
        /// Gets a value indicating whether Umbraco is hosted.
        /// </summary>
        bool IsHosted { get; }
        Version IISVersion { get; }
        string MapPath(string path);

        /// <summary>
        /// Maps a virtual path to the application's web root
        /// </summary>
        /// <param name="virtualPath">The virtual path. Must start with either ~/ or / else an exception is thrown.</param>
        /// <returns></returns>
        /// <remarks>
        /// This maps the virtual path syntax to the web root. For example when hosting in a virtual directory called "site" and the value "~/pages/test" is passed in, it will
        /// map to "/site/pages/test" where "/site" is the value of <see cref="ApplicationVirtualPath"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// If virtualPath does not start with ~/ or /
        /// </exception>
        string ToAbsolute(string virtualPath);
    }
}
