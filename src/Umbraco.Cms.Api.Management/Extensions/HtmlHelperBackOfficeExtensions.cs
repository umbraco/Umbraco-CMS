using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Manifest;
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
    /// </remarks>
    /// <returns>A <see cref="Task"/> containing the html content for the BackOffice import map.</returns>
    public static async Task<IHtmlContent> BackOfficeImportMapScriptAsync(
        this IHtmlHelper html,
        IJsonSerializer jsonSerializer,
        IBackOfficePathGenerator backOfficePathGenerator,
        IPackageManifestService packageManifestService)
    {
        try
        {
            PackageManifestImportmap packageImports = await packageManifestService.GetPackageManifestImportmapAsync();

            var sb = new StringBuilder();
            sb.AppendLine("""<script type="importmap">""");
            sb.AppendLine(jsonSerializer.Serialize(packageImports));
            sb.AppendLine("</script>");

            // Inject the BackOffice cache buster into the import string to handle BackOffice assets
            var importmapScript = sb.ToString()
                .Replace(backOfficePathGenerator.BackOfficeVirtualDirectory, backOfficePathGenerator.BackOfficeAssetsPath)
                .Replace(Constants.Web.CacheBusterToken, backOfficePathGenerator.BackOfficeCacheBustHash);

            return html.Raw(importmapScript);
        }
        catch (NotSupportedException ex)
        {
            throw new NotSupportedException("Failed to serialize the BackOffice import map", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to generate the BackOffice import map", ex);
        }
    }
}
