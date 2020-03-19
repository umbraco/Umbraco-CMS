using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class ImagingSettings : IImagingSettings
    {
        private const string Prefix = Constants.Configuration.ConfigPrefix + "Imaging:";
        private const string CachePrefix = Prefix + "Cache:";
        private const string ResizePrefix = Prefix + "Resize:";
        private readonly IConfiguration _configuration;

        public ImagingSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int MaxBrowserCacheDays => _configuration.GetValue(CachePrefix + "MaxBrowserCacheDays", 7);
        public int MaxCacheDays => _configuration.GetValue(CachePrefix + "MaxCacheDays", 365);
        public uint CachedNameLength => _configuration.GetValue(CachePrefix + "CachedNameLength", (uint) 8);
        public string CacheFolder => _configuration.GetValue(CachePrefix + "Folder", "../App_Data/Cache");
        public int MaxResizeWidth => _configuration.GetValue(ResizePrefix + "MaxWidth", 5000);
        public int MaxResizeHeight => _configuration.GetValue(ResizePrefix + "MaxHeight", 5000);
    }
}
