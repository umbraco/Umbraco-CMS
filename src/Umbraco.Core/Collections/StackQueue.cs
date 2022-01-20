﻿using System.Collections.Generic;

namespace Umbraco.Core.Collections
{
    /// <summary>
    ///     Collection that can be both a queue and a stack.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StackQueue<T>
    {
        private readonly LinkedList<T> _linkedList = new();

        public int Count => _linkedList.Count;

        public void Clear() => _linkedList.Clear();

        public void Push(T obj) => _linkedList.AddFirst(obj);

        public void Enqueue(T obj) => _linkedList.AddFirst(obj);

        public T Pop()
        {
            T obj = _linkedList.First.Value;
            _linkedList.RemoveFirst();
            return obj;
        }

        public T Dequeue()
        {
            T obj = _linkedList.Last.Value;
            _linkedList.RemoveLast();
            return obj;
        }

        public T PeekStack() => _linkedList.First.Value;

        public T PeekQueue() => _linkedList.Last.Value;
    }
}
