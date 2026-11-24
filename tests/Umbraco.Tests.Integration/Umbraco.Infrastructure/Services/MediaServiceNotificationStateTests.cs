// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Verifies that the state of a media deleting notification reaches every deleted notification the operation publishes.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaServiceNotificationStateTests : UmbracoIntegrationTest
{
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private NotificationStateRecorder Recorder => GetRequiredService<NotificationStateRecorder>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<NotificationStateRecorder>();
        builder
            .AddNotificationHandler<MediaDeletingNotification, NotificationStateWritingHandler<MediaDeletingNotification>>()
            .AddNotificationHandler<MediaDeletedNotification, NotificationStateReadingHandler<MediaDeletedNotification>>();
    }

    [Test]
    public async Task Delete_Shares_Deleting_State_With_Deleted_Notifications_For_Item_And_Descendants()
    {
        IMediaType mediaType = await CreateMediaType();
        IMedia root = CreateMedia(mediaType, "Root", Constants.System.Root);
        IMedia child = CreateMedia(mediaType, "Child", root.Id);
        IMedia grandchild = CreateMedia(mediaType, "Grandchild", child.Id);

        MediaService.Delete(root);

        AssertDeletedNotificationsShareDeletingState([root.Key, child.Key, grandchild.Key]);
    }

    [Test]
    public async Task DeleteMediaOfType_Shares_Deleting_State_With_Deleted_Notifications()
    {
        IMediaType mediaType = await CreateMediaType();
        IMedia first = CreateMedia(mediaType, "First", Constants.System.Root);
        IMedia second = CreateMedia(mediaType, "Second", Constants.System.Root);

        MediaService.DeleteMediaOfType(mediaType.Id);

        AssertDeletedNotificationsShareDeletingState([first.Key, second.Key]);
    }

    private void AssertDeletedNotificationsShareDeletingState(Guid[] expectedDeletedKeys)
    {
        MediaDeletingNotification deleting = Recorder.Starting.OfType<MediaDeletingNotification>().Single();
        MediaDeletedNotification[] deleted = Recorder.Ending.OfType<MediaDeletedNotification>().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(deleted.SelectMany(x => x.DeletedEntities).Select(x => x.Key), Is.EquivalentTo(expectedDeletedKeys));
            Assert.That(deleted.Select(x => x.State), Has.All.SameAs(deleting.State));
            Assert.That(deleting.State.ContainsKey(NotificationStateRecorder.StateKey), Is.True);
        });
    }

    private async Task<IMediaType> CreateMediaType()
    {
        IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("stateMedia", "State Media");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        return mediaType;
    }

    private IMedia CreateMedia(IMediaType mediaType, string name, int parentId)
    {
        IMedia media = MediaBuilder.CreateSimpleMedia(mediaType, name, parentId);
        MediaService.Save(media);
        return media;
    }
}
