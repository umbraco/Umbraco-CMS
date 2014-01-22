using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Data;
using System.Drawing;
using System.Security;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Umbraco.Core.Logging;
using System.Web.Security;
using umbraco.businesslogic.Exceptions;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.web;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web;
using User = umbraco.BusinessLogic.User;

namespace umbraco.cms.presentation
{
    /// <summary>
    /// Summary description for login.
    /// </summary>
    public partial class login : BasePages.BasePage
    {
        [Obsolete("This property is no longer used")]
        protected umbWindow treeWindow;

        private MembershipProvider BackOfficeProvider
        {
            get
            {
                var provider = Membership.Providers[UmbracoSettings.DefaultBackofficeProvider];
                if (provider == null)
                {
                    throw new ProviderException("The membership provider " + UmbracoSettings.DefaultBackofficeProvider + " was not found");
                }
                return provider;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ClientLoader.DataBind();

            // validate redirect url
            string redirUrl = Request["redir"];
            if (!string.IsNullOrEmpty(redirUrl))
            {
                ValidateRedirectUrl(redirUrl);
            }
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Button1.Text = ui.Text("general", "login");
            Panel1.Text = ui.Text("general", "welcome");
            Panel1.Style.Add("padding", "10px;");
            username.Text = ui.Text("general", "username");
            password.Text = ui.Text("general", "password");

            // Add bottom and top texts
            TopText.Text = ui.Text("login", "topText");


            BottomText.Text = ui.Text("login", "bottomText", DateTime.Now.Year.ToString(), null);

            //added this little hack to remove unessary formatting, without breaking all current language files.
            if (BottomText.Text.Contains("</p>"))
                BottomText.Text = BottomText.Text.Substring(29).Replace("<br />", "").Replace("</p>", "");
        }


        protected void Button1_Click(object sender, EventArgs e)
        {
            // Authenticate users by using the provider specified in umbracoSettings.config
            if (BackOfficeProvider.ValidateUser(lname.Text, passw.Text))
            {
                IUser user;
                if (BackOfficeProvider.IsUmbracoUsersProvider() == false)
                {
                    user = ApplicationContext.Services.UserService.CreateUserMappingForCustomProvider(
                        BackOfficeProvider.GetUser(lname.Text, false));
                }
                else
                {
                    user = ApplicationContext.Services.UserService.GetByUsername(lname.Text);
                    if (user == null)
                    {
                        throw new InvalidOperationException("No IUser found with username " + lname.Text);
                    }
                }
                
                //do the login
                UmbracoContext.Current.Security.PerformLogin(user.Id);

                // Check if the user should be redirected to live editing
                if (UmbracoSettings.EnableCanvasEditing)
                {
                    //if live editing is enabled, we have to check if we need to redirect there but it is not supported
                    // on the new IUser so we need to get the legacy user object to check
                    var u = new User(user.Id);
                    if (u.DefaultToLiveEditing)
                    {
                        var startNode = u.StartNodeId;
                        // If the startnode is -1 (access to all content), we'll redirect to the top root node
                        if (startNode == -1)
                        {
                            if (Document.CountLeafNodes(-1, Document._objectType) > 0)
                            {
                                //get the first document
                                var firstNodeId = Document.TopMostNodeIds(Document._objectType).First();
                                startNode = new Document(firstNodeId).Id;
                            }
                            else
                            {
                                throw new Exception("There's currently no content to edit. Please contact your system administrator");
                            }
                        }
                        var redir = String.Format("{0}/canvas.aspx?redir=/{1}.aspx", SystemDirectories.Umbraco, startNode);
                        Response.Redirect(redir, true);
                    }
                }                

                if (hf_height.Value != "undefined")
                {
                    Session["windowHeight"] = hf_height.Value;
                    Session["windowWidth"] = hf_width.Value;
                }

                string redirUrl = Request["redir"];

                if (string.IsNullOrEmpty(redirUrl))
                    Response.Redirect("umbraco.aspx");
                else if (ValidateRedirectUrl(redirUrl))
                {
                    Response.Redirect(redirUrl, true);
                }
            }
            else
            {
                loginError.Visible = true;
            }
        }

        private static bool ValidateRedirectUrl(string url)
        {
            if (IsUrlLocalToHost(url) == false)
            {
                LogHelper.Info<login>(String.Format("Security warning: Login redirect was attempted to a site at another domain: '{0}'", url));

                throw new UserAuthorizationException(
                    String.Format(@"There was attempt to redirect to '{0}' which is another domain than where you've logged in. If you clicked a link to reach this login
                    screen, please double check that the link came from someone you trust. You *might* have been exposed to an *attempt* to breach the security of your website. Nothing 
                    have been compromised, though!", url));
            }

            return true;
        }

        private static bool IsUrlLocalToHost(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return false;
            }

            Uri absoluteUri;
            if (Uri.TryCreate(url, UriKind.Absolute, out absoluteUri))
            {
                return String.Equals(HttpContext.Current.Request.Url.Host, absoluteUri.Host,
                            StringComparison.OrdinalIgnoreCase);
            }

            bool isLocal = !url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                           && !url.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
                           && Uri.IsWellFormedUriString(url, UriKind.Relative);
            return isLocal;
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
        /// JsInclude1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

        /// <summary>
        /// JsInclude3 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude3;

        /// <summary>
        /// JsInclude2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude2;

        /// <summary>
        /// Form1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlForm Form1;

        /// <summary>
        /// Panel1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.UmbracoPanel Panel1;

        /// <summary>
        /// TopText control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal TopText;

        /// <summary>
        /// username control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label username;

        /// <summary>
        /// lname control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox lname;

        /// <summary>
        /// password control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label password;

        /// <summary>
        /// passw control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox passw;

        /// <summary>
        /// Button1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button Button1;

        /// <summary>
        /// BottomText control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal BottomText;

        /// <summary>
        /// hf_height control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.HiddenField hf_height;

        /// <summary>
        /// hf_width control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.HiddenField hf_width;

        /// <summary>
        /// loginError control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder loginError;
    }
}
