using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core.Assets;

namespace Umbraco.Core.Runtime
{
    public interface IRuntimeMinifier
    {
        string GetHashValue { get; }

        //return type HtmlHelper
        void RequiresCss(string filePath, string bundleName);

        //return type IHtmlString
        //IClientDependencyPath[]
        string RenderCssHere(string bundleName);

        // return type HtmlHelper
        void RequiresJs(string filePath, string bundleName);

        // return type IHtmlString
        string RenderJsHere(string bundleName);

        IEnumerable<string> GetAssetPaths(AssetType assetType, List<IAssetFile> attributes);

        string Minify(string src, AssetType assetType);
        void Reset();
        string GetScriptForBackOffice();
        IEnumerable<string> GetAssetList();
    }
}
