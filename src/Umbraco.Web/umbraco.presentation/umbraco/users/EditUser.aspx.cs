using System;
using System.Collections;
using System.Configuration.Provider;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Security;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;
using umbraco.controls;
using umbraco.presentation.channels.businesslogic;
using umbraco.uicontrols;
using umbraco.providers;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.IO;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using PropertyType = umbraco.cms.businesslogic.propertytype.PropertyType;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Summary description for EditUser.
    /// </summary>
    public partial class EditUser : UmbracoEnsuredPage
    {
        public EditUser()
        {
            CurrentApp = DefaultApps.users.ToString();
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

        private User u;

        private MembershipHelper _membershipHelper;

        private MembershipProvider BackOfficeProvider
        {
            get { return global::Umbraco.Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider(); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _membershipHelper = new MembershipHelper(UmbracoContext.Current);
            int UID = int.Parse(Request.QueryString["id"]);
            u = BusinessLogic.User.GetUser(UID);

            //the true admin can only edit the true admin
            if (u.Id == 0 && CurrentUser.Id != 0)
            {
                throw new Exception("Only the root user can edit the 'root' user (id:0)");
            }

            //only another admin can edit another admin (who is not the true admin)
            if (u.IsAdmin() && CurrentUser.IsAdmin() == false)
            {
                throw new Exception("Admin users can only be edited by admins");
            }

            // Populate usertype list
            foreach (UserType ut in UserType.getAll)
            {
                if (CurrentUser.IsAdmin() || ut.Alias != "admin")
                {
                    ListItem li = new ListItem(ui.Text("user", ut.Name.ToLower(), UmbracoUser), ut.Id.ToString());
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
            NoConsole.Checked = u.NoConsole;
            Disabled.Checked = u.Disabled;

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

            if (u.StartNodeId > 0)
                contentPicker.Value = u.StartNodeId.ToString();
            else
                contentPicker.Value = "-1";

            content.Controls.Add(contentPicker);


            // Add password changer
            var passwordChanger = (passwordChanger)LoadControl(SystemDirectories.Umbraco + "/controls/passwordChanger.ascx");
            passwordChanger.MembershipProviderName = UmbracoSettings.DefaultBackofficeProvider;

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

            pp.addProperty(ui.Text("user", "username", UmbracoUser), uname);
            pp.addProperty(ui.Text("user", "loginname", UmbracoUser), lname);
            pp.addProperty(ui.Text("user", "password", UmbracoUser), passw);
            pp.addProperty(ui.Text("email", UmbracoUser), email);
            pp.addProperty(ui.Text("user", "usertype", UmbracoUser), userType);
            pp.addProperty(ui.Text("user", "language", UmbracoUser), userLanguage);

            //Media  / content root nodes
            Pane ppNodes = new Pane();
            ppNodes.addProperty(ui.Text("user", "startnode", UmbracoUser), content);
            ppNodes.addProperty(ui.Text("user", "mediastartnode", UmbracoUser), medias);

            //Generel umrbaco access
            Pane ppAccess = new Pane();
            ppAccess.addProperty(ui.Text("user", "noConsole", UmbracoUser), NoConsole);
            ppAccess.addProperty(ui.Text("user", "disabled", UmbracoUser), Disabled);

            //access to which modules... 
            Pane ppModules = new Pane();
            ppModules.addProperty(ui.Text("user", "modules", UmbracoUser), lapps);
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
            save.ToolTip = ui.Text("save");
            save.Text = ui.Text("save");
            save.ButtonType = MenuButtonType.Primary;

            sectionValidator.ServerValidate += new ServerValidateEventHandler(sectionValidator_ServerValidate);
            sectionValidator.ControlToValidate = lapps.ID;
            sectionValidator.ErrorMessage = ui.Text("errorHandling", "errorMandatoryWithoutTab", ui.Text("user", "modules", UmbracoUser), UmbracoUser);
            sectionValidator.CssClass = "error";
            sectionValidator.Style.Add("color", "red");

            SetupForm();
            SetupChannel();

            ClientTools
                .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadUsers>().Tree.Alias)
                .SyncTree(UID.ToString(), IsPostBack);
        }


        void sectionValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = false;

            if (lapps.SelectedIndex >= 0)
                args.IsValid = true;
        }

        private void SetupChannel()
        {
            Channel userChannel;
            try
            {
                userChannel =
                    new Channel(u.Id);
            }
            catch
            {
                userChannel = new Channel();
            }

            // Populate dropdowns
            foreach (DocumentType dt in DocumentType.GetAllAsList())
                cDocumentType.Items.Add(
                    new ListItem(dt.Text, dt.Alias)
                    );

            // populate fields
            ArrayList fields = new ArrayList();
            cDescription.ID = "cDescription";
            cCategories.ID = "cCategories";
            cExcerpt.ID = "cExcerpt";
            cDescription.Items.Add(new ListItem(ui.Text("choose"), ""));
            cCategories.Items.Add(new ListItem(ui.Text("choose"), ""));
            cExcerpt.Items.Add(new ListItem(ui.Text("choose"), ""));

            foreach (PropertyType pt in PropertyType.GetAll())
            {
                if (!fields.Contains(pt.Alias))
                {
                    cDescription.Items.Add(new ListItem(string.Format("{0} ({1})", pt.Name, pt.Alias), pt.Alias));
                    cCategories.Items.Add(new ListItem(string.Format("{0} ({1})", pt.Name, pt.Alias), pt.Alias));
                    cExcerpt.Items.Add(new ListItem(string.Format("{0} ({1})", pt.Name, pt.Alias), pt.Alias));
                    fields.Add(pt.Alias);
                }
            }

            // Handle content and media pickers

            PlaceHolder medias = new PlaceHolder();
            cMediaPicker.AppAlias = Constants.Applications.Media;
            cMediaPicker.TreeAlias = "media";

            if (userChannel.MediaFolder > 0)
                cMediaPicker.Value = userChannel.MediaFolder.ToString();
            else
                cMediaPicker.Value = "-1";

            medias.Controls.Add(cMediaPicker);

            PlaceHolder content = new PlaceHolder();
            cContentPicker.AppAlias = Constants.Applications.Content;
            cContentPicker.TreeAlias = "content";

            if (userChannel.StartNode > 0)
                cContentPicker.Value = userChannel.StartNode.ToString();
            else
                cContentPicker.Value = "-1";

            content.Controls.Add(cContentPicker);


            // Setup the panes
            Pane ppInfo = new Pane();
            ppInfo.addProperty(ui.Text("name", UmbracoUser), cName);
            ppInfo.addProperty(ui.Text("user", "startnode", UmbracoUser), content);
            ppInfo.addProperty(ui.Text("user", "searchAllChildren", UmbracoUser), cFulltree);
            ppInfo.addProperty(ui.Text("user", "mediastartnode", UmbracoUser), medias);

            Pane ppFields = new Pane();
            ppFields.addProperty(ui.Text("user", "documentType", UmbracoUser), cDocumentType);
            ppFields.addProperty(ui.Text("user", "descriptionField", UmbracoUser), cDescription);
            ppFields.addProperty(ui.Text("user", "categoryField", UmbracoUser), cCategories);
            ppFields.addProperty(ui.Text("user", "excerptField", UmbracoUser), cExcerpt);


            TabPage channelInfo = UserTabs.NewTabPage(ui.Text("user", "contentChannel", UmbracoUser));

            channelInfo.Controls.Add(ppInfo);
            channelInfo.Controls.Add(ppFields);


            if (!IsPostBack)
            {
                cName.Text = userChannel.Name;
                cDescription.SelectedValue = userChannel.FieldDescriptionAlias;
                cCategories.SelectedValue = userChannel.FieldCategoriesAlias;
                cExcerpt.SelectedValue = userChannel.FieldExcerptAlias;
                cDocumentType.SelectedValue = userChannel.DocumentTypeAlias;
                cFulltree.Checked = userChannel.FullTree;
            }
        }

        /// <summary>
        /// Setups the form.
        /// </summary>
        private void SetupForm()
        {

            if (!IsPostBack)
            {
                MembershipUser user = BackOfficeProvider.GetUser(u.LoginName, false);
                uname.Text = u.Name;
                lname.Text = (user == null) ? u.LoginName : user.UserName;
                email.Text = (user == null) ? u.Email : user.Email;

                contentPicker.Value = u.StartNodeId.ToString(CultureInfo.InvariantCulture);
                mediaPicker.Value = u.StartMediaId.ToString(CultureInfo.InvariantCulture);

                // get the current users applications
                string currentUserApps = ";";
                foreach (Application a in CurrentUser.Applications)
                    currentUserApps += a.alias + ";";

                Application[] uapps = u.Applications;
                foreach (Application app in BusinessLogic.Application.getAll())
                {
                    if (CurrentUser.IsAdmin() || currentUserApps.Contains(";" + app.alias + ";"))
                    {
                        ListItem li = new ListItem(ui.Text("sections", app.alias), app.alias);
                        if (!IsPostBack) foreach (Application tmp in uapps) if (app.alias == tmp.alias) li.Selected = true;
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

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/CMSNode.asmx"));
            //      ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));

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
                    if (string.IsNullOrEmpty(u.Password)) u.Password = "default";
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
                    var membershipUser = BackOfficeProvider.GetUser(u.LoginName, false);
                    if (membershipUser == null)
                    {
                        throw new ProviderException("Could not find user in the membership provider with login name " + u.LoginName);
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
                    u.UserType = UserType.GetUserType(int.Parse(userType.SelectedValue));
                    u.Email = email.Text.Trim();
                    u.LoginName = lname.Text;
                    u.Disabled = Disabled.Checked;
                    u.NoConsole = NoConsole.Checked;

                    int startNode;
                    if (int.TryParse(contentPicker.Value, out startNode) == false)
                    {
                        //set to default if nothing is choosen
                        if (u.StartNodeId > 0)
                            startNode = u.StartNodeId;
                        else
                            startNode = -1;
                    }
                    u.StartNodeId = startNode;


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

                    u.ClearApplications();
                    foreach (ListItem li in lapps.Items)
                    {
                        if (li.Selected) u.AddApplication(li.Value);
                    }

                    u.Save();

                    // save data
                    if (cName.Text != "")
                    {
                        Channel c;
                        try
                        {
                            c = new Channel(u.Id);
                        }
                        catch
                        {
                            c = new Channel();
                            c.User = u;
                        }

                        c.Name = cName.Text;
                        c.FullTree = cFulltree.Checked;
                        c.StartNode = int.Parse(cContentPicker.Value);
                        c.MediaFolder = int.Parse(cMediaPicker.Value);
                        c.FieldCategoriesAlias = cCategories.SelectedValue;
                        c.FieldDescriptionAlias = cDescription.SelectedValue;
                        c.FieldExcerptAlias = cExcerpt.SelectedValue;
                        c.DocumentTypeAlias = cDocumentType.SelectedValue;

                        //
                        c.MediaTypeAlias = Constants.Conventions.MediaTypes.Image; // [LK:2013-03-22] This was previously lowercase; unsure if using const will cause an issue.
                        c.MediaTypeFileProperty = Constants.Conventions.Media.File;
                        c.ImageSupport = true;

                        c.Save();

                    }

                    ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editUserSaved", UmbracoUser), "");
                }
                catch (Exception ex)
                {
                    ClientTools.ShowSpeechBubble(speechBubbleIcon.error, ui.Text("speechBubbles", "editUserError", UmbracoUser), "");
                    LogHelper.Error<EditUser>("Exception", ex);
                }
            }
            else
            {
                ClientTools.ShowSpeechBubble(speechBubbleIcon.error, ui.Text("speechBubbles", "editUserError", UmbracoUser), "");
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
