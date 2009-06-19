using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Services;
using System.Xml;

using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using umbraco.cms.businesslogic.index;
namespace umbraco.presentation.dashboard
{
	/// <summary>
	/// Summary description for search.
	/// </summary>
	public partial class search : BasePages.UmbracoEnsuredPage
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{

            // Put user code to initialize the page here
			if (helper.Request("q").Trim() != "" && this.getUser() != null) 
			{
                if (!Indexer.IsReindexing()) {
                    string query = helper.Request("q").Trim();

                    // Check for fulltext or title search
                    string prefix = "";
                    if (query.Length > 0 && !query.StartsWith("Id")) {
                        if (query.Substring(0, 1) != "*")
                            prefix = "Text:";
                        else
                            query = query.Substring(1, query.Length - 1);
                    }

                    // Check for spaces
                    if (query.IndexOf("\"") == -1 && query.Trim().IndexOf(" ") > 0) {
                        string[] queries = query.Split(" ".ToCharArray());
                        query = "";
                        for (int i = 0; i < queries.Length; i++)
                            query += prefix + queries[i] + "* AND ";
                        query = query.Substring(0, query.Length - 5);
                    } else
                        query = prefix + query + "*";

                    try {
                        List<SearchItem> items = searcher.Search(
            cms.businesslogic.web.Document._objectType, query, 20);
                        ResultHelper rh = new ResultHelper(items);
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        Response.Write(js.Serialize(rh));
                    } catch {
                        Indexer.ReIndex((HttpApplication)HttpContext.Current.ApplicationInstance);
                        Response.Write("[error:'indexing']");
                    }
                } else {
                    Response.Write("[error:'indexing']");
                }
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
		}
		#endregion
	}

    // helper class for the autocomplete json
    [Serializable]
    public class ResultHelper {
        private List<SearchItem> m_items;

        public List<SearchItem> results {
            get { return m_items; }
            set { m_items = value; }
        }

        public ResultHelper() { }
        public ResultHelper(List<SearchItem> items) { m_items = items; }
	
    }
}
