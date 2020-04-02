using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smidge;
using Smidge.CompositeFiles;
using Smidge.FileProcessors;
using Smidge.Models;
using Smidge.Nuglify;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.WebAssets;
using Umbraco.Web.JavaScript;
using CssFile = Smidge.Models.CssFile;
using JavaScriptFile = Smidge.Models.JavaScriptFile;

namespace Umbraco.Web.Common.RuntimeMinification
{
    public class SmidgeRuntimeMinifier : IRuntimeMinifier
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IIOHelper _ioHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISmidgeConfig _smidgeConfig;
        private readonly IConfigManipulator _configManipulator;
        private readonly PreProcessPipelineFactory _preProcessPipelineFactory;
        private readonly BundleManager _bundles;
        private readonly SmidgeHelper _smidge;

        private PreProcessPipeline _jsPipeline;
        private PreProcessPipeline _cssPipeline;

        public SmidgeRuntimeMinifier(
            BundleManager bundles,
            SmidgeHelper smidge,
            PreProcessPipelineFactory preProcessPipelineFactory,
            IGlobalSettings globalSettings,
            IIOHelper ioHelper,
            IHostingEnvironment hostingEnvironment,
            ISmidgeConfig smidgeConfig,
            IConfigManipulator configManipulator)
        {
            _bundles = bundles;
            _smidge = smidge;
            _preProcessPipelineFactory = preProcessPipelineFactory;
            _globalSettings = globalSettings;
            _ioHelper = ioHelper;
            _hostingEnvironment = hostingEnvironment;
            _smidgeConfig = smidgeConfig;
            _configManipulator = configManipulator;
        }

        private PreProcessPipeline JsPipeline => _jsPipeline ??= _preProcessPipelineFactory.Create(typeof(JsMinifier));
        private PreProcessPipeline CssPipeline => _cssPipeline ??= _preProcessPipelineFactory.Create(typeof(NuglifyCss));

        public string CacheBuster => _smidgeConfig.Version;

        // only issue with creating bundles like this is that we don't have full control over the bundle options, though that could 
        public void CreateCssBundle(string bundleName, params string[] filePaths)
        {
            if (_bundles.Exists(bundleName))
                throw new InvalidOperationException($"The bundle name {bundleName} already exists");

            var bundle = _bundles.Create(bundleName, WebFileType.Css, filePaths);

            // Here we could configure bundle options instead of using smidge's global defaults.
            // For example we can use our own custom cache buster for this bundle without having the global one
            // affect this or vice versa.
        }   

        public string RenderCssHere(string bundleName) => _smidge.CssHereAsync(bundleName, _hostingEnvironment.IsDebugMode).ToString();

        public void CreateJsBundle(string bundleName, params string[] filePaths)
        {
            if (_bundles.Exists(bundleName))
                throw new InvalidOperationException($"The bundle name {bundleName} already exists");

            var bundle = _bundles.Create(bundleName, WebFileType.Js, filePaths);

            // Here we could configure bundle options instead of using smidge's global defaults.
            // For example we can use our own custom cache buster for this bundle without having the global one
            // affect this or vice versa.
        }

        public string RenderJsHere(string bundleName) => _smidge.JsHereAsync(bundleName, _hostingEnvironment.IsDebugMode).ToString();

        public async Task<IEnumerable<string>> GetAssetPathsAsync(string bundleName) => await _smidge.GenerateJsUrlsAsync(bundleName, _hostingEnvironment.IsDebugMode);

        public async Task<string> MinifyAsync(string fileContent, AssetType assetType)
        {
            if (assetType == AssetType.Javascript)
            {
                return await JsPipeline
                    .ProcessAsync(
                        new FileProcessContext(fileContent, new JavaScriptFile(), BundleContext.CreateEmpty()));
            }
            else
            {
                return await CssPipeline
                    .ProcessAsync(new FileProcessContext(fileContent, new CssFile(), BundleContext.CreateEmpty()));
            }
        }


        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// Smidge uses the version number as cache buster (configurable).
        /// We therefore can reset, by updating the version number in config
        /// </remarks>
        public void Reset()
        {
            var version = DateTime.UtcNow.Ticks.ToString();
            _configManipulator.SaveConfigValue(Constants.Configuration.ConfigRuntimeMinificationVersion, version.ToString());
        }


    }
}
