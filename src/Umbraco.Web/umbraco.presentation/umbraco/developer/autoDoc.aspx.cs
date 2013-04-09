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
using System.Linq;
using umbraco.BusinessLogic;

namespace umbraco.developer
{
	/// <summary>
	/// Summary description for autoDoc.
	/// </summary>
	public partial class autoDoc : BasePages.UmbracoEnsuredPage
	{
	    public autoDoc()
	    {
	        CurrentApp = DefaultApps.developer.ToString();
	    }

		protected void Page_Load(object sender, EventArgs e)
		{
			// Put user code to initialize the page here
			foreach(var dt in cms.businesslogic.web.DocumentType.GetAllAsList()) 
			{
				LabelDoc.Text +=
					"<div class=\"propertyType\"><p class=\"documentType\">" + dt.Text + "</p><p class=\"type\">Id: " + dt.Id.ToString() + ", Alias: " + dt.Alias + ")</p>";
				if (dt.PropertyTypes.Count > 0)
					LabelDoc.Text += "<p class=\"docHeader\">Property Types:</p>";
				foreach (var pt in dt.PropertyTypes)
					LabelDoc.Text +=
						"<p class=\"type\">" + pt.Id.ToString() + ", " + pt.Alias + ", " + pt.Name + "</p>";
				if (dt.getVirtualTabs.Length > 0)
					LabelDoc.Text += "<p class=\"docHeader\">Tabs:</p>";
                foreach (var t in dt.getVirtualTabs.ToList())
					LabelDoc.Text +=
						"<p class=\"type\">" + t.Id.ToString() + ", " + t.Caption + "</p>";
				if (dt.AllowedChildContentTypeIDs.Length > 0)
					LabelDoc.Text += "<p class=\"docHeader\">Allowed children:</p>";
				foreach (var child in dt.AllowedChildContentTypeIDs.ToList()) 
				{
					var contentType = new cms.businesslogic.ContentType(child);
					LabelDoc.Text +=
						"<p class=\"type\">" + contentType.Id.ToString() + ", " + contentType.Text + "</p>";
				}

				LabelDoc.Text += "</div>";
			}
		}

	}
}
