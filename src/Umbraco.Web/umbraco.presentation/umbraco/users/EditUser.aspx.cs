using Umbraco.Core.Services;
using System;
using System.Configuration.Provider;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Security;
using umbraco.BusinessLogic;
using umbraco.controls;
using umbraco.uicontrols;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.IO;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.UI;
using Umbraco.Web.UI.Pages;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Summary description for EditUser.
    /// </summary>
    public partial class EditUser : UmbracoEnsuredPage
    {
        public EditUser()
        {
            CurrentApp = Constants.Applications.Users.ToString();
        }
        protected HtmlTable macroProperties;
        protected TextBox uname = new TextBox();
        protected TextBox lname = new TextBox();
        protected PlaceHolder passw = new PlaceHolder();
        protected CheckBoxList lapps = new CheckBoxList();
        protected TextBox email = new TextBox();
        protected DropDownList userType = new DropDownList();
        protected DropDownList userLanguage = new DropDownList();
        protected CheckBox NoConsole = new CheckBox();
        protected CheckBox Disabled = new CheckBox();

        protected ContentPicker mediaPicker = new ContentPicker();
        protected ContentPicker contentPicker = new ContentPicker();

        protected TextBox cName = new TextBox();
        protected CheckBox cFulltree = new CheckBox();
        protected DropDownList cDocumentType = new DropDownList();
        protected DropDownList cDescription = new DropDownList();
        protected DropDownList cCategories = new DropDownList();
        protected DropDownList cExcerpt = new DropDownList();
        protected ContentPicker cMediaPicker = new ContentPicker();
        protected ContentPicker cContentPicker = new ContentPicker();
        protected CustomValidator sectionValidator = new CustomValidator();

        protected Pane pp = new Pane();

        private IUser u;

        private MembershipHelper _membershipHelper;

        private MembershipProvider BackOfficeProvider
        {
            get { return global::Umbraco.Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider(); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _membershipHelper = new MembershipHelper(UmbracoContext.Current);
            int UID = int.Parse(Request.QueryString["id"]);
            u = Services.UserService.GetUserById(UID);

            //the true admin can only edit the true admin
            if (u.Id == 0 && Security.CurrentUser.Id != 0)
            {
                throw new Exception("Only the root user can edit the 'root' user (id:0)");
            }

            //only another admin can edit another admin (who is not the true admin)
            if (u.IsAdmin() && Security.CurrentUser.IsAdmin() == false)
            {
                throw new Exception("Admin users can only be edited by admins");
            }

            // Populate usertype list
            foreach (var ut in Services.UserService.GetAllUserTypes())
            {
                if (Security.CurrentUser.IsAdmin() || ut.Alias != "admin")
                {
                    ListItem li = new ListItem(Services.TextService.Localize("user", ut.Name.ToLower()), ut.Id.ToString());
                    if (ut.Id == u.UserType.Id)
                        li.Selected = true;

                    userType.Items.Add(li);
                }
            }
            
            var userCulture = UserExtensions.GetUserCulture(u.Language, Services.TextService);

            // Populate ui language lsit
            foreach (var lang in Services.TextService.GetSupportedCultures())
            {
                var regionCode = Services.TextService.ConvertToRegionCodeFromSupportedCulture(lang);
                
                var li = new ListItem(lang.DisplayName, regionCode);

                if (Equals(lang, userCulture))
                    li.Selected = true;

                userLanguage.Items.Add(li);
            }

            // Console access and disabling
            NoConsole.Checked = u.IsLockedOut;
            Disabled.Checked = u.IsApproved == false;

            PlaceHolder medias = new PlaceHolder();
            mediaPicker.AppAlias = Constants.Applications.Media;
            mediaPicker.TreeAlias = "media";

            if (u.StartMediaId > 0)
                mediaPicker.Value = u.StartMediaId.ToString();
            else
                mediaPicker.Value = "-1";

            medias.Controls.Add(mediaPicker);

            PlaceHolder content = new PlaceHolder();
            contentPicker.AppAlias = Constants.Applications.Content;
            contentPicker.TreeAlias = "content";

            if (u.StartContentId > 0)
                contentPicker.Value = u.StartContentId.ToString(CultureInfo.InvariantCulture);
            else
                contentPicker.Value = "-1";

            content.Controls.Add(contentPicker);


            // Add password changer
            var passwordChanger = (passwordChanger)LoadControl(SystemDirectories.Umbraco + "/controls/passwordChanger.ascx");
            passwordChanger.MembershipProviderName = UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider;

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
            validatorContainer.Attributes["class"] = "alert alert-error";
            validatorContainer.Style.Add(HtmlTextWriterStyle.MarginTop, "10px");
            validatorContainer.Style.Add(HtmlTextWriterStyle.Width, "300px");
            var validatorContainer2 = new HtmlGenericControl("p");
            validatorContainer.Controls.Add(validatorContainer2);
            validatorContainer2.Controls.Add(passwordValidation);
            passw.Controls.Add(passwordChanger);
            passw.Controls.Add(validatorContainer);

            pp.addProperty(Services.TextService.Localize("user/username"), uname);
            pp.addProperty(Services.TextService.Localize("user/loginname"), lname);
            pp.addProperty(Services.TextService.Localize("user/password"), passw);
            pp.addProperty(Services.TextService.Localize("email"), email);
            pp.addProperty(Services.TextService.Localize("user/usertype"), userType);
            pp.addProperty(Services.TextService.Localize("user/language"), userLanguage);

            //Media  / content root nodes
            Pane ppNodes = new Pane();
            ppNodes.addProperty(Services.TextService.Localize("user/startnode"), content);
            ppNodes.addProperty(Services.TextService.Localize("user/mediastartnode"), medias);

            //Generel umrbaco access
            Pane ppAccess = new Pane();
            ppAccess.addProperty(Services.TextService.Localize("user/noConsole"), NoConsole);
            ppAccess.addProperty(Services.TextService.Localize("user/disabled"), Disabled);

            //access to which modules... 
            Pane ppModules = new Pane();
            ppModules.addProperty(Services.TextService.Localize("user/modules"), lapps);
            ppModules.addProperty(" ", sectionValidator);

            TabPage userInfo = UserTabs.NewTabPage(u.Name);

            userInfo.Controls.Add(pp);

            userInfo.Controls.Add(ppAccess);
            userInfo.Controls.Add(ppNodes);

            userInfo.Controls.Add(ppModules);

            userInfo.HasMenu = true;

            var save = userInfo.Menu.NewButton();
            save.Click += SaveUser_Click;
            save.ID = "save";
            save.ToolTip = Services.TextService.Localize("save");
            save.Text = Services.TextService.Localize("save");
            save.ButtonType = MenuButtonType.Primary;

            sectionValidator.ServerValidate += new ServerValidateEventHandler(sectionValidator_ServerValidate);
            sectionValidator.ControlToValidate = lapps.ID;
            sectionValidator.ErrorMessage = Services.TextService.Localize("errorHandling/errorMandatoryWithoutTab", new[] { Services.TextService.Localize("user/modules") });
            sectionValidator.CssClass = "error";
            sectionValidator.Style.Add("color", "red");

            SetupForm();

            ClientTools
                .SetActiveTreeType(Constants.Trees.Users)
                .SyncTree(UID.ToString(), false);
        }


        void sectionValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = false || lapps.SelectedIndex >= 0;
        }


        /// <summary>
        /// Setups the form.
        /// </summary>
        private void SetupForm()
        {

            if (!IsPostBack)
            {
                MembershipUser user = BackOfficeProvider.GetUser(u.Username, false);
                uname.Text = u.Name;
                lname.Text = (user == null) ? u.Username : user.UserName;
                email.Text = (user == null) ? u.Email : user.Email;

                contentPicker.Value = u.StartContentId.ToString(CultureInfo.InvariantCulture);
                mediaPicker.Value = u.StartMediaId.ToString(CultureInfo.InvariantCulture);

                // get the current users applications
                string currentUserApps = ";";
                foreach (var a in Security.CurrentUser.AllowedSections)
                    currentUserApps += a + ";";

                var uapps = u.AllowedSections.ToArray();
                foreach (var app in Services.SectionService.GetSections())
                {
                    if (Security.CurrentUser.IsAdmin() || currentUserApps.Contains(";" + app.Alias + ";"))
                    {
                        var li = new ListItem(Services.TextService.Localize("sections", app.Alias), app.Alias);
                        if (IsPostBack == false)
                        {
                            foreach (var tmp in uapps)
                            { 
                                if (app.Alias == tmp) li.Selected = true;
                            }
                        }
                        lapps.Items.Add(li);
                    }
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //lapps.SelectionMode = ListSelectionMode.Multiple;
            lapps.RepeatLayout = RepeatLayout.Flow;
            lapps.RepeatDirection = RepeatDirection.Vertical;
        }

     
        /// <summary>
        /// This handles changing the password
        /// </summary>
        /// <param name="passwordChangerControl"></param>
        /// <param name="membershipUser"></param>
        /// <param name="passwordChangerValidator"></param>
        private void ChangePassword(passwordChanger passwordChangerControl, MembershipUser membershipUser, CustomValidator passwordChangerValidator)
        {
            if (passwordChangerControl.IsChangingPassword)
            {
                //SD: not sure why this check is here but must have been for some reason at some point?
                if (string.IsNullOrEmpty(passwordChangerControl.ChangingPasswordModel.NewPassword) == false)
                {
                    // make sure password is not empty
                    if (string.IsNullOrEmpty(u.RawPasswordValue)) u.RawPasswordValue = "default";
                }

                var changePasswordModel = passwordChangerControl.ChangingPasswordModel;

                //now do the actual change
                var changePassResult = _membershipHelper.ChangePassword(
                    membershipUser.UserName, changePasswordModel, BackOfficeProvider);

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
                    passw.Controls[1].Visible = true;
                }

            }
        }

        /// <summary>
        /// Handles the Click event of the saveUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.ImageClickEventArgs"/> instance containing the event data.</param>
        private void SaveUser_Click(object sender, EventArgs e)
        {
            if (base.IsValid)
            {
                try
                {
                    var membershipUser = BackOfficeProvider.GetUser(u.Username, false);
                    if (membershipUser == null)
                    {
                        throw new ProviderException("Could not find user in the membership provider with login name " + u.Username);
                    }

                    var passwordChangerControl = (passwordChanger)passw.Controls[0];
                    var passwordChangerValidator = (CustomValidator)passw.Controls[1].Controls[0].Controls[0];

                    //perform the changing password logic
                    ChangePassword(passwordChangerControl, membershipUser, passwordChangerValidator);

                    //update the membership provider
                    UpdateMembershipProvider(membershipUser);

                    //update the Umbraco user properties - even though we are updating some of these properties in the membership provider that is 
                    // ok since the membership provider might be storing these details someplace totally different! But we want to keep our UI in sync.
                    u.Name = uname.Text.Trim();
                    u.Language = userLanguage.SelectedValue;
                    u.UserType = Services.UserService.GetUserTypeById(int.Parse(userType.SelectedValue));
                    u.Email = email.Text.Trim();
                    u.Username = lname.Text;
                    u.IsApproved = Disabled.Checked == false;
                    u.IsLockedOut = NoConsole.Checked;

                    int startNode;
                    if (int.TryParse(contentPicker.Value, out startNode) == false)
                    {
                        //set to default if nothing is choosen
                        if (u.StartContentId > 0)
                            startNode = u.StartContentId;
                        else
                            startNode = -1;
                    }
                    u.StartContentId = startNode;


                    int mstartNode;
                    if (int.TryParse(mediaPicker.Value, out mstartNode) == false)
                    {
                        //set to default if nothing is choosen
                        if (u.StartMediaId > 0)
                            mstartNode = u.StartMediaId;
                        else
                            mstartNode = -1;
                    }
                    u.StartMediaId = mstartNode;

                    u.ClearAllowedSections();
                    foreach (ListItem li in lapps.Items)
                    {
                        if (li.Selected)
                            u.AddAllowedSection(li.Value);
                    }

                    Services.UserService.Save(u);

                    ClientTools.ShowSpeechBubble(SpeechBubbleIcon.Save, Services.TextService.Localize("speechBubbles/editUserSaved"), "");
                }
                catch (Exception ex)
                {
                    ClientTools.ShowSpeechBubble(SpeechBubbleIcon.Error, Services.TextService.Localize("speechBubbles/editUserError"), "");
                    LogHelper.Error<EditUser>("Exception", ex);
                }
            }
            else
            {
                ClientTools.ShowSpeechBubble(SpeechBubbleIcon.Error, Services.TextService.Localize("speechBubbles/editUserError"), "");
            }
        }

        private void UpdateMembershipProvider(MembershipUser membershipUser)
        {
            //SD: This check must be here for some reason but apparently we don't want to try to 
            // update when the AD provider is active.
            if ((BackOfficeProvider is ActiveDirectoryMembershipProvider) == false)
            {
                var membershipHelper = new MembershipHelper(ApplicationContext, new HttpContextWrapper(Context));
                //set the writable properties that we are editing
                membershipHelper.UpdateMember(membershipUser, BackOfficeProvider,
                                              email.Text.Trim(),
                                              Disabled.Checked == false);
            }
        }

        /// <summary>
        /// UserTabs control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected TabView UserTabs;
    }
}
