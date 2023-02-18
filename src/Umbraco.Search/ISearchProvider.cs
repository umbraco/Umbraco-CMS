using Umbraco.Cms.Core.Services;

namespace Umbraco.Search;

public interface ISearchProvider
{
    IUmbracoIndex<T> GetIndex<T>(string index);
    IUmbracoSearcher<T> GetSearcher<T>(string index);
    IEnumerable<string> GetAllIndexes();
    IEnumerable<string> GetUnhealthyIndexes();
    OperationResult CreateIndex(string indexName);
}
