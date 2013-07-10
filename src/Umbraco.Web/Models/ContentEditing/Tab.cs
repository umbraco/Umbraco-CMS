using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a tab in the UI
    /// </summary>
    [DataContract(Name = "tab", Namespace = "")]
    public class Tab<T>
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "active")]
        public bool IsActive { get; set; }

        [DataMember(Name = "label")]
        public string Label { get; set; }

        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "properties")]
        public IEnumerable<T> Properties { get; set; }
    }
}