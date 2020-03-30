using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core.Assets;

namespace Umbraco.Core.Runtime
{
    public interface IRuntimeMinifier
    {
        string GetHashValue { get; }

        //return type HtmlHelper
        void RequiresCss(string bundleName, params string[] filePaths);

        //return type IHtmlString
        //IClientDependencyPath[]
        string RenderCssHere(string bundleName);

        // return type HtmlHelper
        void RequiresJs(string bundleName, params string[] filePaths);

        // return type IHtmlString
        string RenderJsHere(string bundleName);

        Task<IEnumerable<string>> GetAssetPathsAsync(AssetType assetType, List<IAssetFile> attributes);

        Task<string> MinifyAsync(string fileContent, AssetType assetType);
        void Reset();
        Task<string> GetScriptForBackOfficeAsync();
        Task<IEnumerable<string>> GetAssetListAsync();
    }
}
