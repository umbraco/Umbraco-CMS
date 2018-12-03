using Newtonsoft.Json;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the nested content value editor.
    /// </summary>
    public class NestedContentConfiguration
    {
        [ConfigurationField("contentTypes", "Doc Types", "views/propertyeditors/nestedcontent/nestedcontent.doctypepicker.html", Description = "Select the doc types to use as the data blueprint.")]
        public ContentType[] ContentTypes { get; set; }

        [ConfigurationField("minItems", "Min Items", "number", Description = "Set the minimum number of items allowed.")]
        public int? MinItems { get; set; }

        [ConfigurationField("maxItems", "Max Items", "number", Description = "Set the maximum number of items allowed.")]
        public int? MaxItems { get; set; }

        [ConfigurationField("confirmDeletes", "Confirm Deletes", "boolean", Description = "Set whether item deletions should require confirming.")]
        public bool ConfirmDeletes { get; set; } = true;

        [ConfigurationField("showIcons", "Show Icons", "boolean", Description = "Set whether to show the items doc type icon in the list.")]
        public bool ShowIcons { get; set; } = true;

        [ConfigurationField("hideLabel", "Hide Label", "boolean", Description = "Set whether to hide the editor label and have the list take up the full width of the editor window.")]
        public bool HideLabel { get; set; }

        public class ContentType
        {
            [JsonProperty("ncAlias")]
            public string Alias { get; set; }

            [JsonProperty("ncTabAlias")]
            public string TabAlias { get; set; }

            [JsonProperty("nameTemplate")]
            public string Template { get; set; }
        }
    }
}