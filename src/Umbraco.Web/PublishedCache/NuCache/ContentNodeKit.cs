using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // what's needed to actually build a content node
    struct ContentNodeKit
    {
        public ContentNode Node;
        public int ContentTypeId;
        public ContentData DraftData;
        public ContentData PublishedData;

        public bool IsEmpty => Node == null;

        public void Build(PublishedContentType contentType, IFacadeAccessor facadeAccessor)
        {
            Node.SetContentTypeAndData(contentType, DraftData, PublishedData, facadeAccessor);
        }
    }
}
