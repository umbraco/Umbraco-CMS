using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.EntityBase
{
    /// <summary>
    /// Defines an Entity.
    /// Entities should always have an Id, Created and Modified date
    /// </summary>
    /// <remarks>The current database schema doesn't provide a modified date
    /// for all entities, so this will have to be changed at a later stage.</remarks>
    public interface IEntity : IDeepCloneable
    {
        /// <summary>
        /// The Id of the entity
        /// </summary>
        [DataMember]
        int Id { get; set; }

        /// <summary>
        /// Guid based Id
        /// </summary>
        /// <remarks>The key is currectly used to store the Unique Id from the 
        /// umbracoNode table, which many of the entities are based on.</remarks>
        [DataMember]
        Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the Created Date
        /// </summary>
        [DataMember]
        DateTime CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the Modified Date
        /// </summary>
        [DataMember]
        DateTime UpdateDate { get; set; }

        /// <summary>
        /// Indicates whether the current entity has an identity, eg. Id.
        /// </summary>
        [IgnoreDataMember]
        bool HasIdentity { get; }
    }
}