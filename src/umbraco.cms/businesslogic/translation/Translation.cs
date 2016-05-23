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
using Umbraco.Core;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.translation
{
    [Obsolete("This will be removed in future versions, the translation utility will not work perfectly in v7.x")]
    public class Translation
    {
        public static void MakeNew(CMSNode node, User user, User translator, Language language, string comment, bool includeSubpages, bool sendEmail)
        {
            // Get translation taskType for obsolete task constructor
            var taskType = ApplicationContext.Current.Services.TaskService.GetTaskTypeByAlias("toTranslate");

            // Create pending task
            var task = new Task(new Umbraco.Core.Models.Task(taskType))
            {
                Comment = comment,
                Node = node,
                ParentUser = user,
                User = translator
            };
            task.Save();

            // Add log entry
            // Note: not updated to LogHelper as LogTypes.SendToTranslate might be something special, not sure
            Log.Add(LogTypes.SendToTranslate, user, node.Id, "Translator: " + translator.Name + ", Language: " + language.FriendlyName);

            // send it
            if (sendEmail)
            {
                var serverName = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
                var port = HttpContext.Current.Request.Url.Port;

                if (port != 80)
                    serverName += ":" + port;

                serverName += IOHelper.ResolveUrl(SystemDirectories.Umbraco);

                // Send mail
                var subjectVars = new[] { serverName, node.Text };
                var bodyVars = new[] {
                                        translator.Name, node.Text, user.Name,
                                        serverName, task.Id.ToString(),
                                        language.FriendlyName
                                    };

                if (string.IsNullOrWhiteSpace(user.Email) == false && user.Email.Contains("@") 
                    && string.IsNullOrWhiteSpace(translator.Email) == false && translator.Email.Contains("@"))
                {
                    try
                    {
                        using (var mail = new MailMessage())
                        {
                            mail.From = new MailAddress(user.Email.Trim());
                            mail.To.Add(new MailAddress(translator.Email.Trim()));
                            mail.Subject = ui.Text("translation", "mailSubject", subjectVars, translator); ;
                            mail.IsBodyHtml = false;
                            mail.Body = ui.Text("translation", "mailBody", bodyVars, translator); ;
                            using (var smtpClient = new SmtpClient())
                                smtpClient.Send(mail);
                        }
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

            if (includeSubpages == false)
                return;

            //store children array here because iterating over an Array property object is very inneficient.
            var children = node.Children;
            foreach (var child in children)
            {
                var cmsNode = (CMSNode) child;
                MakeNew(cmsNode, user, translator, language, comment, true, false);
            }
        }

        public static int CountWords(int DocumentId)
        {
            Document d = new Document(DocumentId);

            int words = CountWordsInString(d.Text);
            var props = d.GenericProperties;
            foreach (Property p in props)
            {
                var asString = p.Value as string;
                if (asString != null)
                {
                    var trimmed = asString.Trim();
                    if (trimmed.IsNullOrWhiteSpace() == false)
                    {
                        words += CountWordsInString(trimmed);
                    }
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