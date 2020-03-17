using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Config;
using Umbraco.Core.Assets;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.JavaScript
{
    public class ClientDependencyRuntimeMinifier : IRuntimeMinifier
    {
        private readonly HtmlHelper _htmlHelper;
        private readonly HttpContextBase _httpContext;

        public int Version => ClientDependencySettings.Instance.Version;

        public string FileMapDefaultFolder
        {
            get => XmlFileMapper.FileMapDefaultFolder;
            set => XmlFileMapper.FileMapDefaultFolder = value;
        }

        public ClientDependencyRuntimeMinifier()
        {
            _htmlHelper = new HtmlHelper(new ViewContext(), new ViewPage());
            //_httpContext = httpContext;
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
                if(!(assetFile is null))
                    dependencies.Add(MapAssetFile(assetFile));
            }

            var renderer = ClientDependencySettings.Instance.MvcRendererCollection["Umbraco.DependencyPathRenderer"];
            renderer.RegisterDependencies(dependencies, new HashSet<IClientDependencyPath>(), out var scripts, out var stylesheets, _httpContext);

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
