using System;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    [Flags]
    public enum ContentCacheDataSerializerEntityType
    {
        Document = 1,
        Media = 2,
        Member = 4
    }

}
