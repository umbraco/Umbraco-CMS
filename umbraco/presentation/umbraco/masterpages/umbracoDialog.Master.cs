using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.masterpages {
    public partial class umbracoDialog : System.Web.UI.MasterPage {
        
        public bool reportModalSize { get; set; }
		public static new event MasterPageLoadHandler Load;
        public static event MasterPageLoadHandler Init;

		protected void Page_Load(object sender, EventArgs e)
		{
			ClientLoader.DataBind();
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "setRoot", "UmbClientMgr.setUmbracoPath(\"" + IO.IOHelper.ResolveUrl( IO.SystemDirectories.Umbraco ) + "\");", true);
			FireOnLoad(e);
		}
	
		protected override void OnInit(EventArgs e) {
                base.OnInit(e);

                if (Init != null) {
                    Init(this, e);
                }
            }


            protected virtual void FireOnLoad(EventArgs e) {
                if (Load != null) {
                    Load(this, e);
                }
            }
    }
}
