using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Formatters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.Filters;

[TestFixture]
public class OutgoingEditorModelEventFilterTests : UmbracoTestServerTestBase
{
    private static int _messageCount;
    private static Action<SendingContentNotification> _handler;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<SendingContentNotification, FilterEventHandler>();
    }

    [TearDown]
    public void Reset() => ResetNotifications();

    [Test]
    public async Task Content_Item_With_Schedule_Raises_SendingContentNotification()
    {
        IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();
        IContentService contentService = GetRequiredService<IContentService>();
        IJsonSerializer serializer = GetRequiredService<IJsonSerializer>();

        var contentType = new ContentTypeBuilder().Build();
        contentTypeService.Save(contentType);

        var contentToRequest = new ContentBuilder()
            .WithoutIdentity()
            .WithContentType(contentType)
            .Build();

        contentService.Save(contentToRequest);

        _handler = notification => notification.Content.AllowPreview = false;

        var url = PrepareApiControllerUrl<ContentController>(x => x.GetById(contentToRequest.Id));

        HttpResponseMessage response = await Client.GetAsync(url);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var text = await response.Content.ReadAsStringAsync();
        text = text.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
        var display = serializer.Deserialize<ContentItemDisplayWithSchedule>(text);

        Assert.AreEqual(1, _messageCount);
        Assert.IsNotNull(display);
        Assert.IsFalse(display.AllowPreview);
    }

    private void ResetNotifications()
    {
        _messageCount = 0;
        _handler = null;
    }

    private class FilterEventHandler : INotificationHandler<SendingContentNotification>
    {
        public void Handle(SendingContentNotification notification)
        {
            _messageCount += 1;
            _handler?.Invoke(notification);
        }
    }
}
