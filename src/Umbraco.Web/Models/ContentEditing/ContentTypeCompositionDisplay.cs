using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models.Validation;

namespace Umbraco.Web.Models.ContentEditing
{
    public abstract class ContentTypeCompositionDisplay : ContentTypeBasic, INotificationModel
    {
        protected ContentTypeCompositionDisplay()
        {
            //initialize collections so at least their never null
            AllowedContentTypes = new List<int>();
            CompositeContentTypes = new List<string>();
            Notifications = new List<Notification>();
        }

        //name, alias, icon, thumb, desc, inherited from basic

        //List view
        [DataMember(Name = "isContainer")]
        public bool IsContainer { get; set; }

        [DataMember(Name = "listViewEditorName")]
        [ReadOnly(true)]
        public string ListViewEditorName { get; set; }

        //Allowed child types
        [DataMember(Name = "allowedContentTypes")]
        public IEnumerable<int> AllowedContentTypes { get; set; }

        //Compositions
        [DataMember(Name = "compositeContentTypes")]
        public IEnumerable<string> CompositeContentTypes { get; set; }

        //Locked compositions
        [DataMember(Name = "lockedCompositeContentTypes")]
        public IEnumerable<string> LockedCompositeContentTypes { get; set; }

        [DataMember(Name = "allowAsRoot")]
        public bool AllowAsRoot { get; set; }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        [ReadOnly(true)]
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
        [ReadOnly(true)]
        public IDictionary<string, object> Errors { get; set; }
    }

    [DataContract(Name = "contentType", Namespace = "")]
    public abstract class ContentTypeCompositionDisplay<TPropertyTypeDisplay> : ContentTypeCompositionDisplay
        where TPropertyTypeDisplay : PropertyTypeDisplay
    {
        protected ContentTypeCompositionDisplay()
        {
            //initialize collections so at least their never null
            Groups = new List<PropertyGroupDisplay<TPropertyTypeDisplay>>();
        }

        //Tabs
        [DataMember(Name = "groups")]
        public IEnumerable<PropertyGroupDisplay<TPropertyTypeDisplay>> Groups { get; set; }
        
    }
}
