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

        // TODO: We need to use IConfiguration to get the section (ConfigurationManager is not the way to do it)
        public string GetHashValue => new SmidgeConfig((IConfiguration)ConfigurationManager.GetSection("Umbraco:Smidge")).Version;

        public SmidgeRuntimeMinifier(SmidgeHelper smidge)
        {
            _smidge = smidge;
        }
        public string RequiresCss(string filePath, string bundleName)
        {
            throw new NotImplementedException();
        }

        public string RenderCssHere(string bundleName)
        {
            return _smidge.CssHereAsync(bundleName).ToString();
        }

        public string RequiresJs(string filePath)
        {
            throw new NotImplementedException();
        }

        public string RenderJsHere(string bundleName)
        {
            return _smidge.JsHereAsync(bundleName).ToString();
        }

        public IEnumerable<string> GetAssetPaths(AssetType assetType, List<IAssetFile> attributes)
        {
            var parsed = new List<string>();

            if (assetType == AssetType.Javascript)
                attributes.ForEach(x => parsed.AddRange(_smidge.GenerateJsUrlsAsync(x.Bundle).Result));
            else
                attributes.ForEach(x => parsed.AddRange(_smidge.GenerateCssUrlsAsync(x.Bundle).Result));

            return parsed;
        }

        public string Minify(string src, AssetType assetType)
        {
            if (assetType == AssetType.Javascript)
            {

                // TODO: use NuglifyJs to minify JS files (https://github.com/Shazwazza/Smidge/blob/master/src/Smidge.Nuglify/NuglifyJs.cs)
            }
            else
            {
                // TODO: use NuglifyCss to minify CSS files (https://github.com/Shazwazza/Smidge/blob/master/src/Smidge.Nuglify/NuglifyCss.cs)
            }

            throw new NotImplementedException();
        }

        public void Reset()
        {
            // TODO: Need to figure out how to delete temp directories to make sure we get fresh caches

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
