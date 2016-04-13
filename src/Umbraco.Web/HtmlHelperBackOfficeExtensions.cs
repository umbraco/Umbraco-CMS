using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ClientDependency.Core.Config;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Editors;
using Umbraco.Web.Models;

namespace Umbraco.Web
{
    using Umbraco.Core.Configuration;

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
        public static IHtmlString BareMinimumServerVariablesScript(this HtmlHelper html, UrlHelper uri, string externalLoginsUrl)
        {
            var version = UmbracoVersion.GetSemanticVersion().ToSemanticString();
            var str = @"<script type=""text/javascript"">
                var Umbraco = {};
                Umbraco.Sys = {};
                Umbraco.Sys.ServerVariables = {
                    ""umbracoUrls"": {
                        ""authenticationApiBaseUrl"": """ + uri.GetUmbracoApiServiceBaseUrl<AuthenticationController>(controller => controller.PostLogin(null)) + @""",
                        ""serverVarsJs"": """ + uri.GetUrlWithCacheBust("ServerVariables", "BackOffice") + @""",
                        ""externalLoginsUrl"": """ + externalLoginsUrl + @"""
                    },
                    ""umbracoSettings"": {
                        ""allowPasswordReset"": " + (UmbracoConfig.For.UmbracoSettings().Security.AllowPasswordReset ? "true" : "false") + @"
                    },
                    ""application"": {
                        ""applicationPath"": """ + html.ViewContext.HttpContext.Request.ApplicationPath + @""",
                        ""version"": """ + version + @""",
                        ""cdf"": """ + ClientDependencySettings.Instance.Version + @"""
                    },
                    ""isDebuggingEnabled"" : " + html.ViewContext.HttpContext.IsDebuggingEnabled.ToString().ToLowerInvariant() + @"
                };       
            </script>";

            return html.Raw(str);
        }      

        /// <summary>
        /// Used to render the script that will pass in the angular "externalLoginInfo" service/value on page load
        /// </summary>
        /// <param name="html"></param>
        /// <param name="externalLoginErrors"></param>
        /// <returns></returns>
        public static IHtmlString AngularValueExternalLoginInfoScript(this HtmlHelper html, IEnumerable<string> externalLoginErrors)
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

            var sb = new StringBuilder();
            sb.AppendLine();
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

            return html.Raw(sb.ToString());
        }

        /// <summary>
        /// Used to render the script that will pass in the angular "resetPasswordCodeInfo" service/value on page load
        /// </summary>
        /// <param name="html"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static IHtmlString AngularValueResetPasswordCodeInfoScript(this HtmlHelper html, object val)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(@"var errors = [];");

            var errors = val as IEnumerable<string>;
            if (errors != null)
            {
                foreach (var error in errors)
                {
                    sb.AppendFormat(@"errors.push(""{0}"");", error).AppendLine();
                }
            }

            var resetCodeModel = val as ValidatePasswordResetCodeModel;


            sb.AppendLine(@"app.value(""resetPasswordCodeInfo"", {");
            sb.AppendLine(@"errors: errors,");            
            sb.Append(@"resetCodeModel: ");
            sb.AppendLine(JsonConvert.SerializeObject(resetCodeModel));
            sb.AppendLine(@"});");

            return html.Raw(sb.ToString());
        }
    }
}