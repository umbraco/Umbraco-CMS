using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using Umbraco.Core.IO;
using Umbraco.Core;
using Umbraco.Web;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace umbraco.cms.presentation
{
    /// <summary>
    /// The back office rendering page
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in future versions")]
    public class _umbraco : UmbracoEnsuredPage
    {
        [Obsolete("This property is no longer used")]
        protected umbWindow UmbWindow1;
        protected PlaceHolder bubbleText;

        public string DefaultApp { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var apps = UmbracoUser.Applications.ToList();
            var userHasAccesstodefaultApp = apps.Any(x => x.alias == Umbraco.Core.Constants.Applications.Content);

            // Load user module icons ..
            if (apps.Count() > 1)
            {

                var jsEvents = new StringBuilder();

                PlaceHolderAppIcons.Text = ui.Text("main", "sections", UmbracoUser);
                plcIcons.Text = "";
                foreach (var a in apps.OrderBy(x => x.sortOrder))
                {

                    var appClass = a.icon.StartsWith(".") ? a.icon.Substring(1, a.icon.Length - 1) : a.alias;

                    //adds client side event handlers to the icon buttons
                    jsEvents.Append(@"jQuery('." + appClass + "').click(function() { appClick.call(this, '" + a.alias + "'); } );");
                    jsEvents.Append(@"jQuery('." + appClass + "').dblclick(function() { appDblClick.call(this, '" + a.alias + "'); } );");

                    var iconElement = String.Format("<li><a class=\"{0}\" title=\"" + ui.Text("sections", a.alias, UmbracoUser) + "\" href=\"javascript:void(0);\">", appClass);
                    if (a.icon.StartsWith("."))
                        iconElement +=
                            "<img src=\"images/nada.gif\" class=\"trayHolder\" alt=\"\" /></a></li>";
                    else
                        iconElement += "<img src=\"images/tray/" + a.icon + "\" class=\"trayIcon\" alt=\"" + ui.Text("sections", a.alias, UmbracoUser) + "\"></a></li>";
                    plcIcons.Text += iconElement;

                }

                //registers the jquery event handlers.
                Page.ClientScript.RegisterStartupScript(GetType(), "AppIcons", "jQuery(document).ready(function() { " + jsEvents + " } );", true);

            }
            else
                PlaceHolderAppIcons.Visible = false;


            //if user does not have access to content (ie, he's probably a translator)...
            //then change the default tree app
            if (userHasAccesstodefaultApp == false)
            {
                JTree.App = apps[0].alias;
                DefaultApp = apps[0].alias;
            }
            else
            {
                DefaultApp = Umbraco.Core.Constants.Applications.Content;
            }


            // Load globalized labels
            treeWindow.Text = ui.Text("main", "tree", UmbracoUser);

            RenderActionJs();

            // Version check goes here!

            // zb-00004 #29956 : refactor cookies names & handling
            var updChkCookie = new BusinessLogic.StateHelper.Cookies.Cookie("UMB_UPDCHK", GlobalSettings.VersionCheckPeriod); // was "updateCheck"
            var updateCheckCookie = updChkCookie.HasValue ? updChkCookie.GetValue() : "";

            if (GlobalSettings.VersionCheckPeriod > 0 && String.IsNullOrEmpty(updateCheckCookie) && UmbracoUser.UserType.Alias == "admin")
            {

                // Add scriptmanager version check
                var sm = ScriptManager.GetCurrent(Page);
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
    IOHelper.ResolveUrl(SystemDirectories.Umbraco + "/images/pinnedIcons/umb.ico"),
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
                                        string.Format("{0}/images/pinnedIcons/task_{1}.ico", SystemDirectories.Umbraco, app.alias))))
                                ? "/umbraco/images/pinnedIcons/task_" + app.alias + ".ico"
                                : "/umbraco/images/pinnedIcons/task_default.ico");
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
            foreach (var jsFile in BusinessLogic.Actions.Action.GetJavaScriptFileReferences())
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
                        Page.ClientScript.RegisterClientScriptInclude(GetType(), item.ToString(), jsFile);
                    }
                }
                catch (UriFormatException)
                {
                    isValid = false;
                }

                if (isValid == false)
                {
                    //it is invalid, let's render it as a script block instead as devs may have written real Javascript instead
                    //of a JS path
                    Page.ClientScript.RegisterClientScriptBlock(GetType(), item.ToString(CultureInfo.InvariantCulture), jsFile, true);
                }

                item++;
            }
        }

		/// <summary>
		/// ClientLoader control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.UmbracoClientDependencyLoader ClientLoader;

		/// <summary>
		/// CssInclude1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.CssInclude CssInclude1;

		/// <summary>
		/// CssInclude2 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.CssInclude CssInclude2;

		/// <summary>
		/// JsInclude1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

		/// <summary>
		/// JsInclude2 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude2;

		/// <summary>
		/// JsInclude3 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude3;

		/// <summary>
		/// JsInclude14 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude14;

		/// <summary>
		/// JsInclude5 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude5;

		/// <summary>
		/// JsInclude6 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude6;

		/// <summary>
		/// JsInclude13 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude13;

		/// <summary>
		/// JsInclude7 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude7;

		/// <summary>
		/// JsInclude8 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude8;

		/// <summary>
		/// JsInclude9 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude9;

		/// <summary>
		/// JsInclude10 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude10;

		/// <summary>
		/// JsInclude11 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude11;

		/// <summary>
		/// JsInclude4 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude4;

		/// <summary>
		/// JsInclude17 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude17;

		/// <summary>
		/// JsInclude12 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude12;

		/// <summary>
		/// JsInclude15 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude15;

		/// <summary>
		/// JsInclude16 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude16;

		/// <summary>
		/// Form1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.HtmlControls.HtmlForm Form1;

		/// <summary>
		/// umbracoScriptManager control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.ScriptManager umbracoScriptManager;

		/// <summary>
		/// FindDocuments control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.Panel FindDocuments;

		/// <summary>
		/// Search control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.presentation.Search.QuickSearch Search;

		/// <summary>
		/// treeWindow control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.UmbracoPanel treeWindow;

		/// <summary>
		/// JTree control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.controls.Tree.TreeControl JTree;

		/// <summary>
		/// PlaceHolderAppIcons control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.UmbracoPanel PlaceHolderAppIcons;

		/// <summary>
		/// plcIcons control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.Literal plcIcons;

    }
}
