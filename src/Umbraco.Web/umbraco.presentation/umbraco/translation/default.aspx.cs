using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Web;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.translation;
//using umbraco.cms.businesslogic.utilities;
using umbraco.cms.businesslogic.web;

using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.GZip;
using Umbraco.Core.IO;
using System.Collections.Generic;

namespace umbraco.presentation.translation
{
    public partial class _default : UmbracoEnsuredPage
    {
        public _default()
        {
            CurrentApp = DefaultApps.translation.ToString();

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

            taskList.Columns[0].HeaderText = ui.Text("nodeName");
            taskList.Columns[1].HeaderText = ui.Text("translation", "taskAssignedBy");
            taskList.Columns[2].HeaderText = ui.Text("date");

            ((System.Web.UI.WebControls.HyperLinkField)taskList.Columns[3]).Text = ui.Text("translation", "details");
            ((System.Web.UI.WebControls.HyperLinkField)taskList.Columns[4]).Text = ui.Text("translation", "downloadTaskAsXml");

            Tasks ts = new Tasks();
            if (Request["mode"] == "owned")
            {
                ts = Task.GetOwnedTasks(base.getUser(), false);
                pane_tasks.Text = ui.Text("translation", "ownedTasks");
                Panel2.Text = ui.Text("translation", "ownedTasks");
            }
            else
            {
                ts = Task.GetTasks(base.getUser(), false);
                pane_tasks.Text = ui.Text("translation", "assignedTasks");
                Panel2.Text = ui.Text("translation", "assignedTasks");
            }

            uploadFile.Text = ui.Text("upload");
            pane_uploadFile.Text = ui.Text("translation", "uploadTranslationXml");

            foreach (Task t in ts)
            {
                if (t.Type.Alias == "toTranslate")
                {
                    DataRow task = tasks.NewRow();
                    task["Id"] = t.Id;
                    task["Date"] = t.Date;
                    task["NodeId"] = t.Node.Id;
                    task["NodeName"] = t.Node.Text;
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

                    cms.businesslogic.utilities.Zip.UnPack(tempFileName, tempPath, true);

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

                                    sb.Append("<li>" + translation.Node.Text + " <a target=\"_blank\" href=\"preview.aspx?id=" + translation.Id + "\">" + ui.Text("preview") + "</a></li>");
                                }
                            }
                            catch (Exception ee)
                            {
                                sb.Append("<li style=\"color: red;\">" + ee.ToString() + "</li>");
                            }
                        }

                        feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
                        feedback.Text = "<h3>" + ui.Text("translation", "MultipleTranslationDone") + "</h3><p>" + ui.Text("translation", "translationDoneHelp") + "</p><ul>" + sb.ToString() + "</ul>";
                    }
                    catch (Exception ex)
                    {
                        feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                        feedback.Text = "<h3>" + ui.Text("translation", "translationFailed") + "</h3><p>" + ex.ToString() + "</>";
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    List<Task> l = ImportTranslatationFile(tempFileName);

                    if (l.Count == 1)
                    {
                        feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
                        feedback.Text = "<h3>" + ui.Text("translation", "translationDone") + "</h3><p>" + ui.Text("translation", "translationDoneHelp") + "</p><p><a target=\"_blank\" href=\"preview.aspx?id=" + l[0].Id + "\">" + ui.Text("preview") + "</a></p>";
                    }

                    else
                    {
                        foreach (Task t in l)
                        {
                            sb.Append("<li>" + t.Node.Text + " <a target=\"_blank\" href=\"preview.aspx?id=" + t.Id + "\">" + ui.Text("preview") + "</a></li>");
                        }

                        feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
                        feedback.Text = "<h3>" + ui.Text("translation", "MultipleTranslationDone") + "</h3><p>" + ui.Text("translation", "translationDoneHelp") + "</p><ul>" + sb.ToString() + "</ul>";
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
                    string xpath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "node" : "* [@isDoc]";
                    XmlNode taskNode = taskXml.SelectSingleNode(xpath);

                    // validate file
                    Task t = new Task(int.Parse(taskXml.Attributes.GetNamedItem("Id").Value));
                    if (t != null)
                    {
                        //user auth and content node validation
                        if (t.Node.Id == int.Parse(taskNode.Attributes.GetNamedItem("id").Value) && (t.User.Id == UmbracoUser.Id || t.ParentUser.Id == UmbracoUser.Id))
                        {

                            // update node contents
                            var d = new Document(t.Node.Id);
                            Document.Import(d.ParentId, UmbracoUser, (XmlElement)taskNode);

                            //send notifications! TODO: This should be put somewhere centralized instead of hard coded directly here
                            ApplicationContext.Services.NotificationService.SendNotification(d.ContentEntity, ActionTranslate.Instance, ApplicationContext);

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
