using System.Collections;

namespace Umbraco.Cms.Core.Collections;

/// <summary>
///     A thread-safe representation of a <see cref="HashSet{T}" />.
///     Enumerating this collection is thread-safe and will only operate on a clone that is generated before returning the
///     enumerator.
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class ConcurrentHashSet<T> : ICollection<T>
{
    private readonly HashSet<T> _innerSet = new();
    private readonly ReaderWriterLockSlim _instanceLocker = new(LockRecursionPolicy.NoRecursion);

    /// <summary>
    ///     Gets the number of elements contained in the <see cref="T:System.Collections.ICollection" />.
    /// </summary>
    /// <returns>
    ///     The number of elements contained in the <see cref="T:System.Collections.ICollection" />.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public int Count
    {
        get
        {
            try
            {
                _instanceLocker.EnterReadLock();
                return _innerSet.Count;
            }
            finally
            {
                if (_instanceLocker.IsReadLockHeld)
                {
                    _instanceLocker.ExitReadLock();
                }
            }
        }
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public IEnumerator<T> GetEnumerator() => GetThreadSafeClone().GetEnumerator();

    /// <summary>
    ///     Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     Removes the first occurrence of a specific object from the
    ///     <see cref="T:System.Collections.Generic.ICollection`1" />.
    /// </summary>
    /// <returns>
    ///     true if <paramref name="item" /> was successfully removed from the
    ///     <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if
    ///     <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
    /// </returns>
    /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <exception cref="T:System.NotSupportedException">
    ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is
    ///     read-only.
    /// </exception>
    public bool Remove(T item)
    {
        try
        {
            _instanceLocker.EnterWriteLock();
            return _innerSet.Remove(item);
        }
        finally
        {
            if (_instanceLocker.IsWriteLockHeld)
            {
                _instanceLocker.ExitWriteLock();
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
    /// </summary>
    /// <returns>
    ///     true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
    /// </returns>
    public bool IsReadOnly => false;

    /// <summary>
    ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    /// <exception cref="T:System.NotSupportedException">
    ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is
    ///     read-only.
    /// </exception>
    public void Add(T item)
    {
        try
        {
            _instanceLocker.EnterWriteLock();
            _innerSet.Add(item);
        }
        finally
        {
            if (_instanceLocker.IsWriteLockHeld)
            {
                _instanceLocker.ExitWriteLock();
            }
        }
    }

    /// <summary>
    ///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
    /// </summary>
    /// <exception cref="T:System.NotSupportedException">
    ///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is
    ///     read-only.
    /// </exception>
    public void Clear()
    {
        try
        {
            _instanceLocker.EnterWriteLock();
            _innerSet.Clear();
        }
        finally
        {
            if (_instanceLocker.IsWriteLockHeld)
            {
                _instanceLocker.ExitWriteLock();
            }
        }
    }

    /// <summary>
    ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
    /// </summary>
    /// <returns>
    ///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
    ///     otherwise, false.
    /// </returns>
    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
    public bool Contains(T item)
    {
        try
        {
            _instanceLocker.EnterReadLock();
            return _innerSet.Contains(item);
        }
        finally
        {
            if (_instanceLocker.IsReadLockHeld)
            {
                _instanceLocker.ExitReadLock();
            }
        }
    }

    /// <summary>
    ///     Copies the elements of the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" /> to an
    ///     <see cref="T:System.Array" />, starting at a specified index.
    /// </summary>
    /// <param name="array">
    ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
    ///     from the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />. The array must have
    ///     zero-based indexing.
    /// </param>
    /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     <paramref name="array" /> is a null reference (Nothing in Visual
    ///     Basic).
    /// </exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is less than zero.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///     <paramref name="index" /> is equal to or greater than the length of the
    ///     <paramref name="array" /> -or- The number of elements in the source
    ///     <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> is greater than the available space from
    ///     <paramref name="index" /> to the end of the destination <paramref name="array" />.
    /// </exception>
    public void CopyTo(T[] array, int index)
    {
        HashSet<T> clone = GetThreadSafeClone();
        clone.CopyTo(array, index);
    }

    /// <summary>
    ///     Attempts to add an item to the collection
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool TryAdd(T item)
    {
        if (Contains(item))
        {
            return false;
        }

        try
        {
            _instanceLocker.EnterWriteLock();

            // double check
            if (_innerSet.Contains(item))
            {
                return false;
            }

            _innerSet.Add(item);
            return true;
        }
        finally
        {
            if (_instanceLocker.IsWriteLockHeld)
            {
                _instanceLocker.ExitWriteLock();
            }
        }
    }

    /// <summary>
    ///     Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />,
    ///     starting at a particular <see cref="T:System.Array" /> index.
    /// </summary>
    /// <param name="array">
    ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
    ///     from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based
    ///     indexing.
    /// </param>
    /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null. </exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is less than zero. </exception>
    /// <exception cref="T:System.ArgumentException">
    ///     <paramref name="array" /> is multidimensional.-or- The number of elements
    ///     in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from
    ///     <paramref name="index" /> to the end of the destination <paramref name="array" />.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    ///     The type of the source <see cref="T:System.Collections.ICollection" />
    ///     cannot be cast automatically to the type of the destination <paramref name="array" />.
    /// </exception>
    /// <filterpriority>2</filterpriority>
    public void CopyTo(Array array, int index)
    {
        HashSet<T> clone = GetThreadSafeClone();
        Array.Copy(clone.ToArray(), 0, array, index, clone.Count);
    }

    private HashSet<T> GetThreadSafeClone()
    {
        HashSet<T>? clone = null;
        try
        {
            _instanceLocker.EnterReadLock();
            clone = new HashSet<T>(_innerSet, _innerSet.Comparer);
        }
        finally
        {
            if (_instanceLocker.IsReadLockHeld)
            {
                _instanceLocker.ExitReadLock();
            }
        }

        return clone;
    }
}
