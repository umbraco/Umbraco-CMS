using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation
{
    public partial class endPreview : BasePages.UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
			preview.PreviewContent.ClearPreviewCookie();
            Response.Redirect(helper.Request("redir"), true);
        }
    }
}
