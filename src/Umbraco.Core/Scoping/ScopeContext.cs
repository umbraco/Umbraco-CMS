using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Scoping
{
    internal class ScopeContext : IScopeContext, IInstanceIdentifiable
    {
        private Dictionary<string, IEnlistedObject> _enlisted;

        public void ScopeExit(bool completed)
        {
            if (_enlisted == null)
                return;

            // fixme - can we create infinite loops?
            // fixme - what about nested events? will they just be plainly ignored = really bad?

            List<Exception> exceptions = null;
            List<IEnlistedObject> orderedEnlisted;
            while ((orderedEnlisted = _enlisted.Values.OrderBy(x => x.Priority).ToList()).Count > 0)
            {
                _enlisted.Clear();
                foreach (var enlisted in orderedEnlisted)
                {
                    try
                    {
                        enlisted.Execute(completed);
                    }
                    catch (Exception e)
                    {
                        if (exceptions == null)
                            exceptions = new List<Exception>();
                        exceptions.Add(e);
                    }
                }
            }

            if (exceptions != null)
                throw new AggregateException("Exceptions were thrown by listed actions.", exceptions);
        }

        public Guid InstanceId { get; } = Guid.NewGuid();

        private IDictionary<string, IEnlistedObject> Enlisted => _enlisted
            ?? (_enlisted = new Dictionary<string, IEnlistedObject>());

        private interface IEnlistedObject
        {
            void Execute(bool completed);
            int Priority { get; }
        }

        private class EnlistedObject<T> : IEnlistedObject
        {
            private readonly Action<bool, T> _action;

            public EnlistedObject(T item, Action<bool, T> action, int priority)
            {
                Item = item;
                Priority = priority;
                _action = action;
            }

            public T Item { get; }

            public int Priority { get; }

            public void Execute(bool completed)
            {
                _action(completed, Item);
            }
        }

        public void Enlist(string key, Action<bool> action, int priority = 100)
        {
            Enlist<object>(key, null, (completed, item) => action(completed), priority);
        }

        public T Enlist<T>(string key, Func<T> creator, Action<bool, T> action = null, int priority = 100)
        {
            var enlistedObjects = _enlisted ?? (_enlisted = new Dictionary<string, IEnlistedObject>());

            if (enlistedObjects.TryGetValue(key, out var enlisted))
            {
                if (!(enlisted is EnlistedObject<T> enlistedAs))
                    throw new InvalidOperationException("An item with the key already exists, but with a different type.");
                if (enlistedAs.Priority != priority)
                    throw new InvalidOperationException("An item with the key already exits, but with a different priority.");
                return enlistedAs.Item;
            }
            var enlistedOfT = new EnlistedObject<T>(creator == null ? default : creator(), action, priority);
            Enlisted[key] = enlistedOfT;
            return enlistedOfT.Item;
        }

        public T GetEnlisted<T>(string key)
        {
            var enlistedObjects = _enlisted;
            if (enlistedObjects == null) return default;

            if (enlistedObjects.TryGetValue(key, out var enlisted) == false)
                return default;

            if (!(enlisted is EnlistedObject<T> enlistedAs))
                throw new InvalidOperationException("An item with the key exists, but with a different type.");
            return enlistedAs.Item;
        }
    }
}
