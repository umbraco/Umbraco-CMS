using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.umbraco
{


    public partial class helpRedirect : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Help help = new Help(UmbracoSettings.HelpPages);

            HelpPage requestedHelpPage = new HelpPage
            {
                Application = Request.QueryString["Application"],
                ApplicationUrl = Request.QueryString["ApplicationUrl"],
                Language = Request.QueryString["Language"],
                UserType = Request.QueryString["UserType"]
            };

            Response.Redirect(help.ResolveHelpUrl(requestedHelpPage));
        }
    }
}