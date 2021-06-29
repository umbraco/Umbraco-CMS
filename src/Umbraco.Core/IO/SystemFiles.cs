using System.IO;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.IO
{
    public class SystemFiles
    {
        public static string TinyMceConfig => SystemDirectories.Config + "/tinyMceConfig.config";

        public static string UmbracoSettings => SystemDirectories.Config + "/UmbracoSettings.config";

        // TODO: Kill this off we don't have umbraco.config XML cache we now have NuCache
        public static string GetContentCacheXml(IGlobalSettings globalSettings)
        {
            return Path.Combine(globalSettings.LocalTempPath, "umbraco.config");
        }
    }
}
