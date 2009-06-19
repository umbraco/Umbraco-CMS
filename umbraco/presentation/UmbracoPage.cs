using System;
using System.Collections.Generic;
using System.Web;

namespace umbraco.presentation
{
    public class UmbracoPage : System.Web.UI.Page
    {
        public int PageId { get; set; }

        protected override void OnPreInit(EventArgs e)
        {
            if (UmbracoContext.Current == null)
            {
                // Set umbraco context
                UmbracoContext.Current = new UmbracoContext(HttpContext.Current);
            }

            HttpContext.Current.Items["pageID"] = PageId;

            // setup page properties
            page pageObject = new page(((System.Xml.IHasXmlNode) library.GetXmlNodeCurrent().Current).GetNode());
            System.Web.HttpContext.Current.Items.Add("pageElements", pageObject.Elements);

            base.OnPreInit(e);
        }
    }
}
