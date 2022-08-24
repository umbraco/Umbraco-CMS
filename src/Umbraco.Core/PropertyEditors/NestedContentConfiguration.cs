using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the nested content value editor.
/// </summary>
public class NestedContentConfiguration
{
    [ConfigurationField("contentTypes", "Element Types", "views/propertyeditors/nestedcontent/nestedcontent.doctypepicker.html", Description = "Select the Element Types to use as models for the items.")]
    public ContentType[]? ContentTypes { get; set; }

    [ConfigurationField("minItems", "Min Items", "number", Description = "Minimum number of items allowed.")]
    public int? MinItems { get; set; }

    [ConfigurationField("maxItems", "Max Items", "number", Description = "Maximum number of items allowed.")]
    public int? MaxItems { get; set; }

    [ConfigurationField("confirmDeletes", "Confirm Deletes", "boolean", Description = "Requires editor confirmation for delete actions.")]
    public bool ConfirmDeletes { get; set; } = true;

    [ConfigurationField("showIcons", "Show Icons", "boolean", Description = "Show the Element Type icons.")]
    public bool ShowIcons { get; set; } = true;

    [ConfigurationField("expandsOnLoad", "Expands on load", "boolean", Description = "A single item is automatically expanded")]
    public bool ExpandsOnLoad { get; set; } = true;

    [ConfigurationField("hideLabel", "Hide Label", "boolean", Description = "Hide the property label and let the item list span the full width of the editor window.")]
    public bool HideLabel { get; set; }

    [DataContract]
    public class ContentType
    {
        [DataMember(Name = "ncAlias")]
        public string? Alias { get; set; }

        [DataMember(Name = "ncTabAlias")]
        public string? TabAlias { get; set; }

        [DataMember(Name = "nameTemplate")]
        public string? Template { get; set; }
    }
}
