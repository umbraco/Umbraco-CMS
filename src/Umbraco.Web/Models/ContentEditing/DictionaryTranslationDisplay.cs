﻿namespace Umbraco.Web.Models.ContentEditing
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The dictionary translation display model
    /// </summary>
    [DataContract(Name = "dictionaryTranslation", Namespace = "")]
    public class DictionaryTranslationDisplay : DictionaryTranslationSave
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }        
    }
}
