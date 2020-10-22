using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.WebAssets;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Security;
using Umbraco.Web.Features;
using Umbraco.Web.Models;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebAssets;
using Umbraco.Core;

namespace Umbraco.Extensions
{
    public static class HtmlHelperBackOfficeExtensions
    {
        /// <summary>
        /// Outputs a script tag containing the bare minimum (non secure) server vars for use with the angular app
        /// </summary>
        /// <param name="html"></param>
        /// <param name="linkGenerator"></param>
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
        public static async Task<IHtmlContent> BareMinimumServerVariablesScriptAsync(this IHtmlHelper html, BackOfficeServerVariables backOfficeServerVariables)
        {
            var minVars = await backOfficeServerVariables.BareMinimumServerVariablesAsync();

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
        /// <param name="signInManager"></param>
        /// <returns></returns>
        public static async Task<IHtmlContent> AngularValueExternalLoginInfoScriptAsync(this IHtmlHelper html,
            BackOfficeExternalLoginProviderErrors externalLoginErrors,
            BackOfficeSignInManager signInManager,
            IEnumerable<string> externalLoginErrors)
        {
            var providers = await signInManager.GetExternalAuthenticationSchemesAsync();

            var loginProviders = providers
                // TODO: We need to filter only back office enabled schemes.
                // Before we used to have a property bag to check, now we don't so need to investigate the easiest/best
                // way to do this. We have the type so maybe we check for a marker interface, but maybe there's another way,
                // just need to investigate.
                //.Where(p => p.Properties.ContainsKey("UmbracoBackOffice"))
                .Select(p => new
                {
                    authType = p.Name,
                    caption = p.DisplayName,
                    // TODO: See above, if we need this property bag in the vars then we'll need to figure something out
                    //properties = p.Properties
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
        public static IHtmlContent AngularValueResetPasswordCodeInfoScript(this IHtmlHelper html, object val)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(@"var errors = [];");

            if (val is IEnumerable<string> errors)
            {
                foreach (var error in errors)
                {
                    sb.AppendFormat(@"errors.push(""{0}"");", error).AppendLine();
                }
            }

            sb.AppendLine(@"app.value(""resetPasswordCodeInfo"", {");
            sb.AppendLine(@"errors: errors,");
            sb.Append(@"resetCodeModel: ");
            sb.AppendLine(val?.ToString() ?? "null");
            sb.AppendLine(@"});");

            return html.Raw(sb.ToString());
        }

        public static async Task<IHtmlContent> AngularValueTinyMceAssetsAsync(this IHtmlHelper html, IRuntimeMinifier runtimeMinifier)
        {
            var files = await runtimeMinifier.GetAssetPathsAsync(BackOfficeWebAssets.UmbracoTinyMceJsBundleName);

            var sb = new StringBuilder();

            sb.AppendLine(@"app.value(""tinyMceAssets"",");
            sb.AppendLine(JsonConvert.SerializeObject(files));
            sb.AppendLine(@");");


            return html.Raw(sb.ToString());
        }
    }
}
