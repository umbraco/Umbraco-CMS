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
    /// <summary>
    ///
    /// </summary>
    /// <param name="items"></param>
    /// <typeparam name="T"></typeparam>
    public void IndexItems<T>(T[] items)
    {
        var valueSet = _valueSetBuilder.GetValueSets(items);
        _examineIndex.IndexItems(valueSet);
    }
}
