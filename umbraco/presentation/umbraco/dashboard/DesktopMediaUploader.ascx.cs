using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;

namespace umbraco.presentation.umbraco.dashboard
{
    public partial class DesktopMediaUploader : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected string FullyQualifiedAppPath
        {
            get
            {
                var appPath = "";
                var context = HttpContext.Current;

                if (context != null)
                {
                    appPath = string.Format("{0}://{1}{2}{3}",
                        context.Request.Url.Scheme,
                        context.Request.Url.Host,
                        (context.Request.Url.Port == 80) ? string.Empty : ":" + context.Request.Url.Port,
                        context.Request.ApplicationPath);
                }

                if (!appPath.EndsWith("/"))
                    appPath += "/";

                return appPath;
            }
        }

        protected string AppLaunchArg
        {
            get
            {
                //var ticket = ((FormsIdentity) HttpContext.Current.User.Identity).Ticket;
                var ticket = new FormsAuthenticationTicket(1,
                    UmbracoEnsuredPage.CurrentUser.LoginName,
                    DateTime.Now,
                    DateTime.Now,
                    false,
                    "");

                return HttpUtility.UrlEncode(Base64Encode(string.Format("{0};{1};{2}",
                    FullyQualifiedAppPath.TrimEnd('/'),
                    UmbracoEnsuredPage.CurrentUser.LoginName,
                    FormsAuthentication.Encrypt(ticket)
                )));
            }
        }

        private string Base64Encode(string input)
        {
            byte[] toEncodeAsBytes = UTF8Encoding.UTF8.GetBytes(input);

            return Convert.ToBase64String(toEncodeAsBytes);
        }
    }
}