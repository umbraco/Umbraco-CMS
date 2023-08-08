using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.ValueSet.ValueSetBuilders;

namespace Umbraco.Search.Lifti;

public class UmbracoMemoryIndex<T> : IUmbracoIndex<T> where T : IUmbracoEntity
{
    private readonly ILiftiIndex? _index;
    private readonly IValueSetBuilder<T> _valueSetBuilder;

    public UmbracoMemoryIndex(ILiftiIndex? index, IValueSetBuilder<T> valueSetBuilder)
    {
        _index = index;
        _valueSetBuilder = valueSetBuilder;
    }

    public void IndexItems(T[] members)
    {
        var valueSets = _valueSetBuilder.GetValueSets(members);
        var t = Task.Factory.StartNew(async () => await   _index?.LiftiIndex.AddRangeAsync(valueSets)!);

        t.Result.Wait();
        t.Wait();
    }

    public void Dispose()
    {
    }

    public string Name => _index?.Name ?? "Index not Register Correctly";

    /// <summary>
    ///    An event that is triggered when an index operation is complete
    /// </summary>
    public Action<object?, EventArgs>? IndexOperationComplete { get; set; }

    public ISearchEngine? SearchEngine { get; } = new LiftiSearchEngine();

    public long GetDocumentCount()
    {
        return _index?.LiftiIndex.Count ?? 0;
    }

    public bool Exists()
    {
        return true;
    }

    public void Create()
    {
    }

    public IEnumerable<string> GetFieldNames()
    {
        return _index?.LiftiIndex.FieldLookup.AllFieldNames ?? new List<string>();
    }

    public void RemoveFromIndex(IEnumerable<string> ids)
    {
        foreach (var id in ids)
        {
            _index?.LiftiIndex.RemoveAsync(id);
        }
    }
}
