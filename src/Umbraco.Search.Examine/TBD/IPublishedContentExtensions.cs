using Examine;
using Examine.Search;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Search.Examine.TBD;

public static class IPublishedContentExtensions
{
    #region Search


    public static IEnumerable<PublishedSearchResult> SearchDescendants(
        this IPublishedContent content,
        IExamineManager examineManager,
        IUmbracoContextAccessor umbracoContextAccessor,
        string term,
        string? indexName = null)
    {
        indexName = string.IsNullOrEmpty(indexName) ? Constants.UmbracoIndexes.ExternalIndexName : indexName;
        if (!examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            throw new InvalidOperationException("No index found with name " + indexName);
        }

        // var t = term.Escape().Value;
        // var luceneQuery = "+__Path:(" + content.Path.Replace("-", "\\-") + "*) +" + t;
        IBooleanOperation? query = index.Searcher.CreateQuery()
            .Field(UmbracoSearchFieldNames.IndexPathFieldName, (content.Path + ",").MultipleCharacterWildcard())
            .And()
            .ManagedQuery(term);
        IUmbracoContext umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
        return query.Execute().ToPublishedSearchResults(umbracoContext.Content);
    }

    public static IEnumerable<PublishedSearchResult> SearchChildren(
        this IPublishedContent content,
        IExamineManager examineManager,
        IUmbracoContextAccessor umbracoContextAccessor,
        string term,
        string? indexName = null)
    {
        indexName = string.IsNullOrEmpty(indexName) ? Constants.UmbracoIndexes.ExternalIndexName : indexName;
        if (!examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            throw new InvalidOperationException("No index found with name " + indexName);
        }

        // var t = term.Escape().Value;
        // var luceneQuery = "+parentID:" + content.Id + " +" + t;
        IBooleanOperation? query = index.Searcher.CreateQuery()
            .Field("parentID", content.Id)
            .And()
            .ManagedQuery(term);
        IUmbracoContext umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();

        return query.Execute().ToPublishedSearchResults(umbracoContext.Content);
    }

    #endregion
}
