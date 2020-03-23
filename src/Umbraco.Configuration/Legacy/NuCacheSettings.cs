using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Legacy
{
    public class NuCacheSettings : INuCacheSettings
    {
        public NuCacheSettings()
        {
            BTreeBlockSize = ConfigurationManager.AppSettings["Umbraco.Web.PublishedCache.NuCache.BTree.BlockSize"];
        }
        public string BTreeBlockSize { get; }
    }
}
