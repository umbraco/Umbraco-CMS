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

using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.relation;

namespace umbraco.presentation.translation
{
    public partial class preview : BasePages.UmbracoEnsuredPage
    {
        public string originalUrl = "";
        public string translatedUrl = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            
            int taskId = int.Parse(helper.Request("id"));

            Task t = new Task(taskId);
            Document translated = new Document(t.Node.Id);

            translatedUrl = String.Format("../dialogs/preview.aspx?id={0}", translated.Id.ToString(), translated.Version.ToString());

            Relation[] orgRel = Relation.GetRelations(t.Node.Id, RelationType.GetByAlias("relateDocumentOnCopy"));
            if (orgRel.Length > 0) {
                Document original = new Document(orgRel[0].Parent.Id);
                originalUrl = String.Format("../dialogs/preview.aspx?id={0}", original.Id.ToString(), original.Version.ToString());
            } else {
                Response.Redirect(translatedUrl);
            }
            
            
        }
    }
}
