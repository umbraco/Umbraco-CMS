using System.Collections.Generic;

namespace Umbraco.Core.Models.EntityBase
{
    public interface IUmbracoEntity : IAggregateRoot, IRememberBeingDirty, ICanBeDirty
    {
        /// <summary>
        /// Profile of the user who created this Entity
        /// </summary>
        int CreatorId { get; set; }

        /// <summary>
        /// Gets or sets the level of the Entity
        /// </summary>
        int Level { get; set; }

        /// <summary>
        /// Gets or Sets the Name of the Entity
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Parent Entity
        /// </summary>
        int ParentId { get; set; }

        /// <summary>
        /// Gets or sets the path to the Entity
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Gets or sets the sort order of the Entity
        /// </summary>
        int SortOrder { get; set; }

        /// <summary>
        /// Boolean indicating whether this Entity is Trashed or not.
        /// If an Entity is Trashed it will be located in the Recyclebin.
        /// </summary>
        /// <remarks>
        /// When content is trashed it should be unpublished.
        /// Not all entities support being trashed, they'll always return false.
        /// </remarks>
        bool Trashed { get; }

        /// <summary>
        /// Some entities may expose additional data that other's might not, this custom data will be available in this collection
        /// </summary>
        IDictionary<string, object> AdditionalData { get; }
    }
}