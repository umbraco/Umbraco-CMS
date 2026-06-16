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
    /// <see cref="PackageManifest.Version"/> (falling back to <paramref name="globalHash"/> only when the package has no
    /// version). An explicit <c>%CACHE_BUSTER%</c> token (at any depth in the extension) always resolves to that hash.
    /// When <see cref="PackageManifest.AllowCacheBusting"/> is enabled, clean <c>/App_Plugins</c>-rooted
    /// <c>.js</c>/<c>.mjs</c> URLs are additionally stamped with <c>?umb__rnd=&lt;hash&gt;</c>; disabling it only turns
    /// off that automatic stamping (the token still resolves). URLs that already carry a query string are left
    /// untouched. The models are expected to be in the same order as <paramref name="manifests"/>.
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

        // Avoid the parse/walk entirely when there is nothing to do: no explicit %CACHE_BUSTER% token to resolve and
        // (when auto-stamping) no /App_Plugins path to stamp. The /App_Plugins probe is deliberately broad and
        // case-insensitive — it only needs to avoid false negatives; the per-URL logic makes the precise decision.
        var hasToken = json.Contains(Constants.Web.CacheBusterToken, StringComparison.Ordinal);
        var hasAppPluginsPath = stamp && json.Contains(Constants.SystemDirectories.AppPlugins, StringComparison.OrdinalIgnoreCase);
        if (hasToken is false && hasAppPluginsPath is false)
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

        // Assign the mutated nodes straight onto the model; System.Text.Json serialises a JsonNode the same as the
        // original JsonElement, so there is no need to re-parse the tree back into object[] via Deserialize.
        model.Extensions = extensions.Cast<object>().ToArray();
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
                foreach (var key in obj.Select(property => property.Key).ToList())
                {
                    CacheBustChild(obj[key], hash, stamp, replacement => obj[key] = replacement);
                }

                break;

            case JsonArray array:
                for (var i = 0; i < array.Count; i++)
                {
                    var index = i;
                    CacheBustChild(array[index], hash, stamp, replacement => array[index] = replacement);
                }

                break;
        }
    }

    // A leaf string is cache-busted in place via <paramref name="replace"/>; any container is walked recursively.
    // Only leaf strings are reassigned — a JsonNode that already has a parent cannot be reassigned to its slot, so
    // containers are mutated in place rather than replaced.
    private static void CacheBustChild(JsonNode? child, string hash, bool stamp, Action<JsonNode?> replace)
    {
        if (child is JsonValue value && value.TryGetValue(out string? url))
        {
            replace(PackageManifestCacheBuster.ApplyCacheBust(url, hash, stamp));
        }
        else
        {
            CacheBustAssetUrls(child, hash, stamp);
        }
    }
}
