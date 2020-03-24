using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Smidge;
using Smidge.Cache;
using Smidge.FileProcessors;
using Smidge.Nuglify;
using Smidge.Models;
using Umbraco.Core.Assets;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.BackOffice.Smidge
{
    public class SmidgeRuntimeMinifier : IRuntimeMinifier
    {
        private readonly SmidgeHelper _smidge;
        public string GetHashValue => new SmidgeConfig((IConfiguration) ConfigurationManager.GetSection("smidge")).Version;

        public SmidgeRuntimeMinifier(SmidgeHelper smidge)
        {
            _smidge = smidge;
        }
        public string RequiresCss(string filePath, string bundleName)
        {
            throw new NotImplementedException();
        }

        public string RenderCssHere(params string[] path)
        {
            throw new NotImplementedException();
        }

        public string RequiresJs(string filePath)
        {
            throw new NotImplementedException();
        }

        public string RenderJsHere()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAssetPaths(AssetType assetType, List<IAssetFile> attributes)
        {
            throw new NotImplementedException();
        }

        public string Minify(string src)
        {
            //TextReader reader = new StringReader(src);
           // var jsMinifier = new NuglifyJs();

           // return jsMinifier.ProcessAsync();
           return "";
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public string GetScriptForBackOffice()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAssetList()
        {
            throw new NotImplementedException();
        }
    }
}
