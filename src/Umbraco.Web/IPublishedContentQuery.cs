using System;
using System.Collections.Generic;
using System.Xml.XPath;
using Examine.Search;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;

namespace Umbraco.Web
{
    // TODO: Merge this into IPublishedContentQuery for v9!
    public interface IPublishedContentQuery2 : IPublishedContentQuery
    {
        /// <summary>
        /// Searches content.
        /// </summary>
        /// <param name="term">The term to search.</param>
        /// <param name="skip">The amount of results to skip.</param>
        /// <param name="take">The amount of results to take/return.</param>
        /// <param name="totalRecords">The total amount of records.</param>
        /// <param name="culture">The culture (defaults to a culture insensitive search).</param>
        /// <param name="indexName">The name of the index to search (defaults to <see cref="Constants.UmbracoIndexes.ExternalIndexName" />).</param>
        /// <param name="loadedFields">The fields to load in the results of the search (defaults to all fields loaded).</param>
        /// <returns>
        /// The search results.
        /// </returns>
        /// <remarks>
        /// <para>
        /// When the <paramref name="culture" /> is not specified or is *, all cultures are searched.
        /// To search for only invariant documents and fields use null.
        /// When searching on a specific culture, all culture specific fields are searched for the provided culture and all invariant fields for all documents.
        /// </para>
        /// <para>While enumerating results, the ambient culture is changed to be the searched culture.</para>
        /// </remarks>
        IEnumerable<PublishedSearchResult> Search(string term, int skip, int take, out long totalRecords, string culture = "*", string indexName = Constants.UmbracoIndexes.ExternalIndexName, ISet<string> loadedFields = null);
    }

    /// <summary>
    /// Query methods used for accessing strongly typed content in templates
    /// </summary>
    public interface IPublishedContentQuery
    {
        IPublishedContent Content(int id);
        IPublishedContent Content(Guid id);
        IPublishedContent Content(Udi id);
        IPublishedContent ContentSingleAtXPath(string xpath, params XPathVariable[] vars);
        IEnumerable<IPublishedContent> Content(IEnumerable<int> ids);
        IEnumerable<IPublishedContent> Content(IEnumerable<Guid> ids);
        IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars);
        IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars);
        IEnumerable<IPublishedContent> ContentAtRoot();

        IPublishedContent Media(int id);
        IPublishedContent Media(Guid id);
        IPublishedContent Media(Udi id);
        IEnumerable<IPublishedContent> Media(IEnumerable<int> ids);
        IEnumerable<IPublishedContent> Media(IEnumerable<Guid> ids);
        IEnumerable<IPublishedContent> MediaAtRoot();

        /// <summary>
        /// Searches content.
        /// </summary>
        /// <param name="term">The term to search.</param>
        /// <param name="culture">The culture (defaults to a culture insensitive search).</param>
        /// <param name="indexName">The name of the index to search (defaults to <see cref="Constants.UmbracoIndexes.ExternalIndexName" />).</param>
        /// <returns>
        /// The search results.
        /// </returns>
        /// <remarks>
        /// <para>
        /// When the <paramref name="culture" /> is not specified or is *, all cultures are searched.
        /// To search for only invariant documents and fields use null.
        /// When searching on a specific culture, all culture specific fields are searched for the provided culture and all invariant fields for all documents.
        /// </para>
        /// <para>While enumerating results, the ambient culture is changed to be the searched culture.</para>
        /// </remarks>
        IEnumerable<PublishedSearchResult> Search(string term, string culture = "*", string indexName = Constants.UmbracoIndexes.ExternalIndexName);

        /// <summary>
        /// Searches content.
        /// </summary>
        /// <param name="term">The term to search.</param>
        /// <param name="skip">The amount of results to skip.</param>
        /// <param name="take">The amount of results to take/return.</param>
        /// <param name="totalRecords">The total amount of records.</param>
        /// <param name="culture">The culture (defaults to a culture insensitive search).</param>
        /// <param name="indexName">The name of the index to search (defaults to <see cref="Constants.UmbracoIndexes.ExternalIndexName" />).</param>
        /// <returns>
        /// The search results.
        /// </returns>
        /// <remarks>
        /// <para>
        /// When the <paramref name="culture" /> is not specified or is *, all cultures are searched.
        /// To search for only invariant documents and fields use null.
        /// When searching on a specific culture, all culture specific fields are searched for the provided culture and all invariant fields for all documents.
        /// </para>
        /// <para>While enumerating results, the ambient culture is changed to be the searched culture.</para>
        /// </remarks>
        IEnumerable<PublishedSearchResult> Search(string term, int skip, int take, out long totalRecords, string culture = "*", string indexName = Constants.UmbracoIndexes.ExternalIndexName);

        /// <summary>
        /// Executes the query and converts the results to <see cref="PublishedSearchResult" />.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>
        /// The search results.
        /// </returns>
        IEnumerable<PublishedSearchResult> Search(IQueryExecutor query);

        /// <summary>
        /// Executes the query and converts the results to <see cref="PublishedSearchResult" />.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="skip">The amount of results to skip.</param>
        /// <param name="take">The amount of results to take/return.</param>
        /// <param name="totalRecords">The total amount of records.</param>
        /// <returns>
        /// The search results.
        /// </returns>
        IEnumerable<PublishedSearchResult> Search(IQueryExecutor query, int skip, int take, out long totalRecords);
    }
}
