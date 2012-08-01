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

namespace umbraco.controls
{
    public partial class passwordChanger : System.Web.UI.UserControl
    {
        public string  Password
        {
            get { return umbPasswordChanger_passwordNew.Text; }
        }
	
	
        protected void Page_Load(object sender, EventArgs e)
        {

        }

    }
}