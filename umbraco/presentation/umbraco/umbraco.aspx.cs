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
using ClientDependency.Core.Controls;
using System.Text.RegularExpressions;

namespace umbraco.cms.presentation
{
	/// <summary>
	/// Summary description for _default.
	/// </summary>
 	public partial class _umbraco : UmbracoEnsuredPage
	{
		protected umbWindow UmbWindow1;
		protected System.Web.UI.WebControls.PlaceHolder bubbleText;

        public string DefaultApp { get; private set; }

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

                    string appClass = a.icon.StartsWith(".") ? a.icon.Substring(1, a.icon.Length - 1) : a.alias;
                    
                    //adds client side event handlers to the icon buttons
                    JSEvents.Append(@"jQuery('." + appClass + "').click(function() { appClick.call(this, '" + a.alias + "'); } );");
                    JSEvents.Append(@"jQuery('." + appClass + "').dblclick(function() { appDblClick.call(this, '" + a.alias + "'); } );");

                    string iconElement = String.Format("<li><a class=\"{0}\" title=\"" + ui.Text("sections", a.alias, base.getUser()) + "\" href=\"javascript:void(0);\">", appClass);
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
                DefaultApp = apps[0].alias;
            }
            else
            {
                DefaultApp = "content";
            }


            // Load globalized labels
            treeWindow.Text = ui.Text("main", "tree", base.getUser());

            RenderActionJS();

            // Version check goes here!

			// zb-00004 #29956 : refactor cookies names & handling
			var updChkCookie = new umbraco.BusinessLogic.StateHelper.Cookies.Cookie("UMB_UPDCHK", GlobalSettings.VersionCheckPeriod); // was "updateCheck"
            string updateCheckCookie = updChkCookie.HasValue ? updChkCookie.GetValue() : "";

            if (GlobalSettings.VersionCheckPeriod > 0 && String.IsNullOrEmpty(updateCheckCookie) && base.getUser().UserType.Alias == "admin")
            {

                // Add scriptmanager version check
                ScriptManager sm = ScriptManager.GetCurrent(Page);
                sm.Scripts.Add(new ScriptReference(SystemDirectories.Umbraco + "/js/umbracoUpgradeChecker.js"));
                sm.Services.Add(new ServiceReference(SystemDirectories.Webservices + "/CheckForUpgrade.asmx"));

                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "upgradeChecker", "jQuery(document).ready(function() {umbraco.presentation.webservices.CheckForUpgrade.CallUpgradeService(umbracoCheckUpgrade);});", true);

				updChkCookie.SetValue("1");
            }
            DataBind();
        }

        /// <summary>
        /// Renders out all JavaScript references that have bee declared in IActions
        /// </summary>
        private void RenderActionJS()
        {            
            var item = 0;
            foreach (var jsFile in umbraco.BusinessLogic.Actions.Action.GetJavaScriptFileReferences())
            {
                //validate that this is a url, if it is not, we'll assume that it is a text block and render it as a text
                //block instead.
                var isValid = true;
                try
                {
                    var jsUrl = new Uri(jsFile, UriKind.RelativeOrAbsolute);
                    //ok it validates, but so does alert('hello'); ! so we need to do more checks
                    
                    //here are the valid chars in a url without escaping
                    if (Regex.IsMatch(jsFile, @"[^a-zA-Z0-9-._~:/?#\[\]@!$&'\(\)*\+,%;=]"))
                        isValid = false;

                    //we'll have to be smarter and just check for certain js patterns now too!
                    var jsPatterns = new string[] { @"\+\s*\=", @"\);", @"function\s*\(", @"!=", @"==" };
                    foreach (var p in jsPatterns)
                    {
                        if (Regex.IsMatch(jsFile, p))
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid)
                    {
                        //add to page
                        Page.ClientScript.RegisterClientScriptInclude(this.GetType(), item.ToString(), jsFile);
                    }                    
                }
                catch (UriFormatException)
                {
                    isValid = false;
                }

                if (!isValid)
                {
                    //it is invalid, let's render it as a script block instead as devs may have written real Javascript instead
                    //of a JS path
                    Page.ClientScript.RegisterClientScriptBlock(this.GetType(), item.ToString(), jsFile, true);
                }

                item++;
            }
        }

	}
}
