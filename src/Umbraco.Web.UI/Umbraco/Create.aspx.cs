using System;
using System.Web;
using Umbraco.Web._Legacy.UI;

namespace Umbraco.Web.UI.Umbraco
{
    public partial class CreateDialog : global::umbraco.cms.presentation.Create
    {

        protected override void OnLoad(EventArgs e)
        {
            if (SecurityCheck(Request.QueryString["nodeType"]))
            {
                //if we're allowed, then continue
                base.OnLoad(e);
            }
            else
            {
                //otherwise show an error
                UI.Visible = false;
                AccessError.Visible = true;
            }
        }

        private bool SecurityCheck(string nodeTypeAlias)
        {
            return LegacyDialogHandler.UserHasCreateAccess(
                new HttpContextWrapper(Context),
                Security.CurrentUser,
                nodeTypeAlias);
        }

    }
}
