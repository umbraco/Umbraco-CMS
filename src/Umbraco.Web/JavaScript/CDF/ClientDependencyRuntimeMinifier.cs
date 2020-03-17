using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;
using ClientDependency.Core.Config;
using Umbraco.Core.Assets;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.JavaScript.CDF
{
    public class ClientDependencyRuntimeMinifier : IRuntimeMinifier
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HtmlHelper _htmlHelper;

        public string GetHashValue => ClientDependencySettings.Instance.Version.ToString();

        public ClientDependencyRuntimeMinifier(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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
