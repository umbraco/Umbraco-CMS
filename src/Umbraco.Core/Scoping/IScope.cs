using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    // TODO: This is for backward compat - Merge this in netcore
    public interface IScope2 : IScope
    {
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
        void ReadLock(params int[] lockIds);

        /// <summary>
        /// Write-locks some lock objects.
        /// </summary>
        /// <param name="lockIds">The lock object identifiers.</param>
        void WriteLock(params int[] lockIds);
    }
}
