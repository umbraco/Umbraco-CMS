using System.Collections.Generic;

namespace Umbraco.Core.Media
{
    public interface IImageUrlProvider
    {
        string Name { get; }
        string GetImageUrlFromMedia(int mediaId, IDictionary<string, string> parameters);
        string GetImageUrlFromFileName(string specifiedSrc, IDictionary<string, string> parameters);
    }
}