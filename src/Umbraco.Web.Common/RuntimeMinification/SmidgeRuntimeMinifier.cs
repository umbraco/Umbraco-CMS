using Microsoft.Extensions.Options;
using Smidge;
using Smidge.Cache;
using Smidge.CompositeFiles;
using Smidge.FileProcessors;
using Smidge.Models;
using Smidge.Nuglify;
using Smidge.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.WebAssets;
using CssFile = Smidge.Models.CssFile;
using JavaScriptFile = Smidge.Models.JavaScriptFile;

namespace Umbraco.Cms.Web.Common.RuntimeMinification;

public class SmidgeRuntimeMinifier : IRuntimeMinifier
{
    private readonly IBundleManager _bundles;
    private readonly CacheBusterResolver _cacheBusterResolver;
    private readonly Type _cacheBusterType;
    private readonly IConfigManipulator _configManipulator;
    private readonly Lazy<PreProcessPipeline> _cssMinPipeline;
    private readonly Lazy<PreProcessPipeline> _cssNonOptimizedPipeline;
    private readonly Lazy<PreProcessPipeline> _cssOptimizedPipeline;
    private readonly IHostingEnvironment _hostingEnvironment;

    // used only for minifying in MinifyAsync not for an actual pipeline
    private readonly Lazy<PreProcessPipeline> _jsMinPipeline;
    private readonly Lazy<PreProcessPipeline> _jsNonOptimizedPipeline;

    // default pipelines for processing js/css files for the back office
    private readonly Lazy<PreProcessPipeline> _jsOptimizedPipeline;
    private readonly SmidgeHelperAccessor _smidge;
    private ICacheBuster? _cacheBuster;

    public SmidgeRuntimeMinifier(
        IBundleManager bundles,
        SmidgeHelperAccessor smidge,
        IHostingEnvironment hostingEnvironment,
        IConfigManipulator configManipulator,
        IOptions<RuntimeMinificationSettings> runtimeMinificationSettings,
        CacheBusterResolver cacheBusterResolver)
    {
        _bundles = bundles;
        _smidge = smidge;
        _hostingEnvironment = hostingEnvironment;
        _configManipulator = configManipulator;
        _cacheBusterResolver = cacheBusterResolver;
        _jsMinPipeline = new Lazy<PreProcessPipeline>(() => _bundles.PipelineFactory.Create(typeof(JsMinifier)));
        _cssMinPipeline = new Lazy<PreProcessPipeline>(() => _bundles.PipelineFactory.Create(typeof(NuglifyCss)));

        // replace the default JsMinifier with NuglifyJs and CssMinifier with NuglifyCss in the default pipelines
        // for use with our bundles only (not modifying global options)
        _jsOptimizedPipeline = new Lazy<PreProcessPipeline>(() =>
            bundles.PipelineFactory.DefaultJs().Replace<JsMinifier, SmidgeNuglifyJs>(_bundles.PipelineFactory));
        _jsNonOptimizedPipeline = new Lazy<PreProcessPipeline>(() =>
        {
            PreProcessPipeline defaultJs = bundles.PipelineFactory.DefaultJs();

            // remove minification from this pipeline
            defaultJs.Processors.RemoveAll(x => x is JsMinifier);
            return defaultJs;
        });
        _cssOptimizedPipeline = new Lazy<PreProcessPipeline>(() =>
            bundles.PipelineFactory.DefaultCss().Replace<CssMinifier, NuglifyCss>(_bundles.PipelineFactory));
        _cssNonOptimizedPipeline = new Lazy<PreProcessPipeline>(() =>
        {
            PreProcessPipeline defaultCss = bundles.PipelineFactory.DefaultCss();

            // remove minification from this pipeline
            defaultCss.Processors.RemoveAll(x => x is CssMinifier);
            return defaultCss;
        });

        Type cacheBusterType = runtimeMinificationSettings.Value.CacheBuster switch
        {
            RuntimeMinificationCacheBuster.AppDomain => typeof(AppDomainLifetimeCacheBuster),
            RuntimeMinificationCacheBuster.Version => typeof(UmbracoSmidgeConfigCacheBuster),
            RuntimeMinificationCacheBuster.Timestamp => typeof(TimestampCacheBuster),
            _ => throw new NotImplementedException(),
        };

        _cacheBusterType = cacheBusterType;
    }

    public string CacheBuster => (_cacheBuster ??= _cacheBusterResolver.GetCacheBuster(_cacheBusterType)).GetValue();

    // only issue with creating bundles like this is that we don't have full control over the bundle options, though that could
    public void CreateCssBundle(string bundleName, BundlingOptions bundleOptions, params string[]? filePaths)
    {
        if (filePaths?.Any(f => !f.StartsWith("/") && !f.StartsWith("~/")) ?? false)
        {
            throw new InvalidOperationException("All file paths must be absolute");
        }

        if (_bundles.Exists(bundleName))
        {
            throw new InvalidOperationException($"The bundle name {bundleName} already exists");
        }

        PreProcessPipeline pipeline = bundleOptions.OptimizeOutput
            ? _cssOptimizedPipeline.Value
            : _cssNonOptimizedPipeline.Value;

        Bundle bundle = _bundles.Create(bundleName, pipeline, WebFileType.Css, filePaths);
        bundle.WithEnvironmentOptions(ConfigureBundleEnvironmentOptions(bundleOptions));
    }

    public async Task<string> RenderCssHereAsync(string bundleName) =>
        (await _smidge.SmidgeHelper.CssHereAsync(bundleName, _hostingEnvironment.IsDebugMode)).ToString();

    public void CreateJsBundle(string bundleName, BundlingOptions bundleOptions, params string[]? filePaths)
    {
        if (filePaths?.Any(f => !f.StartsWith("/") && !f.StartsWith("~/")) ?? false)
        {
            throw new InvalidOperationException("All file paths must be absolute");
        }

        if (_bundles.Exists(bundleName))
        {
            throw new InvalidOperationException($"The bundle name {bundleName} already exists");
        }

        PreProcessPipeline pipeline = bundleOptions.OptimizeOutput
            ? _jsOptimizedPipeline.Value
            : _jsNonOptimizedPipeline.Value;

        Bundle bundle = _bundles.Create(bundleName, pipeline, WebFileType.Js, filePaths);
        bundle.WithEnvironmentOptions(ConfigureBundleEnvironmentOptions(bundleOptions));
    }

    public async Task<string> RenderJsHereAsync(string bundleName) =>
        (await _smidge.SmidgeHelper.JsHereAsync(bundleName, _hostingEnvironment.IsDebugMode)).ToString();

    public async Task<IEnumerable<string>> GetJsAssetPathsAsync(string bundleName) =>
        await _smidge.SmidgeHelper.GenerateJsUrlsAsync(bundleName, _hostingEnvironment.IsDebugMode);

    public async Task<IEnumerable<string>> GetCssAssetPathsAsync(string bundleName) =>
        await _smidge.SmidgeHelper.GenerateCssUrlsAsync(bundleName, _hostingEnvironment.IsDebugMode);

    /// <inheritdoc />
    public async Task<string> MinifyAsync(string? fileContent, AssetType assetType)
    {
        switch (assetType)
        {
            case AssetType.Javascript:
                return await _jsMinPipeline.Value
                    .ProcessAsync(
                        new FileProcessContext(fileContent, new JavaScriptFile(), BundleContext.CreateEmpty(CacheBuster)));
            case AssetType.Css:
                return await _cssMinPipeline.Value
                    .ProcessAsync(new FileProcessContext(fileContent, new CssFile(), BundleContext.CreateEmpty(CacheBuster)));
            default:
                throw new NotSupportedException("Unexpected AssetType");
        }
    }

    /// <inheritdoc />
    [Obsolete("Invalidation is handled automatically. Scheduled for removal V11.")]
    public void Reset()
    {
        var version = DateTime.UtcNow.Ticks.ToString();
        _configManipulator.SaveConfigValue(Core.Constants.Configuration.ConfigRuntimeMinificationVersion, version);
    }

    private BundleEnvironmentOptions ConfigureBundleEnvironmentOptions(BundlingOptions bundleOptions)
    {
        var bundleEnvironmentOptions = new BundleEnvironmentOptions();

        // auto-invalidate bundle if files change in debug
        bundleEnvironmentOptions.DebugOptions.FileWatchOptions.Enabled = true;

        // set cache busters
        bundleEnvironmentOptions.DebugOptions.SetCacheBusterType(_cacheBusterType);
        bundleEnvironmentOptions.ProductionOptions.SetCacheBusterType(_cacheBusterType);

        // config if the files should be combined
        bundleEnvironmentOptions.ProductionOptions.ProcessAsCompositeFile = bundleOptions.EnabledCompositeFiles;

        return bundleEnvironmentOptions;
    }
}
