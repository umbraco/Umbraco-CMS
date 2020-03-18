using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    public class ImagingSettings : IImagingSettings
    {
        private readonly IConfiguration _configuration;

        public ImagingSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int MaxBrowserCacheDays => _configuration.GetValue("Umbraco:CMS:Imaging:Cache:MaxBrowserCacheDays", 7);
        public int MaxCacheDays => _configuration.GetValue("Umbraco:CMS:Imaging:Cache:MaxCacheDays", 365);
        public uint CachedNameLength => _configuration.GetValue("Umbraco:CMS:Imaging:Cache:CachedNameLength", (uint) 8);
        public int MaxResizeWidth => _configuration.GetValue("Umbraco:CMS:Imaging:Resize:MaxWidth", 5000);
        public int MaxResizeHeight => _configuration.GetValue("Umbraco:CMS:Imaging:Resize:MaxHeight", 5000);
        public string CacheFolder => _configuration.GetValue("Umbraco:CMS:Imaging:Cache:Folder", "../App_Data/Cache");
    }
}
