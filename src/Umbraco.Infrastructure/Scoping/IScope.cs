using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Core.Scoping
{
    /// <summary>
    /// Represents a scope.
    /// </summary>
    public interface IScope : IDisposable, IInstanceIdentifiable
    {
        /// <summary>
        /// Gets the scope database.
        /// </summary>
        IUmbracoDatabase Database { get; }

        /// <summary>
        /// Gets the Sql context.
        /// </summary>
        ISqlContext SqlContext { get; }

        /// <summary>
        /// Gets the scope event messages.
        /// </summary>
        EventMessages Messages { get; }

        /// <summary>
        /// Gets the scope event dispatcher.
        /// </summary>
        IEventDispatcher Events { get; }

        /// <summary>
        /// Gets the scope notification publisher
        /// </summary>
        IScopedNotificationPublisher Notifications { get; }

        /// <summary>
        /// Gets the repositories cache mode.
        /// </summary>
        RepositoryCacheMode RepositoryCacheMode { get; }

        /// <summary>
        /// Gets the scope isolated cache.
        /// </summary>
        IsolatedCaches IsolatedCaches { get; }

        /// <summary>
        /// Completes the scope.
        /// </summary>
        /// <returns>A value indicating whether the scope has been successfully completed.</returns>
        /// <remarks>Can return false if any child scope has not completed.</remarks>
        bool Complete();

        /// <summary>
        /// Read-locks some lock objects.
        /// </summary>
        /// <param name="lockIds">The lock object identifiers.</param>
        void ReadLock(int lockId);

        /// <summary>
        /// Write-locks some lock objects.
        /// </summary>
        /// <param name="lockIds">The lock object identifiers.</param>
        void WriteLock(int lockId);

        /// <summary>
        /// Write-locks some lock objects.
        /// </summary>
        /// <param name="timeout">The database timeout in milliseconds</param>
        /// <param name="lockId">The lock object identifier.</param>
        void WriteLock(TimeSpan timeout, int lockId);

        /// <summary>
        /// Read-locks some lock objects.
        /// </summary>
        /// <param name="timeout">The database timeout in milliseconds</param>
        /// <param name="lockId">The lock object identifier.</param>
        void ReadLock(TimeSpan timeout, int lockId);
    }
}
