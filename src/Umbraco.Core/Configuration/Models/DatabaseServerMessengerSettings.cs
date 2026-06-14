// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for database server messaging settings.
/// </summary>
public class DatabaseServerMessengerSettings
{
    /// <summary>
    ///     The default maximum number of processing instructions.
    /// </summary>
    internal const int StaticMaxProcessingInstructionCount = 1000;

    /// <summary>
    ///     The default time to retain instructions in the database.
    /// </summary>
    internal const string StaticTimeToRetainInstructions = "2.00:00:00"; // TimeSpan.FromDays(2);

    /// <summary>
    ///     The default time between sync operations.
    /// </summary>
    internal const string StaticTimeBetweenSyncOperations = "00:00:05"; // TimeSpan.FromSeconds(5);

    /// <summary>
    ///     The default time between prune operations.
    /// </summary>
    internal const string StaticTimeBetweenPruneOperations = "00:01:00"; // TimeSpan.FromMinutes(1);

    /// <summary>
    ///     The default timeout for a single synchronization operation.
    /// </summary>
    internal const string StaticSyncTimeout = "00:01:00"; // TimeSpan.FromMinutes(1);

    /// <summary>
    ///     Gets the default timeout for a single synchronization operation, for use as a fallback when an invalid
    ///     <see cref="SyncTimeout" /> is configured.
    /// </summary>
    public static readonly TimeSpan DefaultSyncTimeout = TimeSpan.Parse(StaticSyncTimeout);

    /// <summary>
    ///     Gets or sets a value for the maximum number of instructions that can be processed at startup; otherwise the server
    ///     cold-boots (rebuilds its caches).
    /// </summary>
    [DefaultValue(StaticMaxProcessingInstructionCount)]
    public int MaxProcessingInstructionCount { get; set; } = StaticMaxProcessingInstructionCount;

    /// <summary>
    ///     Gets or sets a value for the time to keep instructions in the database; records older than this number will be
    ///     pruned.
    /// </summary>
    [DefaultValue(StaticTimeToRetainInstructions)]
    public TimeSpan TimeToRetainInstructions { get; set; } = TimeSpan.Parse(StaticTimeToRetainInstructions);

    /// <summary>
    ///     Gets or sets a value for the time to wait between each sync operations.
    /// </summary>
    [DefaultValue(StaticTimeBetweenSyncOperations)]
    public TimeSpan TimeBetweenSyncOperations { get; set; } = TimeSpan.Parse(StaticTimeBetweenSyncOperations);

    /// <summary>
    ///     Gets or sets a value for the time to wait between each prune operations.
    /// </summary>
    [DefaultValue(StaticTimeBetweenPruneOperations)]
    public TimeSpan TimeBetweenPruneOperations { get; set; } = TimeSpan.Parse(StaticTimeBetweenPruneOperations);

    /// <summary>
    ///     Gets or sets the maximum time to wait for a single synchronization operation to complete before it is
    ///     considered stalled (for example, blocked on a hung database connection) and abandoned, so the recurring
    ///     job keeps running rather than stopping permanently. This bounds how long the job waits on a single sync,
    ///     not how long a stalled connection itself takes to recover (which is governed by the database timeouts).
    /// </summary>
    [DefaultValue(StaticSyncTimeout)]
    public TimeSpan SyncTimeout { get; set; } = DefaultSyncTimeout;
}
