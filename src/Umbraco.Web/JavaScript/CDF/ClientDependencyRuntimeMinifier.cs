using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;
using ClientDependency.Core.Config;
using Umbraco.Core.Assets;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.JavaScript.CDF
{
    public class ClientDependencyRuntimeMinifier : IRuntimeMinifier
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIOHelper _ioHelper;
        private readonly ILogger _logger;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IManifestParser _manifestParser;
        private readonly IGlobalSettings _globalSettings;
        private readonly HtmlHelper _htmlHelper;

        public string GetHashValue => ClientDependencySettings.Instance.Version.ToString();

        public ClientDependencyRuntimeMinifier(IHttpContextAccessor httpContextAccessor, IIOHelper ioHelper, ILogger logger, IUmbracoVersion umbracoVersion, IManifestParser manifestParser, IGlobalSettings globalSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _ioHelper = ioHelper;
            _logger = logger;
            _umbracoVersion = umbracoVersion;
            _manifestParser = manifestParser;
            _globalSettings = globalSettings;
            _htmlHelper = new HtmlHelper(new ViewContext(), new ViewPage());
        }

        public string RequiresCss(string filePath, string pathNameAlias)
        {
            throw new NotImplementedException();
            //_htmlHelper.ViewContext.GetLoader().RegisterDependency(filePath, pathNameAlias, ClientDependencyType.Css);
            //return html;
        }

        public string RenderCssHere(params string[] path)
        {
            throw new NotImplementedException();
            //return new HtmlString(_htmlHelper.ViewContext.GetLoader().RenderPlaceholder(
            //    ClientDependencyType.Css, path));
        }

        public string RequiresJs(string filePath)
        {
            throw new NotImplementedException();
            //_htmlHelper.ViewContext.GetLoader().RegisterDependency(filePath, ClientDependencyType.Javascript);
            //return _htmlHelper;
        }

        public string RenderJsHere()
        {
            throw new NotImplementedException();
            //return new HtmlString(
            //    _htmlHelper.ViewContext.GetLoader().RenderPlaceholder(
            //        ClientDependencyType.Javascript, new List<IClientDependencyPath>()));
        }

        public IEnumerable<string> GetAssetPaths(AssetType assetType, List<IAssetFile> attributes)
        {
            // get the output string for these registrations which will be processed by CDF correctly to stagger the output based
            // on internal vs external resources. The output will be delimited based on our custom Umbraco.Web.JavaScript.DependencyPathRenderer
            var dependencies = new List<IClientDependencyFile>();

            foreach (var assetFile in attributes)
            {
                if(!((AssetFile)assetFile is null))
                    dependencies.Add(MapAssetFile(assetFile));
            }

            var renderer = ClientDependencySettings.Instance.MvcRendererCollection["Umbraco.DependencyPathRenderer"];
            renderer.RegisterDependencies(dependencies, new HashSet<IClientDependencyPath>(), out var scripts, out var stylesheets, _httpContextAccessor.HttpContext);

            var toParse = assetType == AssetType.Javascript ? scripts : stylesheets;
            return toParse.Split(new[] { DependencyPathRenderer.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string Minify(string src)
        {
            TextReader reader = new StringReader(src);
            var jsMinifier = new JSMin();

            return jsMinifier.Minify(reader);
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

        public string GetScriptForBackOffice()
        {
            var initJs = new JsInitialization(_manifestParser, this);
            var initCss = new CssInitialization(_manifestParser, this);

            var httpContext = _httpContextAccessor.GetRequiredHttpContext();
            var files = initJs.OptimizeBackOfficeScriptFiles(httpContext, JsInitialization.GetDefaultInitialization());
            var result = JavaScriptHelper.GetJavascriptInitialization(httpContext, files, "umbraco", _globalSettings, _ioHelper);
            result += initCss.GetStylesheetInitialization(httpContext);

            return result;
        }

        public IEnumerable<string> GetAssetList()
        {
            var initJs = new JsInitialization(_manifestParser, this);
            var initCss = new CssInitialization(_manifestParser, this);
            var assets = new List<string>();
            var httpContext = _httpContextAccessor.GetRequiredHttpContext();
            assets.AddRange(initJs.OptimizeBackOfficeScriptFiles(httpContext, Enumerable.Empty<string>()));
            assets.AddRange(initCss.GetStylesheetFiles(httpContext));

            return assets;
        }

        private ClientDependencyType MapDependencyTypeValue(AssetType type)
        {
            switch (type)
            {
                case AssetType.Javascript:
                    return ClientDependencyType.Javascript;

                case AssetType.Css:
                    return ClientDependencyType.Css;
            }

            return (ClientDependencyType)Enum.Parse(typeof(ClientDependencyType), type.ToString(), true);
        }

        private IClientDependencyFile MapAssetFile(IAssetFile assetFile)
        {
            var assetFileType = (AssetFile)assetFile;
            var basicFile = new BasicFile(MapDependencyTypeValue(assetFileType.DependencyType));

            return basicFile;
        }
    }
}
