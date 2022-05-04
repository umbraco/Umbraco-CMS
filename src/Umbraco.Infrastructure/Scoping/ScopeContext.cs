namespace Umbraco.Cms.Core.Scoping;

internal class ScopeContext : IScopeContext, IInstanceIdentifiable
{
    private Dictionary<string, IEnlistedObject>? _enlisted;

    private interface IEnlistedObject
    {
        int Priority { get; }

        void Execute(bool completed);
    }

    public Guid InstanceId { get; } = Guid.NewGuid();

    private IDictionary<string, IEnlistedObject> Enlisted => _enlisted ??= new Dictionary<string, IEnlistedObject>();

    public void ScopeExit(bool completed)
    {
        if (_enlisted == null)
        {
            return;
        }

        // TODO: can we create infinite loops? - what about nested events? will they just be plainly ignored = really bad?
        List<Exception>? exceptions = null;
        List<IEnlistedObject> orderedEnlisted;
        while ((orderedEnlisted = _enlisted.Values.OrderBy(x => x.Priority).ToList()).Count > 0)
        {
            _enlisted.Clear();
            foreach (IEnlistedObject enlisted in orderedEnlisted)
            {
                try
                {
                    enlisted.Execute(completed);
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(e);
                }
            }
        }

        if (exceptions != null)
        {
            throw new AggregateException("Exceptions were thrown by listed actions.", exceptions);
        }
    }

    public int CreatedThreadId { get; } = Thread.CurrentThread.ManagedThreadId;

    public void Enlist(string key, Action<bool> action, int priority = 100) =>
        Enlist<object>(key, null, (completed, item) => action(completed), priority);

    public T? Enlist<T>(string key, Func<T>? creator, Action<bool, T?>? action = null, int priority = 100)
    {
        Dictionary<string, IEnlistedObject> enlistedObjects =
_enlisted ??= new Dictionary<string, IEnlistedObject>();

        if (enlistedObjects.TryGetValue(key, out IEnlistedObject? enlisted))
        {
            if (!(enlisted is EnlistedObject<T> enlistedAs))
            {
                throw new InvalidOperationException("An item with the key already exists, but with a different type.");
            }

            if (enlistedAs.Priority != priority)
            {
                throw new InvalidOperationException(
                    "An item with the key already exits, but with a different priority.");
            }

            return enlistedAs.Item;
        }

        var enlistedOfT = new EnlistedObject<T>(creator == null ? default : creator(), action, priority);
        Enlisted[key] = enlistedOfT;
        return enlistedOfT.Item;
    }

    public T? GetEnlisted<T>(string key)
    {
        Dictionary<string, IEnlistedObject>? enlistedObjects = _enlisted;
        if (enlistedObjects == null)
        {
            return default;
        }

        if (enlistedObjects.TryGetValue(key, out IEnlistedObject? enlisted) == false)
        {
            return default;
        }

        if (!(enlisted is EnlistedObject<T> enlistedAs))
        {
            throw new InvalidOperationException("An item with the key exists, but with a different type.");
        }

        return enlistedAs.Item;
    }

    private class EnlistedObject<T> : IEnlistedObject
    {
        private readonly Action<bool, T?>? _action;

        public EnlistedObject(T? item, Action<bool, T?>? action, int priority)
        {
            Item = item;
            Priority = priority;
            _action = action;
        }

        public T? Item { get; }

        public int Priority { get; }

        public void Execute(bool completed)
        {
            if (_action is not null)
            {
                _action(completed, Item);
            }
        }
    }
}
