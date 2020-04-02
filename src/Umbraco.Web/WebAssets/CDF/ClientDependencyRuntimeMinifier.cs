using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;
using ClientDependency.Core.Config;
using ClientDependency.Core.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.WebAssets;
using Umbraco.Web.JavaScript;
using CssFile = ClientDependency.Core.CssFile;
using JavascriptFile = ClientDependency.Core.JavascriptFile;

namespace Umbraco.Web.WebAssets.CDF
{
    public class ClientDependencyRuntimeMinifier : IRuntimeMinifier
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIOHelper _ioHelper;
        private readonly ILogger _logger;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IManifestParser _manifestParser;
        private readonly IGlobalSettings _globalSettings;
        private readonly PropertyEditorCollection _propertyEditorCollection;

        public string CacheBuster => ClientDependencySettings.Instance.Version.ToString();

        public ClientDependencyRuntimeMinifier(
            IHttpContextAccessor httpContextAccessor,
            IIOHelper ioHelper,
            ILogger logger,
            IUmbracoVersion umbracoVersion,
            IManifestParser manifestParser,
            IGlobalSettings globalSettings,
            PropertyEditorCollection propertyEditorCollection)
        {
            _httpContextAccessor = httpContextAccessor;
            _ioHelper = ioHelper;
            _logger = logger;
            _umbracoVersion = umbracoVersion;
            _manifestParser = manifestParser;
            _globalSettings = globalSettings;
            _propertyEditorCollection = propertyEditorCollection;
        }

        public void CreateCssBundle(string bundleName, params string[] filePaths)
        {
            BundleManager.CreateCssBundle(
                bundleName,
                filePaths.Select(x => new CssFile(x)).ToArray());
        }

        public string RenderCssHere(string bundleName)
        {
            var bundleFiles = GetCssBundleFiles(bundleName);
            if (bundleFiles == null) return string.Empty;
            return RenderOutput(bundleFiles, AssetType.Css);
        }

        public void CreateJsBundle(string bundleName, params string[] filePaths)
        {
            BundleManager.CreateJsBundle(
                bundleName,
                filePaths.Select(x => new JavascriptFile(x)).ToArray());
        }

        public string RenderJsHere(string bundleName)
        {
            var bundleFiles = GetJsBundleFiles(bundleName);
            if (bundleFiles == null) return string.Empty;
            return RenderOutput(bundleFiles, AssetType.Javascript);
        }

        public Task<IEnumerable<string>> GetAssetPathsAsync(string bundleName)
        {
            var bundleFiles = GetJsBundleFiles(bundleName)?.ToList() ?? GetCssBundleFiles(bundleName)?.ToList();
            if (bundleFiles == null || bundleFiles.Count == 0) return Task.FromResult(Enumerable.Empty<string>());

            var assetType = bundleFiles[0].DependencyType == ClientDependencyType.Css ? AssetType.Css : AssetType.Javascript;

            // get the output string for these registrations which will be processed by CDF correctly to stagger the output based
            // on internal vs external resources. The output will be delimited based on our custom Umbraco.Web.JavaScript.DependencyPathRenderer
            var dependencies = new List<IClientDependencyFile>();

            // This is a hack on CDF so that we can resolve CDF urls directly since that isn't directly supported by the lib
            var renderer = ClientDependencySettings.Instance.MvcRendererCollection["Umbraco.DependencyPathRenderer"];
            renderer.RegisterDependencies(dependencies, new HashSet<IClientDependencyPath>(), out var scripts, out var stylesheets, _httpContextAccessor.HttpContext);

            var toParse = assetType == AssetType.Javascript ? scripts : stylesheets;
            return Task.FromResult<IEnumerable<string>>(toParse.Split(new[] { DependencyPathRenderer.Delimiter }, StringSplitOptions.RemoveEmptyEntries));
        }

        public Task<string> MinifyAsync(string fileContent, AssetType assetType)
        {
            TextReader reader = new StringReader(fileContent);

            if (assetType == AssetType.Javascript)
            {
                var jsMinifier = new JSMin();
                return Task.FromResult(jsMinifier.Minify(reader));
            }

            // asset type is Css
            var cssMinifier = new CssMinifier();
            return Task.FromResult(cssMinifier.Minify(reader));
        }

        public void Reset()
        {
            // Update ClientDependency version
            var clientDependencyConfig = new ClientDependencyConfiguration(_logger, _ioHelper);
            var clientDependencyUpdated = clientDependencyConfig.UpdateVersionNumber(
                _umbracoVersion.SemanticVersion, DateTime.UtcNow, "yyyyMMdd");
            // Delete ClientDependency temp directories to make sure we get fresh caches
            var clientDependencyTempFilesDeleted = clientDependencyConfig.ClearTempFiles(_httpContextAccessor.HttpContext);
        }

        private string RenderOutput(IEnumerable<IClientDependencyFile> bundleFiles, AssetType assetType)
        {
            var renderer = ClientDependencySettings.Instance.DefaultMvcRenderer;

            renderer.RegisterDependencies(
                bundleFiles.ToList(),
                new HashSet<IClientDependencyPath>(),
                out var jsOutput, out var cssOutput, _httpContextAccessor.GetRequiredHttpContext());

            return HttpUtility.HtmlEncode(assetType == AssetType.Css ? cssOutput : jsOutput);
        }

        private IEnumerable<IClientDependencyFile> GetCssBundleFiles(string bundleName)
        {
            // the result of this is internal too, so use dynamic
            dynamic bundle = typeof(BundleManager)
                .GetMethod("GetCssBundle", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { bundleName });
            if (bundle == null) return null;
            IEnumerable<IClientDependencyFile> bundleFiles = bundle.Files;
            return bundleFiles;
        }

        private IEnumerable<IClientDependencyFile> GetJsBundleFiles(string bundleName)
        {
            // the result of this is internal too, so use dynamic
            dynamic bundle = typeof(BundleManager)
                .GetMethod("GetJsBundle", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { bundleName });
            if (bundle == null) return null;
            IEnumerable<IClientDependencyFile> bundleFiles = bundle.Files;
            return bundleFiles;
        }

        private ClientDependencyType MapDependencyTypeValue(AssetType type)
        {
            return type switch
            {
                AssetType.Javascript => ClientDependencyType.Javascript,
                AssetType.Css => ClientDependencyType.Css,
                _ => (ClientDependencyType) Enum.Parse(typeof(ClientDependencyType), type.ToString(), true)
            };
        }

        private IClientDependencyFile MapAssetFile(IAssetFile assetFile)
        {
            var assetFileType = (AssetFile)assetFile;
            var basicFile = new BasicFile(MapDependencyTypeValue(assetFileType.DependencyType))
            {
                FilePath = assetFile.FilePath
            };

            return basicFile;
        }
    }
}
