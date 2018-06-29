using System;
using System.Data;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web._Legacy.BusinessLogic;
using Umbraco.Web;

namespace umbraco.presentation.umbraco.translation {
    public partial class details : Umbraco.Web.UI.Pages.UmbracoEnsuredPage {

        public details()
        {
            CurrentApp = Constants.Applications.Translation.ToString();

        }
        protected void closeTask(object sender, EventArgs e) {
            int translationId = int.Parse(Request["id"]);
            Task t = new Task(translationId);

            if (t != null && (t.ParentUser.Id == Security.CurrentUser.Id || t.User.Id == Security.CurrentUser.Id)) {
                t.Closed = true;
                t.Save();
                Response.Redirect("default.aspx");
            }
        }


        protected void Page_Load(object sender, EventArgs e) {
            int translationId = int.Parse(Request["id"]);
            Task t = new Task(translationId);
            //Document page = new Document(t.Node.Id);
            var page = Current.Services.ContentService.GetById(t.TaskEntity.EntityId);

            //Bind meta data and language...
            Literal lt = new Literal();
            lt.Text = t.Date.ToLongDateString() + " " + t.Date.ToLongTimeString();
            pp_date.Controls.Add(lt);
            pp_date.Text = Services.TextService.Localize("translation/taskOpened");

            lt = new Literal();
            lt.Text = t.ParentUser.Name;
            pp_owner.Controls.Add(lt);
            pp_owner.Text = Services.TextService.Localize("translation/taskAssignedBy");

            //TODO: Make this work again with correct APIs and angularized - so none of this code will exist anymore
            //lt = new Literal();
            //lt.Text = Translation.CountWords(t.Node.Id).ToString();
            //pp_totalWords.Controls.Add(lt);
            //pp_totalWords.Text = Services.TextService.Localize("translation/totalWords");

            lt = new Literal();


            var umbHelper = new UmbracoHelper(Current.UmbracoContext, Current.Services, Current.ApplicationCache);
            lt.Text = umbHelper.ReplaceLineBreaksForHtml(t.Comment);

            pp_comment.Controls.Add(lt);
            pp_comment.Text = Services.TextService.Localize("comment");

            lt = new Literal();
            lt.Text =  "<a target=\"_blank\" href=\"xml.aspx?id=" + t.Id + "\">" + Services.TextService.Localize("download") + "</a>";
            pp_xml.Controls.Add(lt);
            pp_xml.Text = Services.TextService.Localize("translation/downloadTaskAsXml");

            pane_details.Text = Services.TextService.Localize("translation/details");
            panel1.Text = Services.TextService.Localize("translation/details");

            pane_fields.Text = Services.TextService.Localize("translation/fields");
            pane_tasks.Text = Services.TextService.Localize("translation/translationOptions");
            lt = new Literal();
            lt.Text = "<a href=\"default.aspx?id=" + t.Id + "\">" + Services.TextService.Localize("upload") + "</a>";
            pp_upload.Controls.Add(lt);
            pp_upload.Text = Services.TextService.Localize("translation/uploadTranslationXml");

            if (t.Closed)
                pp_closeTask.Visible = false;
            else {
                pp_closeTask.Text = Services.TextService.Localize("translation/closeTask");
                bt_close.Text = Services.TextService.Localize("close");
            }


            //Bind page fields
            DataTable pageTable = new DataTable();
            pageTable.Columns.Add(Services.TextService.Localize("name"));
            pageTable.Columns.Add(Services.TextService.Localize("value"));

            DataRow pageRow = pageTable.NewRow();
            pageRow[Services.TextService.Localize("name")] = Services.TextService.Localize("nodeName");
            pageRow[Services.TextService.Localize("value")] = page.Name;
            pageTable.Rows.Add(pageRow);

            //TODO: Make this work again with correct APIs and angularized - so none of this code will exist anymore
            //foreach (PropertyType pt in page.ContentType.PropertyTypes) {
            //    pageRow = pageTable.NewRow();
            //    pageRow[Services.TextService.Localize("name")] = pt.Name;
            //    pageRow[Services.TextService.Localize("value")] = page.getProperty(pt.Alias).Value;
            //    pageTable.Rows.Add(pageRow);
            //}

            dg_fields.DataSource = pageTable;
            dg_fields.DataBind();
        }
    }
}
