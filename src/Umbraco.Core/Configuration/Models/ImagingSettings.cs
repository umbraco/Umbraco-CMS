namespace Umbraco.Core.Configuration.Models
{
    public class ImagingSettings
    {
        public ImagingCacheSettings Cache { get; set; } = new ImagingCacheSettings();

        public ImagingResizeSettings Resize { get; set; } = new ImagingResizeSettings();
    }
}
