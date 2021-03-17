using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

namespace Umbraco.Cms.Web.Common.RuntimeMinification
{
    public class SmidgeRuntimeMinifier : IRuntimeMinifier
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfigManipulator _configManipulator;
        private readonly CacheBusterResolver _cacheBusterResolver;
        private readonly RuntimeMinificationSettings _runtimeMinificationSettings;
        private readonly IBundleManager _bundles;
        private readonly SmidgeHelperAccessor _smidge;

        // used only for minifying in MinifyAsync not for an actual pipeline
        private readonly Lazy<PreProcessPipeline> _jsMinPipeline;
        private readonly Lazy<PreProcessPipeline> _cssMinPipeline;

        // default pipelines for processing js/css files for the back office
        private readonly Lazy<PreProcessPipeline> _jsOptimizedPipeline;
        private readonly Lazy<PreProcessPipeline> _jsNonOptimizedPipeline;
        private readonly Lazy<PreProcessPipeline> _cssOptimizedPipeline;
        private readonly Lazy<PreProcessPipeline> _cssNonOptimizedPipeline;
        private ICacheBuster _cacheBuster;
        private readonly Type _cacheBusterType;

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
            _runtimeMinificationSettings = runtimeMinificationSettings.Value;
            _jsMinPipeline = new Lazy<PreProcessPipeline>(() => _bundles.PipelineFactory.Create(typeof(JsMinifier)));
            _cssMinPipeline = new Lazy<PreProcessPipeline>(() => _bundles.PipelineFactory.Create(typeof(NuglifyCss)));

            // replace the default JsMinifier with NuglifyJs and CssMinifier with NuglifyCss in the default pipelines
            // for use with our bundles only (not modifying global options)
            _jsOptimizedPipeline = new Lazy<PreProcessPipeline>(() => bundles.PipelineFactory.DefaultJs().Replace<JsMinifier, SmidgeNuglifyJs>(_bundles.PipelineFactory));
            _jsNonOptimizedPipeline = new Lazy<PreProcessPipeline>(() => bundles.PipelineFactory.DefaultJs());
            _cssOptimizedPipeline = new Lazy<PreProcessPipeline>(() => bundles.PipelineFactory.DefaultCss().Replace<CssMinifier, NuglifyCss>(_bundles.PipelineFactory));
            _cssNonOptimizedPipeline = new Lazy<PreProcessPipeline>(() => bundles.PipelineFactory.DefaultCss());

            Type cacheBusterType = _runtimeMinificationSettings.CacheBuster switch
            {
                RuntimeMinificationCacheBuster.AppDomain => typeof(AppDomainLifetimeCacheBuster),
                RuntimeMinificationCacheBuster.Version => typeof(ConfigCacheBuster),
                RuntimeMinificationCacheBuster.Timestamp => typeof(TimestampCacheBuster),
                _ => throw new NotImplementedException()
            };

            _cacheBusterType = cacheBusterType;
        }

        public string CacheBuster => (_cacheBuster ??= _cacheBusterResolver.GetCacheBuster(_cacheBusterType)).GetValue();

        // only issue with creating bundles like this is that we don't have full control over the bundle options, though that could
        public void CreateCssBundle(string bundleName, bool optimizeOutput, params string[] filePaths)
        {
            if (filePaths.Any(f => !f.StartsWith("/") && !f.StartsWith("~/")))
            {
                throw new InvalidOperationException("All file paths must be absolute");
            }

            if (_bundles.Exists(bundleName))
            {
                throw new InvalidOperationException($"The bundle name {bundleName} already exists");
            }

            if (optimizeOutput)
            {
                var bundle = _bundles.Create(bundleName, _cssOptimizedPipeline.Value, WebFileType.Css, filePaths)
                    .WithEnvironmentOptions(
                        BundleEnvironmentOptions.Create()
                            .ForDebug(builder => builder
                                // auto-invalidate bundle if files change in debug
                                .EnableFileWatcher()
                                // keep using composite files in debug, not raw static files
                                .EnableCompositeProcessing()
                                // use the cache buster defined in config
                                .SetCacheBusterType(_cacheBusterType))
                            .ForProduction(builder => builder
                                // use the cache buster defined in config
                                .SetCacheBusterType(_cacheBusterType))
                            .Build());
            }
            else
            {
                var bundle = _bundles.Create(bundleName, _cssNonOptimizedPipeline.Value, WebFileType.Css, filePaths)
                    .WithEnvironmentOptions(
                        BundleEnvironmentOptions.Create()
                            .ForDebug(builder => builder
                                // use the cache buster defined in config
                                .SetCacheBusterType(_cacheBusterType))
                            .ForProduction(builder => builder
                                // use the cache buster defined in config
                                .SetCacheBusterType(_cacheBusterType))
                            .Build());
            }            
        }

        public async Task<string> RenderCssHereAsync(string bundleName) => (await _smidge.SmidgeHelper.CssHereAsync(bundleName, _hostingEnvironment.IsDebugMode)).ToString();

        public void CreateJsBundle(string bundleName, bool optimizeOutput, params string[] filePaths)
        {
            if (filePaths.Any(f => !f.StartsWith("/") && !f.StartsWith("~/")))
            {
                throw new InvalidOperationException("All file paths must be absolute");
            }

            if (_bundles.Exists(bundleName))
            {
                throw new InvalidOperationException($"The bundle name {bundleName} already exists");
            }

            if (optimizeOutput)
            {
                var bundle = _bundles.Create(bundleName, _jsOptimizedPipeline.Value, WebFileType.Js, filePaths)
                    .WithEnvironmentOptions(
                        BundleEnvironmentOptions.Create()
                            .ForDebug(builder => builder
                                // auto-invalidate bundle if files change in debug
                                .EnableFileWatcher()
                                // keep using composite files in debug, not raw static files
                                .EnableCompositeProcessing()
                                // use the cache buster defined in config
                                .SetCacheBusterType(_cacheBusterType))
                            .ForProduction(builder => builder
                                // use the cache buster defined in config
                                .SetCacheBusterType(_cacheBusterType))
                            .Build());
            }
            else
            {
                var bundle = _bundles.Create(bundleName, _jsNonOptimizedPipeline.Value, WebFileType.Js, filePaths)
                    .WithEnvironmentOptions(
                        BundleEnvironmentOptions.Create()
                            .ForDebug(builder => builder
                                // use the cache buster defined in config
                                .SetCacheBusterType(_cacheBusterType))
                            .ForProduction(builder => builder
                                // use the cache buster defined in config
                                .SetCacheBusterType(_cacheBusterType))
                            .Build());
            }
        }

        public async Task<string> RenderJsHereAsync(string bundleName) => (await _smidge.SmidgeHelper.JsHereAsync(bundleName, _hostingEnvironment.IsDebugMode)).ToString();

        public async Task<IEnumerable<string>> GetAssetPathsAsync(string bundleName) => await _smidge.SmidgeHelper.GenerateJsUrlsAsync(bundleName, _hostingEnvironment.IsDebugMode);

        /// <inheritdoc />
        public async Task<string> MinifyAsync(string fileContent, AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.Javascript:
                    return await _jsMinPipeline.Value
                        .ProcessAsync(
                            new FileProcessContext(fileContent, new JavaScriptFile(), BundleContext.CreateEmpty()));
                case AssetType.Css:
                    return await _cssMinPipeline.Value
                        .ProcessAsync(new FileProcessContext(fileContent, new CssFile(), BundleContext.CreateEmpty()));
                default:
                    throw new NotSupportedException("Unexpected AssetType");
            }
        }


        /// <inheritdoc />
        /// <remarks>
        /// Smidge uses the version number as cache buster (configurable).
        /// We therefore can reset, by updating the version number in config
        /// </remarks>
        public void Reset()
        {
            var version = DateTime.UtcNow.Ticks.ToString();
            _configManipulator.SaveConfigValue(Cms.Core.Constants.Configuration.ConfigRuntimeMinificationVersion, version.ToString());
        }


    }
}
