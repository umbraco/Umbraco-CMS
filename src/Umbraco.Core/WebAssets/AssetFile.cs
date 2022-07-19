using System.Diagnostics;

namespace Umbraco.Cms.Core.WebAssets;

/// <summary>
///     Represents a dependency file
/// </summary>
[DebuggerDisplay("Type: {DependencyType}, File: {FilePath}")]
public class AssetFile : IAssetFile
{
    public AssetFile(AssetType type) => DependencyType = type;

    #region IAssetFile Members

    public string? FilePath { get; set; }

    public AssetType DependencyType { get; }

    #endregion
}
