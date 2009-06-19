using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.cms.businesslogic.index;

namespace umbraco.presentation.dialogs
{
    public partial class search : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.Page.Form.DefaultButton = this.searchButton.UniqueID;

            if (!IsPostBack && helper.Request("search") != "")
            {
                keyword.Text = helper.Request("search");
                doSearch();

            }
        }

        protected void search_Click(object sender, EventArgs e)
        {
            doSearch();


        }

        private void doSearch()
        {
            string query = keyword.Text;
            query = query.Trim();

            // Check for fulltext or title search
            string prefix = "";
            /*            if (query.Length > 0 && query.Substring(0, 1) != "*")
                            prefix = "Text:";
                        else
                            query = query.Substring(1, query.Length - 1);
                        */

            // Check for spaces
            if (query.IndexOf("\"") == -1 && query.Trim().IndexOf(" ") > 0)
            {
                string[] queries = query.Split(" ".ToCharArray());
                query = "";
                for (int i = 0; i < queries.Length; i++)
                    query += prefix + queries[i] + "* AND ";
                query = query.Substring(0, query.Length - 5);
            }
            else
                query = prefix + query + "*";

            searchResult.XPathNavigator = cms.businesslogic.index.searcher.SearchAsXml(
cms.businesslogic.web.Document._objectType, query, 100).CreateNavigator();

        }
    }
}
