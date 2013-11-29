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
using Umbraco.Web;
using Umbraco.Core;

namespace umbraco
{
    /// <summary>
    /// Summary description for logout.
    /// </summary>
    public class logout : BasePages.BasePage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //We need to check the token in the URL to ensure it is correct otherwise malicious GET requests using CSRF attacks
            // can easily just log the user out.
            var token = Request["t"];
            //only perform the logout if the token matches
            if (token.IsNullOrWhiteSpace() == false && token == umbracoUserContextID)
            {
                ClearLogin();
            }

            //redirect home
            Response.Redirect("Login.aspx?redir=" + Server.UrlEncode(Request["redir"]));
        }

    }
}
