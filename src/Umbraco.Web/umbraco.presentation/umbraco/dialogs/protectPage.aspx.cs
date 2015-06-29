using System;
using System.Globalization;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms.VisualStyles;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using umbraco.controls;
using umbraco.cms.helpers;
using umbraco.BasePages;
using Umbraco.Core.Security;

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

        protected Literal jsShowWindow;
        protected DualSelectbox _memberGroups = new DualSelectbox();
        protected ContentPicker loginPagePicker = new ContentPicker();
        protected ContentPicker errorPagePicker = new ContentPicker();

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

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

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check for editing
            int documentId = int.Parse(helper.Request("nodeId"));
            var documentObject = new Document(documentId);
            jsShowWindow.Text = "";

            ph_errorpage.Controls.Add(errorPagePicker);
            ph_loginpage.Controls.Add(loginPagePicker);

            pp_login.Text = ui.Text("login");
            pp_pass.Text = ui.Text("password");
            pp_loginPage.Text = ui.Text("paLoginPage");
            pp_errorPage.Text = ui.Text("paErrorPage");

            pane_chooseMode.Text = ui.Text("publicAccess", "paHowWould", UmbracoUser);
            pane_pages.Text = ui.Text("publicAccess", "paSelectPages", UmbracoUser);
            pane_simple.Text = ui.Text("publicAccess", "paSimple", UmbracoUser);
            pane_advanced.Text = ui.Text("publicAccess", "paAdvanced", UmbracoUser);

            if (IsPostBack == false)
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
                        var loginPageObj = new Document(loginPage);
                        if (loginPageObj != null)
                        {
                            loginPagePicker.Value = loginPage.ToString(CultureInfo.InvariantCulture);
                        }
                        var errorPageObj = new Document(errorPage);
                        errorPagePicker.Value = errorPage.ToString(CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<protectPage>("An error occurred initializing the protect page editor", ex);
                    }

                    if (Access.GetProtectionType(documentId) == ProtectionType.Simple)
                    {
                        MembershipUser m = Access.GetAccessingMembershipUser(documentId);
                        if (m != null)
                        {
                            pane_simple.Visible = true;
                            pp_pass.Visible = false;
                            simpleLogin.Visible = false;
                            SimpleLoginLabel.Visible = true;
                            SimpleLoginLabel.Text = m.UserName;
                            pane_advanced.Visible = false;
                            bt_protect.CommandName = "simple";
                        }

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
            var selectedGroups = "";
            var roles = Roles.GetAllRoles().OrderBy(x => x);

            if (roles.Any())
            {
                foreach (string role in roles)
                {
                    ListItem li = new ListItem(role, role);
                    if (IsPostBack == false)
                    {
                        if (Access.IsProtectedByMembershipRole(int.Parse(helper.Request("nodeid")), role))
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

        protected void ChangeOnClick(object sender, EventArgs e)
        {
            SimpleLoginNameValidator.IsValid = true;
            SimpleLoginLabel.Visible = false;
            simpleLogin.Visible = true;
            pp_pass.Visible = true;
        }

        protected void protect_Click(object sender, CommandEventArgs e)
        {
            if (string.IsNullOrEmpty(errorPagePicker.Value) || errorPagePicker.Value == "-1")
                cv_errorPage.IsValid = false;

            if (string.IsNullOrEmpty(loginPagePicker.Value) || loginPagePicker.Value == "-1")
                cv_loginPage.IsValid = false;

            //reset
            SimpleLoginNameValidator.IsValid = true;

            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

            if (Page.IsValid)
            {
                int pageId = int.Parse(helper.Request("nodeId"));

                if (e.CommandName == "simple")
                {
                    var memberLogin = simpleLogin.Visible ? simpleLogin.Text : SimpleLoginLabel.Text;

                    var member = provider.GetUser(memberLogin, false);
                    if (member == null)
                    {
                        var tempEmail = "u" + Guid.NewGuid().ToString("N") + "@example.com";

                        // this needs to work differently depending on umbraco members or external membership provider
                        if (provider.IsUmbracoMembershipProvider() == false)
                        {
                            member = provider.CreateUser(memberLogin, simplePassword.Text, tempEmail);
                        }
                        else
                        {
                            //if it's the umbraco membership provider, then we need to tell it what member type to create it with
                            if (MemberType.GetByAlias(Constants.Conventions.MemberTypes.SystemDefaultProtectType) == null)
                            {
                                MemberType.MakeNew(BusinessLogic.User.GetUser(0), Constants.Conventions.MemberTypes.SystemDefaultProtectType);
                            }
                            var castedProvider = provider.AsUmbracoMembershipProvider();
                            MembershipCreateStatus status;
                            member = castedProvider.CreateUser(Constants.Conventions.MemberTypes.SystemDefaultProtectType,
                                                memberLogin, simplePassword.Text, tempEmail, null, null, true, null, out status);
                            if (status != MembershipCreateStatus.Success)
                            {
                                SimpleLoginNameValidator.IsValid = false;
                                SimpleLoginNameValidator.ErrorMessage = "Could not create user: " + status;
                                SimpleLoginNameValidator.Text = "Could not create user: " + status;
                                return;
                            }
                        }
                    }
                    else if (pp_pass.Visible)
                    {
                        SimpleLoginNameValidator.IsValid = false;
                        SimpleLoginLabel.Visible = true;
                        SimpleLoginLabel.Text = memberLogin;
                        simpleLogin.Visible = false;
                        pp_pass.Visible = false;
                        return;
                    }

                    // Create or find a memberGroup
                    var simpleRoleName = "__umbracoRole_" + member.UserName;
                    if (Roles.RoleExists(simpleRoleName) == false)
                    {
                        Roles.CreateRole(simpleRoleName);
                    }
                    if (Roles.IsUserInRole(member.UserName, simpleRoleName) == false)
                    {
                        Roles.AddUserToRole(member.UserName, simpleRoleName);
                    }

                    Access.ProtectPage(true, pageId, int.Parse(loginPagePicker.Value), int.Parse(errorPagePicker.Value));
                    Access.AddMembershipRoleToDocument(pageId, simpleRoleName);
                    Access.AddMembershipUserToDocument(pageId, member.UserName);
                }
                else if (e.CommandName == "advanced")
                {
                    Access.ProtectPage(false, pageId, int.Parse(loginPagePicker.Value), int.Parse(errorPagePicker.Value));

                    foreach (ListItem li in _memberGroups.Items)
                        if (("," + _memberGroups.Value + ",").IndexOf("," + li.Value + ",", StringComparison.Ordinal) > -1)
                            Access.AddMembershipRoleToDocument(pageId, li.Value);
                        else
                            Access.RemoveMembershipRoleFromDocument(pageId, li.Value);
                }

                feedback.Text = ui.Text("publicAccess", "paIsProtected", new cms.businesslogic.CMSNode(pageId).Text) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";

                p_buttons.Visible = false;
                pane_advanced.Visible = false;
                pane_simple.Visible = false;

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

            feedback.Text = ui.Text("publicAccess", "paIsRemoved", new cms.businesslogic.CMSNode(pageId).Text) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";

            ClientTools.ReloadActionNode(true, false);

            feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
        }

        protected CustomValidator SimpleLoginNameValidator;
        protected Label SimpleLoginLabel;

        /// <summary>
        /// tempFile control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlInputHidden tempFile;

        /// <summary>
        /// feedback control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Feedback feedback;

        /// <summary>
        /// p_mode control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel p_mode;

        /// <summary>
        /// pane_chooseMode control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_chooseMode;

        /// <summary>
        /// rb_simple control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RadioButton rb_simple;

        /// <summary>
        /// rb_advanced control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RadioButton rb_advanced;

        /// <summary>
        /// p_noGroupsFound control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel p_noGroupsFound;

        /// <summary>
        /// bt_selectMode control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button bt_selectMode;

        /// <summary>
        /// pane_simple control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_simple;

        /// <summary>
        /// PropertyPanel1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel PropertyPanel1;

        /// <summary>
        /// pp_login control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_login;

        /// <summary>
        /// simpleLogin control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox simpleLogin;

        /// <summary>
        /// pp_pass control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_pass;

        /// <summary>
        /// simplePassword control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox simplePassword;

        /// <summary>
        /// pane_advanced control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_advanced;

        /// <summary>
        /// PropertyPanel3 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel PropertyPanel3;

        /// <summary>
        /// PropertyPanel2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel PropertyPanel2;

        /// <summary>
        /// groupsSelector control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder groupsSelector;

        /// <summary>
        /// p_buttons control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel p_buttons;

        /// <summary>
        /// pane_pages control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_pages;

        /// <summary>
        /// pp_loginPage control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_loginPage;

        /// <summary>
        /// ph_loginpage control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder ph_loginpage;

        /// <summary>
        /// cv_loginPage control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CustomValidator cv_loginPage;

        /// <summary>
        /// pp_errorPage control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_errorPage;

        /// <summary>
        /// ph_errorpage control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder ph_errorpage;

        /// <summary>
        /// cv_errorPage control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CustomValidator cv_errorPage;

        /// <summary>
        /// bt_protect control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button bt_protect;

        /// <summary>
        /// bt_buttonRemoveProtection control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button bt_buttonRemoveProtection;

        /// <summary>
        /// errorId control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlInputHidden errorId;

        /// <summary>
        /// loginId control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlInputHidden loginId;

        /// <summary>
        /// js control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder js;


    }
}
