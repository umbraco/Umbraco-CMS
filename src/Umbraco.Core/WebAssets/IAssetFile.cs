namespace Umbraco.Core.WebAssets
{
    public interface IAssetFile
    {
        string FilePath { get; set; }
        AssetType DependencyType { get; }
    }
}
