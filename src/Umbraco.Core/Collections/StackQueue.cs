using System.Collections.Generic;

namespace Umbraco.Core.Collections
{
    /// <summary>
    /// Collection that can be both a queue and a stack.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StackQueue<T>
    {
        private readonly LinkedList<T> _linkedList = new LinkedList<T>();

        public void Clear()
        {
            _linkedList.Clear();
        }

        public void Push(T obj)
        {
            _linkedList.AddFirst(obj);
        }

        public void Enqueue(T obj)
        {
            _linkedList.AddFirst(obj);
        }

        public T Pop()
        {
            var obj = _linkedList.First.Value;
            _linkedList.RemoveFirst();
            return obj;
        }

        public T Dequeue()
        {
            var obj = _linkedList.Last.Value;
            _linkedList.RemoveLast();
            return obj;
        }

        public T PeekStack()
        {
            return _linkedList.First.Value;
        }

        public T PeekQueue()
        {
            return _linkedList.Last.Value;
        }

        public int Count
        {
            get
            {
                return _linkedList.Count;
            }
        }
    }
}
