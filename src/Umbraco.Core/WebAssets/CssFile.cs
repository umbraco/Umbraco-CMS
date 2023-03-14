namespace Umbraco.Cms.Core.WebAssets;

/// <summary>
///     Represents a CSS asset file
/// </summary>
public class CssFile : AssetFile
{
    public CssFile(string filePath)
        : base(AssetType.Css) =>
        FilePath = filePath;
}
