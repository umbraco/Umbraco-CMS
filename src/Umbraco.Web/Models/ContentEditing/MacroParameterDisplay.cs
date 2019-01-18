namespace Umbraco.Web.Models.ContentEditing
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The macro parameter display.
    /// </summary>
    [DataContract(Name = "parameter", Namespace = "")]
    public class MacroParameterDisplay
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        [DataMember(Name = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        [DataMember(Name = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the editor.
        /// </summary>
        [DataMember(Name = "editor")]
        public string Editor { get; set; }
    }
}
