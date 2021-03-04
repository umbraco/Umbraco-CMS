using System;
using Umbraco.Cms.Core.WebAssets;

namespace Umbraco.Cms.Infrastructure.WebAssets
{
    /// <summary>
    /// Indicates that the property editor requires this asset be loaded when the back office is loaded
    /// </summary>
    /// <remarks>
    /// This wraps a CDF asset
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PropertyEditorAssetAttribute : Attribute
    {
        public AssetType AssetType { get; }
        public string FilePath { get; }

        public PropertyEditorAssetAttribute(AssetType assetType, string filePath)
        {
            AssetType = assetType;
            FilePath = filePath;
        }
    }
}
