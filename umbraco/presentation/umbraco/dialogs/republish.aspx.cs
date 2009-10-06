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
	/// Summary description for republish.
	/// </summary>
	public partial class republish : BasePages.UmbracoEnsuredPage
	{
        protected void go(object sender, EventArgs e) {
            // re-create xml
            if (helper.Request("xml") != "") {
                Server.ScriptTimeout = 100000;
                umbraco.cms.businesslogic.web.Document.RePublishAll();
            } else if (helper.Request("refreshNodes") != "") {
                Server.ScriptTimeout = 100000;
                System.Xml.XmlDocument xd = new System.Xml.XmlDocument();

                //store children array here because iterating over an Array object is very inneficient.
                var doc = new cms.businesslogic.web.Document(int.Parse(helper.Request("refreshNodes")));
                var c = doc.Children;
                foreach (cms.businesslogic.web.Document d in c) {
                    d.XmlGenerate(xd);
                    Response.Write("<li>Creating xml for " + d.Text + "</li>");
                    Response.Flush();
                }
            }
            // library.RePublishNodesDotNet(-1, true);
            //content.Instance.RefreshContentFromDatabaseAsync();

            //PPH changed this to a general library call for load balancing support
            library.RefreshContent();

            p_republish.Visible = false;
            p_done.Visible = true;
        }

		protected void Page_Load(object sender, System.EventArgs e)
		{
			bt_go.Text = ui.Text("republish");
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
