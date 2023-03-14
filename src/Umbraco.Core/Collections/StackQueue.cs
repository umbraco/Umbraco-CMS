namespace Umbraco.Cms.Core.Collections;

/// <summary>
///     Collection that can be both a queue and a stack.
/// </summary>
/// <typeparam name="T"></typeparam>
public class StackQueue<T>
{
    private readonly LinkedList<T?> _linkedList = new();

    public int Count => _linkedList.Count;

    public void Clear() => _linkedList.Clear();

    public void Push(T? obj) => _linkedList.AddFirst(obj);

    public void Enqueue(T? obj) => _linkedList.AddFirst(obj);

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

    public T? PeekStack() => _linkedList.First is not null ? _linkedList.First.Value : default;

    public T? PeekQueue() => _linkedList.Last is not null ? _linkedList.Last.Value : default;
}
