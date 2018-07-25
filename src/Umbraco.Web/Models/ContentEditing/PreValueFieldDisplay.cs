﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Defines a pre value editable field for a data type
    /// </summary>
    [DataContract(Name = "preValue", Namespace = "")]
    public class PreValueFieldDisplay : PreValueFieldSave
    {

        /// <summary>
        /// The name to display for this pre-value field
        /// </summary>
        [DataMember(Name = "label", IsRequired = true)]
        public string Name { get; set; }

        /// <summary>
        /// The description to display for this pre-value field
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Specifies whether to hide the label for the pre-value
        /// </summary>
        [DataMember(Name = "hideLabel")]
        public bool HideLabel { get; set; }

        /// <summary>
        /// The view to render for the field
        /// </summary>
        [DataMember(Name = "view", IsRequired = true)]
        public string View { get; set; }

        /// <summary>
        /// This allows for custom configuration to be injected into the pre-value editor
        /// </summary>        
        [DataMember(Name = "config")]
        public IDictionary<string, object> Config { get; set; }

        /// <summary>
        /// This allows for inner prevalues to be defined, for views such as radiobuttonlist, that require a selection.
        /// </summary>        
        [DataMember(Name = "prevalues")]
        public PreValueInnerListItem[] PreValues { get; set; }
    }
}
