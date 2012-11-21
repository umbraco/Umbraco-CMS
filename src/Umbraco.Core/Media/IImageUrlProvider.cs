using System.Collections.Specialized;

namespace Umbraco.Core.Media
{
    public interface IImageUrlProvider
    {
        string Name { get; }
        string GetImageUrlFromMedia(int mediaId, NameValueCollection parameters);
        string GetImageUrlFromFileName(string specifiedSrc, NameValueCollection parameters);
    }
}