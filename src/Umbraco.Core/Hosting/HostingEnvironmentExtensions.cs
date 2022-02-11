using System.IO;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Extensions
{
    public static class HostingEnvironmentExtensions
    {
        private static bool? s_isCaseSensitiveFileSystem;

        /// <summary>
        /// Check to see if the system is a case sensitive filesytem
        /// </summary>
        public static bool IsCaseSensitiveFileSystem(this IHostingEnvironment hostingEnv)
        {
            if (!s_isCaseSensitiveFileSystem.HasValue)
            {
                var tempDir = hostingEnv.MapPathContentRoot(Constants.SystemDirectories.TempData);

                // Ensure the temp dir exists
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }

                // Check whether it can be found if the casing is changed
                s_isCaseSensitiveFileSystem = !Directory.Exists(tempDir.ToUpper()) || !Directory.Exists(tempDir.ToLower());
            }

            return s_isCaseSensitiveFileSystem.Value;
        }
    }
}
