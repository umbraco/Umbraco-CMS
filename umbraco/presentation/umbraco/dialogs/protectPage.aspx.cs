using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using umbraco.controls;
using umbraco.cms.helpers;
using umbraco.BasePages;

namespace umbraco.presentation.umbraco.dialogs
{
    /// <summary>
    /// Summary description for protectPage.
    /// </summary>
    public partial class protectPage : UmbracoEnsuredPage
    {
        public protectPage()
        {
            CurrentApp = BusinessLogic.DefaultApps.content.ToString();

        }

        protected System.Web.UI.WebControls.Literal jsShowWindow;
        protected DualSelectbox _memberGroups = new DualSelectbox();
        protected ContentPicker loginPagePicker = new ContentPicker();
        protected ContentPicker errorPagePicker = new ContentPicker();

        protected void selectMode(object sender, EventArgs e)
        {
            p_mode.Visible = false;
            p_buttons.Visible = true;

            if (rb_simple.Checked)
            {
                pane_advanced.Visible = false;
                pane_simple.Visible = true;
                bt_protect.CommandName = "simple";
            }
            else
            {
                pane_advanced.Visible = true;
                pane_simple.Visible = false;
                bt_protect.CommandName = "advanced";
            }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Check for editing
            int documentId = int.Parse(helper.Request("nodeId"));
            cms.businesslogic.web.Document documentObject = new cms.businesslogic.web.Document(documentId);
            jsShowWindow.Text = "";

            ph_errorpage.Controls.Add(errorPagePicker);
            ph_loginpage.Controls.Add(loginPagePicker);

            pp_login.Text = ui.Text("login");
            pp_pass.Text = ui.Text("password");
            pp_loginPage.Text = ui.Text("paLoginPage");
            pp_errorPage.Text = ui.Text("paErrorPage");

            pane_chooseMode.Text = ui.Text("publicAccess", "paHowWould", base.getUser());
            pane_pages.Text = ui.Text("publicAccess", "paSelectPages", base.getUser());
            pane_simple.Text = ui.Text("publicAccess", "paSimple", base.getUser());
            pane_advanced.Text = ui.Text("publicAccess", "paAdvanced", base.getUser());

            if (!IsPostBack)
            {
                if (Access.IsProtected(documentId, documentObject.Path) && Access.GetProtectionType(documentId) != ProtectionType.NotProtected)
                {
                    bt_buttonRemoveProtection.Visible = true;
                    bt_buttonRemoveProtection.Attributes.Add("onClick", "return confirm('" + ui.Text("areyousure") + "')");

                    // Get login and error pages
                    int errorPage = Access.GetErrorPage(documentObject.Path);
                    int loginPage = Access.GetLoginPage(documentObject.Path);
                    try
                    {
                        Document loginPageObj = new Document(loginPage);
                        if (loginPageObj != null)
                        {
                            loginPagePicker.Value = loginPage.ToString();
                        }
                        Document errorPageObj = new Document(errorPage);
                        errorPagePicker.Value = errorPage.ToString();
                    }
                    catch
                    {
                    }

                    if (Access.GetProtectionType(documentId) == ProtectionType.Simple)
                    {
                        MembershipUser m = Access.GetAccessingMembershipUser(documentId);
                        simpleLogin.Text = m.UserName;
                        pane_simple.Visible = true;
                        pane_advanced.Visible = false;
                        bt_protect.CommandName = "simple";

                    }
                    else if (Access.GetProtectionType(documentId) == ProtectionType.Advanced)
                    {
                        pane_simple.Visible = false;
                        pane_advanced.Visible = true;
                        bt_protect.CommandName = "advanced";
                    }

                    p_buttons.Visible = true;
                    p_mode.Visible = false;
                }
            }

            // Load up membergrouops
            _memberGroups.ID = "Membergroups";
            _memberGroups.Width = 175;
            string selectedGroups = "";
            string[] _roles = Roles.GetAllRoles();

            if (_roles.Length > 0)
            {
                foreach (string role in _roles)
                {
                    ListItem li = new ListItem(role, role);
                    if (!IsPostBack)
                    {
                        if (cms.businesslogic.web.Access.IsProtectedByMembershipRole(int.Parse(helper.Request("nodeid")), role))
                            selectedGroups += role + ",";
                    }
                    _memberGroups.Items.Add(li);
                }
            }
            else
            {
                p_noGroupsFound.Visible = true;
                rb_advanced.Enabled = false;
            }
            _memberGroups.Value = selectedGroups;
            groupsSelector.Controls.Add(_memberGroups);


            bt_protect.Text = ui.Text("update");
            bt_buttonRemoveProtection.Text = ui.Text("paRemoveProtection");

            // Put user code to initialize the page here
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

        protected void protect_Click(object sender, CommandEventArgs e)
        {
            if (string.IsNullOrEmpty(errorPagePicker.Value))
                cv_errorPage.IsValid = false;

            if (string.IsNullOrEmpty(loginPagePicker.Value))
                cv_loginPage.IsValid = false;


            if (Page.IsValid)
            {
                int pageId = int.Parse(helper.Request("nodeId"));
                p_buttons.Visible = false;
                pane_advanced.Visible = false;
                pane_simple.Visible = false;

                if (e.CommandName == "simple")
                {
                    MembershipUser member = Membership.GetUser(simpleLogin.Text);
                    if (member == null)
                    {
                        // this needs to work differently depending on umbraco members or external membership provider
                        if (!cms.businesslogic.member.Member.InUmbracoMemberMode())
                        {
                            member = Membership.CreateUser(simpleLogin.Text, simplePassword.Text);
                        }
                        else
                        {
                            try
                            {
                                if (cms.businesslogic.member.MemberType.GetByAlias("_umbracoSystemDefaultProtectType") == null)
                                {
                                    cms.businesslogic.member.MemberType.MakeNew(BusinessLogic.User.GetUser(0), "_umbracoSystemDefaultProtectType");
                                }
                            }
                            catch
                            {
                                cms.businesslogic.member.MemberType.MakeNew(BusinessLogic.User.GetUser(0), "_umbracoSystemDefaultProtectType");
                            }
                            // create member
                            cms.businesslogic.member.Member mem = cms.businesslogic.member.Member.MakeNew(simpleLogin.Text, "", cms.businesslogic.member.MemberType.GetByAlias("_umbracoSystemDefaultProtectType"), base.getUser());
                            // working around empty password restriction for Umbraco Member Mode
                            mem.Password = simplePassword.Text;
                            member = Membership.GetUser(simpleLogin.Text);
                        }
                    }

                    // Create or find a memberGroup
                    string simpleRoleName = "__umbracoRole_" + simpleLogin.Text;
                    if (!Roles.RoleExists(simpleRoleName))
                    {
                        Roles.CreateRole(simpleRoleName);
                    }
                    if (!Roles.IsUserInRole(member.UserName, simpleRoleName))
                    {
                        Roles.AddUserToRole(member.UserName, simpleRoleName);
                    }

                    Access.ProtectPage(true, pageId, int.Parse(loginPagePicker.Value), int.Parse(errorPagePicker.Value));
                    Access.AddMembershipRoleToDocument(pageId, simpleRoleName);
                    Access.AddMembershipUserToDocument(pageId, member.UserName);
                }
                else if (e.CommandName == "advanced")
                {
                    cms.businesslogic.web.Access.ProtectPage(false, pageId, int.Parse(loginPagePicker.Value), int.Parse(errorPagePicker.Value));

                    foreach (ListItem li in _memberGroups.Items)
                        if (("," + _memberGroups.Value + ",").IndexOf("," + li.Value + ",") > -1)
                            cms.businesslogic.web.Access.AddMembershipRoleToDocument(pageId, li.Value);
                        else
                            cms.businesslogic.web.Access.RemoveMembershipRoleFromDocument(pageId, li.Value);
                }

                feedback.Text = ui.Text("publicAccess", "paIsProtected", new cms.businesslogic.CMSNode(pageId).Text, null) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";

                ClientTools.ReloadActionNode(true, false);

                feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
            }
        }


        protected void buttonRemoveProtection_Click(object sender, System.EventArgs e)
        {
            int pageId = int.Parse(helper.Request("nodeId"));
            p_buttons.Visible = false;
            pane_advanced.Visible = false;
            pane_simple.Visible = false;

            Access.RemoveProtection(pageId);

            feedback.Text = ui.Text("publicAccess", "paIsRemoved", new cms.businesslogic.CMSNode(pageId).Text, null) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";

            ClientTools.ReloadActionNode(true, false);

            feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
        }
    }
}
