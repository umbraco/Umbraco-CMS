using System;
using System.Collections.Generic;

namespace Umbraco.Core.Media
{
    // note: because this interface is obsolete is is *not* IDiscoverable, and in case the
    // PluginManager is asked to find types implementing this interface it will fall back
    // to a complete scan.

    [Obsolete("IImageUrlProvider is no longer used and will be removed in future versions")]
    public interface IImageUrlProvider // IDiscoverable
    {
        string Name { get; }
        string GetImageUrlFromMedia(int mediaId, IDictionary<string, string> parameters);
        string GetImageUrlFromFileName(string specifiedSrc, IDictionary<string, string> parameters);
    }
}