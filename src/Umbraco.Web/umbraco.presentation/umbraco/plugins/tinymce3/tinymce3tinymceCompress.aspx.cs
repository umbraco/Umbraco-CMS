using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.plugins.tinymce3
{
    public partial class tinymce3tinymceCompress : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            new GzipModule().ProcessRequest(this.Context);
        }
    }
}
