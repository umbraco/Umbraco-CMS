using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the tag value editor.
    /// </summary>
    public class TagConfiguration
    {
        // no field attribute, all defined in the editor, due to validators

        public string Group { get; set; } = "default";

        public TagsStorageType StorageType { get; set; } = TagsStorageType.Csv;

        public char Delimiter { get; set; }
    }
}
