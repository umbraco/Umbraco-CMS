using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace umbraco
{
    /// <summary>
    /// Summary description for logout.
    /// </summary>
    public partial class logout : BasePages.BasePage
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Put user code to initialize the page here
            if (umbracoUserContextID != "")
                base.ClearLogin();
        }

        protected System.Web.UI.HtmlControls.HtmlForm Form1;
    }
}
