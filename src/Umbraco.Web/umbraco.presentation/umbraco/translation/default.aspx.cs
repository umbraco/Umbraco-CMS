using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using Umbraco.Core.Services;
using Umbraco.Core.IO;
using Umbraco.Core;
using System.Collections.Generic;
using Umbraco.Web.UI.Pages;
using Umbraco.Web._Legacy.BusinessLogic;

namespace umbraco.presentation.translation
{
    public partial class _default : UmbracoEnsuredPage
    {
        public _default()
        {
            CurrentApp = Constants.Applications.Translation;

        }
        protected void Page_Load(object sender, EventArgs e)
        {
            DataTable tasks = new DataTable();
            tasks.Columns.Add("Id");
            tasks.Columns.Add("Date");
            tasks.Columns.Add("NodeId");
            tasks.Columns.Add("NodeName");
            tasks.Columns.Add("ReferingUser");
            tasks.Columns.Add("Language");

            taskList.Columns[0].HeaderText = Services.TextService.Localize("nodeName");
            taskList.Columns[1].HeaderText = Services.TextService.Localize("translation/taskAssignedBy");
            taskList.Columns[2].HeaderText = Services.TextService.Localize("date");

            ((System.Web.UI.WebControls.HyperLinkField)taskList.Columns[3]).Text = Services.TextService.Localize("translation/details");
            ((System.Web.UI.WebControls.HyperLinkField)taskList.Columns[4]).Text = Services.TextService.Localize("translation/downloadTaskAsXml");

            Tasks ts = new Tasks();
            if (Request["mode"] == "owned")
            {
                ts = Task.GetOwnedTasks(Security.CurrentUser, false);
                pane_tasks.Text = Services.TextService.Localize("translation/ownedTasks");
                Panel2.Text = Services.TextService.Localize("translation/ownedTasks");
            }
            else
            {
                ts = Task.GetTasks(Security.CurrentUser, false);
                pane_tasks.Text = Services.TextService.Localize("translation/assignedTasks");
                Panel2.Text = Services.TextService.Localize("translation/assignedTasks");
            }

            uploadFile.Text = Services.TextService.Localize("upload");
            pane_uploadFile.Text = Services.TextService.Localize("translation/uploadTranslationXml");

            foreach (Task t in ts)
            {
                if (t.Type.Alias == "toTranslate")
                {
                    DataRow task = tasks.NewRow();
                    task["Id"] = t.Id;
                    task["Date"] = t.Date;
                    task["NodeId"] = t.TaskEntity.EntityId;
                    task["NodeName"] = t.TaskEntityEntity.Name;
                    task["ReferingUser"] = t.ParentUser.Name;
                    tasks.Rows.Add(task);
                }
            }

            taskList.DataSource = tasks;
            taskList.DataBind();
            feedback.Style.Add("margin-top", "10px");

        }

        protected void uploadFile_Click(object sender, EventArgs e)
        {
            // Save temp file
            if (translationFile.PostedFile != null)
            {
                string tempFileName;
                if (translationFile.PostedFile.FileName.ToLower().Contains(".zip"))
                    tempFileName = IOHelper.MapPath(SystemDirectories.Data + "/" + "translationFile_" + Guid.NewGuid().ToString() + ".zip");
                else
                    tempFileName = IOHelper.MapPath(SystemDirectories.Data + "/" + "translationFile_" + Guid.NewGuid().ToString() + ".xml");

                translationFile.PostedFile.SaveAs(tempFileName);

                // xml or zip file
                if (new FileInfo(tempFileName).Extension.ToLower() == ".zip")
                {
                    // Zip Directory
                    string tempPath = IOHelper.MapPath(SystemDirectories.Data + "/" + "translationFiles_" + Guid.NewGuid().ToString());

                    // Add the path to the zipfile to viewstate
                    ViewState.Add("zipFile", tempPath);

                    // Unpack the zip file

                    IOHelper.UnZip(tempFileName, tempPath, true);

                    // Test the number of xml files
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (FileInfo translationFileXml in new DirectoryInfo(ViewState["zipFile"].ToString()).GetFiles("*.xml"))
                        {
                            try
                            {
                                foreach (Task translation in ImportTranslatationFile(translationFileXml.FullName))
                                {

                                    sb.Append("<li>" + translation.TaskEntityEntity.Name + " <a target=\"_blank\" href=\"preview.aspx?id=" + translation.Id + "\">" + Services.TextService.Localize("preview") + "</a></li>");
                                }
                            }
                            catch (Exception ee)
                            {
                                sb.Append("<li style=\"color: red;\">" + ee.ToString() + "</li>");
                            }
                        }

                        feedback.type = global::Umbraco.Web._Legacy.Controls.Feedback.feedbacktype.success;
                        feedback.Text = "<h3>" + Services.TextService.Localize("translation/MultipleTranslationDone") + "</h3><p>" + Services.TextService.Localize("translation/translationDoneHelp") + "</p><ul>" + sb.ToString() + "</ul>";
                    }
                    catch (Exception ex)
                    {
                        feedback.type = global::Umbraco.Web._Legacy.Controls.Feedback.feedbacktype.error;
                        feedback.Text = "<h3>" + Services.TextService.Localize("translation/translationFailed") + "</h3><p>" + ex.ToString() + "</>";
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    List<Task> l = ImportTranslatationFile(tempFileName);

                    if (l.Count == 1)
                    {
                        feedback.type = global::Umbraco.Web._Legacy.Controls.Feedback.feedbacktype.success;
                        feedback.Text = "<h3>" + Services.TextService.Localize("translation/translationDone") + "</h3><p>" + Services.TextService.Localize("translation/translationDoneHelp") + "</p><p><a target=\"_blank\" href=\"preview.aspx?id=" + l[0].Id + "\">" + Services.TextService.Localize("preview") + "</a></p>";
                    }

                    else
                    {
                        foreach (Task t in l)
                        {
                            sb.Append("<li>" + t.TaskEntityEntity.Name + " <a target=\"_blank\" href=\"preview.aspx?id=" + t.Id + "\">" + Services.TextService.Localize("preview") + "</a></li>");
                        }

                        feedback.type = global::Umbraco.Web._Legacy.Controls.Feedback.feedbacktype.success;
                        feedback.Text = "<h3>" + Services.TextService.Localize("translation/MultipleTranslationDone") + "</h3><p>" + Services.TextService.Localize("translation/translationDoneHelp") + "</p><ul>" + sb.ToString() + "</ul>";
                    }
                }

                // clean up
                File.Delete(tempFileName);
            }
        }

        private List<Task> ImportTranslatationFile(string tempFileName)
        {
            try
            {
                List<Task> tl = new List<Task>();

                // open file
                XmlDocument tf = new XmlDocument();
                tf.XmlResolver = null;
                tf.Load(tempFileName);

                // Get task xml node
                XmlNodeList tasks = tf.SelectNodes("//task");

                foreach (XmlNode taskXml in tasks)
                {
                    string xpath = "* [@isDoc]";
                    XmlNode taskNode = taskXml.SelectSingleNode(xpath);

                    // validate file
                    Task t = new Task(int.Parse(taskXml.Attributes.GetNamedItem("Id").Value));
                    if (t != null)
                    {
                        //user auth and content node validation
                        if (t.TaskEntity.EntityId == int.Parse(taskNode.Attributes.GetNamedItem("id").Value) && (t.User.Id == Security.CurrentUser.Id || t.ParentUser.Id == Security.CurrentUser.Id))
                        {

                            //TODO: Make this work again with correct APIs and angularized - so none of this code will exist anymore
                            //// update node contents
                            //var d = new Document(t.Node.Id);
                            //Document.Import(d.ParentId, UmbracoUser, (XmlElement)taskNode);

                            ////send notifications! TODO: This should be put somewhere centralized instead of hard coded directly here
                            //ApplicationContext.Services.NotificationService.SendNotification(d.Content, ActionTranslate.Instance, ApplicationContext);

                            t.Closed = true;
                            t.Save();


                            tl.Add(t);
                        }
                    }
                }

                return tl;
            }
            catch (Exception ee)
            {
                throw new Exception("Error importing translation file '" + tempFileName + "': " + ee.ToString());
            }
        }
    }
}
