using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;
using Umbraco.Infrastructure.PublishedCache.Persistence;
using Umbraco.Web;
using Umbraco.Web.PublishedCache.NuCache;

namespace Umbraco.Tests.Testing.Objects
{

    internal class TestDataSource : INuCacheContentService
    {

        private IPublishedModelFactory PublishedModelFactory { get; } = new NoopPublishedModelFactory();

        public TestDataSource(params ContentNodeKit[] kits)
            : this((IEnumerable<ContentNodeKit>) kits)
        { }

        public TestDataSource(IEnumerable<ContentNodeKit> kits) => Kits = kits.ToDictionary(x => x.Node.Id, x => x);

        public Dictionary<int, ContentNodeKit> Kits { get; }

        // note: it is important to clone the returned kits, as the inner
        // ContentNode is directly reused and modified by the snapshot service
        public ContentNodeKit GetContentSource(int id)
            => Kits.TryGetValue(id, out ContentNodeKit kit) ? kit.Clone(PublishedModelFactory) : default;

        public IEnumerable<ContentNodeKit> GetAllContentSources()
            => Kits.Values
                .OrderBy(x => x.Node.Level)
                .ThenBy(x => x.Node.ParentContentId)
                .ThenBy(x => x.Node.SortOrder)
                .Select(x => x.Clone(PublishedModelFactory));

        public IEnumerable<ContentNodeKit> GetBranchContentSources(int id)
            => Kits.Values
                .Where(x => x.Node.Path.EndsWith("," + id) || x.Node.Path.Contains("," + id + ","))
                .OrderBy(x => x.Node.Level)
                .ThenBy(x => x.Node.ParentContentId)
                .ThenBy(x => x.Node.SortOrder)
                .Select(x => x.Clone(PublishedModelFactory));

        public IEnumerable<ContentNodeKit> GetTypeContentSources(IEnumerable<int> ids)
            => Kits.Values
                .Where(x => ids.Contains(x.ContentTypeId))
                .OrderBy(x => x.Node.Level)
                .ThenBy(x => x.Node.ParentContentId)
                .ThenBy(x => x.Node.SortOrder)
                .Select(x => x.Clone(PublishedModelFactory));

        public ContentNodeKit GetMediaSource(int id) => default;

        public IEnumerable<ContentNodeKit> GetAllMediaSources() => Enumerable.Empty<ContentNodeKit>();

        public IEnumerable<ContentNodeKit> GetBranchMediaSources(int id) => Enumerable.Empty<ContentNodeKit>();

        public IEnumerable<ContentNodeKit> GetTypeMediaSources(IEnumerable<int> ids) => Enumerable.Empty<ContentNodeKit>();
        public void DeleteContentItem(IContentBase item) => throw new NotImplementedException();
        public void RefreshContent(IContent content) => throw new NotImplementedException();
        public void RefreshEntity(IContentBase content) => throw new NotImplementedException();
        public bool VerifyContentDbCache() => throw new NotImplementedException();
        public bool VerifyMediaDbCache() => throw new NotImplementedException();
        public bool VerifyMemberDbCache() => throw new NotImplementedException();
        public void Rebuild(int groupSize = 5000, IReadOnlyCollection<int> contentTypeIds = null, IReadOnlyCollection<int> mediaTypeIds = null, IReadOnlyCollection<int> memberTypeIds = null) => throw new NotImplementedException();
    }
}
