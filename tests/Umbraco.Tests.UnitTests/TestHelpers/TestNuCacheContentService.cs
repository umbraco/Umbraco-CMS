using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.Persistence;

namespace Umbraco.Cms.Tests.UnitTests.TestHelpers;

public class TestNuCacheContentService : INuCacheContentService
{
    public TestNuCacheContentService(params ContentNodeKit[] kits)
        : this((IEnumerable<ContentNodeKit>)kits)
    {
    }

    public TestNuCacheContentService(
        IEnumerable<ContentNodeKit> contentKits,
        IEnumerable<ContentNodeKit> mediaKits = null)
    {
        ContentKits = contentKits?.ToDictionary(x => x.Node.Id, x => x) ?? new Dictionary<int, ContentNodeKit>();
        MediaKits = mediaKits?.ToDictionary(x => x.Node.Id, x => x) ?? new Dictionary<int, ContentNodeKit>();
    }

    private IPublishedModelFactory PublishedModelFactory { get; } = new NoopPublishedModelFactory();

    public Dictionary<int, ContentNodeKit> ContentKits { get; }

    public Dictionary<int, ContentNodeKit> MediaKits { get; }

    // note: it is important to clone the returned kits, as the inner
    // ContentNode is directly reused and modified by the snapshot service
    public ContentNodeKit GetContentSource(int id)
        => ContentKits.TryGetValue(id, out var kit) ? kit.Clone(PublishedModelFactory) : default;

    public IEnumerable<ContentNodeKit> GetAllContentSources()
        => ContentKits.Values
            .OrderBy(x => x.Node.Level)
            .ThenBy(x => x.Node.ParentContentId)
            .ThenBy(x => x.Node.SortOrder)
            .Select(x => x.Clone(PublishedModelFactory));

    public IEnumerable<ContentNodeKit> GetBranchContentSources(int id)
        => ContentKits.Values
            .Where(x => x.Node.Path.EndsWith("," + id) || x.Node.Path.Contains("," + id + ","))
            .OrderBy(x => x.Node.Level)
            .ThenBy(x => x.Node.ParentContentId)
            .ThenBy(x => x.Node.SortOrder)
            .Select(x => x.Clone(PublishedModelFactory));

    public IEnumerable<ContentNodeKit> GetTypeContentSources(IEnumerable<int> ids)
        => ContentKits.Values
            .Where(x => ids.Contains(x.ContentTypeId))
            .OrderBy(x => x.Node.Level)
            .ThenBy(x => x.Node.ParentContentId)
            .ThenBy(x => x.Node.SortOrder)
            .Select(x => x.Clone(PublishedModelFactory));

    public ContentNodeKit GetMediaSource(int id)
        => MediaKits.TryGetValue(id, out var kit) ? kit.Clone(PublishedModelFactory) : default;

    public IEnumerable<ContentNodeKit> GetAllMediaSources()
        => MediaKits.Values
            .OrderBy(x => x.Node.Level)
            .ThenBy(x => x.Node.ParentContentId)
            .ThenBy(x => x.Node.SortOrder)
            .Select(x => x.Clone(PublishedModelFactory));

    public IEnumerable<ContentNodeKit> GetBranchMediaSources(int id)
        => MediaKits.Values
            .Where(x => x.Node.Path.EndsWith("," + id) || x.Node.Path.Contains("," + id + ","))
            .OrderBy(x => x.Node.Level)
            .ThenBy(x => x.Node.ParentContentId)
            .ThenBy(x => x.Node.SortOrder)
            .Select(x => x.Clone(PublishedModelFactory));

    public IEnumerable<ContentNodeKit> GetTypeMediaSources(IEnumerable<int> ids)
        => MediaKits.Values
            .Where(x => ids.Contains(x.ContentTypeId))
            .OrderBy(x => x.Node.Level)
            .ThenBy(x => x.Node.ParentContentId)
            .ThenBy(x => x.Node.SortOrder)
            .Select(x => x.Clone(PublishedModelFactory));

    public void DeleteContentItem(IContentBase item) => throw new NotImplementedException();

    public void DeleteContentItems(IEnumerable<IContentBase> items) => throw new NotImplementedException();

    public void RefreshContent(IContent content) => throw new NotImplementedException();

    public void RebuildDatabaseCacheIfSerializerChanged() => throw new NotImplementedException();

    public void RefreshMedia(IMedia media) => throw new NotImplementedException();

    public void RefreshMember(IMember member) => throw new NotImplementedException();

    public void Rebuild(
        IReadOnlyCollection<int> contentTypeIds = null,
        IReadOnlyCollection<int> mediaTypeIds = null,
        IReadOnlyCollection<int> memberTypeIds = null) =>
        throw new NotImplementedException();

    public bool VerifyContentDbCache() => throw new NotImplementedException();

    public bool VerifyMediaDbCache() => throw new NotImplementedException();

    public bool VerifyMemberDbCache() => throw new NotImplementedException();
}
