using Umbraco.Core.Models;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the tag value editor.
    /// </summary>
    public class TagConfiguration
    {
        // no field attribute, all defined in the editor, due to validators

        public string Group { get; set; }

        public TagCacheStorageType StorageType { get; set; } = TagCacheStorageType.Csv;
    }
}