using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private readonly Locks _locks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryDistributedLockingMechanism"/> class.
    /// </summary>
    public InMemoryDistributedLockingMechanism(
        IOptionsMonitor<GlobalSettings> globalSettings,
        ILogger<InMemoryDistributedLockingMechanism> logger)
    {
        _globalSettings = globalSettings;
        _logger = logger;
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
                if (_locks.TrAcquireReadLock(lockId))
                {
                    _logger.LogDebug("Acquired {lockType} for id {id}", DistributedLockType.ReadLock, lockId);
                    return InMemoryDistributedLock.Read(this, lockId);
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
                if (_locks.TryAcquireWriteLock(lockId))
                {
                    _logger.LogDebug("Acquired {lockType} for id {id}", DistributedLockType.WriteLock, lockId);
                    return InMemoryDistributedLock.Write(this, lockId);
                }
            }

            Thread.Sleep(50);
        }
        while (timer.Elapsed < obtainLockTimeout);

        throw new DistributedWriteLockTimeoutException(lockId);
    }

    private void DropLock(int lockId, DistributedLockType lockType)
    {
        lock (_locks)
        {
            _locks.DropLock(lockId, lockType);
            _logger.LogDebug("Dropped {lockType} for id {id}", lockType, lockId);
        }
    }

    private class Locks
    {
        private readonly IDictionary<int, int> _readLocks = new Dictionary<int, int>();
        private readonly HashSet<int> _writeLocks = new();

        public bool TrAcquireReadLock(int lockId)
        {
            if (_writeLocks.Contains(lockId))
            {
                return false;
            }

            _readLocks[lockId] = GetReaderCount(lockId) + 1;
            return true;
        }

        public bool TryAcquireWriteLock(int lockId)
        {
            if (GetReaderCount(lockId) > 0)
            {
                return false;
            }

            if (_writeLocks.Contains(lockId))
            {
                return false;
            }

            _writeLocks.Add(lockId);
            return true;
        }

        public void DropLock(int lockId, DistributedLockType lockType)
        {
            switch (lockType)
            {
                case DistributedLockType.ReadLock:
                    _readLocks[lockId] = GetReaderCount(lockId) - 1;
                    break;
                case DistributedLockType.WriteLock:
                    _writeLocks.Remove(lockId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lockType), lockType, @"Unsupported lockType");
            }
        }

        private int GetReaderCount(int lockId)
            => _readLocks.TryGetValue(lockId, out var count) ? count : 0;
    }

    private class InMemoryDistributedLock : IDistributedLock
    {
        private readonly InMemoryDistributedLockingMechanism _parent;

        private InMemoryDistributedLock(InMemoryDistributedLockingMechanism parent, int lockId, DistributedLockType lockType)
        {
            _parent = parent;
            LockId = lockId;
            LockType = lockType;
        }

        public int LockId { get; }

        public DistributedLockType LockType { get; }

        public static InMemoryDistributedLock Read(InMemoryDistributedLockingMechanism parent, int lockId) =>
            new(parent, lockId, DistributedLockType.ReadLock);

        public static InMemoryDistributedLock Write(InMemoryDistributedLockingMechanism parent, int lockId) =>
            new(parent, lockId, DistributedLockType.WriteLock);

        public void Dispose()
            => _parent.DropLock(LockId, LockType);

        public override string ToString()
            => $"InMemoryDistributedLock({LockId}, {LockType}";
    }
}
