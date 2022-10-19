// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for database server registrar settings.
/// </summary>
public class DatabaseServerRegistrarSettings
{
    internal const string StaticWaitTimeBetweenCalls = "00:01:00";
    internal const string StaticStaleServerTimeout = "00:02:00";

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
}
