using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic;
using umbraco.BusinessLogic;
using umbraco.BasePages;

namespace umbraco.presentation.dialogs
{
    public partial class sendToTranslation : UmbracoEnsuredPage
    {
        private CMSNode _currentPage;

        public sendToTranslation()
        {
            CurrentApp = DefaultApps.content.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _currentPage = new CMSNode(int.Parse(helper.Request("id")));

            pp_translator.Text = ui.Text("translation","translator", this.getUser());
            pp_language.Text = ui.Text("translation", "translateTo", this.getUser());
            pp_includeSubs.Text = ui.Text("translation","includeSubpages", this.getUser());
            pp_comment.Text = ui.Text("comment", this.getUser());
            pane_form.Text = ui.Text("translation", "sendToTranslate", _currentPage.Text, base.getUser());
            

            if (!IsPostBack)
            {
                // default language
                var selectedLanguage = 0;

                var domains = library.GetCurrentDomains(_currentPage.Id);
                if (domains != null)
                {
                    selectedLanguage = domains[0].Language.id;
                    defaultLanguage.Text = ui.Text("defaultLanguageIs", base.getUser()) + " " + domains[0].Language.FriendlyName;
                }
                else
                {
                    defaultLanguage.Text = ui.Text("defaultLanguageIsNotAssigned", base.getUser());
                }
                
                // languages
                language.Items.Add(new ListItem(ui.Text("general", "choose", base.getUser()), ""));
                foreach (var l in cms.businesslogic.language.Language.getAll)
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
                foreach (var u in BusinessLogic.User.getAll())
                    if (u.UserType.Alias.ToLower() == "translator" || UserHasTranslatePermission(u, _currentPage))
                        translator.Items.Add(new ListItem(u.Name, u.Id.ToString()));

                if (translator.Items.Count == 0) {
                    feedback.Text = ui.Text("translation", "noTranslators");
                    feedback.type = uicontrols.Feedback.feedbacktype.error;
                    doTranslation.Enabled = false;
                }

                // send button
                doTranslation.Text = ui.Text("general", "ok", base.getUser());
            }
        }

        private bool UserHasTranslatePermission(BusinessLogic.User u, CMSNode node)
        {
            //the permissions column in umbracoUserType is legacy and needs to be rewritten but for now this is the only way to test 
            return u.GetPermissions(node.Path).Contains("4");
        }

        protected void doTranslation_Click(object sender, EventArgs e)
        {
            // testing translate
            cms.businesslogic.translation.Translation.MakeNew(
                _currentPage,
                getUser(),
                BusinessLogic.User.GetUser(int.Parse(translator.SelectedValue)),
                new cms.businesslogic.language.Language(int.Parse(language.SelectedValue)),
                comment.Text, includeSubpages.Checked,
                true);

            pane_form.Visible = false;
            pl_buttons.Visible = false;

            feedback.Text = ui.Text("translation","pageHasBeenSendToTranslation", _currentPage.Text, base.getUser()) + "</p><p><a href=\"#\" onclick=\"" + ClientTools.Scripts.CloseModalWindow() + "\">" + ui.Text("defaultdialogs", "closeThisWindow") + "</a></p>";
            feedback.type = uicontrols.Feedback.feedbacktype.success;
        }
    }
}
