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
using umbraco.IO;

namespace umbraco.presentation.config.splashes
{
    /// <summary>
    /// noNodes page is used in case Umbraco doesn't contain any content nodes.
    /// noNodes displays a standard error message to inform how to fix the problem.
    /// </summary>
    public partial class noNodes : System.Web.UI.Page
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
           
        }
    }
}
