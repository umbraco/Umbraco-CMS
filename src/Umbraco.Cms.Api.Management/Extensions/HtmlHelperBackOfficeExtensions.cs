using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Web.Common.Hosting;

namespace Umbraco.Cms.Api.Management.Extensions;

public static class HtmlHelperBackOfficeExtensions
{
    /// <summary>
    ///     Outputs a script tag containing the import map for the BackOffice.
    /// </summary>
    /// <remarks>
    ///     It will replace the token %CACHE_BUSTER% with the cache buster hash.
    ///     It will also replace the /umbraco/backoffice path with the correct path for the BackOffice assets.
    ///     When a CSP nonce is available, it will be added to the script tag.
    /// </remarks>
    /// <returns>A <see cref="Task"/> containing the html content for the BackOffice import map.</returns>
    public static async Task<IHtmlContent> BackOfficeImportMapScriptAsync(
        this IHtmlHelper html,
        IJsonSerializer jsonSerializer,
        IBackOfficePathGenerator backOfficePathGenerator,
        IPackageManifestService packageManifestService,
        ICspNonceService cspNonceService)
    {
        PackageManifestImportmap packageImports = await packageManifestService.GetPackageManifestImportmapAsync();

        var nonce = cspNonceService.GetNonce();
        var nonceAttribute = string.IsNullOrEmpty(nonce) ? string.Empty : $" nonce=\"{nonce}\"";

        var sb = new StringBuilder();
        sb.AppendLine($"<script type=\"importmap\"{nonceAttribute}>");
        sb.AppendLine(jsonSerializer.Serialize(packageImports));
        sb.AppendLine("</script>");

        // Inject the BackOffice cache buster into the import string to handle BackOffice assets
        var importmapScript = sb.ToString()
            .Replace(backOfficePathGenerator.BackOfficeVirtualDirectory, backOfficePathGenerator.BackOfficeAssetsPath)
            .Replace(Constants.Web.CacheBusterToken, backOfficePathGenerator.BackOfficeCacheBustHash);

        return html.Raw(importmapScript);
    }

    /// <summary>
    ///     Outputs a script tag containing the import map for the BackOffice.
    /// </summary>
    /// <remarks>
    ///     It will replace the token %CACHE_BUSTER% with the cache buster hash.
    ///     It will also replace the /umbraco/backoffice path with the correct path for the BackOffice assets.
    /// </remarks>
    /// <returns>A <see cref="Task"/> containing the html content for the BackOffice import map.</returns>
    [Obsolete("Use the overload accepting ICspNonceService. Scheduled for removal in Umbraco 19.")]
    public static async Task<IHtmlContent> BackOfficeImportMapScriptAsync(
        this IHtmlHelper html,
        IJsonSerializer jsonSerializer,
        IBackOfficePathGenerator backOfficePathGenerator,
        IPackageManifestService packageManifestService)
        => await BackOfficeImportMapScriptAsync(
            html,
            jsonSerializer,
            backOfficePathGenerator,
            packageManifestService,
            StaticServiceProvider.Instance.GetRequiredService<ICspNonceService>());
}
