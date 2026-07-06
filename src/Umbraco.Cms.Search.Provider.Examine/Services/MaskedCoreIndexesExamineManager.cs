using System.Diagnostics.CodeAnalysis;
using Examine;

namespace Umbraco.Cms.Search.Provider.Examine.Services;

internal sealed class MaskedCoreIndexesExamineManager : IExamineManager
{
    private readonly string[] _coreIndexes =
    [
        Umbraco.Cms.Core.Constants.UmbracoIndexes.DeliveryApiContentIndexName,
        Umbraco.Cms.Core.Constants.UmbracoIndexes.ExternalIndexName,
        Umbraco.Cms.Core.Constants.UmbracoIndexes.InternalIndexName,
        Umbraco.Cms.Core.Constants.UmbracoIndexes.MembersIndexName,
    ];

    private readonly ExamineManager _inner;

    public MaskedCoreIndexesExamineManager(ExamineManager inner)
        => _inner = inner;

    public void Dispose()
        => _inner.Dispose();

    public bool TryGetIndex(string indexName, [MaybeNullWhen(false)] [UnscopedRef] out IIndex index)
    {
        if (_coreIndexes.Contains(indexName) is false)
        {
            return _inner.TryGetIndex(indexName, out index);
        }

        index = null;
        return false;
    }

    public bool TryGetSearcher(string searcherName, [MaybeNullWhen(false)] [UnscopedRef] out ISearcher searcher)
        => _inner.TryGetSearcher(searcherName, out searcher);

    public IEnumerable<IIndex> Indexes
        => _inner.Indexes.Where(index => _coreIndexes.Contains(index.Name) is false).ToArray();

    public IEnumerable<ISearcher> RegisteredSearchers
        => _inner.RegisteredSearchers;
}
