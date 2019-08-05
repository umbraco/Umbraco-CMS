using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
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
        [Obsolete("Use the overload with reason")]
        [Browsable(false)]
        void WriteLock(params int[] lockIds);

        /// <summary>
        /// Write-locks some lock objects.
        /// </summary>
        /// <param name="writeLockReasonId">The id of the reason to take the write lock. See Constants.Locks.Reason.*</param>
        /// <param name="lockIds">The lock object identifiers.</param>
        void WriteLock(short writeLockReasonId, params int[] lockIds);

        /// <summary>
        /// Bypasses all db-locks and reads the WriteLockReasonId column on the specified locks.
        /// </summary>
        /// <param name="lockIds">The lock ids from the Constants.Locks.*</param>
        /// <returns>A dictionary with the lock is as key, and the last started work as value. Note value can be null if we don't know or use SqlCe.</returns>
        IDictionary<int, short> TrySpyLock(params int[] lockIds);

        /// <summary>
        /// Bypasses all db-locks and reads the WriteLockReasonId column on the specified lock.
        /// </summary>
        /// <param name="lockId">The lock id from the Constants.Locks.*</param>
        /// <returns>The last started work. Note this can be null if we don't know or use SqlCe.</returns>
        short TrySpyLock(int lockId);
    }
}
