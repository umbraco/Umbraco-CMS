using System;
using System.Web.UI;
using umbraco.BasePages;

namespace umbraco.presentation.dialogs
{
    public partial class emptyTrashcan : UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/trashcan.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }
    }
}