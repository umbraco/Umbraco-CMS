using System.IO;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;

namespace Umbraco.Core.IO
{
    public class SystemFiles
    {
        public static string TinyMceConfig => Constants.SystemDirectories.Config + "/tinyMceConfig.config";

        public static string TelemetricsIdentifier => Constants.SystemDirectories.Data + "/telemetrics-id.umb";

        // TODO: Kill this off we don't have umbraco.config XML cache we now have NuCache
        public static string GetContentCacheXml(IHostingEnvironment hostingEnvironment)
        {
            return Path.Combine(hostingEnvironment.LocalTempPath, "umbraco.config");
        }
    }
}
