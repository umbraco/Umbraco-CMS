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

namespace umbraco.developer
{
	/// <summary>
	/// Summary description for autoDoc.
	/// </summary>
	public partial class autoDoc : BasePages.UmbracoEnsuredPage
	{
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
			foreach(cms.businesslogic.web.DocumentType dt in cms.businesslogic.web.DocumentType.GetAllAsList()) 
			{
				LabelDoc.Text +=
					"<div class=\"propertyType\"><p class=\"documentType\">" + dt.Text + "</p><p class=\"type\">Id: " + dt.Id.ToString() + ", Alias: " + dt.Alias + ")</p>";
				if (dt.PropertyTypes.Count > 0)
					LabelDoc.Text += "<p class=\"docHeader\">Property Types:</p>";
				foreach (cms.businesslogic.propertytype.PropertyType pt in dt.PropertyTypes)
					LabelDoc.Text +=
						"<p class=\"type\">" + pt.Id.ToString() + ", " + pt.Alias + ", " + pt.Name + "</p>";
				if (dt.getVirtualTabs.Length > 0)
					LabelDoc.Text += "<p class=\"docHeader\">Tabs:</p>";
				foreach (cms.businesslogic.ContentType.TabI t in dt.getVirtualTabs)
					LabelDoc.Text +=
						"<p class=\"type\">" + t.Id.ToString() + ", " + t.Caption + "</p>";
				if (dt.AllowedChildContentTypeIDs.Length > 0)
					LabelDoc.Text += "<p class=\"docHeader\">Allowed children:</p>";
				foreach (int child in dt.AllowedChildContentTypeIDs) 
				{
					cms.businesslogic.ContentType _child = new cms.businesslogic.ContentType(child);
					LabelDoc.Text +=
						"<p class=\"type\">" + _child.Id.ToString() + ", " + _child.Text + "</p>";
				}

				LabelDoc.Text += "</div>";

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
}
