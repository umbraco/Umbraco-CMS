using System.Configuration.Provider;

namespace Umbraco.Core.Media
{
    public abstract class ImageUrlProviderBase : ProviderBase
    {
        public abstract string GetImageUrlFromMedia(int mediaId);
        public abstract string GetImageUrlFromFileName(string specifiedSrc);
    }
}