using System.Collections.Specialized;
using System.Configuration.Provider;

namespace Umbraco.Core.Media
{
    public abstract class ImageUrlProviderBase : ProviderBase
    {
        public abstract string GetImageUrlFromMedia(int mediaId, NameValueCollection parameters);
        public abstract string GetImageUrlFromFileName(string specifiedSrc, NameValueCollection parameters);
    }
}