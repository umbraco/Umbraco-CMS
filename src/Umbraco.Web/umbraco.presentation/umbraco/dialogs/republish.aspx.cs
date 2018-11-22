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
using Umbraco.Web;

namespace umbraco.cms.presentation
{
	/// <summary>
	/// Summary description for republish.
	/// </summary>
	public partial class republish : BasePages.UmbracoEnsuredPage
	{
	    public republish()
	    {
            CurrentApp = BusinessLogic.DefaultApps.content.ToString();

	    }
        protected void go(object sender, EventArgs e) {
            // re-create xml
            if (Request.GetItemAsString("xml") != "")
            {
                Server.ScriptTimeout = 100000;
                Services.ContentService.RePublishAll();                
            }
            else if (Request.GetItemAsString("previews") != "")
            {
                Server.ScriptTimeout = 100000;
                umbraco.cms.businesslogic.web.Document.RegeneratePreviews();
            }
            else if (Request.GetItemAsString("refreshNodes") != "")
            {
                Server.ScriptTimeout = 100000;
                System.Xml.XmlDocument xd = new System.Xml.XmlDocument();

                var doc = new cms.businesslogic.web.Document(int.Parse(Request.GetItemAsString("refreshNodes")));

                foreach (cms.businesslogic.web.Document d in doc.Children)
                {
                    d.XmlGenerate(xd);
                    Response.Write("<li>Creating xml for " + d.Text + "</li>");
                    Response.Flush();
                }
            }
            
            //PPH changed this to a general library call for load balancing support
            library.RefreshContent();

            p_republish.Visible = false;
            p_done.Visible = true;
        }

		protected void Page_Load(object sender, System.EventArgs e)
		{
			bt_go.Text = ui.Text("republish");
		}

		
	}
}
