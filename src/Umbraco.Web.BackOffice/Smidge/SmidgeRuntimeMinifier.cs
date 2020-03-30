using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Smidge;
using Smidge.CompositeFiles;
using Smidge.FileProcessors;
using Smidge.Nuglify;
using Umbraco.Core;
using Umbraco.Core.Assets;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Runtime;
using Umbraco.Web.JavaScript;
using CssFile = Smidge.Models.CssFile;
using JavaScriptFile = Smidge.Models.JavaScriptFile;

namespace Umbraco.Web.BackOffice.Smidge
{
    public class SmidgeRuntimeMinifier : IRuntimeMinifier
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIOHelper _ioHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISmidgeConfig _smidgeConfig;
        private readonly IManifestParser _manifestParser;
        private readonly PreProcessPipelineFactory _preProcessPipelineFactory;
        private readonly PropertyEditorCollection _propertyEditorCollection;
        private readonly SmidgeHelper _smidge;

        private PreProcessPipeline _jsPipeline;
        private PreProcessPipeline _cssPipeline;

        public SmidgeRuntimeMinifier(
            SmidgeHelper smidge,
            PreProcessPipelineFactory preProcessPipelineFactory,
            IManifestParser manifestParser,
            IHttpContextAccessor httpContextAccessor,
            PropertyEditorCollection propertyEditorCollection,
            IGlobalSettings globalSettings,
            IIOHelper ioHelper,
            IHostingEnvironment hostingEnvironment,
            ISmidgeConfig smidgeConfig)
        {
            _smidge = smidge;
            _preProcessPipelineFactory = preProcessPipelineFactory;
            _manifestParser = manifestParser;
            _httpContextAccessor = httpContextAccessor;
            _propertyEditorCollection = propertyEditorCollection;
            _globalSettings = globalSettings;
            _ioHelper = ioHelper;
            _hostingEnvironment = hostingEnvironment;
            _smidgeConfig = smidgeConfig;
        }

        private PreProcessPipeline JsPipeline  => _jsPipeline ??= _preProcessPipelineFactory.Create(typeof(JsMinifier));
        private PreProcessPipeline CssPipeline  => _cssPipeline ??= _preProcessPipelineFactory.Create(typeof(NuglifyCss));

        private Uri GetRequestUrl() => new Uri(_httpContextAccessor.HttpContext.Request.GetEncodedUrl(), UriKind.Absolute);
        public string GetHashValue => _smidgeConfig.Version;

        public void RequiresCss(string bundleName, params string[] filePaths) => _smidge.CreateCssBundle(bundleName).RequiresCss(filePaths);
        public string RenderCssHere(string bundleName) => _smidge.CssHereAsync(bundleName).ToString();
        public void RequiresJs(string bundleName, params string[] filePaths) =>     _smidge.CreateJsBundle(bundleName).RequiresJs(filePaths);
        public string RenderJsHere(string bundleName) => _smidge.JsHereAsync(bundleName).ToString();

        public async Task<IEnumerable<string>> GetAssetPathsAsync(AssetType assetType, List<IAssetFile> attributes)
        {

            var files =  attributes
                .Where(x => x.DependencyType == assetType)
                .Select(x=>x.FilePath)
                .ToArray();

            if (files.Length == 0) return Array.Empty<string>();

            if (assetType == AssetType.Javascript)
            {
                _smidge.RequiresJs(files);

                return await _smidge.GenerateJsUrlsAsync(JsPipeline, _hostingEnvironment.IsDebugMode);
            }
            else
            {
                _smidge.RequiresCss(files);

                return await _smidge.GenerateJsUrlsAsync(CssPipeline, _hostingEnvironment.IsDebugMode);
            }
        }

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

        public void Reset()
        {
            // TODO: Need to figure out how to delete temp directories to make sure we get fresh caches
        }

        public async Task<string> GetScriptForBackOfficeAsync()
        {
            var initJs = new JsInitialization(_manifestParser, this, _propertyEditorCollection);
            var initCss = new CssInitialization(_manifestParser, this, _propertyEditorCollection);

            var requestUrl = GetRequestUrl();
            var files = await initJs.OptimizeBackOfficeScriptFilesAsync(requestUrl, JsInitialization.GetDefaultInitialization());
            var result = JavaScriptHelper.GetJavascriptInitialization(files, "umbraco", _globalSettings, _ioHelper);
            result += await initCss.GetStylesheetInitializationAsync(requestUrl);

            return result;
        }

        public async Task<IEnumerable<string>> GetAssetListAsync()
        {
            var initJs = new JsInitialization(_manifestParser, this, _propertyEditorCollection);
            var initCss = new CssInitialization(_manifestParser, this, _propertyEditorCollection);
            var assets = new List<string>();
            var requestUrl = GetRequestUrl();
            assets.AddRange(await initJs.OptimizeBackOfficeScriptFilesAsync(requestUrl, Enumerable.Empty<string>()));
            assets.AddRange(await initCss.GetStylesheetFilesAsync(requestUrl));

            return assets;
        }


    }
}
