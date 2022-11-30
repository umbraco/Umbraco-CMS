using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Events;

[TestFixture]
public class EventAggregatorTests : UmbracoTestServerTestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddScoped<EventAggregatorTestScopedService>();
        services.AddTransient<INotificationHandler<EventAggregatorTestNotification>, EventAggregatorTestNotificationHandler>();
    }

    [Test]
    public async Task Publish_HandlerWithScopedDependency_DoesNotThrow()
    {
        var result = await Client.GetAsync("/test-handler-with-scoped-services");
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }
}

public class EventAggregatorTestsController : Controller
{
    private readonly IEventAggregator _eventAggregator;

    public EventAggregatorTestsController(IEventAggregator eventAggregator) => _eventAggregator = eventAggregator;

    [HttpGet("test-handler-with-scoped-services")]
    public async Task<IActionResult> Test()
    {
        var notification = new EventAggregatorTestNotification();
        await _eventAggregator.PublishAsync(notification);

        if (!notification.Mutated)
        {
            throw new ApplicationException("Doesn't work");
        }

        return Ok();
    }
}

public class EventAggregatorTestScopedService
{
}

public class EventAggregatorTestNotification : INotification
{
    public bool Mutated { get; set; }
}

public class EventAggregatorTestNotificationHandler : INotificationHandler<EventAggregatorTestNotification>
{
    private readonly EventAggregatorTestScopedService _scopedService;

    public EventAggregatorTestNotificationHandler(EventAggregatorTestScopedService scopedService) =>
        _scopedService = scopedService;

    // Mutation proves that the handler runs despite depending on scoped service.
    public void Handle(EventAggregatorTestNotification notification) => notification.Mutated = true;
}
