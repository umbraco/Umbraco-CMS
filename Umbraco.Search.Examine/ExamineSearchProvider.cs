using Examine;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Search;

public class ExamineSearchProvider : ISearchProvider
{
    private readonly IExamineManager _examineManager;

    public ExamineSearchProvider(IExamineManager examineManager)
    {
        _examineManager = examineManager;
    }

    public IUmbracoIndex<T> GetIndex<T>(string index)
    {
        var examineIndex = _examineManager.GetIndex(index);
        return new UmbracoExamineIndex<T>(examineIndex);
    }

    public IUmbracoSearcher<T> GetSearcher<T>(string index)
    {
        var examineIndex = _examineManager.GetIndex(index).Searcher;
        return new UmbracoExamineSearcher<T>(examineIndex);
    }
}

public class UmbracoExamineSearcher<T> : IUmbracoSearcher<T>
{
    private readonly ISearcher _examineIndex;
    public UmbracoExamineSearcher(ISearcher examineIndex)
    {
        _examineIndex = examineIndex;
    }
}
