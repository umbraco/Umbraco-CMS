using System;
using System.Data;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.translation;
using umbraco.cms.businesslogic.web;

namespace umbraco.presentation.umbraco.translation {
    public partial class details : BasePages.UmbracoEnsuredPage {

        public details()
        {
            CurrentApp = BusinessLogic.DefaultApps.translation.ToString();

        }
        protected void closeTask(object sender, EventArgs e) {
            int translationId = int.Parse(Request["id"]);
            Task t = new Task(translationId);

            if (t != null && (t.ParentUser.Id == base.getUser().Id || t.User.Id == base.getUser().Id)) {
                t.Closed = true;
                t.Save();
                Response.Redirect("default.aspx");
            }
        }


        protected void Page_Load(object sender, EventArgs e) {
            int translationId = int.Parse(Request["id"]);
            Task t = new Task(translationId);
            Document page = new Document(t.Node.Id);

            //Bind meta data and language... 
            Literal lt = new Literal();
            lt.Text = t.Date.ToLongDateString() + " " + t.Date.ToLongTimeString();
            pp_date.Controls.Add(lt);
            pp_date.Text = ui.Text("translation","taskOpened");

            lt = new Literal();
            lt.Text = t.ParentUser.Name;
            pp_owner.Controls.Add(lt);
            pp_owner.Text = ui.Text("translation", "taskAssignedBy");

            lt = new Literal();
            lt.Text = Translation.CountWords(t.Node.Id).ToString();
            pp_totalWords.Controls.Add(lt);
            pp_totalWords.Text = ui.Text("translation", "totalWords");

            lt = new Literal();
            lt.Text = library.ReplaceLineBreaks(t.Comment);
            pp_comment.Controls.Add(lt);
            pp_comment.Text = ui.Text("comment");

            lt = new Literal();
            lt.Text =  "<a target=\"_blank\" href=\"xml.aspx?id=" + t.Id + "\">" + ui.Text("download") + "</a>";
            pp_xml.Controls.Add(lt);
            pp_xml.Text = ui.Text("translation", "downloadTaskAsXml");

            pane_details.Text = ui.Text("translation", "details");
            panel1.Text = ui.Text("translation", "details");

            pane_fields.Text = ui.Text("translation", "fields");
            pane_tasks.Text = ui.Text("translation", "translationOptions");
            lt = new Literal();
            lt.Text = "<a href=\"default.aspx?id=" + t.Id + "\">" + ui.Text("upload") + "</a>";
            pp_upload.Controls.Add(lt);
            pp_upload.Text = ui.Text("translation", "uploadTranslationXml");

            if (t.Closed)
                pp_closeTask.Visible = false;
            else {
                pp_closeTask.Text = ui.Text("translation", "closeTask");
                bt_close.Text = ui.Text("close");
            }


            //Bind page fields
            DataTable pageTable = new DataTable();
            pageTable.Columns.Add(ui.Text("name"));
            pageTable.Columns.Add(ui.Text("value"));
            
            DataRow pageRow = pageTable.NewRow();
            pageRow[ui.Text("name")] = ui.Text("nodeName");
            pageRow[ui.Text("value")] = page.Text;
            pageTable.Rows.Add(pageRow);
            
            foreach (PropertyType pt in page.ContentType.PropertyTypes) {
                pageRow = pageTable.NewRow();
                pageRow[ui.Text("name")] = pt.Name;
                pageRow[ui.Text("value")] = page.getProperty(pt.Alias).Value;
                pageTable.Rows.Add(pageRow);
            }
            
            dg_fields.DataSource = pageTable;
            dg_fields.DataBind();
        }
    }
}
