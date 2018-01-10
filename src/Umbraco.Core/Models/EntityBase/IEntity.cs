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
        /// Gets or sets the integer identifier of the entity.
        /// </summary>
        [DataMember]
        int Id { get; set; }

        /// <summary>
        /// Gets or sets the Guid unique identifier of the entity.
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
        /// Gets or sets the delete date.
        /// </summary>
        /// <remarks>
        /// <para>The delete date is null when the entity has not been deleted.</para>
        /// <para>The delete date has a value when the entity instance has been deleted, but this value
        /// is transient and not persisted in database (since the entity does not exist anymore).</para>
        /// </remarks>
        [DataMember]
        DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Gets a value indicating whether the entity has an identity.
        /// </summary>
        [IgnoreDataMember]
        bool HasIdentity { get; }
    }
}
