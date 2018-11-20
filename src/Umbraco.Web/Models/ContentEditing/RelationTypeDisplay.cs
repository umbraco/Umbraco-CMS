using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "relationType", Namespace = "")]
    public class RelationTypeDisplay : EntityBasic, INotificationModel
    {
        public RelationTypeDisplay()
        {
            Notifications = new List<Notification>();
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
        /// </summary>
        [DataMember(Name = "isBidirectional", IsRequired = true)]
        public bool IsBidirectional { get; set; }

        /// <summary>
        /// Gets or sets the Parents object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember(Name = "parentObjectType", IsRequired = true)]
        public Guid ParentObjectType { get; set; }

        /// <summary>
        /// Gets or sets the Parent's object type name.
        /// </summary>
        [DataMember(Name = "parentObjectTypeName")]
        [ReadOnly(true)]
        public string ParentObjectTypeName { get; set; }

        /// <summary>
        /// Gets or sets the Childs object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember(Name = "childObjectType", IsRequired = true)]
        public Guid ChildObjectType { get; set; }

        /// <summary>
        /// Gets or sets the Child's object type name.
        /// </summary>
        [DataMember(Name = "childObjectTypeName")]
        [ReadOnly(true)]
        public string ChildObjectTypeName { get; set; }

        /// <summary>
        /// Gets or sets the relations associated with this relation type.
        /// </summary>
        [DataMember(Name = "relations")]
        [ReadOnly(true)]
        public IEnumerable<RelationDisplay> Relations { get; set; }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }
    }
}
