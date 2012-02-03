using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Security;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.BusinessLogic;
using System.Web.Security;
using umbraco.IO;
using umbraco.cms.businesslogic.web;
using System.Linq;

namespace umbraco.cms.presentation
{
    /// <summary>
    /// Summary description for login.
    /// </summary>
    public partial class login : BasePages.BasePage
    {
        protected umbWindow treeWindow;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ClientLoader.DataBind();

            // validate redirect url
		    string redirUrl = Request["redir"];
            if (!String.IsNullOrEmpty(redirUrl))
            {
                validateRedirectUrl(redirUrl);
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

        protected void Button1_Click(object sender, System.EventArgs e)
        {
            // Authenticate users by using the provider specified in umbracoSettings.config
            if (Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].ValidateUser(lname.Text, passw.Text))
            {
                if (Membership.Providers[UmbracoSettings.DefaultBackofficeProvider] is ActiveDirectoryMembershipProvider)
                    ActiveDirectoryMapping(lname.Text, Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].GetUser(lname.Text, false).Email);

                BusinessLogic.User u = new User(lname.Text);
                doLogin(u);

                // Check if the user should be redirected to live editing
                if (u.DefaultToLiveEditing)
                {
                    int startNode = u.StartNodeId;
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
                    string redir = String.Format("{0}/canvas.aspx?redir=/{1}.aspx", SystemDirectories.Umbraco, startNode);
                    Response.Redirect(redir, true);
                }

                if (hf_height.Value != "undefined")
                {
                    Session["windowHeight"] = hf_height.Value;
                    Session["windowWidth"] = hf_width.Value;
                }

                string redirUrl = Request["redir"];

                if (string.IsNullOrEmpty(redirUrl))
                    Response.Redirect("umbraco.aspx");
                else if (validateRedirectUrl(redirUrl))
                {
                    Response.Redirect(redirUrl, true);
                }
            }
            else {
                loginError.Visible = true;
            }
        }

        private bool validateRedirectUrl(string url)
        {
            if (!isUrlLocalToHost(url))
            {
                Log.Add(LogTypes.Error, -1, String.Format("Security warning: Login redirect was attempted to a site at another domain: '{0}'", url));
                throw new SecurityException(
                    String.Format(@"There was attempt to redirect to '{0}' which is another domain than where you've logged in. If you clicked a link to reach this login
                    screen, please double check that the link came from someone you trust. You *might* have been exposed to an *attempt* to breach the security of your website. Nothing 
                    have been compromised, though!", url));
            }

            return true;
        }

        private bool isUrlLocalToHost(string url)
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
        /// Maps active directory account to umbraco user account
        /// </summary>
        /// <param name="loginName">Name of the login.</param>
        private void ActiveDirectoryMapping(string loginName, string email)
        {
            // Password is not copied over because it is stored in active directory for security!
            // The user is create with default access to content and as a writer user type
            if (BusinessLogic.User.getUserId(loginName) == -1)
            {
                BusinessLogic.User.MakeNew(loginName, loginName, string.Empty, email, BusinessLogic.UserType.GetUserType(2));
                BusinessLogic.User u = new BusinessLogic.User(loginName);
                u.addApplication("content");
            }
        }
    }
}
