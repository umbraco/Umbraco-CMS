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

namespace umbraco.cms.presentation.developer
{
	/// <summary>
	/// Summary description for viewCacheItem.
	/// </summary>
	public partial class viewCacheItem : BasePages.UmbracoEnsuredPage
	{
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
			Panel1.Text = ui.Text("viewCacheItem");
			string cacheKey = Request.QueryString["key"];
			LabelCacheAlias.Text = cacheKey;
			LabelCacheValue.Text = System.Web.HttpRuntime.Cache[cacheKey].ToString();
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
}
