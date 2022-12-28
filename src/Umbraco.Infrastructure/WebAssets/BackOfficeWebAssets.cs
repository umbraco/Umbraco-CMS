using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.WebAssets;

public class BackOfficeWebAssets
{
    public const string UmbracoPreviewJsBundleName = "umbraco-preview-js";
    public const string UmbracoPreviewCssBundleName = "umbraco-preview-css";
    public const string UmbracoCssBundleName = "umbraco-backoffice-css";
    public const string UmbracoInitCssBundleName = "umbraco-backoffice-init-css";
    public const string UmbracoCoreJsBundleName = "umbraco-backoffice-js";
    public const string UmbracoExtensionsJsBundleName = "umbraco-backoffice-extensions-js";
    public const string UmbracoNonOptimizedPackageJsBundleName = "umbraco-backoffice-non-optimized-js";
    public const string UmbracoNonOptimizedPackageCssBundleName = "umbraco-backoffice-non-optimized-css";
    public const string UmbracoTinyMceJsBundleName = "umbraco-tinymce-js";
    public const string UmbracoUpgradeCssBundleName = "umbraco-authorize-upgrade-css";
    private readonly CustomBackOfficeAssetsCollection _customBackOfficeAssetsCollection;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IManifestParser _parser;
    private readonly PropertyEditorCollection _propertyEditorCollection;

    private readonly IRuntimeMinifier _runtimeMinifier;
    private GlobalSettings _globalSettings;

    public BackOfficeWebAssets(
        IRuntimeMinifier runtimeMinifier,
        IManifestParser parser,
        PropertyEditorCollection propertyEditorCollection,
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<GlobalSettings> globalSettings,
        CustomBackOfficeAssetsCollection customBackOfficeAssetsCollection)
    {
        _runtimeMinifier = runtimeMinifier;
        _parser = parser;
        _propertyEditorCollection = propertyEditorCollection;
        _hostingEnvironment = hostingEnvironment;
        _globalSettings = globalSettings.CurrentValue;
        _customBackOfficeAssetsCollection = customBackOfficeAssetsCollection;

        globalSettings.OnChange(x => _globalSettings = x);
    }

    public static string GetIndependentPackageBundleName(ManifestAssets manifestAssets, AssetType assetType)
        => $"{manifestAssets.PackageName.ToLowerInvariant()}-{(assetType == AssetType.Css ? "css" : "js")}";

    public void CreateBundles()
    {
        // Create bundles
        _runtimeMinifier.CreateCssBundle(
            UmbracoInitCssBundleName,
            BundlingOptions.NotOptimizedAndComposite,
            FormatPaths(
                "assets/css/umbraco.min.css",
                "lib/umbraco-ui/uui-css/dist/custom-properties.css",
                "lib/umbraco-ui/uui-css/dist/uui-text.css",
                "lib/bootstrap-social/bootstrap-social.css",
                "lib/font-awesome/css/font-awesome.min.css"));

        _runtimeMinifier.CreateCssBundle(
            UmbracoUpgradeCssBundleName,
            BundlingOptions.NotOptimizedAndComposite,
            FormatPaths(
                "assets/css/umbraco.min.css",
                "lib/bootstrap-social/bootstrap-social.css",
                "lib/font-awesome/css/font-awesome.min.css"));

        _runtimeMinifier.CreateCssBundle(
            UmbracoPreviewCssBundleName,
            BundlingOptions.NotOptimizedAndComposite,
            FormatPaths("assets/css/canvasdesigner.min.css"));

        _runtimeMinifier.CreateJsBundle(
            UmbracoPreviewJsBundleName,
            BundlingOptions.NotOptimizedAndComposite,
            FormatPaths(GetScriptsForPreview()));

        _runtimeMinifier.CreateJsBundle(
            UmbracoTinyMceJsBundleName,
            BundlingOptions.NotOptimizedAndComposite,
            FormatPaths(GetScriptsForTinyMce()));

        _runtimeMinifier.CreateJsBundle(
            UmbracoCoreJsBundleName,
            BundlingOptions.NotOptimizedAndComposite,
            FormatPaths(GetScriptsForBackOfficeCore()));

        // get the property editor assets
        var propertyEditorAssets = ScanPropertyEditors()
            .GroupBy(x => x.AssetType)
            .ToDictionary(x => x.Key, x => x.Select(c => c.FilePath));

        // get the back office custom assets
        var customAssets = _customBackOfficeAssetsCollection.GroupBy(x => x.DependencyType)
            .ToDictionary(x => x.Key, x => x.Select(c => c.FilePath));

        // This bundle includes all scripts from property editor assets,
        // custom back office assets, and any scripts found in package manifests
        // that have the default bundle options.
        IEnumerable<string?> jsAssets =
            (customAssets.TryGetValue(AssetType.Javascript, out IEnumerable<string?>? customScripts)
                ? customScripts
                : Enumerable.Empty<string>())
            .Union(propertyEditorAssets.TryGetValue(AssetType.Javascript, out IEnumerable<string>? scripts)
                ? scripts
                : Enumerable.Empty<string>());

        _runtimeMinifier.CreateJsBundle(
            UmbracoExtensionsJsBundleName,
            BundlingOptions.OptimizedAndComposite,
            FormatPaths(
                GetScriptsForBackOfficeExtensions(jsAssets)));

        // Create a bundle per package manifest that is declaring an Independent bundle type
        RegisterPackageBundlesForIndependentOptions(_parser.CombinedManifest.Scripts, AssetType.Javascript);

        // Create a single non-optimized (no file processing) bundle for all manifests declaring None as a bundle option
        RegisterPackageBundlesForNoneOption(_parser.CombinedManifest.Scripts, UmbracoNonOptimizedPackageJsBundleName);

        // This bundle includes all CSS from property editor assets,
        // custom back office assets, and any CSS found in package manifests
        // that have the default bundle options.
        IEnumerable<string?> cssAssets =
            (customAssets.TryGetValue(AssetType.Css, out IEnumerable<string?>? customStyles)
                ? customStyles
                : Enumerable.Empty<string>())
            .Union(propertyEditorAssets.TryGetValue(AssetType.Css, out IEnumerable<string>? styles)
                ? styles
                : Enumerable.Empty<string>());

        _runtimeMinifier.CreateCssBundle(
            UmbracoCssBundleName,
            BundlingOptions.OptimizedAndComposite,
            FormatPaths(
                GetStylesheetsForBackOffice(cssAssets)));

        // Create a bundle per package manifest that is declaring an Independent bundle type
        RegisterPackageBundlesForIndependentOptions(_parser.CombinedManifest?.Stylesheets, AssetType.Css);

        // Create a single non-optimized (no file processing) bundle for all manifests declaring None as a bundle option
        RegisterPackageBundlesForNoneOption(
            _parser.CombinedManifest?.Stylesheets,
            UmbracoNonOptimizedPackageCssBundleName);
    }

    private void RegisterPackageBundlesForNoneOption(
        IReadOnlyDictionary<BundleOptions, IReadOnlyList<ManifestAssets>>? combinedPackageManifestAssets,
        string bundleName)
    {
        var assets = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        // Create a bundle per package manifest that is declaring the matching BundleOptions
        if (combinedPackageManifestAssets?.TryGetValue(
            BundleOptions.None,
            out IReadOnlyList<ManifestAssets>? manifestAssetList) ?? false)
        {
            foreach (var asset in manifestAssetList.SelectMany(x => x.Assets))
            {
                assets.Add(asset);
            }
        }

        _runtimeMinifier.CreateJsBundle(
            bundleName,

            // no optimization, no composite files, just render individual files
            BundlingOptions.NotOptimizedNotComposite,
            FormatPaths(assets.ToArray()));
    }

    private void RegisterPackageBundlesForIndependentOptions(
        IReadOnlyDictionary<BundleOptions, IReadOnlyList<ManifestAssets>>? combinedPackageManifestAssets,
        AssetType assetType)
    {
        // Create a bundle per package manifest that is declaring the matching BundleOptions
        if (combinedPackageManifestAssets?.TryGetValue(
            BundleOptions.Independent,
            out IReadOnlyList<ManifestAssets>? manifestAssetList) ?? false)
        {
            foreach (ManifestAssets manifestAssets in manifestAssetList)
            {
                var bundleName = GetIndependentPackageBundleName(manifestAssets, assetType);
                var filePaths = FormatPaths(manifestAssets.Assets.ToArray());

                switch (assetType)
                {
                    case AssetType.Javascript:
                        _runtimeMinifier.CreateJsBundle(bundleName, BundlingOptions.OptimizedAndComposite, filePaths);
                        break;
                    case AssetType.Css:
                        _runtimeMinifier.CreateCssBundle(bundleName, BundlingOptions.OptimizedAndComposite, filePaths);
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }

    /// <summary>
    ///     Returns scripts used to load the back office
    /// </summary>
    /// <returns></returns>
    private string[] GetScriptsForBackOfficeExtensions(IEnumerable<string?> propertyEditorScripts)
    {
        var scripts = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        // only include scripts with the default bundle options here
        if (_parser.CombinedManifest.Scripts.TryGetValue(
            BundleOptions.Default,
            out IReadOnlyList<ManifestAssets>? manifestAssets))
        {
            foreach (var script in manifestAssets.SelectMany(x => x.Assets))
            {
                scripts.Add(script);
            }
        }

        foreach (var script in propertyEditorScripts)
        {
            if (script is not null)
            {
                scripts.Add(script);
            }
        }

        return scripts.ToArray();
    }

    /// <summary>
    ///     Returns the list of scripts for back office initialization
    /// </summary>
    /// <returns></returns>
    private string[]? GetScriptsForBackOfficeCore()
    {
        JArray? resources = JsonConvert.DeserializeObject<JArray>(Resources.JsInitialize);
        return resources?.Where(x => x.Type == JTokenType.String).Select(x => x.ToString()).ToArray();
    }

    /// <summary>
    ///     Returns stylesheets used to load the back office
    /// </summary>
    /// <returns></returns>
    private string[] GetStylesheetsForBackOffice(IEnumerable<string?> propertyEditorStyles)
    {
        var stylesheets = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        // only include css with the default bundle options here
        if (_parser.CombinedManifest.Stylesheets.TryGetValue(
            BundleOptions.Default,
            out IReadOnlyList<ManifestAssets>? manifestAssets))
        {
            foreach (var script in manifestAssets.SelectMany(x => x.Assets))
            {
                stylesheets.Add(script);
            }
        }

        foreach (var stylesheet in propertyEditorStyles)
        {
            if (stylesheet is not null)
            {
                stylesheets.Add(stylesheet);
            }
        }

        return stylesheets.ToArray();
    }

    /// <summary>
    ///     Returns the scripts used for tinymce
    /// </summary>
    /// <returns></returns>
    private string[]? GetScriptsForTinyMce()
    {
        JArray? resources = JsonConvert.DeserializeObject<JArray>(Resources.TinyMceInitialize);
        return resources?.Where(x => x.Type == JTokenType.String).Select(x => x.ToString()).ToArray();
    }

    /// <summary>
    ///     Returns the scripts used for preview
    /// </summary>
    /// <returns></returns>
    private string[]? GetScriptsForPreview()
    {
        JArray? resources = JsonConvert.DeserializeObject<JArray>(Resources.PreviewInitialize);
        return resources?.Where(x => x.Type == JTokenType.String).Select(x => x.ToString()).ToArray();
    }

    /// <summary>
    ///     Re-format asset paths to be absolute paths
    /// </summary>
    /// <param name="assets"></param>
    /// <returns></returns>
    private string[]? FormatPaths(params string[]? assets)
    {
        var umbracoPath = _globalSettings.GetUmbracoMvcArea(_hostingEnvironment);

        return assets?
            .Where(x => x.IsNullOrWhiteSpace() == false)
            .Select(x => !x.StartsWith("/") && Uri.IsWellFormedUriString(x, UriKind.Relative)

                // most declarations with be made relative to the /umbraco folder, so things
                // like lib/blah/blah.js so we need to turn them into absolutes here
                ? umbracoPath.EnsureStartsWith('/').TrimEnd("/") + x.EnsureStartsWith('/')
                : x).ToArray();
    }

    /// <summary>
    ///     Returns the web asset paths to load for property editors that have the <see cref="PropertyEditorAssetAttribute" />
    ///     attribute applied
    /// </summary>
    /// <returns></returns>
    private IEnumerable<PropertyEditorAssetAttribute> ScanPropertyEditors() =>
        _propertyEditorCollection
            .SelectMany(x => x.GetType().GetCustomAttributes<PropertyEditorAssetAttribute>(false));
}
