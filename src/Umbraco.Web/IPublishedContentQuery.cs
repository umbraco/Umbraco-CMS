using System.Collections.Generic;
using System.Xml.XPath;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;

namespace Umbraco.Web
{
    /// <summary>
    /// Query methods used for accessing strongly typed content in templates
    /// </summary>
    public interface IPublishedContentQuery
    {
        IPublishedContent Content(int id);
        IPublishedContent ContentSingleAtXPath(string xpath, params XPathVariable[] vars);
        IEnumerable<IPublishedContent> Content(IEnumerable<int> ids);
        IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars);
        IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars);
        IEnumerable<IPublishedContent> ContentAtRoot();

        IPublishedContent Media(int id);
        IEnumerable<IPublishedContent> Media(IEnumerable<int> ids);
        IEnumerable<IPublishedContent> MediaAtRoot();

        /// <summary>
        /// Searches content
        /// </summary>
        /// <param name="term"></param>
        /// <param name="useWildCards"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        IEnumerable<IPublishedContent> Search(string term, bool useWildCards = true, string searchProvider = null);

        /// <summary>
        /// Searhes content
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        IEnumerable<IPublishedContent> Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null);
    }
}