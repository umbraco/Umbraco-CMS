using System;
using System.Collections.Generic;

namespace Umbraco.Core.Scoping
{
    // fixme should we have an IScopeContext?
    // fixme document all this properly!

    public class ScopeContext : IInstanceIdentifiable
    {
        private Dictionary<string, IEnlistedObject> _enlisted;

        public void ScopeExit(bool completed)
        {
            List<Exception> exceptions = null;
            foreach (var enlisted in Enlisted.Values)
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

        public Guid InstanceId { get; } = Guid.NewGuid();

        private IDictionary<string, IEnlistedObject> Enlisted => _enlisted
            ?? (_enlisted = new Dictionary<string, IEnlistedObject>());

        private interface IEnlistedObject
        {
            void Execute(bool completed);
        }

        private class EnlistedObject<T> : IEnlistedObject
        {
            private readonly Action<bool, T> _action;

            public EnlistedObject(T item)
            {
                Item = item;
            }

            public EnlistedObject(T item, Action<bool, T> action)
            {
                Item = item;
                _action = action;
            }

            public T Item { get; }

            public void Execute(bool completed)
            {
                _action(completed, Item);
            }
        }

        /// <inheritdoc />
        public T Enlist<T>(string key, Func<T> creator)
        {
            IEnlistedObject enlisted;
            if (Enlisted.TryGetValue(key, out enlisted))
            {
                var enlistedAs = enlisted as EnlistedObject<T>;
                if (enlistedAs == null) throw new Exception("An item with a different type has already been enlisted with the same key.");
                return enlistedAs.Item;
            }
            var enlistedOfT = new EnlistedObject<T>(creator());
            Enlisted[key] = enlistedOfT;
            return enlistedOfT.Item;
        }

        /// <inheritdoc />
        public T Enlist<T>(string key, Func<T> creator, Action<bool, T> action)
        {
            IEnlistedObject enlisted;
            if (Enlisted.TryGetValue(key, out enlisted))
            {
                var enlistedAs = enlisted as EnlistedObject<T>;
                if (enlistedAs == null) throw new Exception("An item with a different type has already been enlisted with the same key.");
                return enlistedAs.Item;
            }
            var enlistedOfT = new EnlistedObject<T>(creator(), action);
            Enlisted[key] = enlistedOfT;
            return enlistedOfT.Item;
        }

        /// <inheritdoc />
        public void Enlist(string key, Action<bool> action)
        {
            IEnlistedObject enlisted;
            if (Enlisted.TryGetValue(key, out enlisted))
            {
                var enlistedAs = enlisted as EnlistedObject<object>;
                if (enlistedAs == null) throw new Exception("An item with a different type has already been enlisted with the same key.");
                return;
            }
            var enlistedOfT = new EnlistedObject<object>(null, (completed, item) => action(completed));
            Enlisted[key] = enlistedOfT;
        }
    }
}
