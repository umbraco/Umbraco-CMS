using System;
using System.Globalization;
using System.Linq;
using System.Web.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Templates;
using Umbraco.Core.Persistence;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Tests.Common;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.Security.Providers;
using Umbraco.Web.WebApi;
using Current = Umbraco.Web.Composing.Current;


namespace Umbraco.Tests.Testing.TestingTests
{
    [TestFixture]
    public class MockTests : UmbracoTestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
        }

        [Test]
        public void Can_Mock_Service_Context()
        {
            // ReSharper disable once UnusedVariable
            var serviceContext = TestObjects.GetServiceContextMock();
            Assert.Pass();
        }

        [Test]
        public void Can_Mock_Umbraco_Context()
        {
            var umbracoContext = TestObjects.GetUmbracoContextMock(Current.UmbracoContextAccessor);
            Assert.AreEqual(umbracoContext, Current.UmbracoContext);
        }

        [Test]
        public void Can_Mock_Umbraco_Helper()
        {
            // unless we can inject them in MembershipHelper, we need need this
            Builder.Services.AddTransient(_ => Mock.Of<IMemberService>());
            Builder.Services.AddTransient(_ => Mock.Of<IMemberTypeService>());
            Builder.Services.AddTransient(_ => Mock.Of<IUserService>());
            Builder.Services.AddTransient(_ => AppCaches.Disabled);
            Builder.Services.AddTransient<ServiceContext>();

            var loggerFactory = Mock.Of<ILoggerFactory>();
            var memberService = Mock.Of<IMemberService>();
            var memberTypeService = Mock.Of<IMemberTypeService>();
            var membershipProvider = new MembersMembershipProvider(memberService, memberTypeService, Mock.Of<IUmbracoVersion>(), TestHelper.GetHostingEnvironment(), TestHelper.GetIpResolver());
            var membershipHelper = new MembershipHelper(Mock.Of<IHttpContextAccessor>(), Mock.Of<IPublishedMemberCache>(), membershipProvider, Mock.Of<RoleProvider>(), memberService, memberTypeService, Mock.Of<IPublicAccessService>(), AppCaches.Disabled, loggerFactory, ShortStringHelper, Mock.Of<IEntityService>());

            // ReSharper disable once UnusedVariable
            var helper = new UmbracoHelper(Mock.Of<IPublishedContent>(),
                Mock.Of<ICultureDictionaryFactory>(),
                Mock.Of<IUmbracoComponentRenderer>(),
                Mock.Of<IPublishedContentQuery>(),
                membershipHelper);
            Assert.Pass();
        }

        [Test]
        public void Can_Mock_UrlProvider()
        {
            var umbracoContext = TestObjects.GetUmbracoContextMock();


            var urlProviderMock = new Mock<IUrlProvider>();
            urlProviderMock.Setup(provider => provider.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(UrlInfo.Url("/hello/world/1234"));
            var urlProvider = urlProviderMock.Object;

            var webRoutingSettings = new WebRoutingSettings();
            var theUrlProvider = new UrlProvider(
                new TestUmbracoContextAccessor(umbracoContext),
                Microsoft.Extensions.Options.Options.Create(webRoutingSettings),
                new UrlProviderCollection(new [] { urlProvider }),
                new MediaUrlProviderCollection( Enumerable.Empty<IMediaUrlProvider>())
                , umbracoContext.VariationContextAccessor);

            var contentType = new PublishedContentType(Guid.NewGuid(), 666, "alias", PublishedItemType.Content, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Nothing);
            var publishedContent = Mock.Of<IPublishedContent>();
            Mock.Get(publishedContent).Setup(x => x.ContentType).Returns(contentType);

            Assert.AreEqual("/hello/world/1234", theUrlProvider.GetUrl(publishedContent));
        }

        [Test]
        public void Can_Mock_UmbracoApiController_Dependencies_With_Injected_UmbracoMapper()
        {
            var profilingLogger  = Mock.Of<IProfilingLogger>();
            var memberService = Mock.Of<IMemberService>();
            var memberTypeService = Mock.Of<IMemberTypeService>();
            var membershipProvider = new MembersMembershipProvider(memberService, memberTypeService, Mock.Of<IUmbracoVersion>(), TestHelper.GetHostingEnvironment(), TestHelper.GetIpResolver());
            var umbracoMapper = new UmbracoMapper(new MapDefinitionCollection(new[] { Mock.Of<IMapDefinition>() }));

            var umbracoApiController = new FakeUmbracoApiController(new GlobalSettings(), Mock.Of<IUmbracoContextAccessor>(), Mock.Of<IBackOfficeSecurityAccessor>(), Mock.Of<ISqlContext>(), ServiceContext.CreatePartial(), AppCaches.NoCache, profilingLogger , Mock.Of<IRuntimeState>(), umbracoMapper, Mock.Of<IPublishedUrlProvider>());

            Assert.Pass();
        }
    }

    internal class FakeUmbracoApiController : UmbracoApiController
    {
        public FakeUmbracoApiController(GlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoMapper umbracoMapper, IPublishedUrlProvider publishedUrlProvider)
            : base(globalSettings, umbracoContextAccessor, backOfficeSecurityAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoMapper, publishedUrlProvider) { }
    }
}
