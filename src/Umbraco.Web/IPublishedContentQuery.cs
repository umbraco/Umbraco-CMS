using System;
using System.Collections.Generic;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;

namespace Umbraco.Web
{
    using Examine = global::Examine;

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
        IEnumerable<PublishedSearchResult> Search(string term, bool useWildCards = true, string indexName = null);

        /// <summary>
        /// Searches content.
        /// </summary>
        IEnumerable<PublishedSearchResult> Search(int skip, int take, out long totalRecords, string term, bool useWildCards = true, string indexName = null);

        /// <summary>
        /// Searches content.
        /// </summary>
        IEnumerable<PublishedSearchResult> Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.ISearcher searchProvider = null);

        /// <summary>
        /// Searches content.
        /// </summary>
        IEnumerable<PublishedSearchResult> Search(int skip, int take, out long totalRecords, Examine.SearchCriteria.ISearchCriteria criteria, Examine.ISearcher searcher = null);
    }
}
