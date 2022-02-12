using System.Runtime.InteropServices;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Extensions
{
    public static class HostingEnvironmentExtensions
    {
        /// <summary>
        /// Check to see if the system is a case sensitive filesytem
        /// </summary>
        public static bool IsCaseSensitiveFileSystem(this IHostingEnvironment hostingEnv)
        {
            return !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
    }
}
