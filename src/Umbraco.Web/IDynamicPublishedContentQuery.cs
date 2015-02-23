using System.Collections.Generic;
using System.Xml.XPath;
using Umbraco.Core.Xml;

namespace Umbraco.Web
{
    /// <summary>
    /// Query methods used for accessing content dynamically in templates
    /// </summary>
    public interface IDynamicPublishedContentQuery
    {
        dynamic Content(int id);
        dynamic ContentSingleAtXPath(string xpath, params XPathVariable[] vars);
        dynamic ContentSingleAtXPath(XPathExpression xpath, params XPathVariable[] vars);
        dynamic Content(IEnumerable<int> ids);
        dynamic ContentAtXPath(string xpath, params XPathVariable[] vars);
        dynamic ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars);
        dynamic ContentAtRoot();
        
        dynamic Media(int id);
        dynamic Media(IEnumerable<int> ids);
        dynamic MediaAtRoot();

        /// <summary>
        /// Searches content
        /// </summary>
        /// <param name="term"></param>
        /// <param name="useWildCards"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        dynamic Search(string term, bool useWildCards = true, string searchProvider = null);

        /// <summary>
        /// Searhes content
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        dynamic Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null);
    }
}