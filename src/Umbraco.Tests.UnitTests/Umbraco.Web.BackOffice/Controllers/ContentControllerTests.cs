using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
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

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Controllers
{
    [TestFixture]
    public class ContentControllerTests
    {
        [Test]
        public void Root_Node_With_Domains_Causes_No_Error()
        {
            // Setup domain service
            var domainServiceMock = new Mock<IDomainService>();
            domainServiceMock.Setup(x => x.GetAssignedDomains(1060, It.IsAny<bool>()))
                .Returns(new List<IDomain>{new UmbracoDomain("/", "da-dk"), new UmbracoDomain("/en", "en-us")});

            // Create content type and content
            IContentType contentType = new ContentTypeBuilder().WithContentVariation(ContentVariation.Culture).Build();

            // Create content, we need to specify and ID configure domain service
            Content rootNode = new ContentBuilder()
                .WithContentType(contentType)
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

            var culturesPublished = new List<string> { "en-us", "da-dk" };
            var publishResult = new PublishResult(new EventMessages(), rootNode);

            ContentController contentController = CreateContentController(domainServiceMock.Object);
            contentController.VerifyDomainsForCultures(rootNode, culturesPublished, publishResult);

            IEnumerable<EventMessage> eventMessages = publishResult.EventMessages.GetAll();
            Assert.IsEmpty(eventMessages);
        }

        private ContentController CreateContentController(IDomainService domainService)
        {
            var controller = new ContentController(
                Mock.Of<ICultureDictionary>(),
                NullLoggerFactory.Instance,
                Mock.Of<IShortStringHelper>(),
                Mock.Of<IEventMessagesFactory>(),
                Mock.Of<ILocalizedTextService>(),
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
                Mock.Of<IScopeProvider>(),
                Mock.Of<IAuthorizationService>()
            );

            return controller;
        }
    }
}
