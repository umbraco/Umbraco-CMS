// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Verifies the deleting notification published when emptying the media recycle bin, and that its state reaches every
///     deleted notification.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaServiceEmptyRecycleBinNotificationTests : UmbracoIntegrationTest
{
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private NotificationStateRecorder Recorder => GetRequiredService<NotificationStateRecorder>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<NotificationStateRecorder>();
        builder
            .AddNotificationHandler<MediaDeletingNotification, NotificationStateRecorder.WritingHandler<MediaDeletingNotification>>()
            .AddNotificationHandler<MediaDeletedNotification, NotificationStateRecorder.ReadingHandler<MediaDeletedNotification>>();
    }

    public static void ConfigureCancelDeleting(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<MediaDeletingNotification, CancelMediaDeletingHandler>();

    [Test]
    public async Task EmptyRecycleBin_Shares_Deleting_State_With_Deleted_Notifications_For_Items_And_Descendants()
    {
        (IMedia first, IMedia firstChild, IMedia second) = await CreateTrashedMedia();

        OperationResult result = MediaService.EmptyRecycleBin();

        MediaDeletingNotification deleting = Recorder.Starting.OfType<MediaDeletingNotification>().Single();
        MediaDeletedNotification[] deleted = Recorder.Ending.OfType<MediaDeletedNotification>().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(deleting.DeletedEntities.Select(x => x.Key), Is.EquivalentTo(new[] { first.Key, second.Key }));
            Assert.That(deleted.SelectMany(x => x.DeletedEntities).Select(x => x.Key), Is.EquivalentTo(new[] { first.Key, firstChild.Key, second.Key }));
            Assert.That(deleted.Select(x => x.State), Has.All.SameAs(deleting.State));
            Assert.That(deleting.State.ContainsKey(NotificationStateRecorder.StateKey), Is.True);
        });
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureCancelDeleting))]
    public async Task EmptyRecycleBin_Is_Cancelled_By_Deleting_Notification()
    {
        (IMedia first, IMedia firstChild, IMedia second) = await CreateTrashedMedia();

        OperationResult result = MediaService.EmptyRecycleBin();

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.EqualTo(OperationResultType.FailedCancelledByEvent));
            Assert.That(Recorder.Ending.OfType<MediaDeletedNotification>(), Is.Empty);
            Assert.That(MediaService.GetById(first.Key), Is.Not.Null);
            Assert.That(MediaService.GetById(firstChild.Key), Is.Not.Null);
            Assert.That(MediaService.GetById(second.Key), Is.Not.Null);
        });
    }

    private async Task<(IMedia First, IMedia FirstChild, IMedia Second)> CreateTrashedMedia()
    {
        IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("stateMedia", "State Media");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        IMedia first = MediaBuilder.CreateSimpleMedia(mediaType, "First", Constants.System.Root);
        MediaService.Save(first);
        IMedia firstChild = MediaBuilder.CreateSimpleMedia(mediaType, "First Child", first.Id);
        MediaService.Save(firstChild);
        IMedia second = MediaBuilder.CreateSimpleMedia(mediaType, "Second", Constants.System.Root);
        MediaService.Save(second);

        MediaService.MoveToRecycleBin(first);
        MediaService.MoveToRecycleBin(second);

        return (first, firstChild, second);
    }

    private sealed class CancelMediaDeletingHandler : INotificationHandler<MediaDeletingNotification>
    {
        public void Handle(MediaDeletingNotification notification) => notification.Cancel = true;
    }
}
