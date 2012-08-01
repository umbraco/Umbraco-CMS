using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace umbraco.presentation.config.splashes
{
    /// <summary>
    /// The booting page is used during Umbraco booting. The page will return a 500 http statuscode.
    /// </summary>
    public partial class booting : System.Web.UI.Page
    {
        /// <summary>
        /// Handles the Load event of the Page control, returns a 500 Statuscode during Umbraco boot.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 500;
        }
    }
}
