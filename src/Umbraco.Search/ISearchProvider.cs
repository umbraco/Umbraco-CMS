using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Search;

public interface ISearchProvider
{
    IUmbracoIndex? GetIndex(string index);

    IUmbracoIndex<T>? GetIndex<T>(string index) where T : IUmbracoEntity;
    IUmbracoSearcher? GetSearcher(string index);
    IEnumerable<string> GetAllIndexes();
    IEnumerable<string> GetUnhealthyIndexes();
    OperationResult CreateIndex(string indexName);
    IEnumerable<string> GetAllSearchers();
}
