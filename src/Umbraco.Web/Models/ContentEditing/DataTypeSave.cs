using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "dataType", Namespace = "")]
    public class DataTypeSave : EntityBasic
    {
        /// <summary>
        /// The action to perform when saving this data type
        /// </summary>
        /// <remarks>
        /// If either of the Publish actions are specified an exception will be thrown.
        /// </remarks>
        [DataMember(Name = "action", IsRequired = true)]
        [Required]
        public ContentSaveAction Action { get; set; }

        [DataMember(Name = "selectedEditor", IsRequired = true)]
        [Required]
        public Guid SelectedEditor { get; set; }

        [DataMember(Name = "preValues")]
        public IEnumerable<PreValueFieldSave> PreValues { get; set; }

    }
}