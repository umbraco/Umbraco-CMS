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
    public class DataTypeDisplay : EntityBasic, INotificationModel
    {
        public DataTypeDisplay()
        {
            Notifications = new List<Notification>();
        }

        /// <summary>
        /// This is nullable because when we are creating a new one, it is nothing
        /// </summary>
        [DataMember(Name = "selectedEditor", IsRequired = true)]
        [Required]
        public Guid? SelectedEditor { get; set; }

        [DataMember(Name = "availableEditors")]
        public IEnumerable<PropertyEditorBasic> AvailableEditors { get; set; }

        [DataMember(Name = "preValues")]
        public IEnumerable<PreValueFieldDisplay> PreValues { get; set; }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }
    }
}