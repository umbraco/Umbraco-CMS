namespace Umbraco.Web.Models.ContentEditing
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The dictionary translation overview display.
    /// </summary>
    [DataContract(Name = "dictionaryTranslation", Namespace = "")]
    public class DictionaryOverviewTranslationDisplay
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether has translation.
        /// </summary>
        [DataMember(Name = "hasTranslation")]
        public bool HasTranslation { get; set; }
    }
}
