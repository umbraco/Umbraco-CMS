using System;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "relationType", Namespace = "")]
    public class RelationType
    {

        /// <summary>
        /// Gets or sets the Name of the RelationType
        /// </summary>
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Alias of the RelationType
        /// </summary>
        [DataMember(Name = "alias", IsRequired = true)]
        public string Alias { get; set; }

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
        /// Gets or sets the Childs object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember(Name = "childObjectType", IsRequired = true)]
        public Guid ChildObjectType { get; set; }
    }
}