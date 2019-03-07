using System;
using Umbraco.Web.Composing;

namespace Umbraco.Web.UI.Config.Splashes
{
    public partial class NoNodes : System.Web.UI.Page
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var store = Current.UmbracoContext.ContentCache;
            if (store.HasContent())
            {
                //if there is actually content, go to the root
                Response.Redirect("~/");
            }
        }

    }
}
