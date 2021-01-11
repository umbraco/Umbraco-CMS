// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Tests.Common;
using Umbraco.Tests.Testing;
using Umbraco.Tests.UnitTests.TestHelpers.Objects;
using Umbraco.Web;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Website.Controllers;
using CoreConstants = Umbraco.Core.Constants;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Website.Controllers
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class SurfaceControllerTests
    {
        private IUmbracoContextAccessor _umbracoContextAccessor;

        [SetUp]
        public void SetUp() => _umbracoContextAccessor = new TestUmbracoContextAccessor();

        [Test]
        public void Can_Construct_And_Get_Result()
        {
            IHostingEnvironment hostingEnvironment = Mock.Of<IHostingEnvironment>();
            IBackOfficeSecurityAccessor backofficeSecurityAccessor = Mock.Of<IBackOfficeSecurityAccessor>();
            Mock.Get(backofficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(Mock.Of<IBackOfficeSecurity>());
            var globalSettings = new GlobalSettings();

            var umbracoContextFactory = TestUmbracoContextFactory.Create(globalSettings, _umbracoContextAccessor);

            UmbracoContextReference umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
            IUmbracoContext umbracoContext = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

            var ctrl = new TestSurfaceController(umbracoContextAccessor, Mock.Of<IPublishedContentQuery>(), Mock.Of<IPublishedUrlProvider>());

            IActionResult result = ctrl.Index();

            Assert.IsNotNull(result);
        }

        [Test]
        public void Umbraco_Context_Not_Null()
        {
            var globalSettings = new GlobalSettings();
            IHostingEnvironment hostingEnvironment = Mock.Of<IHostingEnvironment>();
            IBackOfficeSecurityAccessor backofficeSecurityAccessor = Mock.Of<IBackOfficeSecurityAccessor>();
            Mock.Get(backofficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(Mock.Of<IBackOfficeSecurity>());
            var umbracoContextFactory = TestUmbracoContextFactory.Create(globalSettings, _umbracoContextAccessor);

            UmbracoContextReference umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
            IUmbracoContext umbCtx = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbCtx);

            var ctrl = new TestSurfaceController(umbracoContextAccessor, Mock.Of<IPublishedContentQuery>(), Mock.Of<IPublishedUrlProvider>());

            Assert.IsNotNull(ctrl.UmbracoContext);
        }

        [Test]
        public void Can_Lookup_Content()
        {
            var publishedSnapshot = new Mock<IPublishedSnapshot>();
            publishedSnapshot.Setup(x => x.Members).Returns(Mock.Of<IPublishedMemberCache>());
            var content = new Mock<IPublishedContent>();
            content.Setup(x => x.Id).Returns(2);
            IBackOfficeSecurityAccessor backofficeSecurityAccessor = Mock.Of<IBackOfficeSecurityAccessor>();
            Mock.Get(backofficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(Mock.Of<IBackOfficeSecurity>());
            var publishedSnapshotService = new Mock<IPublishedSnapshotService>();
            IHostingEnvironment hostingEnvironment = Mock.Of<IHostingEnvironment>();
            var globalSettings = new GlobalSettings();

            var umbracoContextFactory = TestUmbracoContextFactory.Create(globalSettings, _umbracoContextAccessor);

            UmbracoContextReference umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
            IUmbracoContext umbracoContext = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

            IPublishedContentQuery publishedContentQuery = Mock.Of<IPublishedContentQuery>(query => query.Content(2) == content.Object);

            var ctrl = new TestSurfaceController(umbracoContextAccessor, publishedContentQuery, Mock.Of<IPublishedUrlProvider>());
            var result = ctrl.GetContent(2) as PublishedContentResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Content);
            Assert.AreEqual(2, result.Content.Id);
        }

        [Test]
        public void Mock_Current_Page()
        {
            var globalSettings = new GlobalSettings();
            IHostingEnvironment hostingEnvironment = Mock.Of<IHostingEnvironment>();
            IBackOfficeSecurityAccessor backofficeSecurityAccessor = Mock.Of<IBackOfficeSecurityAccessor>();
            Mock.Get(backofficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(Mock.Of<IBackOfficeSecurity>());
            var umbracoContextFactory = TestUmbracoContextFactory.Create(globalSettings, _umbracoContextAccessor);

            UmbracoContextReference umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
            IUmbracoContext umbracoContext = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

            IPublishedContent content = Mock.Of<IPublishedContent>(publishedContent => publishedContent.Id == 12345);
            var builder = new PublishedRequestBuilder(umbracoContext.CleanedUmbracoUrl, Mock.Of<IFileService>());
            builder.SetPublishedContent(content);
            IPublishedRequest publishedRequest = builder.Build();

            var routeDefinition = new UmbracoRouteValues(publishedRequest);

            var routeData = new RouteData();
            routeData.Values.Add(CoreConstants.Web.UmbracoRouteDefinitionDataToken, routeDefinition);

            var ctrl = new TestSurfaceController(umbracoContextAccessor, Mock.Of<IPublishedContentQuery>(), Mock.Of<IPublishedUrlProvider>())
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = Mock.Of<HttpContext>(),
                    RouteData = routeData
                }
            };

            var result = ctrl.GetContentFromCurrentPage() as PublishedContentResult;

            Assert.AreEqual(12345, result.Content.Id);
        }

        public class TestSurfaceController : SurfaceController
        {
            private readonly IPublishedContentQuery _publishedContentQuery;

            public TestSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IPublishedContentQuery publishedContentQuery, IPublishedUrlProvider publishedUrlProvider)
                : base(umbracoContextAccessor, null, ServiceContext.CreatePartial(), AppCaches.Disabled, null, publishedUrlProvider) =>
                _publishedContentQuery = publishedContentQuery;

            public IActionResult Index() =>

                // ReSharper disable once Mvc.ViewNotResolved
                View();

            public IActionResult GetContent(int id)
            {
                IPublishedContent content = _publishedContentQuery.Content(id);

                return new PublishedContentResult(content);
            }

            public IActionResult GetContentFromCurrentPage()
            {
                IPublishedContent content = CurrentPage;

                return new PublishedContentResult(content);
            }
        }

        public class PublishedContentResult : IActionResult
        {
            public IPublishedContent Content { get; set; }

            public PublishedContentResult(IPublishedContent content) => Content = content;

            public Task ExecuteResultAsync(ActionContext context) => Task.CompletedTask;
        }
    }
}
