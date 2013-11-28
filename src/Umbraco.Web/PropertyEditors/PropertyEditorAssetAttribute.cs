using System;
using ClientDependency.Core;

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
        public ClientDependencyType AssetType { get; private set; }
        public string FilePath { get; private set; }
        public int Priority { get; set; }
        
        /// <summary>
        /// Returns a CDF file reference
        /// </summary>
        public IClientDependencyFile DependencyFile
        {
            get
            {
                return Priority == int.MinValue
                           ? new BasicFile(AssetType) {FilePath = FilePath}
                           : new BasicFile(AssetType) {FilePath = FilePath, Priority = Priority};

            }
        }

        public PropertyEditorAssetAttribute(ClientDependencyType assetType, string filePath)
        {
            AssetType = assetType;
            FilePath = filePath;
            Priority = int.MinValue;
        }
    }
}