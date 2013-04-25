using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.relation;

namespace umbraco.presentation.translation
{
    public partial class preview : BasePages.UmbracoEnsuredPage
    {
        public string originalUrl = "";
        public string translatedUrl = "";

        public preview()
        {
            CurrentApp = DefaultApps.translation.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
            var taskId = int.Parse(helper.Request("id"));

            var t = new Task(taskId);
            var translated = new Document(t.Node.Id);

            translatedUrl = string.Format("../dialogs/preview.aspx?id={0}", translated.Id.ToString());

            var orgRel = Relation.GetRelations(t.Node.Id, RelationType.GetByAlias("relateDocumentOnCopy"));
            if (orgRel.Length > 0)
            {
                var original = new Document(orgRel[0].Parent.Id);
                originalUrl = String.Format("../dialogs/preview.aspx?id={0}", original.Id.ToString());
            }
            else
            {
                Response.Redirect(translatedUrl);
            }


        }
    }
}
