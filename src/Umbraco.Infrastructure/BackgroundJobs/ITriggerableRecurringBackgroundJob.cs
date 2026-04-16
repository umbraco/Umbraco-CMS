// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
/// Marker interface for recurring background jobs that support being triggered manually.
/// Only jobs implementing this interface can be triggered via <see cref="IRecurringBackgroundJobTrigger{TJob}" />.
/// </summary>
public interface ITriggerableRecurringBackgroundJob : IRecurringBackgroundJob
{ }
