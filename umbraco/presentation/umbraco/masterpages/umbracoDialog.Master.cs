using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.umbraco.masterpages {
    public partial class umbracoDialog : System.Web.UI.MasterPage {
        
        public bool reportModalSize { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			ClientLoader.DataBind();
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "setRoot", "UmbClientMgr.setUmbracoPath(\"" + IO.IOHelper.ResolveUrl( IO.SystemDirectories.Umbraco ) + "\");", true);
		}
    }
}
