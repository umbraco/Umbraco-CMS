// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.ServerEvents;

[TestFixture]
internal sealed class ServerEventSenderTests
{
    /// <summary>
    /// Tests that handling a ContentSavedNotification triggers a RoutesCreated event.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var contentKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var content = Mock.Of<IContent>(c =>
            c.Key == contentKey &&
            c.CreateDate == now &&
            c.UpdateDate == now);

        var notification = new ContentSavedNotification(content, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, contentKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.Document);
    }

    /// <summary>
    /// Tests that when a ContentSavedNotification is handled, a RoutesUpdated event is correctly routed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var contentKey = Guid.NewGuid();
        var createDate = DateTime.UtcNow.AddDays(-1);
        var updateDate = DateTime.UtcNow;
        var content = Mock.Of<IContent>(c =>
            c.Key == contentKey &&
            c.CreateDate == createDate &&
            c.UpdateDate == updateDate);

        var notification = new ContentSavedNotification(content, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, contentKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.Document);
    }

    /// <summary>
    /// Verifies that when a <see cref="ContentSavedBlueprintNotification"/> is handled, a <c>RoutesCreated</c> server event is correctly routed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentSavedBlueprintNotification_RoutesCreatedEvent()
    {
        // Arrange
        var blueprintKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var blueprint = Mock.Of<IContent>(c =>
            c.Key == blueprintKey &&
            c.CreateDate == now &&
            c.UpdateDate == now);

        var notification = new ContentSavedBlueprintNotification(blueprint, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, blueprintKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.DocumentBlueprint);
    }

    /// <summary>
    /// Tests that handling a <see cref="ContentSavedBlueprintNotification"/> triggers an Updated server event with the DocumentBlueprint as the event source.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentSavedBlueprintNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var blueprintKey = Guid.NewGuid();
        var blueprint = Mock.Of<IContent>(c =>
            c.Key == blueprintKey &&
            c.CreateDate == DateTime.UtcNow.AddDays(-1) &&
            c.UpdateDate == DateTime.UtcNow);

        var notification = new ContentSavedBlueprintNotification(blueprint, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, blueprintKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.DocumentBlueprint);
    }

    /// <summary>
    /// Tests that handling a ContentTypeSavedNotification results in a RoutesCreated event being routed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentTypeSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IContentType>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new ContentTypeSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.DocumentType);
    }

    /// <summary>
    /// Verifies that handling a <see cref="ContentTypeSavedNotification"/> triggers a <c>RoutesUpdated</c> server event for the corresponding content type.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentTypeSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IContentType>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new ContentTypeSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.DocumentType);
    }

    /// <summary>
    /// Tests that handling a MediaSavedNotification results in a RoutesCreatedEvent being routed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MediaSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IMedia>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new MediaSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.Media);
    }

    /// <summary>
    /// Tests that handling a MediaSavedNotification triggers a RoutesUpdated event.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MediaSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IMedia>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new MediaSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.Media);
    }

    /// <summary>
    /// Tests that when a MediaTypeSavedNotification is handled, a RoutesCreated event is correctly routed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MediaTypeSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IMediaType>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new MediaTypeSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.MediaType);
    }

    /// <summary>
    /// Tests that handling a MediaTypeSavedNotification triggers a RoutesUpdatedEvent.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MediaTypeSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IMediaType>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new MediaTypeSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.MediaType);
    }

    /// <summary>
    /// Verifies that when a <see cref="MemberSavedNotification"/> is handled, a <c>RoutesCreated</c> server event is correctly routed for the member entity.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MemberSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IMember>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new MemberSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.Member);
    }

    /// <summary>
    /// Tests that when a MemberSavedNotification is handled, a RoutesUpdated event is routed correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_MemberSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IMember>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new MemberSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.Member);
    }

    /// <summary>
    /// Tests that when a MemberTypeSavedNotification is handled, a RoutesCreated event is created and routed correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MemberTypeSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IMemberType>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new MemberTypeSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.MemberType);
    }

    /// <summary>
    /// Tests that handling a MemberTypeSavedNotification triggers a RoutesUpdatedEvent.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_MemberTypeSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IMemberType>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new MemberTypeSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.MemberType);
    }

    /// <summary>
    /// Tests that handling a MemberGroupSavedNotification results in a RoutesCreated event being created and routed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MemberGroupSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IMemberGroup>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new MemberGroupSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.MemberGroup);
    }

    /// <summary>
    /// Tests that handling a MemberGroupSavedNotification triggers a RoutesUpdatedEvent.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MemberGroupSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IMemberGroup>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new MemberGroupSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.MemberGroup);
    }

    /// <summary>
    /// Tests that when a DataTypeSavedNotification is handled, a RoutesCreated event is correctly routed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_DataTypeSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IDataType>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new DataTypeSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.DataType);
    }

    /// <summary>
    /// Tests that handling a DataTypeSavedNotification triggers a RoutesUpdated event.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_DataTypeSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IDataType>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new DataTypeSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.DataType);
    }

    /// <summary>
    /// Tests that handling a LanguageSavedNotification triggers a RoutesCreatedEvent.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_LanguageSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<ILanguage>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new LanguageSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.Language);
    }

    /// <summary>
    /// Tests that handling a LanguageSavedNotification triggers a RoutesUpdated event.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_LanguageSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<ILanguage>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new LanguageSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.Language);
    }

    /// <summary>
    /// Tests that handling a PublicAccessEntrySavedNotification results in both a RoutesCreated event and a DocumentUpdated event being routed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_PublicAccessEntrySavedNotification_RoutesCreatedEventAndDocumentUpdatedEvent()
    {
        // Arrange
        var entryKey = Guid.NewGuid();
        var protectedDocumentKey = Guid.NewGuid();
        var protectedNodeId = 123;
        var now = DateTime.UtcNow;

        var entry = new PublicAccessEntry(entryKey, protectedNodeId, loginNodeId: 456, noAccessNodeId: 789, [])
        {
            CreateDate = now,
            UpdateDate = now,
        };

        var idKeyMapMock = new Mock<IIdKeyMap>();
        idKeyMapMock
            .Setup(x => x.GetKeyForId(protectedNodeId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(protectedDocumentKey));

        var notification = new PublicAccessEntrySavedNotification(entry, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter, idKeyMapMock.Object);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(
            recordingRouter,
            entryKey,
            Constants.ServerEvents.EventType.Created,
            Constants.ServerEvents.EventSource.PublicAccessEntry,
            expectedNumberOfRoutedEvents: 2);

        // Assert the second event is for the protected document.
        var documentEvent = recordingRouter.RoutedEvents[1];
        Assert.Multiple(() =>
        {
            Assert.That(documentEvent.EventType, Is.EqualTo(Constants.ServerEvents.EventType.Updated));
            Assert.That(documentEvent.EventSource, Is.EqualTo(Constants.ServerEvents.EventSource.Document));
            Assert.That(documentEvent.Key, Is.EqualTo(protectedDocumentKey));
        });
    }

    /// <summary>
    /// Tests that handling a <see cref="PublicAccessEntrySavedNotification"/> results in routing both a routes updated event and a document updated event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_PublicAccessEntrySavedNotification_RoutesUpdatedEventAndDocumentUpdatedEvent()
    {
        // Arrange
        var entryKey = Guid.NewGuid();
        var protectedDocumentKey = Guid.NewGuid();
        var protectedNodeId = 123;
        var createDate = DateTime.UtcNow.AddDays(-1);
        var updateDate = DateTime.UtcNow;

        var entry = new PublicAccessEntry(entryKey, protectedNodeId, loginNodeId: 456, noAccessNodeId: 789, [])
        {
            CreateDate = createDate,
            UpdateDate = updateDate,
        };

        var idKeyMapMock = new Mock<IIdKeyMap>();
        idKeyMapMock
            .Setup(x => x.GetKeyForId(protectedNodeId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(protectedDocumentKey));

        var notification = new PublicAccessEntrySavedNotification(entry, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter, idKeyMapMock.Object);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(
            recordingRouter,
            entryKey,
            Constants.ServerEvents.EventType.Updated,
            Constants.ServerEvents.EventSource.PublicAccessEntry,
            expectedNumberOfRoutedEvents: 2);

        // Assert the second event is for the protected document.
        var documentEvent = recordingRouter.RoutedEvents[1];
        Assert.Multiple(() =>
        {
            Assert.That(documentEvent.EventType, Is.EqualTo(Constants.ServerEvents.EventType.Updated));
            Assert.That(documentEvent.EventSource, Is.EqualTo(Constants.ServerEvents.EventSource.Document));
            Assert.That(documentEvent.Key, Is.EqualTo(protectedDocumentKey));
        });
    }

    /// <summary>
    /// Tests that handling a PublicAccessEntryDeletedNotification routes both a Deleted event for the entry
    /// and an Updated event for the associated protected document.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_PublicAccessEntryDeletedNotification_RoutesDeletedEventAndDocumentUpdatedEvent()
    {
        // Arrange
        var entryKey = Guid.NewGuid();
        var protectedDocumentKey = Guid.NewGuid();
        var protectedNodeId = 123;

        var entry = new PublicAccessEntry(entryKey, protectedNodeId, loginNodeId: 456, noAccessNodeId: 789, []);

        var idKeyMapMock = new Mock<IIdKeyMap>();
        idKeyMapMock
            .Setup(x => x.GetKeyForId(protectedNodeId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(protectedDocumentKey));

        var notification = new PublicAccessEntryDeletedNotification(entry, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter, idKeyMapMock.Object);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(
            recordingRouter,
            entryKey,
            Constants.ServerEvents.EventType.Deleted,
            Constants.ServerEvents.EventSource.PublicAccessEntry,
            expectedNumberOfRoutedEvents: 2);

        // Assert the second event is for the protected document.
        var documentEvent = recordingRouter.RoutedEvents[1];
        Assert.Multiple(() =>
        {
            Assert.That(documentEvent.EventType, Is.EqualTo(Constants.ServerEvents.EventType.Updated));
            Assert.That(documentEvent.EventSource, Is.EqualTo(Constants.ServerEvents.EventSource.Document));
            Assert.That(documentEvent.Key, Is.EqualTo(protectedDocumentKey));
        });
    }

    [Test]
    public async Task HandleAsync_ScriptSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IScript>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new ScriptSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.Script);
    }

    /// <summary>
    /// Tests that handling a ScriptSavedNotification triggers a RoutesUpdated event.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ScriptSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IScript>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new ScriptSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.Script);
    }

    /// <summary>
    /// Tests that when a StylesheetSavedNotification is handled, a RoutesCreated event is created and routed correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_StylesheetSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IStylesheet>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new StylesheetSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.Stylesheet);
    }

    /// <summary>
    /// Tests that handling a StylesheetSavedNotification triggers a RoutesUpdatedEvent.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_StylesheetSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IStylesheet>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new StylesheetSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.Stylesheet);
    }

    /// <summary>
    /// Verifies that when a <see cref="TemplateSavedNotification"/> is handled, a <c>RoutesCreated</c> server event is correctly routed.
    /// </summary>
    /// <remarks>
    /// This test ensures that saving a template triggers the expected server event with the correct entity key and event type.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_TemplateSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<ITemplate>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new TemplateSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.Template);
    }

    /// <summary>
    /// Tests that handling a <see cref="TemplateSavedNotification"/> results in a <c>RoutesUpdated</c> server event being routed for the updated template.
    /// </summary>
    /// <remarks>
    /// Asserts that the routed event has the correct entity key, event type, and event source for a template update.
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_TemplateSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<ITemplate>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new TemplateSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.Template);
    }

    /// <summary>
    /// Tests that when a DictionaryItemSavedNotification is handled, a RoutesCreated event is correctly routed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_DictionaryItemSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IDictionaryItem>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new DictionaryItemSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.DictionaryItem);
    }

    /// <summary>
    /// Tests that handling a DictionaryItemSavedNotification triggers a RoutesUpdated event.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_DictionaryItemSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IDictionaryItem>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new DictionaryItemSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.DictionaryItem);
    }

    /// <summary>
    /// Tests that handling a DomainSavedNotification results in a RoutesCreated event being routed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_DomainSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IDomain>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new DomainSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.Domain);
    }

    /// <summary>
    /// Tests that handling a DomainSavedNotification triggers a RoutesUpdatedEvent.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_DomainSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IDomain>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new DomainSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.Domain);
    }

    /// <summary>
    /// Tests that handling a PartialViewSavedNotification results in a RoutesCreated event being routed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_PartialViewSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IPartialView>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new PartialViewSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.PartialView);
    }

    /// <summary>
    /// Verifies that when a <see cref="PartialViewSavedNotification"/> is handled, a <c>RoutesUpdatedEvent</c> is triggered.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_PartialViewSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IPartialView>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new PartialViewSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.PartialView);
    }

    /// <summary>
    /// Tests that handling a RelationSavedNotification results in a RoutesCreated event being routed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_RelationSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IRelation>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new RelationSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.Relation);
    }

    /// <summary>
    /// Tests that handling a RelationSavedNotification triggers a RoutesUpdated event.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_RelationSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IRelation>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new RelationSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.Relation);
    }

    /// <summary>
    /// Tests that handling a RelationTypeSavedNotification results in a RoutesCreatedEvent being routed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_RelationTypeSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IRelationType>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new RelationTypeSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.RelationType);
    }

    /// <summary>
    /// Tests that handling a RelationTypeSavedNotification triggers a RoutesUpdated event.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_RelationTypeSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IRelationType>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new RelationTypeSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.RelationType);
    }

    /// <summary>
    /// Tests that when a UserGroupSavedNotification is handled, a RoutesCreated event is correctly created and routed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_UserGroupSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IUserGroup>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new UserGroupSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.UserGroup);
    }

    /// <summary>
    /// Tests that handling a UserGroupSavedNotification triggers a RoutesUpdatedEvent.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_UserGroupSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IUserGroup>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new UserGroupSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.UserGroup);
    }

    /// <summary>
    /// Verifies that when handling a <see cref="UserSavedNotification"/>, a Created server event is routed for the user and a notification is sent to the current user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_UserSavedNotification_RoutesCreatedEventAndNotifiesUser()
    {
        // Arrange
        var userKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var user = Mock.Of<IUser>(u => u.Key == userKey && u.CreateDate == now && u.UpdateDate == now);

        var notification = new UserSavedNotification(user, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert - Verify the routed User event
        AssertPrimaryRoutedEvent(recordingRouter, userKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.User);

        // Assert - Verify the user notification for CurrentUser
        Assert.That(recordingRouter.UserNotifications, Has.Count.EqualTo(1));
        var (serverEvent, notifiedUserKey) = recordingRouter.UserNotifications[0];
        Assert.Multiple(() =>
        {
            Assert.That(serverEvent.EventType, Is.EqualTo(Constants.ServerEvents.EventType.Updated));
            Assert.That(serverEvent.EventSource, Is.EqualTo(Constants.ServerEvents.EventSource.CurrentUser));
            Assert.That(serverEvent.Key, Is.EqualTo(userKey));
            Assert.That(notifiedUserKey, Is.EqualTo(userKey));
        });
    }

    /// <summary>
    /// Tests that when a UserSavedNotification is handled, a RoutesUpdated event is routed and the user is notified.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_UserSavedNotification_RoutesUpdatedEventAndNotifiesUser()
    {
        // Arrange
        var userKey = Guid.NewGuid();
        var user = Mock.Of<IUser>(u => u.Key == userKey && u.CreateDate == DateTime.UtcNow.AddDays(-1) && u.UpdateDate == DateTime.UtcNow);

        var notification = new UserSavedNotification(user, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert - Verify the routed User event
        AssertPrimaryRoutedEvent(recordingRouter, userKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.User);

        // Assert - Verify the user notification for CurrentUser
        Assert.That(recordingRouter.UserNotifications, Has.Count.EqualTo(1));
        var (serverEvent, notifiedUserKey) = recordingRouter.UserNotifications[0];
        Assert.Multiple(() =>
        {
            Assert.That(serverEvent.EventType, Is.EqualTo(Constants.ServerEvents.EventType.Updated));
            Assert.That(serverEvent.EventSource, Is.EqualTo(Constants.ServerEvents.EventSource.CurrentUser));
            Assert.That(serverEvent.Key, Is.EqualTo(userKey));
            Assert.That(notifiedUserKey, Is.EqualTo(userKey));
        });
    }

    /// <summary>
    /// Tests that handling a WebhookSavedNotification triggers a RoutesCreated event.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_WebhookSavedNotification_RoutesCreatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var entity = Mock.Of<IWebhook>(e => e.Key == entityKey && e.CreateDate == now && e.UpdateDate == now);

        var notification = new WebhookSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Created, Constants.ServerEvents.EventSource.Webhook);
    }

    /// <summary>
    /// Tests that handling a WebhookSavedNotification triggers a RoutesUpdated event.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_WebhookSavedNotification_RoutesUpdatedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IWebhook>(e => e.Key == entityKey && e.CreateDate == DateTime.UtcNow.AddDays(-1) && e.UpdateDate == DateTime.UtcNow);

        var notification = new WebhookSavedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Updated, Constants.ServerEvents.EventSource.Webhook);
    }

    /// <summary>
    /// Tests that handling a DataTypeDeletedNotification triggers a RoutesDeletedEvent.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_DataTypeDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IDataType>(e => e.Key == entityKey);

        var notification = new DataTypeDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.DataType);
    }

    /// <summary>
    /// Tests that handling a ContentDeletedNotification routes a Deleted event for the content entity.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IContent>(e => e.Key == entityKey);

        var notification = new ContentDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.Document);
    }

    /// <summary>
    /// Verifies that when a <see cref="ContentDeletedBlueprintNotification"/> is handled, a deleted event is routed for the corresponding blueprint.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentDeletedBlueprintNotification_RoutesDeletedEvent()
    {
        // Arrange
        var blueprintKey = Guid.NewGuid();
        var blueprint = Mock.Of<IContent>(e => e.Key == blueprintKey);

        var notification = new ContentDeletedBlueprintNotification(blueprint, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, blueprintKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.DocumentBlueprint);
    }

    /// <summary>
    /// Verifies that handling a <see cref="ContentTypeDeletedNotification"/> results in a <c>RoutesDeletedEvent</c> being routed
    /// with the correct entity key, event type, and event source.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentTypeDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IContentType>(e => e.Key == entityKey);

        var notification = new ContentTypeDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.DocumentType);
    }

    /// <summary>
    /// Verifies that when a <see cref="MediaDeletedNotification"/> is handled, a Deleted server event is routed for the corresponding media entity.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MediaDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IMedia>(e => e.Key == entityKey);

        var notification = new MediaDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.Media);
    }

    /// <summary>
    /// Verifies that handling a <see cref="MediaTypeDeletedNotification"/> results in a <c>RoutesDeletedEvent</c> being routed by the server event sender.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MediaTypeDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IMediaType>(e => e.Key == entityKey);

        var notification = new MediaTypeDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.MediaType);
    }

    /// <summary>
    /// Tests that handling a MemberDeletedNotification routes a Deleted event for the Member entity.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MemberDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IMember>(e => e.Key == entityKey);

        var notification = new MemberDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.Member);
    }

    /// <summary>
    /// Tests that when a MemberTypeDeletedNotification is handled, a deleted event is routed for the member type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MemberTypeDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IMemberType>(e => e.Key == entityKey);

        var notification = new MemberTypeDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.MemberType);
    }

    /// <summary>
    /// Verifies that handling a <see cref="MemberGroupDeletedNotification"/> results in routing a deleted server event for the corresponding member group.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MemberGroupDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IMemberGroup>(e => e.Key == entityKey);

        var notification = new MemberGroupDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.MemberGroup);
    }

    /// <summary>
    /// Tests that handling a <see cref="LanguageDeletedNotification"/> results in a <c>RoutesDeletedEvent</c> being routed by the server event sender.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_LanguageDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<ILanguage>(e => e.Key == entityKey);

        var notification = new LanguageDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.Language);
    }

    /// <summary>
    /// Tests that handling a ScriptDeletedNotification routes a Deleted event for the Script entity.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_ScriptDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IScript>(e => e.Key == entityKey);

        var notification = new ScriptDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.Script);
    }

    /// <summary>
    /// Verifies that when a <see cref="StylesheetDeletedNotification"/> is handled, a Deleted server event is routed for the corresponding stylesheet source.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_StylesheetDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IStylesheet>(e => e.Key == entityKey);

        var notification = new StylesheetDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.Stylesheet);
    }

    /// <summary>
    /// Verifies that handling a <see cref="TemplateDeletedNotification"/> results in a <c>RoutesDeletedEvent</c> being routed correctly by the server event sender.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_TemplateDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<ITemplate>(e => e.Key == entityKey);

        var notification = new TemplateDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.Template);
    }

    /// <summary>
    /// Tests that when a DictionaryItemDeletedNotification is handled, a RoutesDeletedEvent is correctly routed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_DictionaryItemDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IDictionaryItem>(e => e.Key == entityKey);

        var notification = new DictionaryItemDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.DictionaryItem);
    }

    /// <summary>
    /// Tests that when a <see cref="DomainDeletedNotification"/> is handled, a Deleted server event is routed for the corresponding domain.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_DomainDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IDomain>(e => e.Key == entityKey);

        var notification = new DomainDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.Domain);
    }

    /// <summary>
    /// Tests that handling a PartialViewDeletedNotification routes a Deleted event for the PartialView entity.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_PartialViewDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IPartialView>(e => e.Key == entityKey);

        var notification = new PartialViewDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.PartialView);
    }

    /// <summary>
    /// Tests that handling a RelationDeletedNotification routes a Deleted event for a Relation entity.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_RelationDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IRelation>(e => e.Key == entityKey);

        var notification = new RelationDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.Relation);
    }

    /// <summary>
    /// Tests that handling a RelationTypeDeletedNotification routes a Deleted event for the RelationType.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_RelationTypeDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IRelationType>(e => e.Key == entityKey);

        var notification = new RelationTypeDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.RelationType);
    }

    /// <summary>
    /// Verifies that when a <see cref="UserGroupDeletedNotification"/> is handled, a deleted server event is correctly routed for the corresponding user group.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_UserGroupDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IUserGroup>(e => e.Key == entityKey);

        var notification = new UserGroupDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.UserGroup);
    }

    /// <summary>
    /// Tests that when a UserDeletedNotification is handled, a RoutesDeletedEvent is routed correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_UserDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IUser>(e => e.Key == entityKey);

        var notification = new UserDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.User);
    }

    /// <summary>
    /// Verifies that handling a <see cref="WebhookDeletedNotification"/> results in routing a deleted server event for the corresponding webhook.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_WebhookDeletedNotification_RoutesDeletedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IWebhook>(e => e.Key == entityKey);

        var notification = new WebhookDeletedNotification(entity, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Deleted, Constants.ServerEvents.EventSource.Webhook);
    }

    /// <summary>
    /// Tests that handling a ContentMovedToRecycleBinNotification routes a Trashed event correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentMovedToRecycleBinNotification_RoutesTrashedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IContent>(e => e.Key == entityKey);
        var moveInfo = new MoveToRecycleBinEventInfo<IContent>(entity, "-1,123");

        var notification = new ContentMovedToRecycleBinNotification(moveInfo, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Trashed, Constants.ServerEvents.EventSource.Document);
    }

    /// <summary>
    /// Tests that handling a MediaMovedToRecycleBinNotification routes a Trashed event for the media entity.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MediaMovedToRecycleBinNotification_RoutesTrashedEvent()
    {
        // Arrange
        var entityKey = Guid.NewGuid();
        var entity = Mock.Of<IMedia>(e => e.Key == entityKey);
        var moveInfo = new MoveToRecycleBinEventInfo<IMedia>(entity, "-1,456");

        var notification = new MediaMovedToRecycleBinNotification(moveInfo, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert
        AssertPrimaryRoutedEvent(recordingRouter, entityKey, Constants.ServerEvents.EventType.Trashed, Constants.ServerEvents.EventSource.Media);
    }

    /// <summary>
    /// Tests that the HandleAsync method correctly routes an updated event for composing content types
    /// and skips removed content types when handling a ContentTypeChangedNotification.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentTypeChangedNotification_RoutesUpdatedEventForComposingTypes()
    {
        // Arrange
        var deletedTypeKey = Guid.NewGuid();
        var composingTypeKey = Guid.NewGuid();

        var deletedType = Mock.Of<IContentType>(t => t.Key == deletedTypeKey);
        var composingType = Mock.Of<IContentType>(t => t.Key == composingTypeKey);

        var changes = new[]
        {
            new ContentTypeChange<IContentType>(deletedType, ContentTypeChangeTypes.Remove),
            new ContentTypeChange<IContentType>(composingType, ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther),
        };

        var notification = new ContentTypeChangedNotification(changes, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert - only the composing type should be routed (removed types are skipped).
        Assert.That(recordingRouter.RoutedEvents, Has.Count.EqualTo(1));
        var serverEvent = recordingRouter.RoutedEvents[0];
        Assert.Multiple(() =>
        {
            Assert.That(serverEvent.EventType, Is.EqualTo(Constants.ServerEvents.EventType.Updated));
            Assert.That(serverEvent.EventSource, Is.EqualTo(Constants.ServerEvents.EventSource.DocumentType));
            Assert.That(serverEvent.Key, Is.EqualTo(composingTypeKey));
        });
    }

    /// <summary>
    /// Tests that the HandleAsync method correctly skips removed content types when processing a ContentTypeChangedNotification.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentTypeChangedNotification_SkipsRemovedTypes()
    {
        // Arrange
        var removedTypeKey = Guid.NewGuid();
        var removedType = Mock.Of<IContentType>(t => t.Key == removedTypeKey);

        var changes = new[]
        {
            new ContentTypeChange<IContentType>(removedType, ContentTypeChangeTypes.Remove),
        };

        var notification = new ContentTypeChangedNotification(changes, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert - no events should be routed for removed types.
        Assert.That(recordingRouter.RoutedEvents, Has.Count.EqualTo(0));
    }

    /// <summary>
    /// Tests that the HandleAsync method skips processing content types that are newly created.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_ContentTypeChangedNotification_SkipsCreatedTypes()
    {
        // Arrange
        var createdTypeKey = Guid.NewGuid();
        var createdType = Mock.Of<IContentType>(t => t.Key == createdTypeKey);

        var changes = new[]
        {
            new ContentTypeChange<IContentType>(createdType, ContentTypeChangeTypes.Create),
        };

        var notification = new ContentTypeChangedNotification(changes, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert - no events should be routed for created types (already handled by Saved notification).
        Assert.That(recordingRouter.RoutedEvents, Has.Count.EqualTo(0));
    }

    /// <summary>
    /// Tests that when handling a MediaTypeChangedNotification, only the composing types trigger route updated events.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task HandleAsync_MediaTypeChangedNotification_RoutesUpdatedEventForComposingTypes()
    {
        // Arrange
        var deletedTypeKey = Guid.NewGuid();
        var composingTypeKey = Guid.NewGuid();

        var deletedType = Mock.Of<IMediaType>(t => t.Key == deletedTypeKey);
        var composingType = Mock.Of<IMediaType>(t => t.Key == composingTypeKey);

        var changes = new[]
        {
            new ContentTypeChange<IMediaType>(deletedType, ContentTypeChangeTypes.Remove),
            new ContentTypeChange<IMediaType>(composingType, ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther),
        };

        var notification = new MediaTypeChangedNotification(changes, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert - only the composing type should be routed.
        Assert.That(recordingRouter.RoutedEvents, Has.Count.EqualTo(1));
        var serverEvent = recordingRouter.RoutedEvents[0];
        Assert.Multiple(() =>
        {
            Assert.That(serverEvent.EventType, Is.EqualTo(Constants.ServerEvents.EventType.Updated));
            Assert.That(serverEvent.EventSource, Is.EqualTo(Constants.ServerEvents.EventSource.MediaType));
            Assert.That(serverEvent.Key, Is.EqualTo(composingTypeKey));
        });
    }

    /// <summary>
    /// Tests that when a MemberTypeChangedNotification is handled, only the composing member types trigger a RoutesUpdated event.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task HandleAsync_MemberTypeChangedNotification_RoutesUpdatedEventForComposingTypes()
    {
        // Arrange
        var deletedTypeKey = Guid.NewGuid();
        var composingTypeKey = Guid.NewGuid();

        var deletedType = Mock.Of<IMemberType>(t => t.Key == deletedTypeKey);
        var composingType = Mock.Of<IMemberType>(t => t.Key == composingTypeKey);

        var changes = new[]
        {
            new ContentTypeChange<IMemberType>(deletedType, ContentTypeChangeTypes.Remove),
            new ContentTypeChange<IMemberType>(composingType, ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther),
        };

        var notification = new MemberTypeChangedNotification(changes, new EventMessages());
        var recordingRouter = new RecordingServerEventRouter();
        var serverEventSender = CreateServerEventSender(recordingRouter);

        // Act
        await serverEventSender.HandleAsync(notification, CancellationToken.None);

        // Assert - only the composing type should be routed.
        Assert.That(recordingRouter.RoutedEvents, Has.Count.EqualTo(1));
        var serverEvent = recordingRouter.RoutedEvents[0];
        Assert.Multiple(() =>
        {
            Assert.That(serverEvent.EventType, Is.EqualTo(Constants.ServerEvents.EventType.Updated));
            Assert.That(serverEvent.EventSource, Is.EqualTo(Constants.ServerEvents.EventSource.MemberType));
            Assert.That(serverEvent.Key, Is.EqualTo(composingTypeKey));
        });
    }

    private static ServerEventSender CreateServerEventSender(RecordingServerEventRouter recordingRouter) => new(recordingRouter, Mock.Of<IIdKeyMap>());

    private static ServerEventSender CreateServerEventSender(RecordingServerEventRouter recordingRouter, IIdKeyMap idKeyMap) => new(recordingRouter, idKeyMap);

    private static void AssertPrimaryRoutedEvent(RecordingServerEventRouter recordingRouter, Guid key, string eventType, string eventSource, int expectedNumberOfRoutedEvents = 1)
    {
        Assert.That(recordingRouter.RoutedEvents, Has.Count.EqualTo(expectedNumberOfRoutedEvents));
        var serverEvent = recordingRouter.RoutedEvents[0];
        Assert.Multiple(() =>
        {
            Assert.That(serverEvent.EventType, Is.EqualTo(eventType));
            Assert.That(serverEvent.EventSource, Is.EqualTo(eventSource));
            Assert.That(serverEvent.Key, Is.EqualTo(key));
        });
    }

    private class RecordingServerEventRouter : IServerEventRouter
    {
    /// <summary>
    /// Routes the specified server event asynchronously.
    /// </summary>
    /// <param name="serverEvent">The server event to route.</param>
    /// <returns>A task that represents the asynchronous routing operation.</returns>
        public Task RouteEventAsync(ServerEvent serverEvent)
        {
            RoutedEvents.Add(serverEvent);
            return Task.CompletedTask;
        }

    /// <summary>
    /// Gets the list of routed server events.
    /// </summary>
        public List<ServerEvent> RoutedEvents { get; } = [];

    /// <summary>
    /// Notifies a user asynchronously with the specified server event.
    /// </summary>
    /// <param name="serverEvent">The server event to notify the user about.</param>
    /// <param name="userKey">The unique identifier of the user to notify.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
        public Task NotifyUserAsync(ServerEvent serverEvent, Guid userKey)
        {
            UserNotifications.Add((serverEvent, userKey));
            return Task.CompletedTask;
        }

    /// <summary>
    /// Gets the list of user notifications consisting of server events and associated user keys.
    /// </summary>
        public List<(ServerEvent ServerEvent, Guid UserKey)> UserNotifications { get; } = [];

    /// <summary>
    /// Broadcasts the specified server event asynchronously.
    /// </summary>
    /// <param name="serverEvent">The server event to broadcast.</param>
    /// <returns>A task that represents the asynchronous broadcast operation.</returns>
        public Task BroadcastEventAsync(ServerEvent serverEvent) => Task.CompletedTask;
    }
}
