using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a member type
    /// </summary>
    public interface IMemberGroup : IAggregateRoot, IRememberBeingDirty, ICanBeDirty
    {
        /// <summary>
        /// The name of the member group
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Profile of the user who created this Entity
        /// </summary>
        int CreatorId { get; set; }

        /// <summary>
        /// Some entities may expose additional data that other's might not, this custom data will be available in this collection
        /// </summary>
        IDictionary<string, object> AdditionalData { get; }
    }
}