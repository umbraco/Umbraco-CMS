using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web._Legacy.BusinessLogic;

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

            translatedUrl = string.Format("../dialogs/preview.aspx?id={0}", t.TaskEntity.EntityId);

            var orgRel = Services.RelationService.GetByParentOrChildId(t.TaskEntity.EntityId, "relateDocumentOnCopy").ToArray();

            if (orgRel.Length > 0)
            {
                originalUrl = String.Format("../dialogs/preview.aspx?id={0}", orgRel[0].ParentId);
            }
            else
            {
                Response.Redirect(translatedUrl);
            }


        }
    }
}
