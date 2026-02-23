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

    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <inheritdoc />
    public object DeepClone()
    {
        var clone = new PropertyGroupCollection();
        foreach (PropertyGroup group in this)
        {
            clone.Add((PropertyGroup)group.DeepClone());
        }

        return clone;
    }

    /// <summary>
    ///     Adds a property group to the collection.
    /// </summary>
    /// <param name="item">The property group to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the property group does not have an alias set.</exception>
    /// <exception cref="ArgumentException">Thrown when changing the alias would result in duplicates.</exception>
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

    /// <summary>
    ///     Determines whether the collection contains a property group with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier to search for.</param>
    /// <returns><c>true</c> if a property group with the specified identifier exists; otherwise, <c>false</c>.</returns>
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

    /// <inheritdoc />
    protected override void SetItem(int index, PropertyGroup item)
    {
        PropertyGroup oldItem = index >= 0 ? this[index] : item;

        base.SetItem(index, item);

        oldItem.Collection = null;
        item.Collection = this;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
    }

    /// <inheritdoc />
    protected override void RemoveItem(int index)
    {
        PropertyGroup removed = this[index];

        base.RemoveItem(index);

        removed.Collection = null;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
    }

    /// <inheritdoc />
    protected override void InsertItem(int index, PropertyGroup item)
    {
        base.InsertItem(index, item);

        item.Collection = this;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    /// <inheritdoc />
    protected override void ClearItems()
    {
        foreach (PropertyGroup item in this)
        {
            item.Collection = null;
        }

        base.ClearItems();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    ///     Changes the key (alias) of a property group in the collection.
    /// </summary>
    /// <param name="item">The property group to update.</param>
    /// <param name="newKey">The new key (alias) value.</param>
    internal void ChangeKey(PropertyGroup item, string newKey) => ChangeItemKey(item, newKey);

    /// <summary>
    ///     Gets the index of the property group with the specified alias.
    /// </summary>
    /// <param name="key">The alias to search for.</param>
    /// <returns>The zero-based index of the property group, or -1 if not found.</returns>
    public int IndexOfKey(string key) => this.FindIndex(x => x.Alias == key);

    /// <summary>
    ///     Gets the index of the property group with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier to search for.</param>
    /// <returns>The zero-based index of the property group, or -1 if not found.</returns>
    public int IndexOfKey(int id) => this.FindIndex(x => x.Id == id);

    /// <summary>
    ///     Clears all <see cref="CollectionChanged" /> event handlers
    /// </summary>
    public void ClearCollectionChangedEvents() => CollectionChanged = null;

    /// <inheritdoc />
    protected override string GetKeyForItem(PropertyGroup item) => item.Alias;

    /// <summary>
    ///     Raises the <see cref="CollectionChanged" /> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args) =>
        CollectionChanged?.Invoke(this, args);
}
