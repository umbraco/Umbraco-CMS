using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;

namespace Umbraco.Web.UI.Config.Splashes
{
    public partial class NoNodes : System.Web.UI.Page
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var store = UmbracoContext.Current.ContentCache;
            if (store.HasContent())
			{
                //if there is actually content, go to the root
			    Response.Redirect("~/");
			}
        }

    }
}