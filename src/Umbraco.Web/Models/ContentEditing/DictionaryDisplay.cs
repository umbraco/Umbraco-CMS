namespace Umbraco.Web.Models.ContentEditing
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

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
        }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        public List<Notification> Notifications { get; private set; }
    }
}
