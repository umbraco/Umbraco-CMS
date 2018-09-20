using System;
using System.Collections.Generic;
using System.Web;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.templateControls
{
    [DefaultProperty("MimeType")]
    [ToolboxData("<{0}:ContentType runat=server></{0}:ContentType>")]
    public class ContentType : WebControl
    {
        [Category("Umbraco")]
        [DefaultValue("")]
        public string MimeType { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            if (!String.IsNullOrEmpty(MimeType))
            {
                Page.Response.ContentType = MimeType;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {

        }
    }
}
