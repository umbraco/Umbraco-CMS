using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScope<TDbContext> : ICoreScope
{
    /// <summary>
    /// Executes the given function on the database.
    /// </summary>
    /// <param name="method">Function to execute.</param>
    /// <typeparam name="T">Type to use and return.</typeparam>
    /// <returns></returns>
    Task<T> ExecuteWithContextAsync<T>(Func<TDbContext, Task<T>> method);

    public IScopeContext? ScopeContext { get; set; }

    /// <summary>
    /// Executes the given function on the database.
    /// </summary>
    /// <param name="method">Function to execute.</param>
    /// <typeparam name="T">Type to use and return.</typeparam>
    /// <returns></returns>
    Task ExecuteWithContextAsync<T>(Func<TDbContext, Task> method);

    /// <summary>
    ///     Gets the scope notification publisher
    /// </summary>
    IScopedNotificationPublisher Notifications { get; }
}
