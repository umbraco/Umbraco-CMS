using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a data type that is being edited
    /// </summary>
    [DataContract(Name = "dataType", Namespace = "")]
    public class DataTypeDisplay : EntityBasic, INotificationModel
    {
        public DataTypeDisplay()
        {
            Notifications = new List<Notification>();
        }

        /// <summary>
        /// The alias of the property editor
        /// </summary>
        [DataMember(Name = "selectedEditor", IsRequired = true)]
        [Required]
        public string SelectedEditor { get; set; }

        [DataMember(Name = "availableEditors")]
        public IEnumerable<PropertyEditorBasic> AvailableEditors { get; set; }

        [DataMember(Name = "preValues")]
        public IEnumerable<PreValueFieldDisplay> PreValues { get; set; }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }
        
        /// <summary>
        /// Whether or not this is a system data type, in which case it cannot be deleted
        /// </summary>
        [DataMember(Name = "isSystem")]
        [ReadOnly(true)]
        public bool IsSystemDataType { get; set; }
    }
}