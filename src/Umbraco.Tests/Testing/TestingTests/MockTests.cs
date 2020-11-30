using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Tests.Common;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
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
            Composition.Register(_ => Mock.Of<IMemberService>());
            Composition.Register(_ => Mock.Of<IMemberTypeService>());
            Composition.Register(_ => Mock.Of<IUserService>());
            Composition.Register(_ => AppCaches.Disabled);
            Composition.Register<ServiceContext>();

            var logger = Mock.Of<IProfilingLogger>();
            var memberService = Mock.Of<IMemberService>();
            var memberTypeService = Mock.Of<IMemberTypeService>();
            var membershipProvider = new MembersMembershipProvider(memberService, memberTypeService, Mock.Of<IUmbracoVersion>(), TestHelper.GetHostingEnvironment(), TestHelper.GetIpResolver());
            var membershipHelper = new MembershipHelper(Mock.Of<IHttpContextAccessor>(), Mock.Of<IPublishedMemberCache>(), membershipProvider, Mock.Of<RoleProvider>(), memberService, memberTypeService, Mock.Of<IPublicAccessService>(), AppCaches.Disabled, logger, ShortStringHelper, Mock.Of<IEntityService>());

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

            var theUrlProvider = new UrlProvider(
                new TestUmbracoContextAccessor(umbracoContext),
                TestHelper.WebRoutingSettings,
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
            var logger = Mock.Of<IProfilingLogger>();
            var memberService = Mock.Of<IMemberService>();
            var memberTypeService = Mock.Of<IMemberTypeService>();
            var membershipProvider = new MembersMembershipProvider(memberService, memberTypeService, Mock.Of<IUmbracoVersion>(), TestHelper.GetHostingEnvironment(), TestHelper.GetIpResolver());
            var membershipHelper = new MembershipHelper(Mock.Of<IHttpContextAccessor>(), Mock.Of<IPublishedMemberCache>(), membershipProvider, Mock.Of<RoleProvider>(), memberService, memberTypeService, Mock.Of<IPublicAccessService>(), AppCaches.Disabled, logger, ShortStringHelper, Mock.Of<IEntityService>());
            var umbracoMapper = new UmbracoMapper(new MapDefinitionCollection(new[] { Mock.Of<IMapDefinition>() }));

            var umbracoApiController = new FakeUmbracoApiController(Mock.Of<IGlobalSettings>(), Mock.Of<IUmbracoContextAccessor>(), Mock.Of<ISqlContext>(), ServiceContext.CreatePartial(), AppCaches.NoCache, logger, Mock.Of<IRuntimeState>(), umbracoMapper, Mock.Of<IPublishedUrlProvider>());

            Assert.Pass();
        }
    }

    internal class FakeUmbracoApiController : UmbracoApiController
    {
        public FakeUmbracoApiController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoMapper umbracoMapper, IPublishedUrlProvider publishedUrlProvider)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoMapper, publishedUrlProvider) { }
    }
}
