using System.Collections.Generic;

namespace Umbraco.Core.Assets
{
    public interface IAssetFile
    {
        string FilePath { get; set; }
        AssetType DependencyType { get; }
        int Priority { get; set; }
        int Group { get; set; }
        string PathNameAlias { get; set; }
        string ForceProvider { get; set; }
        bool ForceBundle { get; set; }
        IDictionary<string, string> HtmlAttributes { get; }
    }
}
