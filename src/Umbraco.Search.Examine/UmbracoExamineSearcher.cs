using Examine;
using Umbraco.Cms.Core.Models.Search;

namespace Umbraco.Search.Examine;

public class UmbracoExamineSearcher<T> : IUmbracoSearcher<T>
{
    private readonly ISearcher _examineIndex;
    public UmbracoExamineSearcher(ISearcher examineIndex)
    {
        _examineIndex = examineIndex;
    }


    public IUmbracoSearchResult Search(string term, int page, int pageSize) => throw new NotImplementedException();

    public string Name => _examineIndex.Name;
}
