using System;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic;
using umbraco.uicontrols;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web;
using Umbraco.Web.UI.Pages;
using Language = umbraco.cms.businesslogic.language.Language;

namespace umbraco.presentation.dialogs
{
    public partial class sendToTranslation : UmbracoEnsuredPage
    {
        private CMSNode _currentPage;

        public sendToTranslation()
        {
            CurrentApp = Constants.Applications.Content.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _currentPage = new CMSNode(Int32.Parse(Request.GetItemAsString("id")));

            pp_translator.Text = Services.TextService.Localize("translation/translator");
            pp_language.Text = Services.TextService.Localize("translation/translateTo");
            pp_includeSubs.Text = Services.TextService.Localize("translation/includeSubpages");
            pp_comment.Text = Services.TextService.Localize("comment");
            pane_form.Text = Services.TextService.Localize("translation/sendToTranslate", new[] { _currentPage.Text});
            

            if (!IsPostBack)
            {
                // default language
                var selectedLanguage = 0;

                var domains = library.GetCurrentDomains(_currentPage.Id);
                if (domains != null)
                {
                    selectedLanguage = domains[0].Language.id;
                    defaultLanguage.Text = Services.TextService.Localize("defaultLanguageIs") + " " + domains[0].Language.FriendlyName;
                }
                else
                {
                    defaultLanguage.Text = Services.TextService.Localize("defaultLanguageIsNotAssigned");
                }
                
                // languages
                language.Items.Add(new ListItem(Services.TextService.Localize("general/choose"), ""));
                foreach (var l in Language.getAll)
                {
                    var li = new ListItem();
                    li.Text = l.FriendlyName;
                    li.Value = l.id.ToString();
                    if (selectedLanguage == l.id)
                        li.Selected = true;
                    language.Items.Add(li);
                }

                // Subpages
                if (_currentPage.Children.Length == 0)
                    includeSubpages.Enabled = false;

                // Translators
                int totalUsers;
                foreach (var u in Services.UserService.GetAll(0, int.MaxValue, out totalUsers))
                    if (u.UserType.Alias.ToLower() == "translator" || UserHasTranslatePermission(u, _currentPage))
                        translator.Items.Add(new ListItem(u.Name, u.Id.ToString()));

                if (translator.Items.Count == 0) {
                    feedback.Text = Services.TextService.Localize("translation/noTranslators");
                    feedback.type = Feedback.feedbacktype.error;
                    doTranslation.Enabled = false;
                }

                // send button
                doTranslation.Text = Services.TextService.Localize("general/ok");
            }
        }

        private bool UserHasTranslatePermission(IUser u, CMSNode node)
        {
            //the permissions column in umbracoUserType is legacy and needs to be rewritten but for now this is the only way to test 
            var permissions = Services.UserService.GetPermissions(u, node.Path);
            return permissions.AssignedPermissions.Contains("4");
        }

        protected void doTranslation_Click(object sender, EventArgs e)
        {
            int languageId;
            if (int.TryParse(language.SelectedValue, out languageId))
            {
                cms.businesslogic.translation.Translation.MakeNew(
                    _currentPage,
                    getUser(),
                    BusinessLogic.User.GetUser(int.Parse(translator.SelectedValue)),
                    new cms.businesslogic.language.Language(languageId),
                    comment.Text, includeSubpages.Checked,
                    true);

                pane_form.Visible = false;
                pl_buttons.Visible = false;

                feedback.Text = ui.Text("translation", "pageHasBeenSendToTranslation", _currentPage.Text, base.getUser()) +
                    "</p><p><a href=\"#\" onclick=\"" + ClientTools.Scripts.CloseModalWindow() + "\">" +
                    ui.Text("defaultdialogs", "closeThisWindow") + "</a></p>";
                feedback.type = uicontrols.Feedback.feedbacktype.success;
            }
            else
            {
                feedback.Text = ui.Text("translation", "noLanguageSelected");
                feedback.type = uicontrols.Feedback.feedbacktype.error;
            }
        }
    }
}
