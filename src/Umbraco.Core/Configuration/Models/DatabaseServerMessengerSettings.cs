// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for database server messaging settings.
/// </summary>
public class DatabaseServerMessengerSettings
{
    internal const int StaticMaxProcessingInstructionCount = 1000;
    internal const string StaticTimeToRetainInstructions = "2.00:00:00"; // TimeSpan.FromDays(2);
    internal const string StaticTimeBetweenSyncOperations = "00:00:05"; // TimeSpan.FromSeconds(5);
    internal const string StaticTimeBetweenPruneOperations = "00:01:00"; // TimeSpan.FromMinutes(1);

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
}
