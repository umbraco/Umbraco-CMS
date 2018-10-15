using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Scoping
{
    public class ScopeContext : IInstanceIdentifiable
    {
        private Dictionary<string, IEnlistedObject> _enlisted;
        private bool _exiting;

        public void ScopeExit(bool completed)
        {
            if (_enlisted == null)
                return;

            _exiting = true;

            List<Exception> exceptions = null;
            foreach (var enlisted in _enlisted.Values.OrderBy(x => x.Priority))
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

            if (exceptions != null)
                throw new AggregateException("Exceptions were thrown by listed actions.", exceptions);
        }

        private readonly Guid _instanceId = Guid.NewGuid();
        public Guid InstanceId { get { return _instanceId; } }

        private IDictionary<string, IEnlistedObject> Enlisted
        {
            get
            {
                return _enlisted ?? (_enlisted
                    = new Dictionary<string, IEnlistedObject>());
            }
        }

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

            public T Item { get; private set; }

            public int Priority { get; private set; }

            public void Execute(bool completed)
            {
                _action(completed, Item);
            }
        }

        // todo: replace with optional parameters when we can break things
        public T Enlist<T>(string key, Func<T> creator)
        {
            return Enlist(key, creator, null, 100);
        }

        // todo: replace with optional parameters when we can break things
        public T Enlist<T>(string key, Func<T> creator, Action<bool, T> action)
        {
            return Enlist(key, creator, action, 100);
        }

        // todo: replace with optional parameters when we can break things
        public void Enlist(string key, Action<bool> action)
        {
            Enlist<object>(key, null, (completed, item) => action(completed), 100);
        }

        public void Enlist(string key, Action<bool> action, int priority)
        {
            Enlist<object>(key, null, (completed, item) => action(completed), priority);
        }

        public T Enlist<T>(string key, Func<T> creator, Action<bool, T> action, int priority)
        {
            if (_exiting)
                throw new InvalidOperationException("Cannot enlist now, context is exiting.");

            var enlistedObjects = _enlisted ?? (_enlisted = new Dictionary<string, IEnlistedObject>());

            IEnlistedObject enlisted;
            if (enlistedObjects.TryGetValue(key, out enlisted))
            {
                var enlistedAs = enlisted as EnlistedObject<T>;
                if (enlistedAs == null) throw new InvalidOperationException("An item with the key already exists, but with a different type.");
                if (enlistedAs.Priority != priority) throw new InvalidOperationException("An item with the key already exits, but with a different priority.");
                return enlistedAs.Item;
            }
            var enlistedOfT = new EnlistedObject<T>(creator == null ? default(T) : creator(), action, priority);
            Enlisted[key] = enlistedOfT;
            return enlistedOfT.Item;
        }

        public T GetEnlisted<T>(string key)
        {
            var enlistedObjects = _enlisted;
            if (enlistedObjects == null) return default (T);

            IEnlistedObject enlisted;
            if (enlistedObjects.TryGetValue(key, out enlisted) == false)
                return default (T);

            var enlistedAs = enlisted as EnlistedObject<T>;
            if (enlistedAs == null) throw new InvalidOperationException("An item with the key exists, but with a different type.");
            return enlistedAs.Item;
        }
    }
}
