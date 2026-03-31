namespace Umbraco.Cms.Core.Scoping;

/// <summary>
/// Represents the context for managing database and operational scopes within the Umbraco CMS infrastructure.
/// Provides access to the current active scope and the scope provider, enabling coordinated resource management and transactional operations.
/// Typically used internally to ensure correct scope handling during repository and service operations.
/// </summary>
public class ScopeContext : IScopeContext, IInstanceIdentifiable
{
    private Dictionary<string, IEnlistedObject>? _enlisted;

    private interface IEnlistedObject
    {
        /// <summary>
        /// Gets the priority value that determines the order in which the enlisted object is processed.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Executes logic associated with the enlisted object, typically as part of a scope completion process.
        /// </summary>
        /// <param name="completed">True if the scope completed successfully; otherwise, false.</param>
        void Execute(bool completed);
    }

    /// <summary>
    /// Gets the unique identifier for this instance of the scope context.
    /// </summary>
    public Guid InstanceId { get; } = Guid.NewGuid();

    private IDictionary<string, IEnlistedObject> Enlisted => _enlisted ??= new Dictionary<string, IEnlistedObject>();

    /// <summary>
    /// Executes all enlisted actions in order of priority when the scope exits.
    /// This method is typically called at the end of a scope to ensure that all registered
    /// (enlisted) actions are executed, passing the completion status to each. If any enlisted
    /// action throws an exception, all exceptions are aggregated and thrown after all actions have run.
    /// </summary>
    /// <param name="completed">True if the scope completed successfully; otherwise, false.</param>
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

    /// <summary>
    /// Gets the ID of the thread that created this scope context.
    /// </summary>
    public int CreatedThreadId { get; } = Thread.CurrentThread.ManagedThreadId;

    /// <summary>
    /// Enlists an action to be executed when the scope completes, with an optional priority.
    /// </summary>
    /// <param name="key">A unique key identifying this enlistment, used to prevent duplicate actions.</param>
    /// <param name="action">The action to execute. The boolean parameter indicates whether the scope completed successfully (<c>true</c>) or was aborted (<c>false</c>).</param>
    /// <param name="priority">The priority of the action; lower values indicate higher priority. Default is 100.</param>
    public void Enlist(string key, Action<bool> action, int priority = 100) =>
        Enlist<object>(key, null, (completed, item) => action(completed), priority);

    /// <summary>
    /// Enlists an item in the scope context with the specified key, optionally creating it if it does not exist, and allows an action to be performed when the scope completes.
    /// </summary>
    /// <param name="key">A unique key used to identify the enlisted item within the scope context.</param>
    /// <param name="creator">A function to create the item if it does not already exist in the scope; if null and the item does not exist, no item is created.</param>
    /// <param name="action">An optional action to invoke when the scope completes. The boolean parameter indicates whether the scope completed successfully, and the second parameter is the enlisted item.</param>
    /// <param name="priority">The priority for the enlisted item, which determines the order in which actions are executed when the scope completes. Lower values indicate higher priority.</param>
    /// <returns>The enlisted item of type <typeparamref name="T"/>, or <c>null</c> if the item was not found or created.</returns>
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

    /// <summary>
    /// Retrieves an enlisted object of the specified type associated with the given key.
    /// </summary>
    /// <param name="key">The key identifying the enlisted object.</param>
    /// <typeparam name="T">The type of the enlisted object to retrieve.</typeparam>
    /// <returns>The enlisted object of type <typeparamref name="T"/> if found; otherwise, <c>null</c>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if an object with the specified key exists but is not of type <typeparamref name="T"/>.</exception>
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

    private sealed class EnlistedObject<T> : IEnlistedObject
    {
        private readonly Action<bool, T?>? _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnlistedObject{T}"/> class with the specified item, optional action, and priority.
        /// </summary>
        /// <param name="item">The item to enlist in the scope context.</param>
        /// <param name="action">An optional action to invoke with a boolean indicating success or failure, and the enlisted item.</param>
        /// <param name="priority">The priority value that determines the order in which the enlisted object is processed.</param>
        public EnlistedObject(T? item, Action<bool, T?>? action, int priority)
        {
            Item = item;
            Priority = priority;
            _action = action;
        }

        /// <summary>
        /// Gets the object of type <typeparamref name="T"/> that has been enlisted in the current scope context.
        /// </summary>
        public T? Item { get; }

        /// <summary>
        /// Gets the priority value that determines the order in which the enlisted object is processed.
        /// Higher priority objects may be processed before lower priority ones.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Executes the associated action for this enlisted object, passing the specified completion status.
        /// </summary>
        /// <param name="completed">True if the operation has completed successfully; otherwise, false.</param>
        public void Execute(bool completed)
        {
            if (_action is not null)
            {
                _action(completed, Item);
            }
        }
    }
}
