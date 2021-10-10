using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Extensions
{
    public static class WebHostEnvironmentExtensions
    {
        private static string s_applicationId;

        /// <summary>
        /// Gets a unique application ID for this Umbraco website.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned value will be the same consistent value for an Umbraco website on a specific server and will the same
        /// between restarts of that Umbraco website/application on that specific server.
        /// </para>
        /// <para>
        /// The value of this does not necessarily distinguish between unique workers/servers for this Umbraco application.
        /// Usage of this must take into account that the same ApplicationId may be returned for the same
        /// Umbraco website hosted on different servers. Similarly the usage of this must take into account that a different
        /// ApplicationId may be returned for the same Umbraco website hosted on different servers.
        /// </para>
        /// </remarks>
        public static string GetApplicationId(this IWebHostEnvironment webHostEnvironment)
        {
            if (s_applicationId != null)
            {
                return s_applicationId;
            }

            s_applicationId = webHostEnvironment.ContentRootPath.GenerateHash();

            return s_applicationId;
        }

        /// <summary>
        /// Maps a virtual path to a physical path to the application's root.
        /// </summary>
        public static string MapPathContentRoot(this IWebHostEnvironment webHostEnvironment, string path) => MapPath(webHostEnvironment.ContentRootPath, path);

        /// <summary>
        /// Maps a virtual path to a physical path to the application's web root i.e. {{project}}/wwwroot
        /// </summary>
        public static string MapPathWebRoot(this IWebHostEnvironment webHostEnvironment, string path) => MapPath(webHostEnvironment.WebRootPath, path);

        private static string MapPath(string root, string path)
        {
            var newPath = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

            if (newPath.StartsWith(root))
            {
                throw new ArgumentException("The path appears to already be fully qualified.  Please remove the call to MapPath");
            }

            return Path.Combine(root, newPath.TrimStart(Core.Constants.CharArrays.TildeForwardSlashBackSlash));
        }
    }
}
