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
    public PropertyTypeCollection(bool supportsPublishing) => SupportsPublishing = supportsPublishing;

    public PropertyTypeCollection(bool supportsPublishing, IEnumerable<IPropertyType> properties)
        : this(supportsPublishing) =>
        Reset(properties);

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public bool SupportsPublishing { get; }

    // This baseclass calling is needed, else compiler will complain about nullability

    /// <inheritdoc />
    public bool IsReadOnly => ((ICollection<IPropertyType>)this).IsReadOnly;

    // 'new' keyword is required! we can explicitly implement ICollection<IPropertyType>.Add BUT since normally a concrete PropertyType type
    // is passed in, the explicit implementation doesn't get called, this ensures it does get called.
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

    protected override void SetItem(int index, IPropertyType item)
    {
        item.SupportsPublishing = SupportsPublishing;
        IPropertyType oldItem = index >= 0 ? this[index] : item;
        base.SetItem(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
        item.PropertyChanged += Item_PropertyChanged;
    }

    protected override void RemoveItem(int index)
    {
        IPropertyType removed = this[index];
        base.RemoveItem(index);
        removed.PropertyChanged -= Item_PropertyChanged;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
    }

    protected override void InsertItem(int index, IPropertyType item)
    {
        item.SupportsPublishing = SupportsPublishing;
        base.InsertItem(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        item.PropertyChanged += Item_PropertyChanged;
    }

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

    public bool RemoveItem(string propertyTypeAlias)
    {
        var key = IndexOfKey(propertyTypeAlias);
        if (key != -1)
        {
            RemoveItem(key);
        }

        return key != -1;
    }

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

    protected override string GetKeyForItem(IPropertyType item) => item.Alias;

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args) =>
        CollectionChanged?.Invoke(this, args);
}
