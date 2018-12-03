using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace umbraco.presentation.templateControls
{
    /// <summary>
    /// This control disables request validation (equalevant of setting validateRequest to false in page directive)
    /// </summary>
    [ToolboxData("<{0}:DisableRequestValidation runat=\"server\"></{0}:Item>")]
    [Designer("umbraco.presentation.templateControls.ItemDesigner, Umbraco.Web")]
    public class DisableRequestValidation : System.Web.UI.WebControls.WebControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ((UmbracoDefault)base.Page).ValidateRequest = false;
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
        }
    }
}
