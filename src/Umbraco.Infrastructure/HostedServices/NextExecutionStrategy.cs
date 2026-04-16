// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
/// Determines the next execution strategy after a manually triggered execution completes.
/// </summary>
public enum NextExecutionStrategy
{
    /// <summary>
    /// Keep the current scheduled run unchanged.
    /// The next execution occurs at the originally-scheduled time.
    /// If that time has already passed (e.g. the triggered execution took longer than the remaining wait), it is skipped and the next period tick is awaited instead.
    /// </summary>
    None,

    /// <summary>
    /// Reset the period: wait a full period after the triggered execution completes.
    /// The triggered execution effectively shifts the schedule forward.
    /// </summary>
    Reset,

    /// <summary>
    /// The triggered execution replaces the next scheduled run.
    /// The following execution occurs one full period after the originally-scheduled time.
    /// Use this when the manual trigger is an early execution of the next scheduled run.
    /// </summary>
    Replace,
}
