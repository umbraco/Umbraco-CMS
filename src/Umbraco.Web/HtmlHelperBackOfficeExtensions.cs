using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Umbraco.Web.Editors;

namespace Umbraco.Web
{
    /// <summary>
    /// HtmlHelper extensions for the back office
    /// </summary>
    public static class HtmlHelperBackOfficeExtensions
    {
        /// <summary>
        /// Outputs a script tag containing the bare minimum (non secure) server vars for use with the angular app
        /// </summary>
        /// <param name="html"></param>
        /// <param name="uri"></param>
        /// <param name="externalLoginsUrl">
        /// The post url used to sign in with external logins - this can change depending on for what service the external login is service.
        /// Example: normal back office login or authenticating upgrade login
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// These are the bare minimal server variables that are required for the application to start without being authenticated,
        /// we will load the rest of the server vars after the user is authenticated.
        /// </remarks>
        public static IHtmlString BareMinimumServerVariables(this HtmlHelper html, UrlHelper uri, string externalLoginsUrl)
        {
            var str = @"<script type=""text/javascript"">
                var Umbraco = {};
                Umbraco.Sys = {};
                Umbraco.Sys.ServerVariables = {
                    ""umbracoUrls"": {
                        ""authenticationApiBaseUrl"": """ + uri.GetUmbracoApiServiceBaseUrl<AuthenticationController>(controller => controller.PostLogin(null)) + @""",
                        ""serverVarsJs"": """ + uri.GetUrlWithCacheBust("ServerVariables", "BackOffice") + @""",
                        ""externalLoginsUrl"": """ + externalLoginsUrl + @"""
                    },
                    ""application"": {
                        ""applicationPath"": """ + html.ViewContext.HttpContext.Request.ApplicationPath + @"""
                    },
                    ""isDebuggingEnabled"" : " + html.ViewContext.HttpContext.IsDebuggingEnabled.ToString().ToLowerInvariant() + @"
                };       
            </script>";

            return html.Raw(str);
        }

        /// <summary>
        /// Used to render the script tag that will pass in the angular externalLoginInfo service on page load
        /// </summary>
        /// <param name="html"></param>
        /// <param name="externalLoginErrors"></param>
        /// <returns></returns>
        public static IHtmlString AngularExternalLoginInfoValues(this HtmlHelper html, IEnumerable<string> externalLoginErrors)
        {
            var loginProviders = html.ViewContext.HttpContext.GetOwinContext().Authentication.GetExternalAuthenticationTypes()
                .Where(p => p.Properties.ContainsKey("UmbracoBackOffice"))
                .Select(p => new
                {
                    authType = p.AuthenticationType,
                    caption = p.Caption,
                    properties = p.Properties
                })
                .ToArray();


            //define a callback that is executed when we bootstrap angular, this is used to inject angular values
            //with server side info

            var sb = new StringBuilder(@"<script type=""text/javascript"">");
            sb.AppendLine(@"document.angularReady = function(app) {");
            sb.AppendLine(@"var errors = [];");

            if (externalLoginErrors != null)
            {
                foreach (var error in externalLoginErrors)
                {
                    sb.AppendFormat(@"errors.push(""{0}"");", error).AppendLine();
                }
            }

            sb.AppendLine(@"app.value(""externalLoginInfo"", {");
            sb.AppendLine(@"errors: errors,");
            sb.Append(@"providers: ");
            sb.AppendLine(JsonConvert.SerializeObject(loginProviders));
            sb.AppendLine(@"});");
            sb.AppendLine(@"}");
            sb.AppendLine("</script>");

            return html.Raw(sb.ToString());
        }
    }
}