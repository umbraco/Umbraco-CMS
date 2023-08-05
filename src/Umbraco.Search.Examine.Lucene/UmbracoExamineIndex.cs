using Examine;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Examine.Extensions;
using Umbraco.Search.Indexing.Notifications;
using Umbraco.Search.ValueSet.ValueSetBuilders;

namespace Umbraco.Search.Examine.Lucene;

public class UmbracoExamineIndex
{
    public UmbracoExamineLuceneIndex ExamineIndex;
    public IEventAggregator EventAggregator;

    /// <summary>
    ///
    /// </summary>
    /// <param name="examineIndex"></param>
    /// <param name="eventAggregator"></param>
    public UmbracoExamineIndex(UmbracoExamineLuceneIndex examineIndex, IEventAggregator eventAggregator)
    {
        ExamineIndex = examineIndex;
        EventAggregator = eventAggregator;
    }
};

public class UmbracoExamineIndex<T> : UmbracoExamineIndex, IUmbracoIndex<T> where T : IUmbracoEntity
{
    private readonly IValueSetBuilder<T> _valueSetBuilder;
    private readonly IDisposable[]? _attachedDisposables;

    public UmbracoExamineIndex(IIndex examineIndex, IValueSetBuilder<T> valueSetBuilder,
        IEventAggregator eventAggregator) : base(
        (UmbracoExamineLuceneIndex)examineIndex, eventAggregator)
    {
        _valueSetBuilder = valueSetBuilder;
        examineIndex.IndexOperationComplete += runIndexOperationComplete;
    }

    public UmbracoExamineIndex(IIndex examineIndex, IValueSetBuilder<T> valueSetBuilder,
        IEventAggregator eventAggregator,
        params IDisposable[]? attachedDisposables) : base((UmbracoExamineLuceneIndex)examineIndex, eventAggregator)
    {
        _valueSetBuilder = valueSetBuilder;
        _attachedDisposables = attachedDisposables;
        examineIndex.IndexOperationComplete += runIndexOperationComplete;
    }

    private void runIndexOperationComplete(object? sender, IndexOperationEventArgs e)
    {
        IndexOperationComplete?.Invoke(this, e);
    }

    public string Name => ExamineIndex.Name;
    public Action<object?, EventArgs>? IndexOperationComplete { get; set; }
    public bool Exists() => ExamineIndex.IndexExists();

    public ISearchEngine? SearchEngine { get { return ExamineIndex.SearchEngine; } }
    public long GetDocumentCount() => ExamineIndex.GetDocumentCount();

    public void Create() => ExamineIndex.CreateIndex();
    public IEnumerable<string> GetFieldNames() => ExamineIndex.FieldDefinitions.Select(x => x.Name);

    public void RemoveFromIndex(IEnumerable<string> ids)
    {
        EventAggregator.Publish(new RemoveFromIndexNotification(ids));

        ExamineIndex.DeleteFromIndex(ids);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="items"></param>
    /// <typeparam name="T"></typeparam>
    public void IndexItems(T[] items)
    {
        var valueSet = _valueSetBuilder.GetValueSets(items);
        EventAggregator.Publish(new IndexingNotification(valueSet));


        ExamineIndex.IndexItems(valueSet.Select(x => x.ToExamineValueSet()));
    }

    public void Dispose()
    {
        if (_attachedDisposables?.Any() == true)
        {
            foreach (var disposable in _attachedDisposables)
            {
                disposable.Dispose();
            }
        }

        ExamineIndex.Dispose();
    }
}
