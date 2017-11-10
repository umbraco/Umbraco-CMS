namespace Umbraco.Web.Models.ContentEditing
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The dictionary translation display.
    /// </summary>
    [DataContract(Name = "dictionaryTranslation", Namespace = "")]
    public class DictionaryTranslationDisplay
    {
        /// <summary>
        /// Gets or sets the iso code.
        /// </summary>
        [DataMember(Name = "isoCode")]        
        public string IsoCode { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the translation.
        /// </summary>
        [DataMember(Name = "translation")]
        public string Translation { get; set; }
    }
}
