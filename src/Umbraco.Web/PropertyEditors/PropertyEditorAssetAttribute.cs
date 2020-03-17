using System;
using Umbraco.Core.Assets;

namespace Umbraco.Web.PropertyEditors
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
        public AssetType AssetType { get; private set; }
        public string FilePath { get; private set; }
        public int Priority { get; set; }

        /// <summary>
        /// Returns a CDF file reference
        /// </summary>
        public IAssetFile DependencyFile =>
            Priority == int.MinValue
                ? new AssetFile(AssetType) {FilePath = FilePath}
                : new AssetFile(AssetType) {FilePath = FilePath, Priority = Priority};

        public PropertyEditorAssetAttribute(AssetType assetType, string filePath)
        {
            AssetType = assetType;
            FilePath = filePath;
            Priority = int.MinValue;
        }
    }
}
