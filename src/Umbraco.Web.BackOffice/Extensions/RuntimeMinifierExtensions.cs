using System.Text;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Infrastructure.WebAssets;

namespace Umbraco.Extensions;

public static class RuntimeMinifierExtensions
{
    /// <summary>
    ///     Returns the JavaScript to load the back office's assets
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetScriptForLoadingBackOfficeAsync(
        this IRuntimeMinifier minifier,
        GlobalSettings globalSettings,
        IHostingEnvironment hostingEnvironment,
        ILegacyManifestParser legacyManifestParser)
    {
        var files = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var file in await minifier.GetJsAssetPathsAsync(BackOfficeWebAssets.UmbracoCoreJsBundleName))
        {
            files.Add(file);
        }

        foreach (var file in await minifier.GetJsAssetPathsAsync(BackOfficeWebAssets.UmbracoExtensionsJsBundleName))
        {
            files.Add(file);
        }

        // process the independent bundles
        if (legacyManifestParser.CombinedManifest.Scripts.TryGetValue(BundleOptions.Independent,
                out IReadOnlyList<LegacyManifestAssets>? independentManifestAssetsList))
        {
            foreach (LegacyManifestAssets manifestAssets in independentManifestAssetsList)
            {
                var bundleName =
                    BackOfficeWebAssets.GetIndependentPackageBundleName(manifestAssets, AssetType.Javascript);
                foreach (var asset in await minifier.GetJsAssetPathsAsync(bundleName))
                {
                    files.Add(asset);
                }
            }
        }

        // process the "None" bundles, meaning we'll just render the script as-is
        foreach (var asset in await minifier.GetJsAssetPathsAsync(BackOfficeWebAssets
                     .UmbracoNonOptimizedPackageJsBundleName))
        {
            files.Add(asset);
        }

        var result = BackOfficeJavaScriptInitializer.GetJavascriptInitialization(
            files,
            "umbraco",
            globalSettings,
            hostingEnvironment);

        result += await GetStylesheetInitializationAsync(minifier, legacyManifestParser);

        return result;
    }

    /// <summary>
    ///     Gets the back office css bundle paths and formats a JS call to lazy load them
    /// </summary>
    private static async Task<string> GetStylesheetInitializationAsync(
        IRuntimeMinifier minifier,
        ILegacyManifestParser legacyManifestParser)
    {
        var files = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var file in await minifier.GetCssAssetPathsAsync(BackOfficeWebAssets.UmbracoCssBundleName))
        {
            files.Add(file);
        }

        // process the independent bundles
        if (legacyManifestParser.CombinedManifest.Stylesheets.TryGetValue(BundleOptions.Independent,
                out IReadOnlyList<LegacyManifestAssets>? independentManifestAssetsList))
        {
            foreach (LegacyManifestAssets manifestAssets in independentManifestAssetsList)
            {
                var bundleName = BackOfficeWebAssets.GetIndependentPackageBundleName(manifestAssets, AssetType.Css);
                foreach (var asset in await minifier.GetCssAssetPathsAsync(bundleName))
                {
                    files.Add(asset);
                }
            }
        }

        // process the "None" bundles, meaning we'll just render the script as-is
        foreach (var asset in await minifier.GetCssAssetPathsAsync(BackOfficeWebAssets
                     .UmbracoNonOptimizedPackageCssBundleName))
        {
            files.Add(asset);
        }

        var sb = new StringBuilder();
        foreach (var file in files)
        {
            sb.AppendFormat("{0}LazyLoad.css('{1}');", Environment.NewLine, file);
        }

        return sb.ToString();
    }
}
