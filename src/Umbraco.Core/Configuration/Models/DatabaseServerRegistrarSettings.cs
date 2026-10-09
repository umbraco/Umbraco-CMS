// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for database server registrar settings.
/// </summary>
public class DatabaseServerRegistrarSettings
{
    /// <summary>
    ///     The default time to wait between database calls.
    /// </summary>
    internal const string StaticWaitTimeBetweenCalls = "00:01:00";

    /// <summary>
    ///     The default timeout for considering a server stale.
    /// </summary>
    internal const string StaticStaleServerTimeout = "00:02:00";

    /// <summary>
    ///     The default timeout for a single server touch operation.
    /// </summary>
    internal const string StaticTouchTimeout = "00:01:00"; // TimeSpan.FromMinutes(1);

    /// <summary>
    ///     Gets the default timeout for a single server touch operation, for use as a fallback when an invalid
    ///     <see cref="TouchTimeout" /> is configured.
    /// </summary>
    public static readonly TimeSpan DefaultTouchTimeout = TimeSpan.Parse(StaticTouchTimeout);

    /// <summary>
    ///     Gets or sets a value for the amount of time to wait between calls to the database on the background thread.
    /// </summary>
    [DefaultValue(StaticWaitTimeBetweenCalls)]
    public TimeSpan WaitTimeBetweenCalls { get; set; } = TimeSpan.Parse(StaticWaitTimeBetweenCalls);

    /// <summary>
    ///     Gets or sets a value for the time span to wait before considering a server stale, after it has last been accessed.
    /// </summary>
    [DefaultValue(StaticStaleServerTimeout)]
    public TimeSpan StaleServerTimeout { get; set; } = TimeSpan.Parse(StaticStaleServerTimeout);

    /// <summary>
    ///     Gets or sets the maximum time to wait for a single server touch operation to complete before it is
    ///     considered stalled (for example, blocked on a hung database connection) and abandoned, so the recurring
    ///     job keeps running rather than stopping permanently. This bounds how long the job waits on a single touch,
    ///     not how long a stalled connection itself takes to recover (which is governed by the database timeouts).
    /// </summary>
    [DefaultValue(StaticTouchTimeout)]
    public TimeSpan TouchTimeout { get; set; } = DefaultTouchTimeout;
}
