using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Search;

public interface ISearchProvider
{
    IUmbracoIndex<T> GetIndex<T>(string index);
    IUmbracoSearcher<T> GetSearcher<T>(string index);
}
