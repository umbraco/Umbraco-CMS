using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.install.utills
{
    public partial class p : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            string feed = Request.QueryString["feed"];
            string url = "http://our.umbraco.org/html/twitter";

            if (feed == "blogs")
                url = "http://our.umbraco.org/html/blogs";

            Response.Write(library.GetXmlDocumentByUrl(url).Current.OuterXml);
        }
    }
}