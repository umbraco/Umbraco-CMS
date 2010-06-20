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
using umbraco.BasePages;
using System.Xml;
using System.Xml.XPath;
using umbraco.BusinessLogic.Actions;
using ClientDependency.Core;
using umbraco.IO;
using System.Linq;
using System.Text;

namespace umbraco.cms.presentation
{
	/// <summary>
	/// Summary description for _default.
	/// </summary>
 	public partial class _umbraco : UmbracoEnsuredPage
	{
		protected umbWindow UmbWindow1;
		protected System.Web.UI.WebControls.PlaceHolder bubbleText;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            var apps = this.getUser().Applications.ToList();
            bool userHasAccesstodefaultApp = apps.Where(x => x.alias == "content").Count() > 0;

            // Load user module icons ..
            if (apps.Count() > 1)
            {

                var JSEvents = new StringBuilder();

                PlaceHolderAppIcons.Text = ui.Text("main", "sections", base.getUser());
                plcIcons.Text = "";
                foreach (BusinessLogic.Application a in apps)
                {

                    string aiconSubstring = a.icon.Substring(1, a.icon.Length - 1);
                    
                    //adds client side event handlers to the icon buttons
                    JSEvents.Append(@"jQuery('." + aiconSubstring + "').click(function() { appClick.call(this, '" + a.alias + "'); } );");
                    JSEvents.Append(@"jQuery('." + aiconSubstring + "').dblclick(function() { appDblClick.call(this, '" + a.alias + "'); } );");
                    
                    string iconElement = String.Format("<li><a class=\"{0}\" title=\"" + ui.Text("sections", a.alias, base.getUser()) + "\" href=\"javascript:void(0);\">", aiconSubstring);
                    if (a.icon.StartsWith("."))
                        iconElement +=
                            "<img src=\"images/nada.gif\" class=\"trayHolder\" alt=\"\" /></a></li>";
                    else
                        iconElement += "<img src=\"images/tray/" + a.icon + "\" class=\"trayIcon\" alt=\"" + ui.Text("sections", a.alias, base.getUser()) + "\"></a></li>";
                    plcIcons.Text += iconElement;

                }

                //registers the jquery event handlers.
                Page.ClientScript.RegisterStartupScript(this.GetType(), "AppIcons", "jQuery(document).ready(function() { " + JSEvents.ToString() + " } );", true);

            }
            else
                PlaceHolderAppIcons.Visible = false;


            //if user does not have access to content (ie, he's probably a translator)...
            //then change the default tree app
            if (!userHasAccesstodefaultApp)
            {
                JTree.App = apps[0].alias;
            }


            // Load globalized labels
            treeWindow.Text = ui.Text("main", "tree", base.getUser());

            RenderActionJS();

            // Load default right action
            //            string rightAction = String.Format(@"
            //                var initApp = '{0}';
            //                var rightAction = '{1}';
            //                var rightActionId = '{2}';", umbraco.presentation.UmbracoContext.Current.Request["app"], helper.Request("rightAction"), helper.Request("id"));
            //            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "rightAction", rightAction, true);

            // Version check goes here!

            string updateCheckCookie = "";
            if (Request.Cookies["updateCheck"] != null)
            {
                updateCheckCookie = Request.Cookies["updateCheck"].Value.ToString();
            }
            if (GlobalSettings.VersionCheckPeriod > 0 && String.IsNullOrEmpty(updateCheckCookie) && base.getUser().UserType.Alias == "admin")
            {

                // Add scriptmanager version check
                ScriptManager sm = ScriptManager.GetCurrent(Page);
                sm.Scripts.Add(new ScriptReference(SystemDirectories.Umbraco + "/js/umbracoUpgradeChecker.js"));
                sm.Services.Add(new ServiceReference(SystemDirectories.Webservices + "/CheckForUpgrade.asmx"));

                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "upgradeChecker", "jQuery(document).ready(function() {umbraco.presentation.webservices.CheckForUpgrade.CallUpgradeService(umbracoCheckUpgrade);});", true);

                Response.Cookies["updateCheck"].Value = "1";
                Response.Cookies["updateCheck"].Expires = DateTime.Now.AddDays(GlobalSettings.VersionCheckPeriod);
            }
            DataBind();
        }

        /// <summary>
        /// Renders out all JavaScript references that have bee declared in IActions
        /// </summary>
        private void RenderActionJS()
        {
            string script = @"<script type=""text/javascript"" src=""{0}""></script>";
            foreach (string jsFile in umbraco.BusinessLogic.Actions.Action.GetJavaScriptFileReferences())
                IActionJSFileRef.Controls.Add(new LiteralControl(string.Format(script, jsFile)));
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
