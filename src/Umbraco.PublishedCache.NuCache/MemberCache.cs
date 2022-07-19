using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

public class MemberCache : IPublishedMemberCache, IDisposable
{
    private readonly PublishedContentTypeCache _contentTypeCache;
    private readonly bool _previewDefault;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private bool _disposedValue;

    public MemberCache(
        bool previewDefault,
        PublishedContentTypeCache contentTypeCache,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IVariationContextAccessor variationContextAccessor,
        IPublishedModelFactory publishedModelFactory)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _variationContextAccessor = variationContextAccessor;
        _publishedModelFactory = publishedModelFactory;
        _previewDefault = previewDefault;
        _contentTypeCache = contentTypeCache;
    }

    #region Content types

    public IPublishedContentType GetContentType(int id) => _contentTypeCache.Get(PublishedItemType.Member, id);

    public IPublishedContentType GetContentType(string alias) => _contentTypeCache.Get(PublishedItemType.Member, alias);

    public IPublishedContent? Get(IMember member)
        =>
            PublishedMember.Create(
                member,
                GetContentType(member.ContentTypeId),
                _previewDefault,
                _publishedSnapshotAccessor,
                _variationContextAccessor,
                _publishedModelFactory);

    public void Dispose() =>

        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);

    #endregion

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _contentTypeCache.Dispose();
            }

            _disposedValue = true;
        }
    }

    #endregion
}
