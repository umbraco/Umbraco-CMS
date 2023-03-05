using Examine;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Search.Examine;

public class UmbracoExamineIndex<T> : IUmbracoIndex<T>
{
    private readonly UmbracoExamineLuceneIndex _examineIndex;
    private readonly IValueSetBuilder<T> _valueSetBuilder;

    public UmbracoExamineIndex(IIndex examineIndex, IValueSetBuilder<T> valueSetBuilder)
    {
        _examineIndex = (UmbracoExamineLuceneIndex)examineIndex;
        _valueSetBuilder = valueSetBuilder;
        examineIndex.IndexOperationComplete += runIndexOperationComplete;
    }

    private void runIndexOperationComplete(object? sender, IndexOperationEventArgs e)
    {
        IndexOperationComplete?.Invoke(this, e);
    }

    public bool EnableDefaultEventHandler { get; } = true;
    public bool PublishedValuesOnly { get; }
    public string Name => _examineIndex.Name;
    public Action<object?, EventArgs>? IndexOperationComplete { get; set; }
    public bool Exists() => _examineIndex.IndexExists();

    public long GetDocumentCount() => _examineIndex.GetDocumentCount();

    public void Create() => _examineIndex.CreateIndex();
    public IEnumerable<string> GetFieldNames() => _examineIndex.FieldDefinitions.Select(x=>x.Name);

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
