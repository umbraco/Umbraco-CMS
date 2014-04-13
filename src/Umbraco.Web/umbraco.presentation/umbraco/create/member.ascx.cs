using Umbraco.Web.UI;
using System.Globalization;
using Umbraco.Core.Security;

namespace umbraco.cms.presentation.create.controls
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Web;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;

    using System.Web.Security;
	using umbraco.cms.helpers;
	using umbraco.BasePages;
    using umbraco.cms.businesslogic.member;

    /// <summary>
    ///		Summary description for member.
    /// </summary>
    public partial class member : System.Web.UI.UserControl
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

            sbmt.Text = ui.Text("create");
            if (provider.IsUmbracoMembershipProvider())
            {
                nameLiteral.Text = ui.Text("name");
                memberChooser.Attributes.Add("style", "padding-top: 10px");
                foreach (var dt in MemberType.GetAll)
                {
                    ListItem li = new ListItem();
                    li.Text = dt.Text;
                    li.Value = dt.Id.ToString();
                    nodeType.Items.Add(li);
                }
            }
            else
            {
                nameLiteral.Text = ui.Text("login");
                memberChooser.Visible = false;
            }

            string[] pwRules =
            {
                provider.MinRequiredPasswordLength.ToString(CultureInfo.InvariantCulture),
                provider.MinRequiredNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture)
            };

            PasswordRules.Text = PasswordRules.Text = ui.Text(
                "errorHandling", "", pwRules, UmbracoEnsuredPage.CurrentUser);

            if (!IsPostBack)
            {
                passwordRequired.ErrorMessage = ui.Text("errorHandling", "errorMandatoryWithoutTab", ui.Text("password"), BasePages.UmbracoEnsuredPage.CurrentUser);
                nameRequired.ErrorMessage = ui.Text("errorHandling", "errorMandatoryWithoutTab", nameLiteral.Text, BasePages.UmbracoEnsuredPage.CurrentUser);
                emailRequired.ErrorMessage = ui.Text("errorHandling", "errorMandatoryWithoutTab", "E-mail", BasePages.UmbracoEnsuredPage.CurrentUser);
                loginRequired.ErrorMessage = ui.Text("errorHandling", "errorMandatoryWithoutTab", "Login Name", BasePages.UmbracoEnsuredPage.CurrentUser);
                loginExistsCheck.ErrorMessage = ui.Text("errorHandling", "errorExistsWithoutTab", "Login Name", BasePages.UmbracoEnsuredPage.CurrentUser);
                emailExistsCheck.ErrorMessage = ui.Text("errorHandling", "errorExistsWithoutTab", "E-mail", BasePages.UmbracoEnsuredPage.CurrentUser);
                memberTypeRequired.ErrorMessage = ui.Text("errorHandling", "errorMandatoryWithoutTab", "Member Type", BasePages.UmbracoEnsuredPage.CurrentUser);
                Password.Text =
                    Membership.GeneratePassword(provider.MinRequiredPasswordLength, provider.MinRequiredNonAlphanumericCharacters);


            }


        }

        protected void sbmt_Click(object sender, System.EventArgs e)
        {
            if (Page.IsValid)
            {
                var memberType = memberChooser.Visible ? int.Parse(nodeType.SelectedValue) : -1;
                var emailAppend = String.IsNullOrEmpty(Email.Text) ? "" : String.Format("|{0}|{1}|{2}", Email.Text, Password.Text,Login.Text);
                var returnUrl = LegacyDialogHandler.Create(
                    new HttpContextWrapper(Context),
                    BasePage.Current.getUser(),
                    helper.Request("nodeType"),
                    -1,                    
                    rename.Text + emailAppend,
                    memberType);

				BasePage.Current.ClientTools
					.ChangeContentFrameUrl(returnUrl)
					.CloseModalWindow();

            }

        }


        /// <summary>
        /// Validation to Check if Login Name Exists
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void LoginExistsCheck(object sender, ServerValidateEventArgs e)
        {
            if (Login.Text != "" && Member.GetMemberFromLoginName(Login.Text.Replace(" ", "").ToLower()) != null)
                e.IsValid = false;
            else
                e.IsValid = true;
        }


        /// <summary>
        /// Validation to Check if Member with email Exists
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void EmailExistsCheck(object sender, ServerValidateEventArgs e)
        {
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

            if (Email.Text != "" && Member.GetMemberFromEmail(Email.Text.ToLower()) != null && provider.RequiresUniqueEmail)
                e.IsValid = false;
            else
                e.IsValid = true;
        }

        protected void EmailValidator_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = MembershipProviderBase.IsEmailValid(args.Value);
        }
    }
}
