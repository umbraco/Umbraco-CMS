using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    public abstract class ContentItemDisplayBase<T, TPersisted> : TabbedContentItem<T, TPersisted>, INotificationModel, IErrorModel
        where T : ContentPropertyBasic 
        where TPersisted : IContentBase
    {
        protected ContentItemDisplayBase()
        {
            Notifications = new List<Notification>();
            Errors = new Dictionary<string, object>();
        }

        /// <summary>
        /// The name of the content type
        /// </summary>
        [DataMember(Name = "contentTypeName")]
        public string ContentTypeName { get; set; }

        /// <summary>
        /// Indicates if the content is configured as a list view container
        /// </summary>
        [DataMember(Name = "isContainer")]
        public bool IsContainer { get; set; }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }

        /// <summary>
        /// This is used for validation of a content item.
        /// </summary>
        /// <remarks>
        /// A content item can be invalid but still be saved. This occurs when there's property validation errors, we will
        /// still save the item but it cannot be published. So we need a way of returning validation errors as well as the
        /// updated model.
        /// 
        /// NOTE: The ProperCase is important because when we return ModeState normally it will always be proper case.
        /// </remarks>
        [DataMember(Name = "ModelState")]
        public IDictionary<string, object> Errors { get; set; }
    }
}
