using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.EntityBase
{
    /// <summary>
    /// Defines an entity.
    /// </summary>
    public interface IEntity : IDeepCloneable
    {
        /// <summary>
        /// The integer identifier of the entity.
        /// </summary>
        [DataMember]
        int Id { get; set; }

        /// <summary>
        /// The Guid unique identifier of the entity.
        /// </summary>
        [DataMember]
        Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        [DataMember]
        DateTime CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the last update date.
        /// </summary>
        [DataMember]
        DateTime UpdateDate { get; set; }

        /// <summary>
        /// Gets a value indicating whether the entity has an identity.
        /// </summary>
        [IgnoreDataMember]
        bool HasIdentity { get; }
    }
}
