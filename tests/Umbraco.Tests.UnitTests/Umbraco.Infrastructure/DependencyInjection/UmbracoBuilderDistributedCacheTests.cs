// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.DependencyInjection;

[TestFixture]
public class UmbracoBuilderDistributedCacheTests
{
    /// <summary>
    /// The request-end flush takes the cache-version lock and must not await on a second thread-pool thread while
    /// it holds it, so the synchronous handler has to be the one that is registered. Registering the asynchronous
    /// one as well would flush twice.
    /// </summary>
    [Test]
    public void AddDistributedCache_RegistersTheSynchronousRequestEndHandler_Only()
    {
        var builder = new UmbracoBuilder(TestHelper.GetServiceCollection(), Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        builder.AddDistributedCache();

        Assert.Multiple(() =>
        {
            Assert.That(
                builder.Services.Any(d => d.ServiceType == typeof(INotificationHandler<UmbracoRequestEndNotification>) && d.ImplementationType == typeof(DatabaseServerMessengerNotificationHandler)),
                Is.True,
                "The synchronous request-end handler must be registered.");
            Assert.That(
                builder.Services.Any(d => d.ServiceType == typeof(INotificationAsyncHandler<UmbracoRequestEndNotification>) && d.ImplementationType == typeof(DatabaseServerMessengerNotificationHandler)),
                Is.False,
                "The asynchronous request-end handler must not be registered beside it.");
        });
    }
}
