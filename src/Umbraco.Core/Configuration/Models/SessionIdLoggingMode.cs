// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Determines how request logging enriches log events with a session identifier.
/// </summary>
public enum SessionIdLoggingMode
{
    /// <summary>
    /// Do not enrich log events with a session identifier.
    /// </summary>
    None = 0,

    /// <summary>
    /// Enrich log events with the actual ASP.NET Core session id. This is the default and matches the
    /// historical behaviour, but reading the session id forces the session to be loaded from its store, which
    /// is a blocking round-trip per request when the session is backed by an <c>IDistributedCache</c>.
    /// </summary>
    SessionId,

    /// <summary>
    /// Enrich log events with a one-way hash of the session cookie value. This provides the same per-session
    /// correlation as <see cref="SessionId" /> without loading the session from its store, so it never incurs
    /// a distributed-cache round-trip.
    /// </summary>
    CookieHash,
}
