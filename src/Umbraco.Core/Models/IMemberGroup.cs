using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a member type
    /// </summary>
    public interface IMemberGroup : IAggregateRoot
    {
        /// <summary>
        /// The name of the member group
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Profile of the user who created this Entity
        /// </summary>
        int CreatorId { get; set; }
    }
}