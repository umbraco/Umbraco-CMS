using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;
using ClientDependency.Core.Config;
using Umbraco.Core.Configuration;
using Umbraco.Core.WebAssets;
using CssFile = ClientDependency.Core.CssFile;
using JavascriptFile = ClientDependency.Core.JavascriptFile;
using Umbraco.Core.Hosting;

namespace Umbraco.Web.WebAssets.CDF
{
    public class ClientDependencyRuntimeMinifier : IRuntimeMinifier
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ClientDependencyRuntimeMinifier> _logger;
        private readonly IUmbracoVersion _umbracoVersion;

        public string CacheBuster => ClientDependencySettings.Instance.Version.ToString();

        public ClientDependencyRuntimeMinifier(
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory,
            IUmbracoVersion umbracoVersion)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<ClientDependencyRuntimeMinifier>();
            _umbracoVersion = umbracoVersion;
        }

        public void CreateCssBundle(string bundleName, params string[] filePaths)
        {
            if (filePaths.Any(f => !f.StartsWith("/") && !f.StartsWith("~/")))
                throw new InvalidOperationException("All file paths must be absolute");

            BundleManager.CreateCssBundle(
                bundleName,
                filePaths.Select(x => new CssFile(x)).ToArray());
        }

        public Task<string> RenderCssHereAsync(string bundleName)
        {
            var bundleFiles = GetCssBundleFiles(bundleName);
            if (bundleFiles == null) return Task.FromResult(string.Empty);
            return Task.FromResult(RenderOutput(bundleFiles, AssetType.Css));
        }

        public void CreateJsBundle(string bundleName, params string[] filePaths)
        {
            if (filePaths.Any(f => !f.StartsWith("/") && !f.StartsWith("~/")))
                throw new InvalidOperationException("All file paths must be absolute");

            BundleManager.CreateJsBundle(
                bundleName,
                filePaths.Select(x => new JavascriptFile(x)).ToArray());
        }

        public Task<string> RenderJsHereAsync(string bundleName)
        {
            var bundleFiles = GetJsBundleFiles(bundleName);
            if (bundleFiles == null) return Task.FromResult(string.Empty);
            return Task.FromResult(RenderOutput(bundleFiles, AssetType.Javascript));
        }

        public Task<IEnumerable<string>> GetAssetPathsAsync(string bundleName)
        {
            var bundleFiles = GetJsBundleFiles(bundleName)?.ToList() ?? GetCssBundleFiles(bundleName)?.ToList();
            if (bundleFiles == null || bundleFiles.Count == 0) return Task.FromResult(Enumerable.Empty<string>());

            var assetType = bundleFiles[0].DependencyType == ClientDependencyType.Css ? AssetType.Css : AssetType.Javascript;

            // This is a hack on CDF so that we can resolve CDF urls directly since that isn't directly supported by the lib
            var renderer = ClientDependencySettings.Instance.MvcRendererCollection["Umbraco.DependencyPathRenderer"];
            renderer.RegisterDependencies(bundleFiles, new HashSet<IClientDependencyPath>(), out var scripts, out var stylesheets, _httpContextAccessor.HttpContext);

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
            var clientDependencyConfig = new ClientDependencyConfiguration(_loggerFactory.CreateLogger<ClientDependencyConfiguration>(), _hostingEnvironment);
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

            return assetType == AssetType.Css ? cssOutput : jsOutput;
        }

        private IEnumerable<IClientDependencyFile> GetCssBundleFiles(string bundleName)
        {
            // internal methods needs reflection
            var bundle = typeof(BundleManager)
                .GetMethod("GetCssBundle", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { bundleName });
            var bundleFiles = (IEnumerable<IClientDependencyFile>) bundle?.GetType().GetProperty("Files").GetValue(bundle, null);
            return bundleFiles;
        }

        private IEnumerable<IClientDependencyFile> GetJsBundleFiles(string bundleName)
        {
            // internal methods needs reflection
            var bundle = typeof(BundleManager)
                .GetMethod("GetJsBundle", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { bundleName });
            var bundleFiles = (IEnumerable<IClientDependencyFile>)bundle?.GetType().GetProperty("Files").GetValue(bundle, null);
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
