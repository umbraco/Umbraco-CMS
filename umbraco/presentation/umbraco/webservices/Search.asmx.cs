using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;

using System.Web.Script.Services;
using System.Collections.Generic;
using umbraco.cms.businesslogic.index;

namespace umbraco.presentation.webservices {
    /// <summary>
    /// Summary description for Search
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class Search : System.Web.Services.WebService {

        [WebMethod]
        [ScriptMethod]
        public List<SearchItem> GetDocumentsBySearch(string query, int startNodeId, string contextID, int maxResults) {
            if (BasePages.BasePage.ValidateUserContextID(contextID)) {

                query = query.Trim();

                // Check for fulltext or title search
                string prefix = "";
                if (query.Length > 0 && query.Substring(0, 1) != "*")
                    prefix = "Text:";
                else
                    query = query.Substring(1, query.Length - 1);


                // Check for spaces
                if (query.IndexOf("\"") == -1 && query.Trim().IndexOf(" ") > 0) {
                    string[] queries = query.Split(" ".ToCharArray());
                    query = "";
                    for (int i = 0; i < queries.Length; i++)
                        query += prefix + queries[i] + "* AND ";
                    query = query.Substring(0, query.Length - 5);
                } else
                    query = prefix + query + "*";


                return cms.businesslogic.index.searcher.Search(
    cms.businesslogic.web.Document._objectType, query, maxResults);


            } else {
                throw new ArgumentException("Error validating credentials");
            }
        }
    }
}
