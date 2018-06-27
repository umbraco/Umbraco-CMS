using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// The dictionary display model
    /// </summary>
    [DataContract(Name = "dictionary", Namespace = "")]
    public class DictionaryDisplay : EntityBasic, INotificationModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryDisplay"/> class.
        /// </summary>
        public DictionaryDisplay()
        {
            this.Notifications = new List<Notification>();
            this.Translations = new List<DictionaryTranslationDisplay>();
        }

        /// <inheritdoc />
        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }

        /// <summary>
        /// Gets or sets the parent id.
        /// </summary>
        [DataMember(Name = "parentId")]
        public new Guid ParentId { get; set; }

        /// <summary>
        /// Gets the translations.
        /// </summary>
        [DataMember(Name = "translations")]
        public List<DictionaryTranslationDisplay> Translations { get; private set; }
    }
}
