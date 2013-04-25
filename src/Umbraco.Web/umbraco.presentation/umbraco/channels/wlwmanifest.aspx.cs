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

namespace umbraco.presentation.channels
{
    [Obsolete("This class is no longer used and will be removed from the codebase in future versions")]
    public partial class wlwmanifest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool useXhtml = false;
            if (bool.TryParse(GlobalSettings.EditXhtmlMode, out useXhtml) && !useXhtml)
            {
                xhtml.Text = "no";
            }
            else
            {
                xhtml.Text = "yes";
            }
        }

        /// <summary>
        /// xhtml control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal xhtml;
    }
}
