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
    [DataContract(Name = "contentType", Namespace = "")]
    public class ContentTypeCompositionDisplay : ContentTypeBasic, INotificationModel
    {
        public ContentTypeCompositionDisplay()
        {
            //initialize collections so at least their never null
            Groups = new List<PropertyGroupDisplay>();
            AllowedContentTypes = new List<int>();
            CompositeContentTypes = new List<string>();
            AvailableCompositeContentTypes = new List<EntityBasic>();
            Notifications = new List<Notification>();
        }

        //name, alias, icon, thumb, desc, inherited from basic
        
        //List view
        [DataMember(Name = "isContainer")]
        public bool IsContainer { get; set; }

        [DataMember(Name = "listViewEditorName")]
        [ReadOnly(true)]
        public string ListViewEditorName { get; set; }

        //Tabs
        [DataMember(Name = "groups")]
        public IEnumerable<PropertyGroupDisplay> Groups { get; set; }

        //Allowed child types
        [DataMember(Name = "allowedContentTypes")]
        public IEnumerable<int> AllowedContentTypes { get; set; }

        //Compositions
        [DataMember(Name = "compositeContentTypes")]
        public IEnumerable<string> CompositeContentTypes { get; set; }

        [DataMember(Name = "availableCompositeContentTypes")]
        [ReadOnly(true)]
        public IEnumerable<EntityBasic> AvailableCompositeContentTypes { get; set; }

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
}
