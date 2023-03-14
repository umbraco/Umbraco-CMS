using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a collection of property values.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class PropertyCollection : KeyedCollection<string, IProperty>, IPropertyCollection
{
    private readonly object _addLocker = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyCollection" /> class.
    /// </summary>
    public PropertyCollection()
        : base(StringComparer.InvariantCultureIgnoreCase)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyCollection" /> class.
    /// </summary>
    public PropertyCollection(IEnumerable<IProperty> properties)
        : this() =>
        Reset(properties);

    /// <summary>
    ///     Occurs when the collection changes.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    ///     Gets the property with the specified PropertyType.
    /// </summary>
    internal IProperty? this[IPropertyType propertyType] =>
        this.FirstOrDefault(x => x.Alias.InvariantEquals(propertyType.Alias));

    /// <inheritdoc />
    public new void Add(IProperty property)
    {
        // TODO: why are we locking here and not everywhere else?!
        lock (_addLocker)
        {
            var key = GetKeyForItem(property);
            if (key != null)
            {
                if (Contains(key))
                {
                    // transfer id and values if ...
                    IProperty existing = this[key];

                    if (property.Id == 0 && existing.Id != 0)
                    {
                        property.Id = existing.Id;
                    }

                    if (property.Values.Count == 0 && existing.Values.Count > 0)
                    {
                        property.Values = existing.Values.Select(x => x.Clone()).ToList();
                    }

                    // replace existing with property and return,
                    // SetItem invokes OnCollectionChanged (but not OnAdd)
                    SetItem(IndexOfKey(key), property);
                    return;
                }
            }

            // collection events will be raised in InsertItem with Add
            base.Add(property);
        }
    }

    public new bool TryGetValue(string propertyTypeAlias, [MaybeNullWhen(false)] out IProperty property)
    {
        property = this.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
        return property != null;
    }

    public void ClearCollectionChangedEvents() => CollectionChanged = null;

    /// <inheritdoc />
    public void EnsurePropertyTypes(IEnumerable<IPropertyType> propertyTypes)
    {
        if (propertyTypes == null)
        {
            return;
        }

        foreach (IPropertyType propertyType in propertyTypes)
        {
            Add(new Property(propertyType));
        }
    }

    /// <inheritdoc />
    public void EnsureCleanPropertyTypes(IEnumerable<IPropertyType> propertyTypes)
    {
        if (propertyTypes == null)
        {
            return;
        }

        IPropertyType[] propertyTypesA = propertyTypes.ToArray();

        IEnumerable<string> thisAliases = this.Select(x => x.Alias);
        IEnumerable<string> typeAliases = propertyTypesA.Select(x => x.Alias);
        var remove = thisAliases.Except(typeAliases).ToArray();
        foreach (var alias in remove)
        {
            if (alias is not null)
            {
                Remove(alias);
            }
        }

        foreach (IPropertyType propertyType in propertyTypesA)
        {
            Add(new Property(propertyType));
        }
    }

    /// <summary>
    ///     Deep clones.
    /// </summary>
    public object DeepClone()
    {
        var clone = new PropertyCollection();
        foreach (IProperty property in this)
        {
            clone.Add((Property)property.DeepClone());
        }

        return clone;
    }

    /// <summary>
    ///     Replaces the property at the specified index with the specified property.
    /// </summary>
    protected override void SetItem(int index, IProperty property)
    {
        IProperty oldItem = index >= 0 ? this[index] : property;
        base.SetItem(index, property);
        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, property, oldItem));
    }

    /// <summary>
    ///     Replaces all properties, whilst maintaining validation delegates.
    /// </summary>
    private void Reset(IEnumerable<IProperty> properties)
    {
        // collection events will be raised in each of these calls
        Clear();

        // collection events will be raised in each of these calls
        foreach (IProperty property in properties)
        {
            Add(property);
        }
    }

    /// <summary>
    ///     Removes the property at the specified index.
    /// </summary>
    protected override void RemoveItem(int index)
    {
        IProperty removed = this[index];
        base.RemoveItem(index);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
    }

    /// <summary>
    ///     Inserts the specified property at the specified index.
    /// </summary>
    protected override void InsertItem(int index, IProperty property)
    {
        base.InsertItem(index, property);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, property));
    }

    /// <summary>
    ///     Removes all properties.
    /// </summary>
    protected override void ClearItems()
    {
        base.ClearItems();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    protected override string GetKeyForItem(IProperty item) => item.Alias;

    /// <summary>
    ///     Gets the index for a specified property alias.
    /// </summary>
    private int IndexOfKey(string key)
    {
        for (var i = 0; i < Count; i++)
        {
            if (this[i].Alias?.InvariantEquals(key) ?? false)
            {
                return i;
            }
        }

        return -1;
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args) =>
        CollectionChanged?.Invoke(this, args);
}
