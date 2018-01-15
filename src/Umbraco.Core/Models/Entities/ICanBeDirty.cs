using System.Collections.Generic;

namespace Umbraco.Core.Models.Entities
{
    /// <summary>
    /// Defines an entity that tracks property changes and can be dirty.
    /// </summary>
    public interface ICanBeDirty
    {
        /// <summary>
        /// Determines whether the current entity is dirty.
        /// </summary>
        bool IsDirty();

        /// <summary>
        /// Determines whether a specific property is dirty.
        /// </summary>
        bool IsPropertyDirty(string propName);

        /// <summary>
        /// Gets properties that are dirty.
        /// </summary>
        IEnumerable<string> GetDirtyProperties();

        /// <summary>
        /// Resets dirty properties.
        /// </summary>
        void ResetDirtyProperties();
    }
}
