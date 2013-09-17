using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

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
        public string SelectedEditor { get; set; }

        [DataMember(Name = "preValues")]
        public IEnumerable<PreValueFieldSave> PreValues { get; set; }

        /// <summary>
        /// The real persisted data type
        /// </summary>
        [JsonIgnore]
        internal IDataTypeDefinition PersistedDataType { get; set; }

        /// <summary>
        /// The PropertyEditor assigned
        /// </summary>
        [JsonIgnore]
        internal PropertyEditor PropertyEditor { get; set; }

    }
}