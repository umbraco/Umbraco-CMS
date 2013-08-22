using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using Umbraco.Core.IO;
using umbraco;
using Umbraco.Core;

namespace Umbraco.Web.UI.Umbraco
{
    public partial class Umbraco : Pages.UmbracoEnsuredPage
	{        
        public string DefaultApp { get; private set; }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            var apps = UmbracoUser.Applications.ToList();
            bool userHasAccesstodefaultApp = apps.Any(x => x.alias == Constants.Applications.Content);

            // Load user module icons ..
            if (apps.Count() > 1)
            {

                var jsEvents = new StringBuilder();

                PlaceHolderAppIcons.Text = ui.Text("main", "sections", UmbracoUser);
                plcIcons.Text = "";
                foreach (global::umbraco.BusinessLogic.Application a in apps.OrderBy(x => x.sortOrder))
                {

                    string appClass = a.icon.StartsWith(".") ? a.icon.Substring(1, a.icon.Length - 1) : a.alias;

                    //adds client side event handlers to the icon buttons
                    jsEvents.Append(@"jQuery('." + appClass + "').click(function() { appClick.call(this, '" + a.alias + "'); } );");
                    jsEvents.Append(@"jQuery('." + appClass + "').dblclick(function() { appDblClick.call(this, '" + a.alias + "'); } );");

                    string iconElement = String.Format("<li><a class=\"{0}\" title=\"" + ui.Text("sections", a.alias, UmbracoUser) + "\" href=\"javascript:void(0);\">", appClass);
                    if (a.icon.StartsWith("."))
                        iconElement +=
                            "<img src=\"images/nada.gif\" class=\"trayHolder\" alt=\"\" /></a></li>";
                    else
                        iconElement += "<img src=\"images/tray/" + a.icon + "\" class=\"trayIcon\" alt=\"" + ui.Text("sections", a.alias, UmbracoUser) + "\"></a></li>";
                    plcIcons.Text += iconElement;

                }

                //registers the jquery event handlers.
                Page.ClientScript.RegisterStartupScript(this.GetType(), "AppIcons", "jQuery(document).ready(function() { " + jsEvents.ToString() + " } );", true);

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
                DefaultApp = Constants.Applications.Content;
            }


            // Load globalized labels
            treeWindow.Text = ui.Text("main", "tree", UmbracoUser);

            RenderActionJs();

            // Version check goes here!

            // zb-00004 #29956 : refactor cookies names & handling
            var updChkCookie = new global::umbraco.BusinessLogic.StateHelper.Cookies.Cookie("UMB_UPDCHK", GlobalSettings.VersionCheckPeriod); // was "updateCheck"
            string updateCheckCookie = updChkCookie.HasValue ? updChkCookie.GetValue() : "";

            if (GlobalSettings.VersionCheckPeriod > 0 && String.IsNullOrEmpty(updateCheckCookie) && UmbracoUser.UserType.Alias == "admin")
            {

                // Add scriptmanager version check
                ScriptManager sm = ScriptManager.GetCurrent(Page);
                sm.Scripts.Add(new ScriptReference(SystemDirectories.Umbraco + "/js/umbracoUpgradeChecker.js"));
                sm.Services.Add(new ServiceReference(SystemDirectories.WebServices + "/CheckForUpgrade.asmx"));

                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "upgradeChecker", "jQuery(document).ready(function() {umbraco.presentation.webservices.CheckForUpgrade.CallUpgradeService(umbracoCheckUpgrade);});", true);

                updChkCookie.SetValue("1");
            }
            DataBind();

            AddIe9Meta();
        }

        private void AddIe9Meta()
        {
            if (Request.Browser.Browser == "IE" && Request.Browser.MajorVersion == 9)
            {
                var metadata = new StringBuilder();
                metadata.AppendFormat(
    @"<link rel='icon' href='{0}' type='image/x-icon'>
                <link rel='shortcut icon' href='{0}' type='image/x-icon'> 
                <meta name='application-name' content='Umbraco CMS - {1}' />
                <meta name='msapplication-tooltip' content='Umbraco CMS - {1}' />
                <meta name='msapplication-navbutton-color' content='#f36f21' />
                <meta name='msapplication-starturl' content='./umbraco.aspx' />",
    IOHelper.ResolveUrl(SystemDirectories.Umbraco + "/Images/PinnedIcons/umb.ico"),
    HttpContext.Current.Request.Url.Host.ToLower().Replace("www.", ""));

                var user = UmbracoUser;
                if (user != null && user.Applications != null && user.Applications.Length > 0)
                {
                    foreach (var app in user.Applications)
                    {
                        metadata.AppendFormat(
                            @"<meta name='msapplication-task' content='name={0}; action-uri={1}; icon-uri={2}' />",
                            ui.Text("sections", app.alias, user),
                            IOHelper.ResolveUrl(string.Format("{0}/umbraco.aspx#{1}", SystemDirectories.Umbraco, app.alias)),
                            File.Exists(
                                IOHelper.MapPath(
                                    IOHelper.ResolveUrl(
                                        string.Format("{0}/Images/PinnedIcons/task_{1}.ico", SystemDirectories.Umbraco, app.alias))))
                                ? "/umbraco/Images/PinnedIcons/task_" + app.alias + ".ico"
                                : "/umbraco/Images/PinnedIcons/task_default.ico");
                    }
                }

                this.Header.Controls.Add(new LiteralControl(metadata.ToString()));
            }
        }

        /// <summary>
        /// Renders out all JavaScript references that have bee declared in IActions
        /// </summary>
        private void RenderActionJs()
        {
            var item = 0;
            foreach (var jsFile in global::umbraco.BusinessLogic.Actions.Action.GetJavaScriptFileReferences())
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
                    if (jsPatterns.Any(p => Regex.IsMatch(jsFile, p)))
                    {
                        isValid = false;
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