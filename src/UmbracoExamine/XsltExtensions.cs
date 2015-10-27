using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using System.Xml.XPath;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Examine.Providers;
using Umbraco.Core.Macros;
using UmbracoExamine.DataServices;

namespace UmbracoExamine
{
    /// <summary>
    /// Methods to support Umbraco XSLT extensions.
    /// </summary>
    /// <remarks>
    /// XSLT extensions will ONLY work for provider that have a base class of BaseUmbracoIndexer
    /// </remarks>
    [XsltExtension("Examine")]
    
    public class XsltExtensions
    {
        ///<summary>
        /// Uses the provider specified to search, returning an XPathNodeIterator
        ///</summary>
        ///<param name="searchText"></param>
        ///<param name="useWildcards"></param>
        ///<param name="provider"></param>
        ///<param name="indexType"></param>
        ///<returns></returns>
        internal static XPathNodeIterator Search(string searchText, bool useWildcards, BaseSearchProvider provider, string indexType)
        {
            if (provider == null) throw new ArgumentNullException("provider");

            var results = provider.Search(searchText, useWildcards, indexType);
            return GetResultsAsXml(results);            
        }

        /// <summary>
        /// Uses the provider specified to search, returning an XPathNodeIterator
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="useWildcards">if set to <c>true</c> [use wildcards].</param>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="indexType">Type of the index.</param>
        /// <returns></returns>
        public static XPathNodeIterator Search(string searchText, bool useWildcards, string providerName, string indexType)
        {
            var provider = ExamineManager.Instance.SearchProviderCollection[providerName];

            return Search(searchText, useWildcards, provider, indexType);
        }

        /// <summary>
        /// Uses the provider specified to search, returning an XPathNodeIterator
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static XPathNodeIterator Search(string searchText, bool useWildcards, string providerName)
        {
            return Search(searchText, useWildcards, providerName, string.Empty);         
        }

        /// <summary>
        /// Uses the default provider specified to search, returning an XPathNodeIterator
        /// </summary>
        /// <param name="searchText">The search query</param>
        /// <param name="useWildcards">Enable a wildcard search query</param>
        /// <returns>A node-set of the search results</returns>
        public static XPathNodeIterator Search(string searchText, bool useWildcards)
        {
            return Search(searchText, useWildcards, ExamineManager.Instance.DefaultSearchProvider.Name);
        }

        /// <summary>
        /// Uses the default provider specified to search, returning an XPathNodeIterator
        /// </summary>
        /// <param name="searchText">The search query</param>
        /// <returns>A node-set of the search results</returns>
        public static XPathNodeIterator Search(string searchText)
        {
            return Search(searchText, true);
        }

        /// <summary>
        /// Will perform a search against the media index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchMediaOnly(string searchText, bool useWildcards, string providerName)
        {
            return Search(searchText, useWildcards, providerName, IndexTypes.Media);
        }

        /// <summary>
        /// Will perform a search against the media index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchMediaOnly(string searchText, bool useWildcards)
        {
            return SearchMediaOnly(searchText, useWildcards, ExamineManager.Instance.DefaultSearchProvider.Name);
        }

        /// <summary>
        /// Will perform a search against the media index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchMediaOnly(string searchText)
        {
            return SearchMediaOnly(searchText, true);
        }

        /// <summary>
        /// Searches the member only.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="useWildcards">if set to <c>true</c> [use wildcards].</param>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
        public static XPathNodeIterator SearchMemberOnly(string searchText, bool useWildcards, string providerName)
        {
            return Search(searchText, useWildcards, providerName, IndexTypes.Member);
        }

        /// <summary>
        /// Searches the member only.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="useWildcards">if set to <c>true</c> [use wildcards].</param>
        /// <returns></returns>
        public static XPathNodeIterator SearchMemberOnly(string searchText, bool useWildcards)
        {
            return SearchMemberOnly(searchText, useWildcards, ExamineManager.Instance.DefaultSearchProvider.Name);
        }

        /// <summary>
        /// Searches the member only.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <returns></returns>
        public static XPathNodeIterator SearchMemberOnly(string searchText)
        {
            return SearchMemberOnly(searchText, true);
        }

        /// <summary>
        /// Will perform a search against the content index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchContentOnly(string searchText, bool useWildcards, string providerName)
        {
            return Search(searchText, useWildcards, providerName, IndexTypes.Content);
        }


        /// <summary>
        /// Will perform a search against the content index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchContentOnly(string searchText, bool useWildcards)
        {
            return SearchContentOnly(searchText, useWildcards, ExamineManager.Instance.DefaultSearchProvider.Name);
        }

        /// <summary>
        /// Will perform a search against the content index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchContentOnly(string searchText)
        {
            return SearchContentOnly(searchText, true);
        }

        /// <summary>
        /// Gets the results as XML.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <returns></returns>
        private static XPathNodeIterator GetResultsAsXml(ISearchResults results)
        {
            // create the XDocument
            XDocument doc = new XDocument();

            // check there are any search results
            if (results.TotalItemCount > 0)
            {
                // create the root element
                XElement root = new XElement("nodes");

                // iterate through the search results
                foreach (SearchResult result in results)
                {
                    // create a new <node> element
                    XElement node = new XElement("node");

                    // create the @id attribute
                    XAttribute nodeId = new XAttribute("id", result.Id);

                    // create the @score attribute
                    XAttribute nodeScore = new XAttribute("score", result.Score);

                    // add the content
                    node.Add(nodeId, nodeScore);

                    foreach (KeyValuePair<String, String> field in result.Fields)
                    {
                        // create a new <data> element
                        XElement data = new XElement("data");

                        // create the @alias attribute
                        XAttribute alias = new XAttribute("alias", field.Key);

                        // assign the value to a CDATA section
                        XCData value = new XCData(field.Value);

                        // append the content
                        data.Add(alias, value);

                        // append the <data> element
                        node.Add(data);
                    }

                    // add the node
                    root.Add(node);
                }

                // add the root node
                doc.Add(root);
            }
            else
            {
                doc.Add(new XElement("error", "There were no search results."));
            }

            return doc.CreateNavigator().Select("/");
        }
    }
}