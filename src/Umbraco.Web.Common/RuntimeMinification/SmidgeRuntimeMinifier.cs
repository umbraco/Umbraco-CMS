using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smidge;
using Smidge.CompositeFiles;
using Smidge.FileProcessors;
using Smidge.Models;
using Smidge.Nuglify;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.WebAssets;
using CssFile = Smidge.Models.CssFile;
using JavaScriptFile = Smidge.Models.JavaScriptFile;

namespace Umbraco.Web.Common.RuntimeMinification
{
    public class SmidgeRuntimeMinifier : IRuntimeMinifier
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISmidgeConfig _smidgeConfig;
        private readonly IConfigManipulator _configManipulator;
        private readonly IBundleManager _bundles;
        private readonly SmidgeHelperAccessor _smidge;

        // used only for minifying in MinifyAsync not for an actual pipeline
        private Lazy<PreProcessPipeline> _jsMinPipeline;
        private Lazy<PreProcessPipeline> _cssMinPipeline;

        // default pipelines for processing js/css files for the back office
        private Lazy<PreProcessPipeline> _jsPipeline;
        private Lazy<PreProcessPipeline> _cssPipeline;

        public SmidgeRuntimeMinifier(
            IBundleManager bundles,
            SmidgeHelperAccessor smidge,
            IHostingEnvironment hostingEnvironment,
            ISmidgeConfig smidgeConfig,
            IConfigManipulator configManipulator)
        {
            _bundles = bundles;
            _smidge = smidge;
            _hostingEnvironment = hostingEnvironment;
            _smidgeConfig = smidgeConfig;
            _configManipulator = configManipulator;

            _jsMinPipeline = new Lazy<PreProcessPipeline>(() => _bundles.PipelineFactory.Create(typeof(JsMinifier)));
            _cssMinPipeline = new Lazy<PreProcessPipeline>(() => _bundles.PipelineFactory.Create(typeof(NuglifyCss)));

            // replace the default JsMinifier with NuglifyJs and CssMinifier with NuglifyCss in the default pipelines
            // for use with our bundles only (not modifying global options)
            _jsPipeline = new Lazy<PreProcessPipeline>(() => bundles.PipelineFactory.DefaultJs().Replace<JsMinifier, SmidgeNuglifyJs>(_bundles.PipelineFactory));
            _cssPipeline = new Lazy<PreProcessPipeline>(() => bundles.PipelineFactory.DefaultCss().Replace<CssMinifier, NuglifyCss>(_bundles.PipelineFactory));

        }

        public string CacheBuster => _smidgeConfig.Version;

        // only issue with creating bundles like this is that we don't have full control over the bundle options, though that could
        public void CreateCssBundle(string bundleName, params string[] filePaths)
        {
            if (filePaths.Any(f => !f.StartsWith("/") && !f.StartsWith("~/")))
                throw new InvalidOperationException("All file paths must be absolute");

            if (_bundles.Exists(bundleName))
                throw new InvalidOperationException($"The bundle name {bundleName} already exists");

            // Here we could configure bundle options instead of using smidge's global defaults.
            // For example we can use our own custom cache buster for this bundle without having the global one
            // affect this or vice versa.
            var bundle = _bundles.Create(bundleName, _cssPipeline.Value, WebFileType.Css, filePaths);
        }

        public async Task<string> RenderCssHereAsync(string bundleName) => (await _smidge.SmidgeHelper.CssHereAsync(bundleName, _hostingEnvironment.IsDebugMode)).ToString();

        public void CreateJsBundle(string bundleName, bool optimizeOutput, params string[] filePaths)
        {
            if (filePaths.Any(f => !f.StartsWith("/") && !f.StartsWith("~/")))
                throw new InvalidOperationException("All file paths must be absolute");

            if (_bundles.Exists(bundleName))
                throw new InvalidOperationException($"The bundle name {bundleName} already exists");

            // Here we could configure bundle options instead of using smidge's global defaults.
            // For example we can use our own custom cache buster for this bundle without having the global one
            // affect this or vice versa.
            var pipeline = optimizeOutput ? _jsPipeline.Value : _bundles.PipelineFactory.Create();
            var bundle = _bundles.Create(bundleName, pipeline, WebFileType.Js, filePaths);
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
            _configManipulator.SaveConfigValue(Core.Constants.Configuration.ConfigRuntimeMinificationVersion, version.ToString());
        }


    }
}
