using System.Collections.ObjectModel;

namespace Umbraco.Cms.Core.Collections;

/// <summary>
///     A custom collection similar to HashSet{T} which only contains unique items, however this collection keeps items in
///     order
///     and is customizable to keep the newest or oldest equatable item
/// </summary>
/// <typeparam name="T"></typeparam>
public class OrderedHashSet<T> : KeyedCollection<T, T>
    where T : notnull
{
    private readonly bool _keepOldest;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OrderedHashSet{T}" /> class.
    /// </summary>
    /// <param name="keepOldest">
    ///     When <c>true</c>, the oldest item is kept when a duplicate is added;
    ///     when <c>false</c>, the newest item replaces the oldest.
    /// </param>
    public OrderedHashSet(bool keepOldest = true) => _keepOldest = keepOldest;

    /// <inheritdoc />
    protected override void InsertItem(int index, T item)
    {
        if (Dictionary == null)
        {
            base.InsertItem(index, item);
        }
        else
        {
            var exists = Dictionary.ContainsKey(item);

            // if we want to keep the newest, then we need to remove the old item and add the new one
            if (exists == false)
            {
                base.InsertItem(index, item);
            }
            else if (_keepOldest == false)
            {
                if (Remove(item))
                {
                    index--;
                }

                base.InsertItem(index, item);
            }
        }
    }

    /// <inheritdoc />
    protected override T GetKeyForItem(T item) => item;
}
