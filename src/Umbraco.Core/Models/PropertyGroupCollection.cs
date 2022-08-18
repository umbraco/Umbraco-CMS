using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a collection of <see cref="PropertyGroup" /> objects
/// </summary>
[Serializable]
[DataContract]

// TODO: Change this to ObservableDictionary so we can reduce the INotifyCollectionChanged implementation details
public class PropertyGroupCollection : KeyedCollection<string, PropertyGroup>, INotifyCollectionChanged, IDeepCloneable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyGroupCollection" /> class.
    /// </summary>
    public PropertyGroupCollection()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyGroupCollection" /> class.
    /// </summary>
    /// <param name="groups">The groups.</param>
    public PropertyGroupCollection(IEnumerable<PropertyGroup> groups) => Reset(groups);

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public object DeepClone()
    {
        var clone = new PropertyGroupCollection();
        foreach (PropertyGroup group in this)
        {
            clone.Add((PropertyGroup)group.DeepClone());
        }

        return clone;
    }

    public new void Add(PropertyGroup item)
    {
        // Ensure alias is set
        if (string.IsNullOrEmpty(item.Alias))
        {
            throw new InvalidOperationException("Set an alias before adding the property group.");
        }

        // Note this is done to ensure existing groups can be renamed
        if (item.HasIdentity && item.Id > 0)
        {
            var index = IndexOfKey(item.Id);
            if (index != -1)
            {
                var keyExists = Contains(item.Alias);
                if (keyExists)
                {
                    throw new ArgumentException(
                        $"Naming conflict: changing the alias of property group '{item.Name}' would result in duplicates.");
                }

                // Collection events will be raised in SetItem
                SetItem(index, item);
                return;
            }
        }
        else
        {
            var index = IndexOfKey(item.Alias);
            if (index != -1)
            {
                // Collection events will be raised in SetItem
                SetItem(index, item);
                return;
            }
        }

        // Collection events will be raised in InsertItem
        base.Add(item);
    }

    public bool Contains(int id) => IndexOfKey(id) != -1;

    /// <summary>
    ///     Resets the collection to only contain the <see cref="PropertyGroup" /> instances referenced in the
    ///     <paramref name="groups" /> parameter.
    /// </summary>
    /// <param name="groups">The property groups.</param>
    /// <remarks></remarks>
    internal void Reset(IEnumerable<PropertyGroup> groups)
    {
        // Collection events will be raised in each of these calls
        Clear();

        // Collection events will be raised in each of these calls
        foreach (PropertyGroup group in groups)
        {
            Add(group);
        }
    }

    protected override void SetItem(int index, PropertyGroup item)
    {
        PropertyGroup oldItem = index >= 0 ? this[index] : item;

        base.SetItem(index, item);

        oldItem.Collection = null;
        item.Collection = this;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
    }

    protected override void RemoveItem(int index)
    {
        PropertyGroup removed = this[index];

        base.RemoveItem(index);

        removed.Collection = null;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
    }

    protected override void InsertItem(int index, PropertyGroup item)
    {
        base.InsertItem(index, item);

        item.Collection = this;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    protected override void ClearItems()
    {
        foreach (PropertyGroup item in this)
        {
            item.Collection = null;
        }

        base.ClearItems();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    internal void ChangeKey(PropertyGroup item, string newKey) => ChangeItemKey(item, newKey);

    public int IndexOfKey(string key) => this.FindIndex(x => x.Alias == key);

    public int IndexOfKey(int id) => this.FindIndex(x => x.Id == id);

    /// <summary>
    ///     Clears all <see cref="CollectionChanged" /> event handlers
    /// </summary>
    public void ClearCollectionChangedEvents() => CollectionChanged = null;

    protected override string GetKeyForItem(PropertyGroup item) => item.Alias;

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args) =>
        CollectionChanged?.Invoke(this, args);
}
