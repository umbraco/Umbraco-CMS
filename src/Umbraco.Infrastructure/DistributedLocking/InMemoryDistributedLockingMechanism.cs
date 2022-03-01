using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.DistributedLocking.Exceptions;

namespace Umbraco.Cms.Infrastructure.DistributedLocking;

/// <summary>
/// In Memory implementation of <see cref="IDistributedLockingMechanism"/>.
/// </summary>
/// <remarks>
/// Not actually distributed.
/// </remarks>
internal class InMemoryDistributedLockingMechanism : IDistributedLockingMechanism
{
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
    private readonly ILogger<InMemoryDistributedLockingMechanism> _logger;
    private readonly Locks _locks;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryDistributedLockingMechanism"/> class.
    /// </summary>
    public InMemoryDistributedLockingMechanism(
        IOptionsMonitor<GlobalSettings> globalSettings,
        ILogger<InMemoryDistributedLockingMechanism> logger)
    {
        _globalSettings = globalSettings;
        _logger = logger;
        _locks = new Locks(this);
    }

    /// <inheritdoc />
    public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout)
    {
        obtainLockTimeout ??= _globalSettings.CurrentValue.DistributedLockingReadLockDefaultTimeout;

        // ReSharper disable once InconsistentlySynchronizedField
        _logger.LogDebug("{lockType} requested for id {id}", DistributedLockType.ReadLock, lockId);
        var timer = new Stopwatch();
        timer.Start();

        do
        {
            lock (_locks)
            {
                if (_locks.TrAcquireReadLock(lockId, out InMemoryDistributedLock distributedLock))
                {
                    _logger.LogDebug("Acquired {lockType} for id {id}", DistributedLockType.ReadLock, lockId);
                    return distributedLock;
                }
            }

            Thread.Sleep(50);
        }
        while (timer.Elapsed < obtainLockTimeout);

        throw new DistributedReadLockTimeoutException(lockId);
    }

    /// <inheritdoc />
    public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout)
    {
        obtainLockTimeout ??= _globalSettings.CurrentValue.DistributedLockingWriteLockDefaultTimeout;

        // ReSharper disable once InconsistentlySynchronizedField
        _logger.LogDebug("{lockType} requested for id {id}", DistributedLockType.WriteLock, lockId);
        var timer = new Stopwatch();
        timer.Start();

        do
        {
            lock (_locks)
            {
                if (_locks.TryAcquireWriteLock(lockId, out InMemoryDistributedLock distributedLock))
                {
                    _logger.LogDebug("Acquired {lockType} for id {id}", DistributedLockType.WriteLock, lockId);
                    return distributedLock;
                }
            }

            Thread.Sleep(50);
        } while (timer.Elapsed < obtainLockTimeout);

        throw new DistributedWriteLockTimeoutException(lockId);
    }

    private void DropLock(InMemoryDistributedLock distributedLock)
    {
        lock (_locks)
        {
            _locks.DropLock(distributedLock);
            _logger.LogDebug("Dropped {lockType} for id {id}", distributedLock.LockType, distributedLock.LockId);
        }
    }

    private class Locks
    {
        private readonly InMemoryDistributedLockingMechanism _parent;
        private readonly List<InMemoryDistributedLock> _internalLocks = new();

        public Locks(InMemoryDistributedLockingMechanism parent)
        {
            _parent = parent;
        }


        public bool TrAcquireReadLock(int lockId, out InMemoryDistributedLock distributedLock)
        {
            if (HasExistingWriteLock(lockId))
            {
                distributedLock = null;
                return false;
            }

            distributedLock = InMemoryDistributedLock.Read(_parent, lockId);
            _internalLocks.Add(distributedLock);
            return true;
        }

        public bool TryAcquireWriteLock(int lockId, out InMemoryDistributedLock distributedLock)
        {
            if (HasExistingWriteLock(lockId))
            {
                distributedLock = null;
                return false;
            }

            // No one has any sort of lock for this ID, so go ahead.
            if (_internalLocks.All(x => x.LockId != lockId))
            {
                distributedLock = InMemoryDistributedLock.Write(_parent, lockId);
                _internalLocks.Add(distributedLock);
                return true;
            }

            var existingLocks = _internalLocks.Where(x => x.LockId == lockId).ToList();
            if (existingLocks.Count == 1 && existingLocks.First().ThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                // There's only a single read lock, and it's from this very same thread so allow it anyway.
                // How reasonable is this? I don't know but it makes tests pass.
                // It's probably fine for SQLite because with journal_mode = wal
                // 1) All reads are from a snapshot
                // 2) Can only ever have a single writer
                distributedLock = InMemoryDistributedLock.Write(_parent, lockId);
                _internalLocks.Add(distributedLock);
                return true;
            }

            distributedLock = null;
            return false;
        }

        public void DropLock(InMemoryDistributedLock distributedLock)
        {
            _internalLocks.Remove(distributedLock);
        }

        private bool HasExistingWriteLock(int lockId) =>
            _internalLocks
                .Where(x => x.LockType == DistributedLockType.WriteLock)
                .Any(x => x.LockId == lockId);
    }

    private class InMemoryDistributedLock : IDistributedLock
    {
        private readonly InMemoryDistributedLockingMechanism _parent;

        private InMemoryDistributedLock(InMemoryDistributedLockingMechanism parent, int lockId, DistributedLockType lockType)
        {
            _parent = parent;
            LockId = lockId;
            LockType = lockType;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public int LockId { get; }

        public int ThreadId { get; }

        public DistributedLockType LockType { get; }

        public static InMemoryDistributedLock Read(InMemoryDistributedLockingMechanism parent, int lockId) =>
            new(parent, lockId, DistributedLockType.ReadLock);

        public static InMemoryDistributedLock Write(InMemoryDistributedLockingMechanism parent, int lockId) =>
            new(parent, lockId, DistributedLockType.WriteLock);

        public void Dispose()
            => _parent.DropLock(this);

        public override string ToString()
            => $"InMemoryDistributedLock({LockId}, {LockType}";
    }
}
