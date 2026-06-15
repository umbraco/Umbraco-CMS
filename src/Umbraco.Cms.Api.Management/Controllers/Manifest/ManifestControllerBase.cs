using System.Text.Json;
using System.Text.Json.Nodes;
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
        // Legacy contract: resolve an explicit %CACHE_BUSTER% token only, never auto-stamp.
        foreach (ManifestResponseModel model in models)
        {
            ApplyCacheBusting(model, cacheBustHash, stamp: false);
        }
    }

    /// <summary>
    /// Applies cache-busting to each model's extension asset URLs using a per-package hash derived from the package's
    /// <see cref="PackageManifest.Version"/> (falling back to <paramref name="globalHash"/>), honouring each package's
    /// <see cref="PackageManifest.AllowCacheBusting"/> opt-out. When busting is enabled every clean
    /// <c>/App_Plugins</c>-rooted URL (at any depth in the extension) is stamped with <c>?umb__rnd=&lt;hash&gt;</c> and an
    /// explicit <c>%CACHE_BUSTER%</c> token is resolved to the same hash; when disabled only the token is resolved (to
    /// <paramref name="globalHash"/>). URLs that already carry a query string are left untouched. The models are
    /// expected to be in the same order as <paramref name="manifests"/>.
    /// </summary>
    protected static void ReplaceCacheBusterTokens(
        IEnumerable<ManifestResponseModel> models, IEnumerable<PackageManifest> manifests, string globalHash)
    {
        foreach ((ManifestResponseModel model, PackageManifest manifest) in models.Zip(manifests))
        {
            (var hash, var stamp) = PackageManifestCacheBuster.ResolvePackageCacheBust(manifest, globalHash);
            ApplyCacheBusting(model, hash, stamp);
        }
    }

    private static void ApplyCacheBusting(ManifestResponseModel model, string hash, bool stamp)
    {
        if (model.Extensions.Length == 0)
        {
            return;
        }

        var json = JsonSerializer.Serialize(model.Extensions);

        // Nothing to do when auto-stamping is off and there is no explicit token to resolve.
        if (stamp is false && json.Contains(Constants.Web.CacheBusterToken, StringComparison.Ordinal) is false)
        {
            return;
        }

        if (JsonNode.Parse(json) is not JsonArray extensions)
        {
            return;
        }

        foreach (JsonNode? extension in extensions)
        {
            CacheBustAssetUrls(extension, hash, stamp);
        }

        model.Extensions = extensions.Deserialize<object[]>() ?? model.Extensions;
    }

    // Walks every string value in the extension tree (at any depth) and applies cache-busting per URL.
    // PackageManifestCacheBuster.ApplyCacheBust resolves an explicit %CACHE_BUSTER% token, stamps clean
    // /App_Plugins paths when enabled, and leaves everything else — bare specifiers, CDNs, the backoffice core,
    // and author-managed query strings — untouched.
    private static void CacheBustAssetUrls(JsonNode? node, string hash, bool stamp)
    {
        switch (node)
        {
            case JsonObject obj:
            {
                foreach (var key in obj.Select(property => property.Key).ToList())
                {
                    if (obj[key] is JsonValue value && value.TryGetValue(out string? url))
                    {
                        obj[key] = PackageManifestCacheBuster.ApplyCacheBust(url, hash, stamp);
                    }
                    else
                    {
                        CacheBustAssetUrls(obj[key], hash, stamp);
                    }
                }

                break;
            }

            case JsonArray array:
            {
                for (var i = 0; i < array.Count; i++)
                {
                    if (array[i] is JsonValue value && value.TryGetValue(out string? url))
                    {
                        array[i] = PackageManifestCacheBuster.ApplyCacheBust(url, hash, stamp);
                    }
                    else
                    {
                        CacheBustAssetUrls(array[i], hash, stamp);
                    }
                }

                break;
            }
        }
    }
}
