namespace Umbraco.Cms.Core.Collections;

/// <summary>
///     Collection that can be both a queue and a stack.
/// </summary>
/// <typeparam name="T"></typeparam>
public class StackQueue<T>
{
    private readonly LinkedList<T?> _linkedList = new();

    /// <summary>
    ///     Gets the number of elements contained in the collection.
    /// </summary>
    public int Count => _linkedList.Count;

    /// <summary>
    ///     Removes all elements from the collection.
    /// </summary>
    public void Clear() => _linkedList.Clear();

    /// <summary>
    ///     Pushes an object onto the stack (adds to the front).
    /// </summary>
    /// <param name="obj">The object to push onto the stack.</param>
    public void Push(T? obj) => _linkedList.AddFirst(obj);

    /// <summary>
    ///     Adds an object to the queue (adds to the front).
    /// </summary>
    /// <param name="obj">The object to add to the queue.</param>
    public void Enqueue(T? obj) => _linkedList.AddFirst(obj);

    /// <summary>
    ///     Removes and returns the object at the top of the stack (from the front).
    /// </summary>
    /// <returns>The object removed from the top of the stack.</returns>
    public T Pop()
    {
        var obj = default(T);
        if (_linkedList.First is not null)
        {
            obj = _linkedList.First.Value;
        }

        _linkedList.RemoveFirst();
        return obj!;
    }

    /// <summary>
    ///     Removes and returns the object at the beginning of the queue (from the back).
    /// </summary>
    /// <returns>The object removed from the beginning of the queue.</returns>
    public T Dequeue()
    {
        var obj = default(T);
        if (_linkedList.Last is not null)
        {
            obj = _linkedList.Last.Value;
        }

        _linkedList.RemoveLast();
        return obj!;
    }

    /// <summary>
    ///     Returns the object at the top of the stack without removing it.
    /// </summary>
    /// <returns>The object at the top of the stack, or the default value if the collection is empty.</returns>
    public T? PeekStack() => _linkedList.First is not null ? _linkedList.First.Value : default;

    /// <summary>
    ///     Returns the object at the beginning of the queue without removing it.
    /// </summary>
    /// <returns>The object at the beginning of the queue, or the default value if the collection is empty.</returns>
    public T? PeekQueue() => _linkedList.Last is not null ? _linkedList.Last.Value : default;
}
