using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public interface IProperty : IEntity, IRememberBeingDirty
    {

        ValueStorageType ValueStorageType { get; }
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

        /// <summary>
        /// Gets the value.
        /// </summary>
        object GetValue(string culture = null, string segment = null, bool published = false);

        /// <summary>
        /// Sets a value.
        /// </summary>
        void SetValue(object value, string culture = null, string segment = null);

        int PropertyTypeId { get; }
        void PublishValues(string culture = "*", string segment = "*");
        void UnpublishValues(string culture = "*", string segment = "*");

    }
}
