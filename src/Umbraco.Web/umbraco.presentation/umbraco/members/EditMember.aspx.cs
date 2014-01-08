using System;
using System.Configuration.Provider;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using Umbraco.Web;
using Umbraco.Web.Security;
using umbraco.cms.businesslogic.member;
using System.Web.Security;
using umbraco.controls;

namespace umbraco.cms.presentation.members
{
    /// <summary>
    /// Summary description for EditMember.
    /// </summary>
    public partial class EditMember : BasePages.UmbracoEnsuredPage
    {
        public EditMember()
        {
            CurrentApp = BusinessLogic.DefaultApps.member.ToString();
        }
        protected uicontrols.TabView TabView1;
        protected TextBox documentName;
        private Member _document;
        private MembershipUser _member;
        ContentControl _contentControl;
        protected uicontrols.UmbracoPanel m_MemberShipPanel = new uicontrols.UmbracoPanel();

        protected TextBox MemberLoginNameTxt = new TextBox();
        protected RequiredFieldValidator MemberLoginNameVal = new RequiredFieldValidator();
        protected CustomValidator MemberLoginNameExistCheck = new CustomValidator();

        protected PlaceHolder MemberPasswordTxt = new PlaceHolder();
        protected TextBox MemberEmail = new TextBox();
        protected CustomValidator MemberEmailExistCheck = new CustomValidator();
        protected DualSelectbox _memberGroups = new DualSelectbox();

        protected void Page_Load(object sender, EventArgs e)
        {

            // Add password changer
            var passwordChanger = (passwordChanger)LoadControl(SystemDirectories.Umbraco + "/controls/passwordChanger.ascx");
            passwordChanger.MembershipProviderName = Membership.Provider.Name;
            //Add a custom validation message for the password changer
            var passwordValidation = new CustomValidator
            {
                ID = "PasswordChangerValidator"
            };
            var validatorContainer = new HtmlGenericControl("div")
            {
                Visible = false,
                EnableViewState = false
            };
            validatorContainer.Attributes["class"] = "error";
            validatorContainer.Style.Add(HtmlTextWriterStyle.MarginTop, "10px");
            validatorContainer.Style.Add(HtmlTextWriterStyle.Width, "300px");
            var validatorContainer2 = new HtmlGenericControl("p");
            validatorContainer.Controls.Add(validatorContainer2);
            validatorContainer2.Controls.Add(passwordValidation);
            MemberPasswordTxt.Controls.Add(passwordChanger);
            MemberPasswordTxt.Controls.Add(validatorContainer);

            if (Member.InUmbracoMemberMode())
            {
                _document = new Member(int.Parse(Request.QueryString["id"]));
                _member = Membership.GetUser(_document.LoginName, false);
                _contentControl = new ContentControl(_document, controls.ContentControl.publishModes.NoPublish, "TabView1");
                _contentControl.Width = Unit.Pixel(666);
                _contentControl.Height = Unit.Pixel(666);

                //this must be set to false as we don't want to proceed to save anything if the page is invalid
                _contentControl.SavePropertyDataWhenInvalid = false;

                plc.Controls.Add(_contentControl);

                if (!IsPostBack)
                {
                    MemberLoginNameTxt.Text = _document.LoginName;
                    MemberEmail.Text = _document.Email;
                }
                var ph = new PlaceHolder();
                MemberLoginNameTxt.ID = "loginname";
                ph.Controls.Add(MemberLoginNameTxt);
                ph.Controls.Add(MemberLoginNameVal);
                MemberLoginNameVal.ControlToValidate = MemberLoginNameTxt.ID;
                string[] errorVars = { ui.Text("login") };
                MemberLoginNameVal.ErrorMessage = " " + ui.Text("errorHandling", "errorMandatoryWithoutTab", errorVars, null);
                MemberLoginNameVal.EnableClientScript = false;
                MemberLoginNameVal.Display = ValidatorDisplay.Dynamic;

                MemberLoginNameExistCheck.ErrorMessage = ui.Text("errorHandling", "errorExistsWithoutTab", "Login Name", BasePages.UmbracoEnsuredPage.CurrentUser);
                MemberLoginNameExistCheck.EnableClientScript = false;
                MemberLoginNameExistCheck.ValidateEmptyText = false;
                MemberLoginNameExistCheck.ControlToValidate = MemberLoginNameTxt.ID;
                MemberLoginNameExistCheck.ServerValidate += MemberLoginNameExistCheck_ServerValidate;

                MemberEmailExistCheck.ErrorMessage = ui.Text("errorHandling", "errorExistsWithoutTab", "E-mail", BasePages.UmbracoEnsuredPage.CurrentUser);
                MemberEmailExistCheck.EnableClientScript = false;
                MemberEmailExistCheck.ValidateEmptyText = false;
                MemberEmailExistCheck.ControlToValidate = MemberEmail.ID;
                MemberEmailExistCheck.ServerValidate += MemberEmailExistCheck_ServerValidate;

                _contentControl.PropertiesPane.addProperty("", MemberLoginNameExistCheck);
                _contentControl.PropertiesPane.addProperty(ui.Text("login"), ph);
                _contentControl.PropertiesPane.addProperty(ui.Text("password"), MemberPasswordTxt);
                _contentControl.PropertiesPane.addProperty("", MemberEmailExistCheck);
                _contentControl.PropertiesPane.addProperty("Email", MemberEmail);
            }
            else
            {
                m_MemberShipPanel.hasMenu = true;
                var menuSave = m_MemberShipPanel.Menu.NewImageButton();
                menuSave.ID = m_MemberShipPanel.ID + "_save";
                menuSave.ImageUrl = SystemDirectories.Umbraco + "/images/editor/save.gif";
                menuSave.Click += new ImageClickEventHandler(MenuSaveClick);
                menuSave.AltText = ui.Text("buttons", "save", null);

                _member = Membership.GetUser(Request.QueryString["id"], false);
                MemberLoginNameTxt.Text = _member.UserName;
                if (IsPostBack == false)
                {
                    MemberEmail.Text = _member.Email;
                }

                m_MemberShipPanel.Width = 300;
                m_MemberShipPanel.Text = ui.Text("edit") + " " + _member.UserName;
                var props = new uicontrols.Pane();
                MemberLoginNameTxt.Enabled = false;
                
                props.Controls.Add(AddProperty(ui.Text("login"), MemberLoginNameTxt));
                props.Controls.Add(AddProperty(ui.Text("password"), MemberPasswordTxt));
                props.Controls.Add(AddProperty("Email", MemberEmail));
                m_MemberShipPanel.Controls.Add(props);
                plc.Controls.Add(m_MemberShipPanel);
            }

            // Groups
            var p = new uicontrols.Pane();
            _memberGroups.ID = "Membergroups";
            _memberGroups.Width = 175;
            var selectedMembers = "";
            foreach (var role in Roles.GetAllRoles())
            {
                // if a role starts with __umbracoRole we won't show it as it's an internal role used for public access
                if (role.StartsWith("__umbracoRole") == false)
                {
                    var li = new ListItem(role);
                    if (IsPostBack == false)
                    {

                        if (Roles.IsUserInRole(_member.UserName, role))
                        {
                            selectedMembers += role + ",";
                        }
                    }
                    _memberGroups.Items.Add(li);
                }
            }
            _memberGroups.Value = selectedMembers;

            p.addProperty(ui.Text("membergroup"), _memberGroups);

            if (Member.InUmbracoMemberMode())
            {
                _contentControl.tpProp.Controls.Add(p);
                _contentControl.Save += new System.EventHandler(tmp_save);
            }
            else
            {
                m_MemberShipPanel.Controls.Add(p);
            }

        }

        void MemberLoginNameExistCheck_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var oldLoginName = _document.LoginName.Replace(" ", "").ToLower();
            var newLoginName = MemberLoginNameTxt.Text.Replace(" ", "").ToLower();

            if (oldLoginName != newLoginName && newLoginName != "" && Member.GetMemberFromLoginName(newLoginName) != null)
                args.IsValid = false;
            else
                args.IsValid = true;
        }

        void MemberEmailExistCheck_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var oldEmail = _document.Email.ToLower();
            var newEmail = MemberEmail.Text.ToLower();

            var requireUniqueEmail = Membership.Provider.RequiresUniqueEmail;

            var howManyMembersWithEmail = 0;
            var membersWithEmail = Member.GetMembersFromEmail(newEmail);
            if (membersWithEmail != null)
                howManyMembersWithEmail = membersWithEmail.Length;

            if (((oldEmail == newEmail && howManyMembersWithEmail > 1) ||
                (oldEmail != newEmail && howManyMembersWithEmail > 0))
                && requireUniqueEmail)
                // If the value hasn't changed and there are more than 1 member with that email, then false
                // If the value has changed and there are any member with that new email, then false
                args.IsValid = false;
            else
                args.IsValid = true;
        }

        void MenuSaveClick(object sender, ImageClickEventArgs e)
        {

            tmp_save(sender, e);

        }

        private void ChangePassword(passwordChanger passwordChangerControl, MembershipUser membershipUser, CustomValidator passwordChangerValidator)
        {
            //Change the password            
            
            if (passwordChangerControl.IsChangingPassword)
            {
                var changePassResult = UmbracoContext.Current.Security.ChangePassword(
                    membershipUser.UserName, passwordChangerControl.ChangingPasswordModel, Membership.Provider);

                if (changePassResult.Success)
                {
                    //if it is successful, we need to show the generated password if there was one, so set
                    //that back on the control
                    passwordChangerControl.ChangingPasswordModel.GeneratedPassword = changePassResult.Result.ResetPassword;
                }
                else
                {
                    passwordChangerValidator.IsValid = false;
                    passwordChangerValidator.ErrorMessage = changePassResult.Result.ChangeError.ErrorMessage;
                    MemberPasswordTxt.Controls[1].Visible = true;
                }
            }
        }

        private void UpdateMembershipProvider(MembershipUser membershipUser)
        {
            var membershipHelper = new MembershipHelper();
            //set the writable properties that we are editing
            membershipHelper.UpdateMember(membershipUser, Membership.Provider,
                                          MemberEmail.Text.Trim());
        }

        private void UpdateRoles(MembershipUser membershipUser)
        {
            // Groups
            foreach (ListItem li in _memberGroups.Items)
            {
                if (("," + _memberGroups.Value + ",").IndexOf("," + li.Value + ",", StringComparison.Ordinal) > -1)
                {
                    if (Roles.IsUserInRole(membershipUser.UserName, li.Value) == false)
                        Roles.AddUserToRole(membershipUser.UserName, li.Value);
                }
                else if (Roles.IsUserInRole(membershipUser.UserName, li.Value))
                {
                    Roles.RemoveUserFromRole(membershipUser.UserName, li.Value);
                }
            }
        }

        protected void tmp_save(object sender, EventArgs e)
        {
            Page.Validate();
            if (Page.IsValid == false)
            {
                foreach (uicontrols.TabPage tp in _contentControl.GetPanels())
                {
                    tp.ErrorControl.Visible = true;
                    tp.ErrorHeader = ui.Text("errorHandling", "errorHeader");
                    tp.CloseCaption = ui.Text("close");
                }
            }
            else
            {
                // hide validation summaries
                if (Member.InUmbracoMemberMode())
                {                    
                    foreach (uicontrols.TabPage tp in _contentControl.GetPanels())
                    {
                        tp.ErrorControl.Visible = false;
                    }
                }

                //Change the password
                var passwordChangerControl = (passwordChanger)MemberPasswordTxt.Controls[0];
                var passwordChangerValidator = (CustomValidator)MemberPasswordTxt.Controls[1].Controls[0].Controls[0];
                ChangePassword(passwordChangerControl, _member, passwordChangerValidator);

                //update the membership provider
                UpdateMembershipProvider(_member);

                if (Member.InUmbracoMemberMode())
                {
                    //Hrm, with the membership provider you cannot change the login name - I guess this will do that 
                    // in the underlying data layer
                    _document.LoginName = MemberLoginNameTxt.Text;

                    UpdateRoles(_member);

                    //The value of the properties has been set on IData through IDataEditor in the ContentControl
                    //so we need to 'retrieve' that value and set it on the property of the new IContent object.
                    //NOTE This is a workaround for the legacy approach to saving values through the DataType instead of the Property 
                    //- (The DataType shouldn't be responsible for saving the value - especically directly to the db).
                    foreach (var item in _contentControl.DataTypes)
                    {
                        _document.getProperty(item.Key).Value = item.Value.Data.Value;
                    }

                    // refresh cache
                    _document.XmlGenerate(new System.Xml.XmlDocument());
                    _document.Save();
                }
                else
                {
                    UpdateRoles(_member);
                }

                ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editMemberSaved", UmbracoUser), "");
            }
        }

        private uicontrols.PropertyPanel AddProperty(string caption, Control c)
        {
            var pp = new uicontrols.PropertyPanel();
            pp.Controls.Add(c);
            pp.Text = caption;
            return pp;
        }

        /// <summary>
        /// doSave control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlInputHidden doSave;

        /// <summary>
        /// doPublish control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlInputHidden doPublish;

        /// <summary>
        /// plc control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder plc;

    }
}
