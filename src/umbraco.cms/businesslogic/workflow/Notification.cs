using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Models.Rdbms;
using umbraco.DataLayer;
using umbraco.interfaces;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.workflow
{
    /// <summary>
    /// Notifications are a part of the umbraco workflow.
    /// A notification is created every time an action on a node occurs and a umbraco user has subscribed to this specific action on this specific node.
    /// Notifications generates an email, which is send to the subscribing users.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Private constructor as this object should not be allowed to be created currently
        /// </summary>
        private Notification()
        {
        }

        public int NodeId { get; private set; }
        public int UserId { get; private set; }
        public char ActionId { get; private set; }

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Sends the notifications for the specified user regarding the specified node and action.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="user">The user.</param>
        /// <param name="action">The action.</param>
        public static void GetNotifications(CMSNode node, User user, IAction action)
        {
            User[] allUsers = User.getAll();
            foreach (User u in allUsers)
            {
                try
                {
                    if (u.Disabled == false && u.GetNotifications(node.Path).IndexOf(action.Letter.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) > -1)
                    {
                        LogHelper.Debug<Notification>(string.Format("Notification about {0} sent to {1} ({2})", ui.Text(action.Alias, u), u.Name, u.Email));
                        SendNotification(user, u, (Document)node, action);
                    }
                }
                catch (Exception notifyExp)
                {
					LogHelper.Error<Notification>("Error in notification", notifyExp);
                }
            }
        }

        ///TODO: Include update with html mail notification and document contents
        private static void SendNotification(User performingUser, User mailingUser, Document documentObject, IAction action)
        {
            // retrieve previous version of the document
            DocumentVersionList[] versions = documentObject.GetVersions();
            int versionCount = (versions.Length > 1) ? (versions.Length - 2) : (versions.Length - 1);
            var oldDoc = new Document(documentObject.Id, versions[versionCount].Version);

            // build summary
            var summary = new StringBuilder();
            var props = documentObject.GenericProperties;
            foreach (Property p in props)
            {
                // check if something was changed and display the changes otherwise display the fields
                Property oldProperty = oldDoc.getProperty(p.PropertyType.Alias);
                string oldText = oldProperty.Value != null ? oldProperty.Value.ToString() : "";
                string newText = p.Value != null ? p.Value.ToString() : "";

                // replace html with char equivalent
                ReplaceHtmlSymbols(ref oldText);
                ReplaceHtmlSymbols(ref newText);

                // make sure to only highlight changes done using TinyMCE editor... other changes will be displayed using default summary
                //TODO PPH: Had to change this, as a reference to the editorcontrols is not allowed, so a string comparison is the only way, this should be a DIFF or something instead.. 
                if (p.PropertyType.DataTypeDefinition.DataType.ToString() ==
                    "umbraco.editorControls.tinymce.TinyMCEDataType" &&
                    string.CompareOrdinal(oldText, newText) != 0)
                {
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'> Note: </th>");
                    summary.Append(
                        "<td style='text-align: left; vertical-align: top;'> <span style='background-color:red;'>Red for deleted characters</span>&nbsp;<span style='background-color:yellow;'>Yellow for inserted characters</span></td>");
                    summary.Append("</tr>");
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'> New " +
                                   p.PropertyType.Name + "</th>");
                    summary.Append("<td style='text-align: left; vertical-align: top;'>" +
                                   ReplaceLinks(CompareText(oldText, newText, true, false,
                                                            "<span style='background-color:yellow;'>", string.Empty)) +
                                   "</td>");
                    summary.Append("</tr>");
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'> Old " +
                                   oldProperty.PropertyType.Name + "</th>");
                    summary.Append("<td style='text-align: left; vertical-align: top;'>" +
                                   ReplaceLinks(CompareText(newText, oldText, true, false,
                                                            "<span style='background-color:red;'>", string.Empty)) +
                                   "</td>");
                    summary.Append("</tr>");
                }
                else
                {
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'>" +
                                   p.PropertyType.Name + "</th>");
                    summary.Append("<td style='text-align: left; vertical-align: top;'>" + newText + "</td>");
                    summary.Append("</tr>");
                }
                summary.Append(
                    "<tr><td colspan=\"2\" style=\"border-bottom: 1px solid #CCC; font-size: 2px;\">&nbsp;</td></tr>");
            }

            string protocol = GlobalSettings.UseSSL ? "https" : "http";


            string[] subjectVars = {
                                       HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + ":" +
                                       HttpContext.Current.Request.Url.Port +
                                       IOHelper.ResolveUrl(SystemDirectories.Umbraco), ui.Text(action.Alias)
                                       ,
                                       documentObject.Text
                                   };
            string[] bodyVars = {
                                    mailingUser.Name, ui.Text(action.Alias), documentObject.Text, performingUser.Name,
                                    HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + ":" +
                                    HttpContext.Current.Request.Url.Port +
                                    IOHelper.ResolveUrl(SystemDirectories.Umbraco),
                                    documentObject.Id.ToString(), summary.ToString(),
                                    String.Format("{2}://{0}/{1}",
                                                  HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + ":" +
                                                  HttpContext.Current.Request.Url.Port,
                                                  /*umbraco.library.NiceUrl(documentObject.Id))*/
                                                  documentObject.Id + ".aspx",
                                                  protocol)
                                    //TODO: PPH removed the niceURL reference... cms.dll cannot reference the presentation project...
                                    //TODO: This should be moved somewhere else..
                                };

            // create the mail message 
            var mail = new MailMessage(UmbracoConfig.For.UmbracoSettings().Content.NotificationEmailAddress, mailingUser.Email);

            // populate the message
            mail.Subject = ui.Text("notifications", "mailSubject", subjectVars, mailingUser);
            if (UmbracoConfig.For.UmbracoSettings().Content.DisableHtmlEmail)
            {
                mail.IsBodyHtml = false;
                mail.Body = ui.Text("notifications", "mailBody", bodyVars, mailingUser);
            }
            else
            {
                mail.IsBodyHtml = true;
                mail.Body =
                    @"<html><head>
</head>
<body style='font-family: Trebuchet MS, arial, sans-serif; font-color: black;'>
" +
                    ui.Text("notifications", "mailBodyHtml", bodyVars, mailingUser) + "</body></html>";
            }

            // nh, issue 30724. Due to hardcoded http strings in resource files, we need to check for https replacements here
            // adding the server name to make sure we don't replace external links
            if (GlobalSettings.UseSSL && string.IsNullOrEmpty(mail.Body) == false)
            {
                string serverName = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
                mail.Body = mail.Body.Replace(
                    string.Format("http://{0}", serverName),
                    string.Format("https://{0}", serverName));
            }

            // send it
            var sender = new SmtpClient();
            sender.Send(mail);
        }

        private static string ReplaceLinks(string text)
        {
            string domain = GlobalSettings.UseSSL ? "https://" : "http://";
            domain += HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + ":" +
                      HttpContext.Current.Request.Url.Port + "/";
            text = text.Replace("href=\"/", "href=\"" + domain);
            text = text.Replace("src=\"/", "src=\"" + domain);
            return text;
        }

        /// <summary>
        /// Returns the notifications for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static IEnumerable<Notification> GetUserNotifications(User user)
        {
            var items = new List<Notification>();
            var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<User2NodeNotifyDto>(
                "WHERE userId = @UserId ORDER BY nodeId", new { UserId = user.Id });

            foreach (var dto in dtos)
            {
                items.Add(new Notification
                          {
                              NodeId = dto.NodeId,
                              ActionId = Convert.ToChar(dto.Action),
                              UserId = dto.UserId
                          });
            }

            return items;
        }

        /// <summary>
        /// Returns the notifications for a node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<Notification> GetNodeNotifications(CMSNode node)
        {
            var items = new List<Notification>();
            var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<User2NodeNotifyDto>(
                "WHERE userId = @UserId ORDER BY nodeId", new { nodeId = node.Id });

            foreach (var dto in dtos)
            {
                items.Add(new Notification
                {
                    NodeId = dto.NodeId,
                    ActionId = Convert.ToChar(dto.Action),
                    UserId = dto.UserId
                });
            }
            return items;
        }

        /// <summary>
        /// Deletes notifications by node
        /// </summary>
        /// <param name="node"></param>
        public static void DeleteNotifications(CMSNode node)
        {
            // delete all settings on the node for this node id
            ApplicationContext.Current.DatabaseContext.Database.Delete<User2NodeNotifyDto>("WHERE nodeId = @nodeId",
                new {nodeId = node.Id});
        }

        /// <summary>
        /// Delete notifications by user
        /// </summary>
        /// <param name="user"></param>
        public static void DeleteNotifications(User user)
        {
            // delete all settings on the node for this node id
            ApplicationContext.Current.DatabaseContext.Database.Delete<User2NodeNotifyDto>("WHERE userId = @userId",
                new { userId = user.Id });
        }

        /// <summary>
        /// Delete notifications by user and node
        /// </summary>
        /// <param name="user"></param>
        /// <param name="node"></param>
        public static void DeleteNotifications(User user, CMSNode node)
        {
            // delete all settings on the node for this user
            ApplicationContext.Current.DatabaseContext.Database.Delete<User2NodeNotifyDto>(
                "WHERE userId = @userId AND nodeId = @nodeId", new {userId = user.Id, nodeId = node.Id});
        }

        /// <summary>
        /// Creates a new notification
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="node">The node.</param>
        /// <param name="actionLetter">The action letter.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeNew(User user, CMSNode node, char actionLetter)
        {
            bool exists = ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(
                "SELECT COUNT(userId) FROM umbracoUser2nodeNotify WHERE userId = @userId AND nodeId = @nodeId AND action = @action", 
                new { userId = user.Id, nodeId = node.Id, action = actionLetter.ToString()}) > 0;

            if (exists == false)
            {
                ApplicationContext.Current.DatabaseContext.Database.Insert(new User2NodeNotifyDto
                                                                           {
                                                                               Action = actionLetter.ToString(),
                                                                               NodeId = node.Id,
                                                                               UserId = user.Id
                                                                           });
            }
        }

        /// <summary>
        /// Updates the notifications.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="node">The node.</param>
        /// <param name="notifications">The notifications.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void UpdateNotifications(User user, CMSNode node, string notifications)
        {
            // delete all settings on the node for this user
            DeleteNotifications(user, node);

            // Loop through the permissions and create them
            foreach (char c in notifications)
                MakeNew(user, node, c);
        }

        /// <summary>
        /// Replaces the HTML symbols with the character equivalent.
        /// </summary>
        /// <param name="oldString">The old string.</param>
        private static void ReplaceHtmlSymbols(ref string oldString)
        {
            oldString = oldString.Replace("&nbsp;", " ");
            oldString = oldString.Replace("&rsquo;", "'");
            oldString = oldString.Replace("&amp;", "&");
            oldString = oldString.Replace("&ldquo;", "“");
            oldString = oldString.Replace("&rdquo;", "”");
            oldString = oldString.Replace("&quot;", "\"");
        }
    
        /// <summary>
        /// Compares the text.
        /// </summary>
        /// <param name="oldText">The old text.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="displayInsertedText">if set to <c>true</c> [display inserted text].</param>
        /// <param name="displayDeletedText">if set to <c>true</c> [display deleted text].</param>
        /// <param name="insertedStyle">The inserted style.</param>
        /// <param name="deletedStyle">The deleted style.</param>
        /// <returns></returns>
        private static string CompareText(string oldText, string newText, bool displayInsertedText,
                                          bool displayDeletedText, string insertedStyle, string deletedStyle)
        {
            var sb = new StringBuilder();
            Diff.Item[] diffs = Diff.DiffText1(oldText, newText);

            int pos = 0;
            for (int n = 0; n < diffs.Length; n++)
            {
                Diff.Item it = diffs[n];

                // write unchanged chars
                while ((pos < it.StartB) && (pos < newText.Length))
                {
                    sb.Append(newText[pos]);
                    pos++;
                } // while

                // write deleted chars
                if (displayDeletedText && it.deletedA > 0)
                {
                    sb.Append(deletedStyle);
                    for (int m = 0; m < it.deletedA; m++)
                    {
                        sb.Append(oldText[it.StartA + m]);
                    } // for
                    sb.Append("</span>");
                }

                // write inserted chars
                if (displayInsertedText && pos < it.StartB + it.insertedB)
                {
                    sb.Append(insertedStyle);
                    while (pos < it.StartB + it.insertedB)
                    {
                        sb.Append(newText[pos]);
                        pos++;
                    } // while
                    sb.Append("</span>");
                } // if                
            } // while

            // write rest of unchanged chars
            while (pos < newText.Length)
            {
                sb.Append(newText[pos]);
                pos++;
            } // while

            return sb.ToString();
        }
    }
}