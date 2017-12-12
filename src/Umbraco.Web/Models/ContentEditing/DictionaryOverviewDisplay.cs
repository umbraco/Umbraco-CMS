namespace Umbraco.Web.Models.ContentEditing
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// The dictionary overview display.
    /// </summary>
    [DataContract(Name = "dictionary", Namespace = "")]
    public class DictionaryOverviewDisplay
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryOverviewDisplay"/> class.
        /// </summary>
        public DictionaryOverviewDisplay()
        {
            this.Translations = new List<DictionaryOverviewTranslationDisplay>();
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        [DataMember(Name = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the translations.
        /// </summary>
        [DataMember(Name = "translations")]
        public IEnumerable<DictionaryOverviewTranslationDisplay> Translations { get; set; }
    }
}
