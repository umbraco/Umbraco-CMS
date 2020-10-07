using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.WebAssets;
using Umbraco.Web.Composing;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.Models;
using Umbraco.Web.Security;
using Umbraco.Web.Trees;
using Umbraco.Web.WebAssets;
using Umbraco.Core;

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
        /// <param name="features"></param>
        /// <param name="globalSettings"></param>
        /// <param name="umbracoVersion"></param>
        /// <param name="contentSettings"></param>
        /// <param name="treeCollection"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="settings"></param>
        /// <param name="securitySettings"></param>
        /// <param name="runtimeMinifier"></param>
        /// <returns></returns>
        /// <remarks>
        /// These are the bare minimal server variables that are required for the application to start without being authenticated,
        /// we will load the rest of the server vars after the user is authenticated.
        /// </remarks>
        public static IHtmlString BareMinimumServerVariablesScript(this HtmlHelper html, UrlHelper uri, UmbracoFeatures features, IGlobalSettings globalSettings, IUmbracoVersion umbracoVersion, IContentSettings contentSettings, TreeCollection treeCollection, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IRuntimeSettings settings, ISecuritySettings securitySettings, IRuntimeMinifier runtimeMinifier)
        {
            var serverVars = new BackOfficeServerVariables(uri, Current.RuntimeState, features, globalSettings, umbracoVersion, contentSettings, treeCollection, httpContextAccessor, hostingEnvironment, settings, securitySettings, runtimeMinifier);
            var minVars = serverVars.BareMinimumServerVariables();

            var str = @"<script type=""text/javascript"">
                var Umbraco = {};
                Umbraco.Sys = {};
                Umbraco.Sys.ServerVariables = " + JsonConvert.SerializeObject(minVars) + @";
            </script>";

            return html.Raw(str);
        }

        /// <summary>
        /// Used to render the script that will pass in the angular "externalLoginInfo" service/value on page load
        /// </summary>
        /// <param name="html"></param>
        /// <param name="externalLoginErrors"></param>
        /// <returns></returns>
        public static IHtmlString AngularValueExternalLoginInfoScript(this HtmlHelper html, BackOfficeExternalLoginProviderErrors externalLoginErrors)
        {
            var loginProviders = html.ViewContext.HttpContext.GetOwinContext().Authentication.GetBackOfficeExternalLoginProviders()
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
                foreach (var error in externalLoginErrors.Errors)
                {
                    sb.AppendFormat(@"errors.push(""{0}"");", error.ToSingleLine()).AppendLine();
                }
            }

            sb.AppendLine(@"app.value(""externalLoginInfo"", {");
            if (externalLoginErrors?.AuthenticationType != null)
                sb.AppendLine($@"errorProvider: '{externalLoginErrors.AuthenticationType}',");
            sb.AppendLine(@"errors: errors,");
            sb.Append(@"providers: ");
            sb.AppendLine(JsonConvert.SerializeObject(loginProviders));
            sb.AppendLine(@"});");

            return html.Raw(sb.ToString());
        }

        [Obsolete("Use the other overload instead")]
        public static IHtmlString AngularValueExternalLoginInfoScript(this HtmlHelper html, IEnumerable<string> externalLoginErrors)
        {
            return html.AngularValueExternalLoginInfoScript(new BackOfficeExternalLoginProviderErrors(string.Empty, externalLoginErrors));
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

        public static IHtmlString AngularValueTinyMceAssets(this HtmlHelper html, IRuntimeMinifier runtimeMinifier)
        {
            var files = runtimeMinifier.GetAssetPathsAsync(BackOfficeWebAssets.UmbracoTinyMceJsBundleName).GetAwaiter().GetResult();

            var sb = new StringBuilder();

            sb.AppendLine(@"app.value(""tinyMceAssets"",");
            sb.AppendLine(JsonConvert.SerializeObject(files));
            sb.AppendLine(@");");


            return html.Raw(sb.ToString());
        }
    }
}
