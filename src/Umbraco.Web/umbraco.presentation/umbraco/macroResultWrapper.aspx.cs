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

// using System.Collections;

namespace umbraco.presentation
{
	/// <summary>
	/// Summary description for macroResultWrapper.
	/// </summary>
	public partial class macroResultWrapper : BasePages.UmbracoEnsuredPage
	{
	
		protected void Page_Load(object sender, System.EventArgs e)
		{

			int macroID = cms.businesslogic.macro.Macro.GetByAlias(helper.Request("umb_macroAlias")).Id;
			int pageID = int.Parse(helper.Request("umbPageId"));
			Guid pageVersion = new Guid(helper.Request("umbVersionId"));

			System.Web.HttpContext.Current.Items["macrosAdded"] = 0;
			System.Web.HttpContext.Current.Items["pageID"] = pageID.ToString();
			
			// Collect attributes
			Hashtable attributes = new Hashtable();
			foreach(string key in Request.QueryString.AllKeys) 
			{
				if (key.IndexOf("umb_") > -1) 
				{
					attributes.Add(key.Substring(4, key.Length-4), Request.QueryString[key]);
				}
			}


			page p = new page(pageID, pageVersion);
			macro m = macro.GetMacro(macroID);
			
			Control c = m.renderMacro(attributes, p.Elements, p.PageID);
			PlaceHolder1.Controls.Add(c);
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
