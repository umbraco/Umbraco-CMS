using Examine;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Search.Examine;

public class UmbracoExamineIndex<T> : IUmbracoIndex<T>
{
    private readonly IIndex _examineIndex;
    private readonly IValueSetBuilder<T> _valueSetBuilder;

    public UmbracoExamineIndex(IIndex examineIndex, IValueSetBuilder<T> valueSetBuilder)
    {
        _examineIndex = examineIndex;
        _valueSetBuilder = valueSetBuilder;
    }

    public bool EnableDefaultEventHandler { get; }
    public bool PublishedValuesOnly { get; }
    public string Name => _examineIndex.Name;
    public bool Exists() => _examineIndex.IndexExists();

    public long GetDocumentCount() => throw new NotImplementedException();

    public bool IndexExists() => throw new NotImplementedException();

    public void Create() => throw new NotImplementedException();

    /// <summary>
    ///
    /// </summary>
    /// <param name="items"></param>
    /// <typeparam name="T"></typeparam>
    public void IndexItems(T[] items)
    {
        var valueSet = _valueSetBuilder.GetValueSets(items);
        _examineIndex.IndexItems(valueSet);
    }
}
