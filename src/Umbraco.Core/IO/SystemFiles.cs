using System.IO;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Core.IO
{
    public class SystemFiles
    {
        public static string TinyMceConfig => Constants.SystemDirectories.Config + "/tinyMceConfig.config";

        // TODO: Kill this off we don't have umbraco.config XML cache we now have NuCache
        public static string GetContentCacheXml(IHostingEnvironment hostingEnvironment)
        {
            return Path.Combine(hostingEnvironment.LocalTempPath, "umbraco.config");
        }
    }
}
