using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure;

namespace UmbracoProject;

/// <summary>
/// Suspends/disables scheduled publishing, because that takes an eager write lock every minute, resulting in flaky test runs on SQLite.
/// </summary>
public sealed class SuspendScheduledPublishingComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => Suspendable.ScheduledPublishing.Suspend();
}
