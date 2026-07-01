using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Manifest;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Manifest;

/// <summary>
/// Serves as the base controller for manifest operations in the management API.
/// </summary>
[VersionedApiBackOfficeRoute("manifest")]
[ApiExplorerSettings(GroupName = "Manifest")]
public abstract class ManifestControllerBase : ManagementApiControllerBase
{
    /// <summary>
    ///     Replaces the {Constants.Web.CacheBusterToken} with the supplied cache buster hash.
    /// </summary>
    /// <param name="models">The collection of manifest response models.</param>
    /// <param name="cacheBustHash">The cache buster hash to replace the token with.</param>
    [Obsolete("You do not need to use this anymore, as cache busting is now appended automatically by the BackOffice client. Scheduled for removal in Umbraco 20.")]
    protected static void ReplaceCacheBusterTokens(
        IEnumerable<ManifestResponseModel> models, string cacheBustHash)
    {
        foreach (ManifestResponseModel model in models)
        {
            if (model.Extensions.Length == 0)
            {
                continue;
            }

            var json = JsonSerializer.Serialize(model.Extensions);
            if (json.Contains(Constants.Web.CacheBusterToken) is false)
            {
                continue;
            }

            json = json.Replace(Constants.Web.CacheBusterToken, JsonEncodedText.Encode(cacheBustHash).ToString());
            model.Extensions = JsonSerializer.Deserialize<object[]>(json) ?? model.Extensions;
        }
    }
}
