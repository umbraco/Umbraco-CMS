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

namespace umbraco.cms.presentation
{
	/// <summary>
	/// Summary description for cacheBrowser.
	/// </summary>
	public partial class cacheBrowser : System.Web.UI.Page
	{
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Cache removal checks
			if (Request.QueryString["clearByType"] != null)
				cms.businesslogic.cache.Cache.ClearCacheObjectTypes(Request.QueryString["clearByType"]);
			else if (Request.QueryString["clearByKey"] != null)
				cms.businesslogic.cache.Cache.ClearCacheItem(Request.QueryString["clearByKey"]);

			// Put user code to initialize the page here
			Hashtable ht = cms.businesslogic.cache.Cache.ReturnCacheItemsOrdred();
			foreach(string key in ht.Keys) 
			{
				Response.Write("<a href=\"?key=" + key + "\">" + key + "</a>: " + ((ArrayList) ht[key]).Count.ToString() + " (<a href=\"?clearByType=" + key + "\">Delete</a>)<br />");
				if (Request.QueryString["key"] == key)
					for (int i=0; i<((ArrayList) ht[key]).Count;i++) 
						Response.Write(" - " + ((ArrayList) ht[key])[i] + " (<a href=\"?clearByKey=" + ((ArrayList) ht[key])[i] + "\">Delete</a>)<br />");
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

		protected void Button1_Click(object sender, System.EventArgs e)
		{
			cms.businesslogic.cache.Cache.ClearAllCache();
		}
	}
}
