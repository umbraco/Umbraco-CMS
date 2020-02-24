using System.Collections.Generic;
using System.Collections.Specialized;

namespace Umbraco.Core.Models
{
    public interface IPropertyCollection : IEnumerable<IProperty>, IDeepCloneable, INotifyCollectionChanged
    {
        bool TryGetValue(string propertyTypeAlias, out IProperty property);
        bool Contains(string key);

        /// <summary>
        /// Ensures that the collection contains properties for the specified property types.
        /// </summary>
        void EnsurePropertyTypes(IEnumerable<IPropertyType> propertyTypes);

        /// <summary>
        /// Ensures that the collection does not contain properties not in the specified property types.
        /// </summary>
        void EnsureCleanPropertyTypes(IEnumerable<IPropertyType> propertyTypes);

        /// <summary>
        /// Gets the property with the specified alias.
        /// </summary>
        IProperty this[string name] { get; }

        /// <summary>
        /// Gets the property at the specified index.
        /// </summary>
        IProperty this[int index] { get; }

        /// <summary>
        /// Adds or updates a property.
        /// </summary>
        void Add(IProperty property);

        int Count { get; }
    }
}
