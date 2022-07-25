namespace Umbraco.Cms.Core.WebAssets;

/// <summary>
///     Represents a JS asset file
/// </summary>
public class JavaScriptFile : AssetFile
{
    public JavaScriptFile(string filePath)
        : base(AssetType.Javascript) =>
        FilePath = filePath;
}
