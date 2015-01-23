using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.translation
{
    [Obsolete("This will be removed in future versions, the translation utility will not work perfectly in v7.x")]
    public class Translation
    {
        public static void MakeNew(CMSNode Node, User User, User Translator, Language Language, string Comment,
                                   bool IncludeSubpages, bool SendEmail)
        {
            // Create pending task
            Task t = new Task();
            t.Comment = Comment;
            t.Node = Node;
            t.ParentUser = User;
            t.User = Translator;
            t.Type = new TaskType("toTranslate");
            t.Save();

            // Add log entry
            Log.Add(LogTypes.SendToTranslate, User, Node.Id,
                    "Translator: " + Translator.Name + ", Language: " + Language.FriendlyName);

            // send it
            if (SendEmail)
            {
                string serverName = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
                int port = HttpContext.Current.Request.Url.Port;

                if(port != 80)
                    serverName += ":" + port.ToString();

                serverName += IOHelper.ResolveUrl(SystemDirectories.Umbraco);

                // Send mail
                string[] subjectVars = {serverName, Node.Text};
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
						LogHelper.Error<Translation>("Error sending translation e-mail", ex);
                    }
                }
                else
                {
					LogHelper.Warn<Translation>("Could not send translation e-mail because either user or translator lacks e-mail in settings");					
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

        public static int CountWords(int DocumentId)
        {
            Document d = new Document(DocumentId);

            int words = CountWordsInString(d.Text);
            var props = d.GenericProperties;
            foreach (Property p in props)
            {
                if (p.Value.GetType() == "".GetType())
                {
                    if (p.Value.ToString().Trim() != "")
                        words += CountWordsInString(p.Value.ToString());
                }
            }

            return words;
        }

        private static int CountWordsInString(string Text)
        {
            string pattern = @"<(.|\n)*?>";
            string tmpStr = Regex.Replace(Text, pattern, string.Empty);

            tmpStr = tmpStr.Replace("\t", " ").Trim();
            tmpStr = tmpStr.Replace("\n", " ");
            tmpStr = tmpStr.Replace("\r", " ");

            MatchCollection collection = Regex.Matches(tmpStr, @"[\S]+");
            return collection.Count; 
        }
    }
}