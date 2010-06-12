using System;
using System.Data;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Collections.Generic;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.IO;
using System.Collections;

namespace umbraco.cms.businesslogic.workflow
{
    /// <summary>
    /// Notifications are a part of the umbraco workflow.
    /// A notification is created every time an action on a node occurs and a umbraco user has subscribed to this specific action on this specific node.
    /// Notifications generates an email, which is send to the subscribing users.
    /// </summary>
    public class Notification
    {

        public int NodeId { get; private set; }
        public int UserId { get; private set; }
        public char ActionId { get; private set; }

        /// <summary>
        /// Private constructor as this object should not be allowed to be created currently
        /// </summary>
        private Notification() { }

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Sends the notifications for the specified user regarding the specified node and action.
        /// </summary>
        /// <param name="Node">The node.</param>
        /// <param name="user">The user.</param>
        /// <param name="Action">The action.</param>
        public static void GetNotifications(CMSNode Node, User user, IAction Action)
        {
            User[] allUsers = User.getAll();
            foreach (User u in allUsers)
            {
                try
                {
                    if (!u.Disabled && u.GetNotifications(Node.Path).IndexOf(Action.Letter.ToString()) > -1)
                    {
                        Log.Add(LogTypes.Notify, User.GetUser(0), Node.Id,
                                "Notification about " + ui.Text(Action.Alias, u) + " sent to " + u.Name + " (" + u.Email +
                                ")");
                        sendNotification(user, u, (Document) Node, Action);
                    }
                }
                catch (Exception notifyExp)
                {
                    Log.Add(LogTypes.Error, u, Node.Id, "Error in notification: " + notifyExp);
                }
            }
        }
        
        ///TODO: Include update with html mail notification and document contents
        private static void sendNotification(User performingUser, User mailingUser, Document documentObject,
                                             IAction Action)
        {
            // retrieve previous version of the document
            DocumentVersionList[] versions = documentObject.GetVersions();
            int versionCount = (versions.Length > 1) ? (versions.Length - 2) : (versions.Length - 1);
            Document oldDoc = new Document(documentObject.Id, versions[versionCount].Version);

            // build summary
            StringBuilder summary = new StringBuilder();
            var props = documentObject.getProperties;
            foreach (Property p in props)
            {
                // check if something was changed and display the changes otherwise display the fields
                Property oldProperty = oldDoc.getProperty(p.PropertyType.Alias);
                string oldText = oldProperty.Value.ToString();
                string newText = p.Value.ToString();

                // replace html with char equivalent
                ReplaceHTMLSymbols(ref oldText);
                ReplaceHTMLSymbols(ref newText);

                // make sure to only highlight changes done using TinyMCE editor... other changes will be displayed using default summary
                ///TODO PPH: Had to change this, as a reference to the editorcontrols is not allowed, so a string comparison is the only way, this should be a DIFF or something instead.. 
                if (p.PropertyType.DataTypeDefinition.DataType.ToString() == "umbraco.editorControls.tinymce.TinyMCEDataType" &&
                    string.Compare( oldText, newText ) != 0)
                {
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'> Note: </th>");
                    summary.Append("<td style='text-align: left; vertical-align: top;'> <span style='background-color:red;'>Red for deleted characters</span>&nbsp;<span style='background-color:yellow;'>Yellow for inserted characters</span></td>");
                    summary.Append("</tr>");
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'> New " + p.PropertyType.Name + "</th>");
                    summary.Append("<td style='text-align: left; vertical-align: top;'>" + replaceLinks(CompareText(oldText, newText, true, false, "<span style='background-color:yellow;'>", string.Empty)) + "</td>");
                    summary.Append("</tr>");
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'> Old " + oldProperty.PropertyType.Name + "</th>");
                    summary.Append("<td style='text-align: left; vertical-align: top;'>" + replaceLinks(CompareText(newText, oldText, true, false, "<span style='background-color:red;'>", string.Empty)) + "</td>");
                    summary.Append("</tr>");
                }
                else
                {
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'>" + p.PropertyType.Name + "</th>");
                    summary.Append("<style='text-align: left; vertical-align: top;'>" + p.Value.ToString() + "</td>");
                    summary.Append("</tr>");
                }
                summary.Append("<tr><td colspan=\"2\" style=\"border-bottom: 1px solid #CCC; font-size: 2px;\">&nbsp;</td></tr>");
            }


            string[] subjectVars = {
                                       HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + ":" + HttpContext.Current.Request.Url.Port.ToString() + SystemDirectories.Umbraco, ui.Text(Action.Alias)
                                       ,
                                       documentObject.Text
                                   };
            string[] bodyVars = {
                                    mailingUser.Name, ui.Text(Action.Alias), documentObject.Text, performingUser.Name,
                                    HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + ":" + HttpContext.Current.Request.Url.Port.ToString() + SystemDirectories.Umbraco,
                                    documentObject.Id.ToString(), summary.ToString(), 
                                    String.Format("http://{0}/{1}", 
                                        HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + ":" + HttpContext.Current.Request.Url.Port.ToString(), 
                                        /*umbraco.library.NiceUrl(documentObject.Id))*/ documentObject.Id.ToString() + ".aspx")
                ///TODO: PPH removed the niceURL reference... cms.dll cannot reference the presentation project...
                ///TODO: This should be moved somewhere else..
                                };

            // create the mail message 
            MailMessage mail = new MailMessage(UmbracoSettings.NotificationEmailSender, mailingUser.Email);

            // populate the message
            mail.Subject = ui.Text("notifications", "mailSubject", subjectVars, mailingUser);
            if (UmbracoSettings.NotificationDisableHtmlEmail)
            {
                mail.IsBodyHtml = false;
                mail.Body = ui.Text("notifications", "mailBody", bodyVars, mailingUser);
            } else
            {
                mail.IsBodyHtml = true;
                mail.Body =
                    @"<html><head>
</head>
<body style='font-family: Trebuchet MS, arial, sans-serif; font-color: black;'>
" +
                    ui.Text("notifications", "mailBodyHtml", bodyVars, mailingUser) + "</body></html>";
            }

            // send it
            SmtpClient sender = new SmtpClient();
            sender.Send(mail);
        }

		private static string replaceLinks(string text) {
            string domain = "http://" + HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + ":" + HttpContext.Current.Request.Url.Port.ToString() + "/";
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
            using (IRecordsReader dr = SqlHelper.ExecuteReader("select * from umbracoUser2NodeNotify where userId = @userId order by nodeId", SqlHelper.CreateParameter("@userId", user.Id)))
            {
                while (dr.Read())
                {
                    items.Add(new Notification()
                    {
                        NodeId = dr.GetInt("nodeId"),
                        ActionId = Convert.ToChar(dr.GetString("action")),
                        UserId = dr.GetInt("userId")
                    });
                }
            }
            return items;
        }

        /// <summary>
        /// Returns the notifications for a node
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static IEnumerable<Notification> GetNodeNotifications(CMSNode node)
        {
            var items = new List<Notification>();
            using (IRecordsReader dr = SqlHelper.ExecuteReader("select * from umbracoUser2NodeNotify where nodeId = @nodeId order by nodeId", SqlHelper.CreateParameter("@nodeId", node.Id)))
            {
                while (dr.Read())
                {
                    items.Add(new Notification()
                    {
                        NodeId = dr.GetInt("nodeId"),
                        ActionId = Convert.ToChar(dr.GetString("action")),
                        UserId = dr.GetInt("userId")
                    });
                }
            }
            return items;
        }

        /// <summary>
        /// Deletes notifications by node
        /// </summary>
        /// <param name="nodeId"></param>
        public static void DeleteNotifications(CMSNode node)
        {
            // delete all settings on the node for this node id
            SqlHelper.ExecuteNonQuery("delete from umbracoUser2NodeNotify where nodeId = @nodeId",
                                      SqlHelper.CreateParameter("@nodeId", node.Id));
        }

        /// <summary>
        /// Delete notifications by user
        /// </summary>
        /// <param name="user"></param>
        public static void DeleteNotifications(User user)
        {
            // delete all settings on the node for this node id
            SqlHelper.ExecuteNonQuery("delete from umbracoUser2NodeNotify where userId = @userId",
                                      SqlHelper.CreateParameter("@userId", user.Id));
        }

        /// <summary>
        /// Delete notifications by user and node
        /// </summary>
        /// <param name="user"></param>
        /// <param name="node"></param>
        public static void DeleteNotifications(User user, CMSNode node)
        {
            // delete all settings on the node for this user
            SqlHelper.ExecuteNonQuery("delete from umbracoUser2NodeNotify where userId = @userId and nodeId = @nodeId",
                                      SqlHelper.CreateParameter("@userId", user.Id), SqlHelper.CreateParameter("@nodeId", node.Id));
        }

        /// <summary>
        /// Creates a new notification
        /// </summary>
        /// <param name="User">The user.</param>
        /// <param name="Node">The node.</param>
        /// <param name="ActionLetter">The action letter.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeNew(User User, CMSNode Node, char ActionLetter)
        {
            IParameter[] parameters = new IParameter[] {
                                        SqlHelper.CreateParameter("@userId", User.Id),
                                        SqlHelper.CreateParameter("@nodeId", Node.Id),
                                        SqlHelper.CreateParameter("@action", ActionLetter.ToString()) };

            // Method is synchronized so exists remains consistent (avoiding race condition)
            bool exists = SqlHelper.ExecuteScalar<int>("SELECT COUNT(userId) FROM umbracoUser2nodeNotify WHERE userId = @userId AND nodeId = @nodeId AND action = @action",
                                                       parameters) > 0;
            if (!exists)
                SqlHelper.ExecuteNonQuery("INSERT INTO umbracoUser2nodeNotify (userId, nodeId, action) VALUES (@userId, @nodeId, @action)",
                                          parameters);
        }

        /// <summary>
        /// Updates the notifications.
        /// </summary>
        /// <param name="User">The user.</param>
        /// <param name="Node">The node.</param>
        /// <param name="Notifications">The notifications.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void UpdateNotifications(User User, CMSNode Node, string Notifications)
        {
            // delete all settings on the node for this user
            DeleteNotifications(User, Node);

            // Loop through the permissions and create them
            foreach (char c in Notifications.ToCharArray())
                MakeNew(User, Node, c);
        }

        /// <summary>
        /// Replaces the HTML symbols with the character equivalent.
        /// </summary>
        /// <param name="oldString">The old string.</param>
        private static void ReplaceHTMLSymbols(ref string oldString)
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
        /// <returns></returns>
        private static string CompareText(string oldText, string newText)
        {
            return CompareText(oldText, newText, true, true);
        }

        /// <summary>
        /// Compares the text.
        /// </summary>
        /// <param name="oldText">The old text.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="displayInsertedText">if set to <c>true</c> [display inserted text].</param>
        /// <param name="displayDeletedText">if set to <c>true</c> [display deleted text].</param>
        /// <returns></returns>
        private static string CompareText(string oldText, string newText, bool displayInsertedText, bool displayDeletedText)
        {
            return CompareText(oldText, newText, displayInsertedText, displayDeletedText, "<span style='background-color:red;'>", "<span style='background-color:yellow;'>");
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
        private static string CompareText(string oldText, string newText, bool displayInsertedText, bool displayDeletedText, string insertedStyle, string deletedStyle)
        {
            StringBuilder sb = new StringBuilder();
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