using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Manifest;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Cms.Api.Management.Controllers.Manifest;

/// <summary>
/// Serves as the base controller for manifest operations in the management API.
/// </summary>
[VersionedApiBackOfficeRoute("manifest")]
[ApiExplorerSettings(GroupName = "Manifest")]
public abstract class ManifestControllerBase : ManagementApiControllerBase
{
    [Obsolete("Use the overload that accepts the source package manifests so the cache-buster can be resolved per package. Scheduled for removal in Umbraco 19.")]
    protected static void ReplaceCacheBusterTokens(
        IEnumerable<ManifestResponseModel> models, string cacheBustHash)
    {
        foreach (ManifestResponseModel model in models)
        {
            ReplaceCacheBusterToken(model, cacheBustHash);
        }
    }

    /// <summary>
    /// Resolves the <c>%CACHE_BUSTER%</c> token in each model's extensions to a per-package hash derived from the
    /// package's <see cref="PackageManifest.Version"/> (falling back to <paramref name="globalHash"/>), honouring each
    /// package's <see cref="PackageManifest.AllowCacheBusting"/> opt-out (when disabled, the token resolves to
    /// <paramref name="globalHash"/>). The models are expected to be in the same order as <paramref name="manifests"/>.
    /// </summary>
    protected static void ReplaceCacheBusterTokens(
        IEnumerable<ManifestResponseModel> models, IEnumerable<PackageManifest> manifests, string globalHash)
    {
        foreach ((ManifestResponseModel model, PackageManifest manifest) in models.Zip(manifests))
        {
            var hash = manifest.AllowCacheBusting
                ? PackageManifestCacheBuster.ResolvePackageCacheBustHash(manifest.Version, globalHash)
                : globalHash;

            ReplaceCacheBusterToken(model, hash);
        }
    }

    private static void ReplaceCacheBusterToken(ManifestResponseModel model, string hash)
    {
        if (model.Extensions.Length == 0)
        {
            return;
        }

        var json = JsonSerializer.Serialize(model.Extensions);
        if (json.Contains(Constants.Web.CacheBusterToken) is false)
        {
            return;
        }

        json = json.Replace(Constants.Web.CacheBusterToken, JsonEncodedText.Encode(hash).ToString());
        model.Extensions = JsonSerializer.Deserialize<object[]>(json) ?? model.Extensions;
    }
}
