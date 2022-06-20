using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
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

    [Test]
    public async Task Publish_Schedule_Is_Mapped_Correctly()
    {
        const string UsIso = "en-US";
        const string DkIso = "da-DK";
        const string SweIso = "sv-SE";
        var contentTypeService = GetRequiredService<IContentTypeService>();
        var contentService = GetRequiredService<IContentService>();
        var localizationService = GetRequiredService<ILocalizationService>();
        IJsonSerializer serializer = GetRequiredService<IJsonSerializer>();

        var contentType = new ContentTypeBuilder()
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        contentTypeService.Save(contentType);

        var dkLang = new LanguageBuilder()
            .WithCultureInfo(DkIso)
            .WithIsDefault(false)
            .Build();

        var sweLang = new LanguageBuilder()
            .WithCultureInfo(SweIso)
            .WithIsDefault(false)
            .Build();

        localizationService.Save(dkLang);
        localizationService.Save(sweLang);

        var content = new ContentBuilder()
            .WithoutIdentity()
            .WithContentType(contentType)
            .WithCultureName(UsIso, "Same Name")
            .WithCultureName(SweIso, "Same Name")
            .WithCultureName(DkIso, "Same Name")
            .Build();

        contentService.Save(content);
        var schedule = new ContentScheduleCollection();

        var dkReleaseDate = new DateTime(2022, 06, 22, 21, 30, 42);
        var dkExpireDate = new DateTime(2022, 07, 15, 18, 00, 00);

        var sweReleaseDate = new DateTime(2022, 06, 23, 22, 30, 42);
        var sweExpireDate = new DateTime(2022, 07, 10, 14, 20, 00);
        schedule.Add(DkIso, dkReleaseDate, dkExpireDate);
        schedule.Add(SweIso, sweReleaseDate, sweExpireDate);
        contentService.PersistContentSchedule(content, schedule);

        var url = PrepareApiControllerUrl<ContentController>(x => x.GetById(content.Id));

        HttpResponseMessage response = await Client.GetAsync(url);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var text = await response.Content.ReadAsStringAsync();
        text = text.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
        var display = serializer.Deserialize<ContentItemDisplayWithSchedule>(text);

        Assert.IsNotNull(display);
        Assert.AreEqual(1, _messageCount);

        var dkVariant = display.Variants.FirstOrDefault(x => x.Language?.IsoCode == DkIso);
        Assert.IsNotNull(dkVariant);
        Assert.AreEqual(dkReleaseDate, dkVariant.ReleaseDate);
        Assert.AreEqual(dkExpireDate, dkVariant.ExpireDate);

        var sweVariant = display.Variants.FirstOrDefault(x => x.Language?.IsoCode == SweIso);
        Assert.IsNotNull(sweVariant);
        Assert.AreEqual(sweReleaseDate, sweVariant.ReleaseDate);
        Assert.AreEqual(sweExpireDate, sweVariant.ExpireDate);

        var usVariant = display.Variants.FirstOrDefault(x => x.Language?.IsoCode == UsIso);
        Assert.IsNotNull(usVariant);
        Assert.IsNull(usVariant.ReleaseDate);
        Assert.IsNull(usVariant.ExpireDate);
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
