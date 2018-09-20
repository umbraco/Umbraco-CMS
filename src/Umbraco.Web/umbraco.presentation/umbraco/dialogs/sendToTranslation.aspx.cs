using System;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.UI.Pages;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Entities;

namespace umbraco.presentation.dialogs
{
    public partial class sendToTranslation : UmbracoEnsuredPage
    {
        private IUmbracoEntity _currentPage;

        public sendToTranslation()
        {
            CurrentApp = Constants.Applications.Content.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _currentPage = Services.EntityService.Get(Int32.Parse(Request.GetItemAsString("id")));

            pp_translator.Text = Services.TextService.Localize("translation/translator");
            pp_language.Text = Services.TextService.Localize("translation/translateTo");
            pp_includeSubs.Text = Services.TextService.Localize("translation/includeSubpages");
            pp_comment.Text = Services.TextService.Localize("comment");
            pane_form.Text = Services.TextService.Localize("translation/sendToTranslate", new[] { _currentPage.Name});


            if (!IsPostBack)
            {
                // default language
                var selectedLanguage = 0;

                var domains = Current.Services.DomainService.GetAssignedDomains(_currentPage.Id, false).ToArray();
                //var domains = library.GetCurrentDomains(_currentPage.Id);
                if (domains != null)
                {
                    var lang = Current.Services.LocalizationService.GetLanguageById(domains[0].LanguageId.Value);
                    selectedLanguage = lang.Id;
                    defaultLanguage.Text = Services.TextService.Localize("defaultLanguageIs") + " " + lang.CultureName;
                }
                else
                {
                    defaultLanguage.Text = Services.TextService.Localize("defaultLanguageIsNotAssigned");
                }

                // languages
                language.Items.Add(new ListItem(Services.TextService.Localize("general/choose"), ""));
                foreach (var l in Current.Services.LocalizationService.GetAllLanguages())
                {
                    var li = new ListItem();
                    li.Text = l.CultureName;
                    li.Value = l.Id.ToString();
                    if (selectedLanguage == l.Id)
                        li.Selected = true;
                    language.Items.Add(li);
                }

                // Subpages
                var c = Services.EntityService.GetChildren(_currentPage.Id);
                if (c.Any())
                    includeSubpages.Enabled = false;

                // Translators
                var translatorsGroup = Services.UserService.GetUserGroupByAlias("translators");
                var users = Services.UserService.GetAllInGroup(translatorsGroup.Id);
                foreach (var u in users)
                    if (UserHasTranslatePermission(u, _currentPage))
                        translator.Items.Add(new ListItem(u.Name, u.Id.ToString()));

                if (translator.Items.Count == 0) {
                    feedback.Text = Services.TextService.Localize("translation/noTranslators");
                    feedback.type = Umbraco.Web._Legacy.Controls.Feedback.feedbacktype.error;
                    doTranslation.Enabled = false;
                }

                // send button
                doTranslation.Text = Services.TextService.Localize("general/ok");
            }
        }

        private bool UserHasTranslatePermission(IUser u, IUmbracoEntity node)
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
                // testing translate
                MakeNew(
                    _currentPage,
                    Security.CurrentUser,
                    Services.UserService.GetUserById(int.Parse(translator.SelectedValue)),
                    Current.Services.LocalizationService.GetLanguageById(int.Parse(language.SelectedValue)),
                    comment.Text, includeSubpages.Checked,
                    true);

                pane_form.Visible = false;
                pl_buttons.Visible = false;

                feedback.Text = Services.TextService.Localize("translation/pageHasBeenSendToTranslation", _currentPage.Name) +
                    "</p><p><a href=\"#\" onclick=\"" + ClientTools.Scripts.CloseModalWindow() + "\">" +
                    Services.TextService.Localize("defaultdialogs/closeThisWindow") + "</a></p>";
                feedback.type = Umbraco.Web._Legacy.Controls.Feedback.feedbacktype.success;
            }
            else
            {
                feedback.Text = Services.TextService.Localize("translation/noLanguageSelected");
                feedback.type = Umbraco.Web._Legacy.Controls.Feedback.feedbacktype.error;
            }
        }

        public void MakeNew(IUmbracoEntity Node, IUser User, IUser Translator, ILanguage Language, string Comment,
            bool IncludeSubpages, bool SendEmail)
        {
            // Get translation taskType for obsolete task constructor
            var taskType = Services.TaskService.GetTaskTypeByAlias("toTranslate");

            // Create pending task
            var t = new Umbraco.Web._Legacy.BusinessLogic.Task(new Task(taskType));
            t.Comment = Comment;
            t.TaskEntity.EntityId = Node.Id;
            t.ParentUser = User;
            t.User = Translator;
            t.Save();

            Services.AuditService.Add(AuditType.SendToTranslate,
                "Translator: " + Translator.Name + ", Language: " + Language.CultureName,
                User.Id, Node.Id);

            // send it
            if (SendEmail)
            {
                string serverName = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
                int port = HttpContext.Current.Request.Url.Port;

                if (port != 80)
                    serverName += ":" + port;

                serverName += IOHelper.ResolveUrl(SystemDirectories.Umbraco);

                // Send mail
                string[] subjectVars = { serverName, Node.Name };
                string[] bodyVars = {
                    Translator.Name, Node.Name, User.Name,
                    serverName, t.Id.ToString(),
                    Language.CultureName
                };

                if (User.Email != "" && User.Email.Contains("@") && Translator.Email != "" &&
                    Translator.Email.Contains("@"))
                {
                    // create the mail message
                    using (MailMessage mail = new MailMessage(User.Email, Translator.Email))
                    {
                        // populate the message
                        mail.Subject = Services.TextService.Localize("translation/mailSubject", Translator.GetUserCulture(Services.TextService, UmbracoConfig.For.GlobalSettings()), subjectVars);
                        mail.IsBodyHtml = false;
                        mail.Body = Services.TextService.Localize("translation/mailBody", Translator.GetUserCulture(Services.TextService, UmbracoConfig.For.GlobalSettings()), bodyVars);
                        try
                        {
                            using (SmtpClient sender = new SmtpClient())
                            {
                                sender.Send(mail);
                            }
                        }
                        catch (Exception ex)
                        {
                            Current.Logger.Error<sendToTranslation>(ex, "Error sending translation e-mail");
                        }
                    }

                }
                else
                {
                    Current.Logger.Warn<sendToTranslation>("Could not send translation e-mail because either user or translator lacks e-mail in settings");
                }

            }

            if (IncludeSubpages)
            {
                //store children array here because iterating over an Array property object is very inneficient.
                var c = Services.EntityService.GetChildren(Node.Id);
                //var c = Node.Children;
                foreach (var n in c)
                {
                    MakeNew(n, User, Translator, Language, Comment, true, false);
                }
            }
        }
    }
}
