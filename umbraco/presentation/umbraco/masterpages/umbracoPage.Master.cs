using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

//This is only in case an upgrade goes wrong and the the /masterpages/ files are not copied over
//which would result in an error. so we have kept the old namespaces intact with references to new ones
using mp = umbraco.presentation.masterpages;
namespace umbraco.presentation.umbraco.masterpages {
    public class umbracoPage : mp.umbracoPage { }
    public class umbracoDialog : mp.umbracoDialog { }
}

namespace umbraco.presentation.masterpages
{
    public delegate void MasterPageLoadHandler(object sender, System.EventArgs e);

	public partial class umbracoPage : System.Web.UI.MasterPage
	{

		public static event MasterPageLoadHandler Load;
        public static event MasterPageLoadHandler Init;

		protected void Page_Load(object sender, EventArgs e)
		{
			ClientLoader.DataBind();
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
