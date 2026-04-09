using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

// public interface IPropertyTypeCollection: IEnumerable<IPropertyType>

/// <summary>
///     Represents a collection of <see cref="IPropertyType" /> objects.
/// </summary>
[Serializable]
[DataContract]

// TODO: Change this to ObservableDictionary so we can reduce the INotifyCollectionChanged implementation details
public class PropertyTypeCollection : KeyedCollection<string, IPropertyType>, INotifyCollectionChanged, IDeepCloneable,
    ICollection<IPropertyType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyTypeCollection" /> class.
    /// </summary>
    /// <param name="supportsPublishing">A value indicating whether the property types in this collection support publishing.</param>
    public PropertyTypeCollection(bool supportsPublishing) => SupportsPublishing = supportsPublishing;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyTypeCollection" /> class with initial property types.
    /// </summary>
    /// <param name="supportsPublishing">A value indicating whether the property types in this collection support publishing.</param>
    /// <param name="properties">The initial property types to add to the collection.</param>
    public PropertyTypeCollection(bool supportsPublishing, IEnumerable<IPropertyType> properties)
        : this(supportsPublishing) =>
        Reset(properties);

    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    ///     Gets a value indicating whether the property types in this collection support publishing.
    /// </summary>
    public bool SupportsPublishing { get; }

    // This baseclass calling is needed, else compiler will complain about nullability

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <summary>
    ///     Adds a property type to the collection.
    /// </summary>
    /// <param name="item">The property type to add.</param>
    /// <remarks>
    ///     The 'new' keyword is required because we can explicitly implement <see cref="ICollection{T}.Add" />,
    ///     but since normally a concrete PropertyType type is passed in, the explicit implementation doesn't get called.
    ///     This ensures it does get called.
    /// </remarks>
    public new void Add(IPropertyType item)
    {
        item.SupportsPublishing = SupportsPublishing;

        // TODO: this is not pretty and should be refactored
        var key = GetKeyForItem(item);
        if (key != null)
        {
            var exists = Contains(key);
            if (exists)
            {
                // collection events will be raised in SetItem
                SetItem(IndexOfKey(key), item);
                return;
            }
        }

        // check if the item's sort order is already in use
        if (this.Any(x => x.SortOrder == item.SortOrder))
        {
            // make it the next iteration
            item.SortOrder = this.Max(x => x.SortOrder) + 1;
        }

        // collection events will be raised in InsertItem
        base.Add(item);
    }

    /// <inheritdoc />
    public object DeepClone()
    {
        var clone = new PropertyTypeCollection(SupportsPublishing);
        foreach (IPropertyType propertyType in this)
        {
            clone.Add((IPropertyType)propertyType.DeepClone());
        }

        return clone;
    }

    /// <summary>
    ///     Determines whether this collection contains a <see cref="Property" /> whose alias matches the specified
    ///     PropertyType.
    /// </summary>
    /// <param name="propertyAlias">Alias of the PropertyType.</param>
    /// <returns><c>true</c> if the collection contains the specified alias; otherwise, <c>false</c>.</returns>
    /// <remarks></remarks>
    public new bool Contains(string propertyAlias) => this.Any(x => x.Alias == propertyAlias);

    /// <summary>
    ///     Resets the collection to only contain the <see cref="IPropertyType" /> instances referenced in the
    ///     <paramref name="properties" /> parameter.
    /// </summary>
    /// <param name="properties">The properties.</param>
    /// <remarks></remarks>
    internal void Reset(IEnumerable<IPropertyType> properties)
    {
        // collection events will be raised in each of these calls
        Clear();

        // collection events will be raised in each of these calls
        foreach (IPropertyType property in properties)
        {
            Add(property);
        }
    }

    /// <inheritdoc />
    protected override void SetItem(int index, IPropertyType item)
    {
        item.SupportsPublishing = SupportsPublishing;
        IPropertyType oldItem = index >= 0 ? this[index] : item;
        base.SetItem(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
        item.PropertyChanged += Item_PropertyChanged;
    }

    /// <inheritdoc />
    protected override void RemoveItem(int index)
    {
        IPropertyType removed = this[index];
        base.RemoveItem(index);
        removed.PropertyChanged -= Item_PropertyChanged;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
    }

    /// <inheritdoc />
    protected override void InsertItem(int index, IPropertyType item)
    {
        item.SupportsPublishing = SupportsPublishing;
        base.InsertItem(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        item.PropertyChanged += Item_PropertyChanged;
    }

    /// <inheritdoc />
    protected override void ClearItems()
    {
        base.ClearItems();
        foreach (IPropertyType item in this)
        {
            item.PropertyChanged -= Item_PropertyChanged;
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    ///     Occurs when a property changes on a IPropertyType that exists in this collection
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var propType = (IPropertyType?)sender;
        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, propType, propType));
    }

    /// <summary>
    ///     Removes the property type with the specified alias from the collection.
    /// </summary>
    /// <param name="propertyTypeAlias">The alias of the property type to remove.</param>
    /// <returns><c>true</c> if the property type was found and removed; otherwise, <c>false</c>.</returns>
    public bool RemoveItem(string propertyTypeAlias)
    {
        var key = IndexOfKey(propertyTypeAlias);
        if (key != -1)
        {
            RemoveItem(key);
        }

        return key != -1;
    }

    /// <summary>
    ///     Gets the index of the property type with the specified alias.
    /// </summary>
    /// <param name="key">The alias to search for.</param>
    /// <returns>The zero-based index of the property type, or -1 if not found.</returns>
    public int IndexOfKey(string key)
    {
        for (var i = 0; i < Count; i++)
        {
            if (this[i].Alias == key)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    ///     Clears all <see cref="CollectionChanged" /> event handlers
    /// </summary>
    public void ClearCollectionChangedEvents() => CollectionChanged = null;

    /// <inheritdoc />
    protected override string GetKeyForItem(IPropertyType item) => item.Alias;

    /// <summary>
    ///     Raises the <see cref="CollectionChanged" /> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args) =>
        CollectionChanged?.Invoke(this, args);
}
