using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Persistence;

// ReSharper disable once CheckNamespace
namespace Umbraco.Cms.Core.Scoping;

[Obsolete("Please use Umbraco.Cms.Infrastructure.Scoping.IScope or Umbraco.Cms.Core.Scoping.ICoreScope instead.")]
public interface IScope : Infrastructure.Scoping.IScope
{
    /// <summary>
    /// Gets the scope database.
    /// </summary>
    new IUmbracoDatabase Database { get; }

    /// <summary>
    /// Gets the Sql context.
    /// </summary>
    new ISqlContext SqlContext { get; }

    /// <summary>
    ///     Gets the scope event messages.
    /// </summary>
    EventMessages Messages { get; }

    /// <summary>
    ///     Gets the scope event dispatcher.
    /// </summary>
    IEventDispatcher Events { get; }

    /// <summary>
    /// Gets the scope notification publisher
    /// </summary>
    new IScopedNotificationPublisher Notifications { get; }

    /// <summary>
    /// Gets the repositories cache mode.
    /// </summary>
    new RepositoryCacheMode RepositoryCacheMode { get; }

    /// <summary>
    /// Gets the scope isolated cache.
    /// </summary>
    new IsolatedCaches IsolatedCaches { get; }

    /// <summary>
    /// Completes the scope.
    /// </summary>
    /// <returns>A value indicating whether the scope has been successfully completed.</returns>
    /// <remarks>Can return false if any child scope has not completed.</remarks>
    new bool Complete();

    /// <summary>
    /// Read-locks some lock objects.
    /// </summary>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    new void ReadLock(params int[] lockIds);

    /// <summary>
    /// Write-locks some lock objects.
    /// </summary>
    /// <param name="lockIds">Array of object identifiers.</param>
    new void WriteLock(params int[] lockIds);

    /// <summary>
    /// Write-locks some lock objects.
    /// </summary>
    /// <param name="timeout">The database timeout in milliseconds</param>
    /// <param name="lockId">The lock object identifier.</param>
    new void WriteLock(TimeSpan timeout, int lockId);

    /// <summary>
    /// Read-locks some lock objects.
    /// </summary>
    /// <param name="timeout">The database timeout in milliseconds</param>
    /// <param name="lockId">The lock object identifier.</param>
    new void ReadLock(TimeSpan timeout, int lockId);

    new void EagerWriteLock(params int[] lockIds);
    new void EagerWriteLock(TimeSpan timeout, int lockId);

    new void EagerReadLock(TimeSpan timeout, int lockId);

    new void EagerReadLock(params int[] lockIds);
}
