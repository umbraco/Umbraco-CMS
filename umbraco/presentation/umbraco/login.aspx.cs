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
using umbraco.BusinessLogic;
using System.Web.Security;

namespace umbraco.cms.presentation
{
    /// <summary>
    /// Summary description for login.
    /// </summary>
    public partial class login : BasePages.BasePage
    {
        protected umbWindow treeWindow;

        protected void Page_Load(object sender, System.EventArgs e)
        {

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
                        if (umbraco.cms.businesslogic.web.Document.GetRootDocuments().Length > 0)
                        {
                            startNode = umbraco.cms.businesslogic.web.Document.GetRootDocuments()[0].Id;
                        }
                        else
                        {
                            throw new Exception("There's currently no content to edit. Please contact your system administrator");
                        }
                    }
                    string redir = String.Format("{0}/canvas.aspx?redir=/{1}.aspx", GlobalSettings.Path, startNode);
                    Response.Redirect(redir, true);
                }

                if (hf_height.Value != "undefined")
                {
                    Session["windowHeight"] = hf_height.Value;
                    Session["windowWidth"] = hf_width.Value;
                }

                if (string.IsNullOrEmpty(Request["redir"]))
                    Response.Redirect("umbraco.aspx");
                else
                    Response.Redirect(Request["redir"]);
            }
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
