using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.UI.Umbraco
{
    public partial class umbraco : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Status = "301 Moved Permanently";
            Response.AddHeader("Location", GlobalSettings.Path);
        }
    }
}