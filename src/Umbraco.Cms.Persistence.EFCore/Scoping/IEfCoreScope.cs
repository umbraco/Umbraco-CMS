using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Cms.Persistence.EFCore.Services;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScope : IDisposable
{
    /// <summary>
    /// Gets the distance from the root scope.
    /// </summary>
    /// <remarks>
    /// A zero represents a root scope, any value greater than zero represents a child scope.
    /// </remarks>
    public int Depth => -1;

    /// <summary>
    /// Instance ID of the current scope.
    /// </summary>
    Guid InstanceId { get; }

    /// <summary>
    /// Executes the given function on the database.
    /// </summary>
    /// <param name="method">Function to execute.</param>
    /// <typeparam name="T">Type to use and return.</typeparam>
    /// <returns></returns>
    Task<T> ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task<T>> method);

    public IScopeContext? ScopeContext { get; set; }

    public ILockingMechanism Locks { get; }

    /// <summary>
    /// Executes the given function on the database.
    /// </summary>
    /// <param name="method">Function to execute.</param>
    /// <typeparam name="T">Type to use and return.</typeparam>
    /// <returns></returns>
    Task ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task> method);

    /// <summary>
    /// Completes the scope, if this is not call, the transaction will be rolled back.
    /// </summary>
    void Complete();

    /// <summary>
    ///     Gets the scope notification publisher
    /// </summary>
    IScopedNotificationPublisher Notifications { get; }

    /// <summary>
    ///     Gets the repositories cache mode.
    /// </summary>
    RepositoryCacheMode RepositoryCacheMode { get; }

    /// <summary>
    ///     Gets the scope isolated cache.
    /// </summary>
    IsolatedCaches IsolatedCaches { get; }
}
