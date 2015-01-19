using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public interface IRelationType : IAggregateRoot, IRememberBeingDirty, ICanBeDirty
    {
        /// <summary>
        /// Gets or sets the Name of the RelationType
        /// </summary>
        [DataMember]
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the Alias of the RelationType
        /// </summary>
        [DataMember]
        string Alias { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
        /// </summary>
        [DataMember]
        bool IsBidirectional { get; set; }

        /// <summary>
        /// Gets or sets the Parents object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        Guid ParentObjectType { get; set; }

        /// <summary>
        /// Gets or sets the Childs object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        Guid ChildObjectType { get; set; }
    }
}