using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a data type that is being edited
    /// </summary>
    [DataContract(Name = "dataType", Namespace = "")]
    public class DataTypeDisplay : EntityBasic
    {
        [DataMember(Name = "selectedEditor", IsRequired = true)]
        [Required]
        public Guid SelectedEditor { get; set; }

        [DataMember(Name = "availableEditors")]
        public IEnumerable<PropertyEditorBasic> AvailableEditors { get; set; }

        [DataMember(Name = "preValues")]
        public IEnumerable<PreValueFieldDisplay> PreValues { get; set; }
    }
}