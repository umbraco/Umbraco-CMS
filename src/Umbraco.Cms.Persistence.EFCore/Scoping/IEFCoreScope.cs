using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

/// <summary>
/// Represents an EF Core scope that provides database context access and transaction management.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
public interface IEfCoreScope<TDbContext> : ICoreScope
{
    /// <summary>
    /// Executes the given function on the database.
    /// </summary>
    /// <param name="method">Function to execute.</param>
    /// <typeparam name="T">Type to use and return.</typeparam>
    /// <returns>A task containing the result of the function.</returns>
    Task<T> ExecuteWithContextAsync<T>(Func<TDbContext, Task<T>> method);

    /// <summary>
    /// Gets or sets the scope context.
    /// </summary>
    IScopeContext? ScopeContext { get; set; }

    /// <summary>
    /// Executes the given function on the database.
    /// </summary>
    /// <param name="method">Function to execute.</param>
    /// <typeparam name="T">Type to use and return.</typeparam>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteWithContextAsync<T>(Func<TDbContext, Task> method);

    /// <summary>
    /// Gets the scope notification publisher.
    /// </summary>
    new IScopedNotificationPublisher Notifications { get; }
}
