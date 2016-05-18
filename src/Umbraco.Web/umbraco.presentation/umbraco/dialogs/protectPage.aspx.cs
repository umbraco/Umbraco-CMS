using Umbraco.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms.VisualStyles;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.web;
using umbraco.controls;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Security;
using Umbraco.Web;
using Umbraco.Web.UI.Pages;
using MembershipProviderExtensions = Umbraco.Core.Security.MembershipProviderExtensions;
using MemberType = umbraco.cms.businesslogic.member.MemberType;

namespace umbraco.presentation.umbraco.dialogs
{
    /// <summary>
    /// Summary description for protectPage.
    /// </summary>
    public partial class protectPage : UmbracoEnsuredPage
    {
        public protectPage()
        {
            CurrentApp = Constants.Applications.Content.ToString();

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

        private ProtectionType GetProtectionType(int documentId)
        {
            var content = Services.ContentService.GetById(documentId);
            if (content == null) return ProtectionType.NotProtected;

            var entry = Services.PublicAccessService.GetEntryForContent(content);
            if (entry == null) return ProtectionType.NotProtected;

            //legacy states that if it is protected by a member id then it is 'simple'
            return entry.Rules.Any(x => x.RuleType == Constants.Conventions.PublicAccess.MemberIdRuleType)
                ? ProtectionType.Simple
                : ProtectionType.Advanced;
        }

        private enum ProtectionType
        {
            NotProtected,
            Simple,
            Advanced
        }

        private int GetErrorPage(string path)
        {
            var entry = Services.PublicAccessService.GetEntryForContent(path);
            if (entry == null) return -1;
            var entity = Services.EntityService.Get(entry.NoAccessNodeId, UmbracoObjectTypes.Document, false);
            return entity.Id;
        }

        private int GetLoginPage(string path)
        {
            var entry = Services.PublicAccessService.GetEntryForContent(path);
            if (entry == null) return -1;
            var entity = Services.EntityService.Get(entry.LoginNodeId, UmbracoObjectTypes.Document, false);
            return entity.Id;
        }

        private MembershipUser GetAccessingMembershipUser(int documentId)
        {
            var content = Services.ContentService.GetById(documentId);
            if (content == null) return null;
            var entry = Services.PublicAccessService.GetEntryForContent(content);
            if (entry == null) return null;
            //legacy would throw an exception here if it was not 'simple' and simple means based on a username
            if (entry.Rules.All(x => x.RuleType != Constants.Conventions.PublicAccess.MemberUsernameRuleType))
            {
                throw new Exception("Document isn't protected using Simple mechanism. Use GetAccessingMemberGroups instead");
            }
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
            var usernameRule = entry.Rules.First(x => x.RuleType == Constants.Conventions.PublicAccess.MemberUsernameRuleType);
            return provider.GetUser(usernameRule.RuleValue, false);
        }

        private bool IsProtectedByMembershipRole(int documentId, string role)
        {
            var content = Services.ContentService.GetById(documentId);
            var entry = Services.PublicAccessService.GetEntryForContent(content);
            if (entry == null) return false;
            return entry.Rules
                .Any(x => x.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType
                    && x.RuleValue == role);
        }

        private void ProtectPage(bool Simple, int DocumentId, int LoginDocumentId, int ErrorDocumentId)
        {
            var doc = new Document(DocumentId);
            var loginContent = Services.ContentService.GetById(LoginDocumentId);
            if (loginContent == null) throw new NullReferenceException("No content item found with id " + LoginDocumentId);
            var noAccessContent = Services.ContentService.GetById(ErrorDocumentId);
            if (noAccessContent == null) throw new NullReferenceException("No content item found with id " + ErrorDocumentId);

            var entry = Services.PublicAccessService.GetEntryForContent(doc.ContentEntity.Id.ToString());
            if (entry != null)
            {
                if (Simple)
                {
                    // if using simple mode, make sure that all existing groups are removed
                    entry.ClearRules();
                }

                //ensure the correct ids are applied
                entry.LoginNodeId = loginContent.Id;
                entry.NoAccessNodeId = noAccessContent.Id;
            }
            else
            {
                entry = new PublicAccessEntry(doc.ContentEntity,
                    Services.ContentService.GetById(LoginDocumentId),
                    Services.ContentService.GetById(ErrorDocumentId),
                    new List<PublicAccessRule>());
            }
            Services.PublicAccessService.Save(entry);
        }

        private void AddMembershipRoleToDocument(int documentId, string role)
        {
            //event
            var doc = new Document(documentId);

            var entry = Services.PublicAccessService.AddRule(
                doc.ContentEntity,
                Constants.Conventions.PublicAccess.MemberRoleRuleType,
                role);

            if (entry.Success == false && entry.Result.Value == null)
            {
                throw new Exception("Document is not protected!");
            }            
        }

        private void AddMembershipUserToDocument(int documentId, string membershipUserName)
        {
            //event
            var doc = new Document(documentId);
            var entry = Services.PublicAccessService.AddRule(
                doc.ContentEntity,
                Constants.Conventions.PublicAccess.MemberUsernameRuleType,
                membershipUserName);

            if (entry.Success == false && entry.Result.Value == null)
            {
                throw new Exception("Document is not protected!");
            }
        }

        private void RemoveMembershipRoleFromDocument(int documentId, string role)
        {
            var doc = new Document(documentId);
            Services.PublicAccessService.RemoveRule(
                doc.ContentEntity,
                Constants.Conventions.PublicAccess.MemberRoleRuleType,
                role);
        }

        private void RemoveProtection(int documentId)
        {
            var doc = new Document(documentId);
            var entry = Services.PublicAccessService.GetEntryForContent(doc.ContentEntity);
            if (entry != null)
            {
                Services.PublicAccessService.Delete(entry);
            }            
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check for editing
            int documentId = int.Parse(Request.GetItemAsString("nodeId"));
            var documentObject = new Document(documentId);
            jsShowWindow.Text = "";

            ph_errorpage.Controls.Add(errorPagePicker);
            ph_loginpage.Controls.Add(loginPagePicker);

            pp_login.Text = Services.TextService.Localize("login");
            pp_pass.Text = Services.TextService.Localize("password");
            pp_loginPage.Text = Services.TextService.Localize("paLoginPage");
            pp_errorPage.Text = Services.TextService.Localize("paErrorPage");

            pane_chooseMode.Text = Services.TextService.Localize("publicAccess/paHowWould");
            pane_pages.Text = Services.TextService.Localize("publicAccess/paSelectPages");
            pane_simple.Text = Services.TextService.Localize("publicAccess/paSimple");
            pane_advanced.Text = Services.TextService.Localize("publicAccess/paAdvanced");

            if (IsPostBack == false)
            {
                if (Services.PublicAccessService.IsProtected(documentId.ToString()) 
                    && GetProtectionType(documentId) != ProtectionType.NotProtected)
                {
                    bt_buttonRemoveProtection.Visible = true;
                    bt_buttonRemoveProtection.Attributes.Add("onClick", "return confirm('" + Services.TextService.Localize("areyousure") + "')");

                    // Get login and error pages
                    int errorPage = GetErrorPage(documentObject.Path);
                    int loginPage = GetLoginPage(documentObject.Path);
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

                    if (GetProtectionType(documentId) == ProtectionType.Simple)
                    {
                        MembershipUser m = GetAccessingMembershipUser(documentId);
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
                    else if (GetProtectionType(documentId) == ProtectionType.Advanced)
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
                        if (IsProtectedByMembershipRole(int.Parse(Request.GetItemAsString("nodeid")), role))
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


            bt_protect.Text = Services.TextService.Localize("update");
            bt_buttonRemoveProtection.Text = Services.TextService.Localize("paRemoveProtection");

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

            var provider = Umbraco.Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
            
                int pageId = int.Parse(Request.GetItemAsString("nodeId"));

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
                            MemberType.MakeNew(Services.UserService.GetUserById(0), Constants.Conventions.MemberTypes.SystemDefaultProtectType);
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

                    ProtectPage(true, pageId, int.Parse(loginPagePicker.Value), int.Parse(errorPagePicker.Value));
                    AddMembershipRoleToDocument(pageId, simpleRoleName);
                    AddMembershipUserToDocument(pageId, member.UserName);
            }
            else if (e.CommandName == "advanced")
            {
                if (cv_errorPage.IsValid && cv_loginPage.IsValid)
                {
                    ProtectPage(false, pageId, int.Parse(loginPagePicker.Value), int.Parse(errorPagePicker.Value));

                    foreach (ListItem li in _memberGroups.Items)
                        if (("," + _memberGroups.Value + ",").IndexOf("," + li.Value + ",", StringComparison.Ordinal) > -1)
                            AddMembershipRoleToDocument(pageId, li.Value);
                        else
                            RemoveMembershipRoleFromDocument(pageId, li.Value);
                }
                else
                {
                    return;
                }
            }

            feedback.Text = Services.TextService.Localize("publicAccess/paIsProtected", new[] { new cms.businesslogic.CMSNode(pageId).Text}) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + Services.TextService.Localize("closeThisWindow") + "</a>";

            p_buttons.Visible = false;
            pane_advanced.Visible = false;
            pane_simple.Visible = false;
                var content = Services.ContentService.GetById(pageId);
            //reloads the current node in the tree
            ClientTools.SyncTree(content.Path, true);
            //reloads the current node's children in the tree
            ClientTools.ReloadActionNode(false, true);
            feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
        }


        protected void buttonRemoveProtection_Click(object sender, System.EventArgs e)
        {
            int pageId = int.Parse(Request.GetItemAsString("nodeId"));
            p_buttons.Visible = false;
            pane_advanced.Visible = false;
            pane_simple.Visible = false;

            RemoveProtection(pageId);

            feedback.Text = Services.TextService.Localize("publicAccess/paIsRemoved", new[] { new cms.businesslogic.CMSNode(pageId).Text}) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + Services.TextService.Localize("closeThisWindow") + "</a>";

            var content = Services.ContentService.GetById(pageId);
            //reloads the current node in the tree
            ClientTools.SyncTree(content.Path, true);
            //reloads the current node's children in the tree
            ClientTools.ReloadActionNode(false, true);
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
