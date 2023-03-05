using Examine;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Search.Examine;

public class UmbracoExamineIndex
{
    public UmbracoExamineLuceneIndex ExamineIndex;

    public UmbracoExamineIndex(UmbracoExamineLuceneIndex examineIndex) => ExamineIndex = examineIndex;
};

public class UmbracoExamineIndex<T> : UmbracoExamineIndex,IUmbracoIndex<T>
{

    private readonly IValueSetBuilder<T> _valueSetBuilder;

    public UmbracoExamineIndex(IIndex examineIndex, IValueSetBuilder<T> valueSetBuilder): base((UmbracoExamineLuceneIndex)examineIndex)
    {
        _valueSetBuilder = valueSetBuilder;
        examineIndex.IndexOperationComplete += runIndexOperationComplete;
    }

    private void runIndexOperationComplete(object? sender, IndexOperationEventArgs e)
    {
        IndexOperationComplete?.Invoke(this, e);
    }

    public bool EnableDefaultEventHandler { get; } = true;
    public bool PublishedValuesOnly { get; }
    public string Name => ExamineIndex.Name;
    public Action<object?, EventArgs>? IndexOperationComplete { get; set; }
    public bool Exists() => ExamineIndex.IndexExists();

    public long GetDocumentCount() => ExamineIndex.GetDocumentCount();

    public void Create() => ExamineIndex.CreateIndex();
    public IEnumerable<string> GetFieldNames() => ExamineIndex.FieldDefinitions.Select(x=>x.Name);

    /// <summary>
    ///
    /// </summary>
    /// <param name="items"></param>
    /// <typeparam name="T"></typeparam>
    public void IndexItems(T[] items)
    {
        var valueSet = _valueSetBuilder.GetValueSets(items);
        ExamineIndex.IndexItems(valueSet);
    }
}
