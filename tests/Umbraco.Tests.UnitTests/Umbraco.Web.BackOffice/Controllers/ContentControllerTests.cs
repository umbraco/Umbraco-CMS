using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Controllers;

[TestFixture]
public class ContentControllerTests
{
    [Test]
    public void Root_Node_With_Domains_Causes_No_Warning()
    {
        // Setup domain service
        var domainServiceMock = new Mock<IDomainService>();
        domainServiceMock.Setup(x => x.GetAssignedDomains(1060, It.IsAny<bool>()))
            .Returns(new[] { new UmbracoDomain("/", "da-dk"), new UmbracoDomain("/en", "en-us") });

        // Create content, we need to specify and ID in order to be able to configure domain service
        var rootNode = new ContentBuilder()
            .WithContentType(CreateContentType())
            .WithId(1060)
            .AddContentCultureInfosCollection()
            .AddCultureInfos()
            .WithCultureIso("da-dk")
            .Done()
            .AddCultureInfos()
            .WithCultureIso("en-us")
            .Done()
            .Done()
            .Build();

        var culturesPublished = new[] { "en-us", "da-dk" };
        var notifications = new SimpleNotificationModel();

        var contentController = CreateContentController(domainServiceMock.Object);
        contentController.AddDomainWarnings(rootNode, culturesPublished, notifications);

        Assert.IsEmpty(notifications.Notifications);
    }

    [Test]
    public void Node_With_Single_Published_Culture_Causes_No_Warning()
    {
        var domainServiceMock = new Mock<IDomainService>();
        domainServiceMock.Setup(x => x.GetAssignedDomains(It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Enumerable.Empty<IDomain>());

        var rootNode = new ContentBuilder()
            .WithContentType(CreateContentType())
            .WithId(1060)
            .AddContentCultureInfosCollection()
            .AddCultureInfos()
            .WithCultureIso("da-dk")
            .Done()
            .Done()
            .Build();

        var culturesPublished = new[] { "da-dk" };
        var notifications = new SimpleNotificationModel();

        var contentController = CreateContentController(domainServiceMock.Object);
        contentController.AddDomainWarnings(rootNode, culturesPublished, notifications);

        Assert.IsEmpty(notifications.Notifications);
    }

    [Test]
    public void Root_Node_Without_Domains_Causes_SingleWarning()
    {
        var domainServiceMock = new Mock<IDomainService>();
        domainServiceMock.Setup(x => x.GetAssignedDomains(It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Enumerable.Empty<IDomain>());

        var rootNode = new ContentBuilder()
            .WithContentType(CreateContentType())
            .WithId(1060)
            .AddContentCultureInfosCollection()
            .AddCultureInfos()
            .WithCultureIso("da-dk")
            .Done()
            .AddCultureInfos()
            .WithCultureIso("en-us")
            .Done()
            .Done()
            .Build();

        var culturesPublished = new[] { "en-us", "da-dk" };
        var notifications = new SimpleNotificationModel();

        var contentController = CreateContentController(domainServiceMock.Object);
        contentController.AddDomainWarnings(rootNode, culturesPublished, notifications);
        Assert.AreEqual(1, notifications.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
    }

    [Test]
    public void One_Warning_Per_Culture_Being_Published()
    {
        var domainServiceMock = new Mock<IDomainService>();
        domainServiceMock.Setup(x => x.GetAssignedDomains(It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(new[] { new UmbracoDomain("/", "da-dk") });

        var rootNode = new ContentBuilder()
            .WithContentType(CreateContentType())
            .WithId(1060)
            .AddContentCultureInfosCollection()
            .AddCultureInfos()
            .WithCultureIso("da-dk")
            .Done()
            .AddCultureInfos()
            .WithCultureIso("en-us")
            .Done()
            .Done()
            .Build();

        var culturesPublished = new[] { "en-us", "da-dk", "nl-bk", "se-sv" };
        var notifications = new SimpleNotificationModel();

        var contentController = CreateContentController(domainServiceMock.Object);
        contentController.AddDomainWarnings(rootNode, culturesPublished, notifications);
        Assert.AreEqual(3, notifications.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
    }

    [Test]
    public void Ancestor_Domains_Counts()
    {
        var rootId = 1060;
        var level1Id = 1061;
        var level2Id = 1062;
        var level3Id = 1063;

        var domainServiceMock = new Mock<IDomainService>();
        domainServiceMock.Setup(x => x.GetAssignedDomains(rootId, It.IsAny<bool>()))
            .Returns(new[] { new UmbracoDomain("/", "da-dk") });

        domainServiceMock.Setup(x => x.GetAssignedDomains(level1Id, It.IsAny<bool>()))
            .Returns(new[] { new UmbracoDomain("/en", "en-us") });

        domainServiceMock.Setup(x => x.GetAssignedDomains(level2Id, It.IsAny<bool>()))
            .Returns(new[] { new UmbracoDomain("/se", "se-sv"), new UmbracoDomain("/nl", "nl-bk") });

        var level3Node = new ContentBuilder()
            .WithContentType(CreateContentType())
            .WithId(level3Id)
            .WithPath($"-1,{rootId},{level1Id},{level2Id},{level3Id}")
            .AddContentCultureInfosCollection()
            .AddCultureInfos()
            .WithCultureIso("da-dk")
            .Done()
            .AddCultureInfos()
            .WithCultureIso("en-us")
            .Done()
            .AddCultureInfos()
            .WithCultureIso("se-sv")
            .Done()
            .AddCultureInfos()
            .WithCultureIso("nl-bk")
            .Done()
            .AddCultureInfos()
            .WithCultureIso("de-de")
            .Done()
            .Done()
            .Build();

        var culturesPublished = new[] { "en-us", "da-dk", "nl-bk", "se-sv", "de-de" };

        var contentController = CreateContentController(domainServiceMock.Object);
        var notifications = new SimpleNotificationModel();

        contentController.AddDomainWarnings(level3Node, culturesPublished, notifications);

        // We expect one error because all domains except "de-de" is registered somewhere in the ancestor path
        Assert.AreEqual(1, notifications.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
    }

    [Test]
    public void Only_Warns_About_Cultures_Being_Published()
    {
        var domainServiceMock = new Mock<IDomainService>();
        domainServiceMock.Setup(x => x.GetAssignedDomains(It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(new[] { new UmbracoDomain("/", "da-dk") });

        var rootNode = new ContentBuilder()
            .WithContentType(CreateContentType())
            .WithId(1060)
            .AddContentCultureInfosCollection()
            .AddCultureInfos()
            .WithCultureIso("da-dk")
            .Done()
            .AddCultureInfos()
            .WithCultureIso("en-us")
            .Done()
            .AddCultureInfos()
            .WithCultureIso("se-sv")
            .Done()
            .AddCultureInfos()
            .WithCultureIso("de-de")
            .Done()
            .Done()
            .Build();

        var culturesPublished = new[] { "en-us", "se-sv" };
        var notifications = new SimpleNotificationModel();

        var contentController = CreateContentController(domainServiceMock.Object);
        contentController.AddDomainWarnings(rootNode, culturesPublished, notifications);

        // We only get two errors, one for each culture being published, so no errors from previously published cultures.
        Assert.AreEqual(2, notifications.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
    }

    private ContentController CreateContentController(IDomainService domainService)
    {
        // We have to configure ILocalizedTextService to return a new string every time Localize is called
        // Otherwise it won't add the notification because it skips dupes
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns(() => Guid.NewGuid().ToString());

        var controller = new ContentController(
            Mock.Of<ICultureDictionary>(),
            NullLoggerFactory.Instance,
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IEventMessagesFactory>(),
            localizedTextServiceMock.Object,
            new PropertyEditorCollection(new DataEditorCollection(() => null)),
            Mock.Of<IContentService>(),
            Mock.Of<IUserService>(),
            Mock.Of<IBackOfficeSecurityAccessor>(),
            Mock.Of<IContentTypeService>(),
            Mock.Of<IUmbracoMapper>(),
            Mock.Of<IPublishedUrlProvider>(),
            domainService,
            Mock.Of<IDataTypeService>(),
            Mock.Of<ILocalizationService>(),
            Mock.Of<IFileService>(),
            Mock.Of<INotificationService>(),
            new ActionCollection(() => null),
            Mock.Of<ISqlContext>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<IAuthorizationService>(),
            Mock.Of<IContentVersionService>(),
            Mock.Of<ICultureImpactFactory>());

        return controller;
    }

    private IContentType CreateContentType() =>
        new ContentTypeBuilder().WithContentVariation(ContentVariation.Culture).Build();
}
