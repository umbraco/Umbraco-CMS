using System.Collections.Generic;
using System.Collections.Specialized;

namespace Umbraco.Core.Models
{
    public interface IPropertyCollection : IEnumerable<IProperty>
    {
        bool TryGetValue(string propertyTypeAlias, out IProperty property);
        bool Contains(string key);

        event NotifyCollectionChangedEventHandler CollectionChanged;

        void EnsurePropertyTypes(IEnumerable<IPropertyType> propertyTypes);
        void EnsureCleanPropertyTypes(IEnumerable<PropertyType> propertyTypes);
        object DeepClone();

        IProperty this[string name] { get; }
        IProperty this[int index] { get; }
        void Add(IProperty property);

        int Count { get; }
    }
}
