using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Notifications;

/// <summary>
/// Notification that is raised when a recurring background job is cancelled during host shutdown.
/// </summary>
public sealed class RecurringBackgroundJobCanceledNotification : RecurringBackgroundJobNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringBackgroundJobCanceledNotification" /> class.
    /// </summary>
    /// <param name="target">The instance of the recurring background job that was cancelled.</param>
    /// <param name="messages">The <see cref="EventMessages" /> associated with the cancellation.</param>
    public RecurringBackgroundJobCanceledNotification(IRecurringBackgroundJob target, EventMessages messages)
        : base(target, messages)
    { }
}
