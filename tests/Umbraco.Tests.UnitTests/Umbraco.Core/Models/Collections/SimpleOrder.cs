// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.Collections;

public class SimpleOrder : KeyedCollection<int, OrderItem>, INotifyCollectionChanged
{
    // The parameterless constructor of the base class creates a
    // KeyedCollection with an internal dictionary. For this code
    // example, no other constructors are exposed.
    public SimpleOrder()
    {
    }

    public SimpleOrder(IEnumerable<OrderItem> properties) => Reset(properties);

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    // This is the only method that absolutely must be overridden,
    // because without it the KeyedCollection cannot extract the
    // keys from the items. The input parameter type is the
    // second generic type argument, in this case OrderItem, and
    // the return value type is the first generic type argument,
    // in this case int.
    protected override int GetKeyForItem(OrderItem item) =>

        // In this example, the key is the part number.
        item.PartNumber;

    internal void Reset(IEnumerable<OrderItem> properties)
    {
        Clear();
        foreach (var property in properties)
        {
            Add(property);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    protected override void SetItem(int index, OrderItem item)
    {
        base.SetItem(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    protected override void RemoveItem(int index)
    {
        var removed = this[index];
        base.RemoveItem(index);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
    }

    protected override void InsertItem(int index, OrderItem item)
    {
        base.InsertItem(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public new bool Contains(int partNumber) => this.Any(x => x.PartNumber == partNumber);

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args) =>
        CollectionChanged?.Invoke(this, args);
}
