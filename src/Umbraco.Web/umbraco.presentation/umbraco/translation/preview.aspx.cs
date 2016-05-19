using System;
using System.Linq;
using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Web;

namespace umbraco.presentation.translation
{
    public partial class preview : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        public string originalUrl = "";
        public string translatedUrl = "";

        public preview()
        {
            CurrentApp = Constants.Applications.Translation.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
            var taskId = int.Parse(Request.GetItemAsString("id"));

            var t = new Task(taskId);
            var translated = new Document(t.Node.Id);

            translatedUrl = string.Format("../dialogs/preview.aspx?id={0}", translated.Id.ToString());

            var orgRel = Services.RelationService.GetByParentOrChildId(t.Node.Id, "relateDocumentOnCopy").ToArray();
            
            if (orgRel.Length > 0)
            {
                var original = new Document(orgRel[0].ParentId);
                originalUrl = String.Format("../dialogs/preview.aspx?id={0}", original.Id.ToString());
            }
            else
            {
                Response.Redirect(translatedUrl);
            }


        }
    }
}
