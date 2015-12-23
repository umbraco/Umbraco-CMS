using System;
using System.Net.Mail;
using System.Web;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic;
using umbraco.BusinessLogic;
using umbraco.uicontrols;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
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

            pp_translator.Text = ui.Text("translation","translator", Security.CurrentUser);
            pp_language.Text = ui.Text("translation", "translateTo", Security.CurrentUser);
            pp_includeSubs.Text = ui.Text("translation","includeSubpages", Security.CurrentUser);
            pp_comment.Text = ui.Text("comment", Security.CurrentUser);
            pane_form.Text = ui.Text("translation", "sendToTranslate", _currentPage.Text, Security.CurrentUser);
            

            if (!IsPostBack)
            {
                // default language
                var selectedLanguage = 0;

                var domains = library.GetCurrentDomains(_currentPage.Id);
                if (domains != null)
                {
                    selectedLanguage = domains[0].Language.id;
                    defaultLanguage.Text = ui.Text("defaultLanguageIs", Security.CurrentUser) + " " + domains[0].Language.FriendlyName;
                }
                else
                {
                    defaultLanguage.Text = ui.Text("defaultLanguageIsNotAssigned", Security.CurrentUser);
                }
                
                // languages
                language.Items.Add(new ListItem(ui.Text("general", "choose", Security.CurrentUser), ""));
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
                foreach (var u in BusinessLogic.User.getAll())
                    if (u.UserType.Alias.ToLower() == "translator" || UserHasTranslatePermission(u, _currentPage))
                        translator.Items.Add(new ListItem(u.Name, u.Id.ToString()));

                if (translator.Items.Count == 0) {
                    feedback.Text = ui.Text("translation", "noTranslators");
                    feedback.type = Feedback.feedbacktype.error;
                    doTranslation.Enabled = false;
                }

                // send button
                doTranslation.Text = ui.Text("general", "ok", Security.CurrentUser);
            }
        }

        private bool UserHasTranslatePermission(User u, CMSNode node)
        {
            //the permissions column in umbracoUserType is legacy and needs to be rewritten but for now this is the only way to test 
            return u.GetPermissions(node.Path).Contains("4");
        }

        protected void doTranslation_Click(object sender, EventArgs e)
        {
            // testing translate
            MakeNew(
                _currentPage,
                UmbracoContext.UmbracoUser,
                BusinessLogic.User.GetUser(Int32.Parse(translator.SelectedValue)),
                new Language(Int32.Parse(language.SelectedValue)),
                comment.Text, includeSubpages.Checked,
                true);

            pane_form.Visible = false;
            pl_buttons.Visible = false;

            feedback.Text = ui.Text("translation","pageHasBeenSendToTranslation", _currentPage.Text, Security.CurrentUser) + "</p><p><a href=\"#\" onclick=\"" + ClientTools.Scripts.CloseModalWindow() + "\">" + ui.Text("defaultdialogs", "closeThisWindow") + "</a></p>";
            feedback.type = Feedback.feedbacktype.success;
        }

        public void MakeNew(CMSNode Node, User User, User Translator, Language Language, string Comment,
            bool IncludeSubpages, bool SendEmail)
        {
            // Get translation taskType for obsolete task constructor
            var taskType = Services.TaskService.GetTaskTypeByAlias("toTranslate");

            // Create pending task
            var t = new cms.businesslogic.task.Task(new Task(taskType));
            t.Comment = Comment;
            t.Node = Node;
            t.ParentUser = User;
            t.User = Translator;
            t.Save();

            Services.AuditService.Add(AuditType.SendToTranslate,
                "Translator: " + Translator.Name + ", Language: " + Language.FriendlyName,
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
                string[] subjectVars = { serverName, Node.Text };
                string[] bodyVars = {
                    Translator.Name, Node.Text, User.Name,
                    serverName, t.Id.ToString(),
                    Language.FriendlyName
                };

                if (User.Email != "" && User.Email.Contains("@") && Translator.Email != "" &&
                    Translator.Email.Contains("@"))
                {
                    // create the mail message 
                    MailMessage mail = new MailMessage(User.Email, Translator.Email);

                    // populate the message
                    mail.Subject = ui.Text("translation", "mailSubject", subjectVars, Translator);
                    mail.IsBodyHtml = false;
                    mail.Body = ui.Text("translation", "mailBody", bodyVars, Translator);
                    try
                    {
                        SmtpClient sender = new SmtpClient();
                        sender.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<sendToTranslation>("Error sending translation e-mail", ex);
                    }
                }
                else
                {
                    LogHelper.Warn<sendToTranslation>("Could not send translation e-mail because either user or translator lacks e-mail in settings");
                }

            }

            if (IncludeSubpages)
            {
                //store children array here because iterating over an Array property object is very inneficient.
                var c = Node.Children;
                foreach (CMSNode n in c)
                {
                    MakeNew(n, User, Translator, Language, Comment, true, false);
                }
            }
        }
    }
}
