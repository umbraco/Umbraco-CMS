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

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.ServerEvents;

[TestFixture]
internal sealed class ServerEventSenderTests
{
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
        public Task RouteEventAsync(ServerEvent serverEvent)
        {
            RoutedEvents.Add(serverEvent);
            return Task.CompletedTask;
        }

        public List<ServerEvent> RoutedEvents { get; } = [];

        public Task NotifyUserAsync(ServerEvent serverEvent, Guid userKey)
        {
            UserNotifications.Add((serverEvent, userKey));
            return Task.CompletedTask;
        }

        public List<(ServerEvent ServerEvent, Guid UserKey)> UserNotifications { get; } = [];

        public Task BroadcastEventAsync(ServerEvent serverEvent) => Task.CompletedTask;
    }
}
