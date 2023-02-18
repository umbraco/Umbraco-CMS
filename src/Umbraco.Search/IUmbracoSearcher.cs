using Umbraco.Cms.Core.Models.Search;

namespace Umbraco.Search;

public interface IUmbracoSearcher<T>
{
    public IUmbracoSearchResult Search(string term, int page, int pageSize);
}
