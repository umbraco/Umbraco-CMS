using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Security;

namespace Umbraco.Extensions;

public static class HtmlHelperBackOfficeExtensions
{
    /// <summary>
    ///     Outputs a script tag containing the bare minimum (non secure) server vars for use with the angular app
    /// </summary>
    /// <param name="html"></param>
    /// <param name="backOfficeServerVariables"></param>
    /// <returns></returns>
    /// <remarks>
    ///     These are the bare minimal server variables that are required for the application to start without being
    ///     authenticated,
    ///     we will load the rest of the server vars after the user is authenticated.
    /// </remarks>
    public static async Task<IHtmlContent> BareMinimumServerVariablesScriptAsync(this IHtmlHelper html,
        BackOfficeServerVariables backOfficeServerVariables)
    {
        Dictionary<string, object> minVars = await backOfficeServerVariables.BareMinimumServerVariablesAsync();

        var str = @"<script type=""text/javascript"">
                var Umbraco = {};
                Umbraco.Sys = {};
                Umbraco.Sys.ServerVariables = " + JsonConvert.SerializeObject(minVars) + @";
            </script>";

        return html.Raw(str);
    }

    /// <summary>
    ///     Used to render the script that will pass in the angular "externalLoginInfo" service/value on page load
    /// </summary>
    /// <param name="html"></param>
    /// <param name="externalLogins"></param>
    /// <param name="externalLoginErrors"></param>
    /// <returns></returns>
    [Obsolete("This is deprecated and will be removed in V15")]
    public static async Task<IHtmlContent> AngularValueExternalLoginInfoScriptAsync(this IHtmlHelper html,
        IBackOfficeExternalLoginProviders externalLogins,
        BackOfficeExternalLoginProviderErrors externalLoginErrors)
    {
        IEnumerable<BackOfficeExternaLoginProviderScheme>
            providers = await externalLogins.GetBackOfficeProvidersAsync();

        var loginProviders = providers
            .Select(p => new
            {
                authType = p.ExternalLoginProvider.AuthenticationType,
                caption = p.AuthenticationScheme.DisplayName,
                options = new
                {
                    allowManualLinking = p.ExternalLoginProvider.Options.AutoLinkOptions.AllowManualLinking,
                    buttonStyle = p.ExternalLoginProvider.Options.ButtonStyle,
                    buttonLook = p.ExternalLoginProvider.Options.ButtonLook.ToString().ToLowerInvariant(),
                    buttonColor = p.ExternalLoginProvider.Options.ButtonColor.ToString().ToLowerInvariant(),
                    customBackOfficeView = p.ExternalLoginProvider.Options.CustomBackOfficeView,
                    denyLocalLogin = p.ExternalLoginProvider.Options.DenyLocalLogin,
                    icon = p.ExternalLoginProvider.Options.Icon,
                },
                properties = p.ExternalLoginProvider.Options,
            })
            .ToArray();

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine(@"var errors = [];");

        if (externalLoginErrors != null)
        {
            if (externalLoginErrors.Errors is not null)
            {
                foreach (var error in externalLoginErrors.Errors)
                {
                    sb.AppendFormat(@"errors.push(""{0}"");", error.ToSingleLine()).AppendLine();
                }
            }
        }

        sb.AppendLine(@"app.value(""externalLoginInfo"", {");
        if (externalLoginErrors?.AuthenticationType != null)
        {
            sb.AppendLine($@"errorProvider: '{externalLoginErrors.AuthenticationType}',");
        }

        sb.AppendLine(@"errors: errors,");
        sb.Append(@"providers: ");
        sb.AppendLine(JsonConvert.SerializeObject(loginProviders));
        sb.AppendLine(@"});");

        return html.Raw(sb.ToString());
    }

    /// <summary>
    ///     Used to render the script that will pass in the angular "resetPasswordCodeInfo" service/value on page load
    /// </summary>
    /// <param name="html"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    [Obsolete("This is deprecated and will be removed in V15")]
    public static IHtmlContent AngularValueResetPasswordCodeInfoScript(this IHtmlHelper html, object? val)
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

            val = null;
        }

        sb.AppendLine(@"app.value(""resetPasswordCodeInfo"", {");
        sb.AppendLine(@"errors: errors,");
        sb.Append(@"resetCodeModel: ");
        sb.AppendLine(val?.ToString() ?? "null");
        sb.AppendLine(@"});");

        return html.Raw(sb.ToString());
    }

    public static async Task<IHtmlContent> AngularValueTinyMceAssetsAsync(this IHtmlHelper html,
        IRuntimeMinifier runtimeMinifier)
    {
        IEnumerable<string> files =
            await runtimeMinifier.GetJsAssetPathsAsync(BackOfficeWebAssets.UmbracoTinyMceJsBundleName);

        var sb = new StringBuilder();

        sb.AppendLine(@"app.value(""tinyMceAssets"",");
        sb.AppendLine(JsonConvert.SerializeObject(files));
        sb.AppendLine(@");");


        return html.Raw(sb.ToString());
    }
}
