using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScope : ICoreScope
{
    /// <summary>
    /// Executes the given function on the database.
    /// </summary>
    /// <param name="method">Function to execute.</param>
    /// <typeparam name="T">Type to use and return.</typeparam>
    /// <returns></returns>
    Task<T> ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task<T>> method);

    public IScopeContext? ScopeContext { get; set; }

    /// <summary>
    /// Executes the given function on the database.
    /// </summary>
    /// <param name="method">Function to execute.</param>
    /// <typeparam name="T">Type to use and return.</typeparam>
    /// <returns></returns>
    Task ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task> method);

    /// <summary>
    ///     Gets the scope notification publisher
    /// </summary>
    IScopedNotificationPublisher Notifications { get; }
}
