using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Core.Models;
using Umbraco.Web.PublishedCache.NuCache;

namespace Umbraco.Infrastructure.PublishedCache.Persistence
{
    public interface INuCacheContentRepository
    {
        void DeleteContentItem(IContentBase item);
        IEnumerable<ContentNodeKit> GetAllContentSources();
        IEnumerable<ContentNodeKit> GetAllMediaSources();
        IEnumerable<ContentNodeKit> GetBranchContentSources(int id);
        IEnumerable<ContentNodeKit> GetBranchMediaSources(int id);
        ContentNodeKit GetContentSource(int id);
        ContentNodeKit GetMediaSource(int id);
        IEnumerable<ContentNodeKit> GetTypeContentSources(IEnumerable<int> ids);
        IEnumerable<ContentNodeKit> GetTypeMediaSources(IEnumerable<int> ids);

        /// <summary>
        /// Refreshes the nucache database row for the <see cref="IContent"/>
        /// </summary>
        void RefreshContent(IContent content);

        /// <summary>
        /// Refreshes the nucache database row for the <see cref="IContentBase"/> (used for media/members)
        /// </summary>
        void RefreshEntity(IContentBase content);

        /// <summary>
        /// Rebuilds the caches for content, media and/or members based on the content type ids specified
        /// </summary>
        /// <param name="groupSize">The operation batch size to process the items</param>
        /// <param name="contentTypeIds">If not null will process content for the matching content types, if empty will process all content</param>
        /// <param name="mediaTypeIds">If not null will process content for the matching media types, if empty will process all media</param>
        /// <param name="memberTypeIds">If not null will process content for the matching members types, if empty will process all members</param>
        void Rebuild(
            int groupSize = 5000,
            IReadOnlyCollection<int> contentTypeIds = null,
            IReadOnlyCollection<int> mediaTypeIds = null,
            IReadOnlyCollection<int> memberTypeIds = null);

        bool VerifyContentDbCache();
        bool VerifyMediaDbCache();
        bool VerifyMemberDbCache();
    }
}
