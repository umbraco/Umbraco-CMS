using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
internal class MediaSortingNotificationTests : UmbracoIntegrationTest
{
    private readonly List<bool> _sortingNotifications = new();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.Services.AddSingleton<INotificationHandler<MediaSavingNotification>>(new RecordingHandler(_sortingNotifications));
    }

    [Test]
    public async Task Sort_Identifies_Sorting_Without_Suppressing_Save_Notifications()
    {
        var typeService = GetRequiredService<IMediaTypeService>();
        var mediaService = GetRequiredService<IMediaService>();
        IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("sortingTest", "Sorting Test");
        Assert.That((await typeService.CreateAsync(mediaType, Constants.Security.SuperUserKey)).Success, Is.True);
        IMedia first = MediaBuilder.CreateSimpleMedia(mediaType, "First", -1);
        IMedia second = MediaBuilder.CreateSimpleMedia(mediaType, "Second", -1);
        mediaService.Save(first);
        mediaService.Save(second);
        Assert.That(_sortingNotifications, Is.EqualTo(new[] { false, false }));
        _sortingNotifications.Clear();

        Assert.That(mediaService.Sort(new[] { second, first }), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(_sortingNotifications, Is.EqualTo(new[] { true }));
            Assert.That(mediaService.GetById(second.Id)!.SortOrder, Is.EqualTo(0));
            Assert.That(mediaService.GetById(first.Id)!.SortOrder, Is.EqualTo(1));
        });
    }

    private sealed class RecordingHandler(List<bool> notifications) : INotificationHandler<MediaSavingNotification>
    {
        public void Handle(MediaSavingNotification notification) => notifications.Add(notification.IsSorting);
    }
}
