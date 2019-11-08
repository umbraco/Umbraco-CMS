using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public interface IProperty
    {
        /// <summary>
        /// Returns the PropertyType, which this Property is based on
        /// </summary>
        IPropertyType PropertyType { get; }

        /// <summary>
        /// Gets the list of values.
        /// </summary>
        IReadOnlyCollection<IPropertyValue> Values { get; set; }

        /// <summary>
        /// Returns the Alias of the PropertyType, which this Property is based on
        /// </summary>
        string Alias { get; }

        /// <inheritdoc />
        int Id { get; set; }

        /// <inheritdoc />
        Guid Key { get; set; }

        /// <inheritdoc />
        DateTime CreateDate { get; set; }

        /// <inheritdoc />
        DateTime UpdateDate { get; set; }

        /// <inheritdoc />
        DateTime? DeleteDate { get; set; } // no change tracking - not persisted

        /// <inheritdoc />
        bool HasIdentity { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        object GetValue(string culture = null, string segment = null, bool published = false);

        /// <summary>
        /// Sets a value.
        /// </summary>
        void SetValue(object value, string culture = null, string segment = null);

        /// <summary>
        /// Resets the entity identity.
        /// </summary>
        void ResetIdentity();

        bool Equals(EntityBase other);
        bool Equals(object obj);
        int GetHashCode();
        object DeepClone();

        /// <inheritdoc />
        bool IsDirty();

        /// <inheritdoc />
        bool IsPropertyDirty(string propertyName);

        /// <inheritdoc />
        IEnumerable<string> GetDirtyProperties();

        /// <inheritdoc />
        /// <remarks>Saves dirty properties so they can be checked with WasDirty.</remarks>
        void ResetDirtyProperties();

        /// <inheritdoc />
        bool WasDirty();

        /// <inheritdoc />
        bool WasPropertyDirty(string propertyName);

        /// <inheritdoc />
        void ResetWereDirtyProperties();

        /// <inheritdoc />
        void ResetDirtyProperties(bool rememberDirty);

        /// <inheritdoc />
        IEnumerable<string> GetWereDirtyProperties();

        /// <summary>
        /// Disables change tracking.
        /// </summary>
        void DisableChangeTracking();

        /// <summary>
        /// Enables change tracking.
        /// </summary>
        void EnableChangeTracking();
    }
}
