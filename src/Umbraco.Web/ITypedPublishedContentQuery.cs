using System.Collections.Generic;
using System.Xml.XPath;
using Umbraco.Core.Models;
using Umbraco.Core.Xml;

namespace Umbraco.Web
{
    /// <summary>
    /// Query methods used for accessing strongly typed content in templates
    /// </summary>
    public interface ITypedPublishedContentQuery
    {
        IPublishedContent TypedContent(int id);
        IPublishedContent TypedContentSingleAtXPath(string xpath, params XPathVariable[] vars);
        IEnumerable<IPublishedContent> TypedContent(IEnumerable<int> ids);
        IEnumerable<IPublishedContent> TypedContentAtXPath(string xpath, params XPathVariable[] vars);
        IEnumerable<IPublishedContent> TypedContentAtXPath(XPathExpression xpath, params XPathVariable[] vars);
        IEnumerable<IPublishedContent> TypedContentAtRoot();

        IPublishedContent TypedMedia(int id);
        IEnumerable<IPublishedContent> TypedMedia(IEnumerable<int> ids);
        IEnumerable<IPublishedContent> TypedMediaAtRoot();

        /// <summary>
        /// Searches content
        /// </summary>
        /// <param name="term"></param>
        /// <param name="useWildCards"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        IEnumerable<IPublishedContent> TypedSearch(string term, bool useWildCards = true, string searchProvider = null);

        /// <summary>
        /// Searhes content
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        IEnumerable<IPublishedContent> TypedSearch(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null);
    }
}