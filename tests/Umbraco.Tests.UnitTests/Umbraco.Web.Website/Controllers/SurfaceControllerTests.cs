// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.UnitTests.TestHelpers.Objects;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Controllers;

[TestFixture]
[UmbracoTest(WithApplication = true)]
public class SurfaceControllerTests
{
    [SetUp]
    public void SetUp() => _umbracoContextAccessor = new TestUmbracoContextAccessor();

    private IUmbracoContextAccessor _umbracoContextAccessor;

    [Test]
    public void Can_Construct_And_Get_Result()
    {
        var backofficeSecurityAccessor = Mock.Of<IBackOfficeSecurityAccessor>();
        Mock.Get(backofficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(Mock.Of<IBackOfficeSecurity>());
        var globalSettings = new GlobalSettings();

        var umbracoContextFactory = TestUmbracoContextFactory.Create(globalSettings, _umbracoContextAccessor);

        var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
        var umbracoContext = umbracoContextReference.UmbracoContext;

        var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

        var ctrl = new TestSurfaceController(umbracoContextAccessor, Mock.Of<IPublishedContentQuery>(), Mock.Of<IPublishedUrlProvider>());

        var result = ctrl.Index();

        Assert.IsNotNull(result);
    }

    [Test]
    public void Umbraco_Context_Not_Null()
    {
        var globalSettings = new GlobalSettings();
        var backofficeSecurityAccessor = Mock.Of<IBackOfficeSecurityAccessor>();
        Mock.Get(backofficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(Mock.Of<IBackOfficeSecurity>());
        var umbracoContextFactory = TestUmbracoContextFactory.Create(globalSettings, _umbracoContextAccessor);

        var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
        var umbCtx = umbracoContextReference.UmbracoContext;

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
        var backofficeSecurityAccessor = Mock.Of<IBackOfficeSecurityAccessor>();
        Mock.Get(backofficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(Mock.Of<IBackOfficeSecurity>());
        var globalSettings = new GlobalSettings();

        var umbracoContextFactory = TestUmbracoContextFactory.Create(globalSettings, _umbracoContextAccessor);

        var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
        var umbracoContext = umbracoContextReference.UmbracoContext;

        var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

        var publishedContentQuery = Mock.Of<IPublishedContentQuery>(query => query.Content(2) == content.Object);

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
        var backofficeSecurityAccessor = Mock.Of<IBackOfficeSecurityAccessor>();
        Mock.Get(backofficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(Mock.Of<IBackOfficeSecurity>());
        var umbracoContextFactory = TestUmbracoContextFactory.Create(globalSettings, _umbracoContextAccessor);

        var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
        var umbracoContext = umbracoContextReference.UmbracoContext;

        var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

        var content = Mock.Of<IPublishedContent>(publishedContent => publishedContent.Id == 12345);
        var builder = new PublishedRequestBuilder(umbracoContext.CleanedUmbracoUrl, Mock.Of<IFileService>());
        builder.SetPublishedContent(content);
        var publishedRequest = builder.Build();

        var routeDefinition = new UmbracoRouteValues(publishedRequest, null);

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set(routeDefinition);

        var ctrl =
            new TestSurfaceController(umbracoContextAccessor, Mock.Of<IPublishedContentQuery>(), Mock.Of<IPublishedUrlProvider>())
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext, RouteData = new RouteData() },
            };

        var result = ctrl.GetContentFromCurrentPage() as PublishedContentResult;

        Assert.AreEqual(12345, result.Content.Id);
    }

    public class TestSurfaceController : SurfaceController
    {
        private readonly IPublishedContentQuery _publishedContentQuery;

        public TestSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IPublishedContentQuery publishedContentQuery,
            IPublishedUrlProvider publishedUrlProvider)
            : base(umbracoContextAccessor, null, ServiceContext.CreatePartial(), AppCaches.Disabled, null, publishedUrlProvider) =>
            _publishedContentQuery = publishedContentQuery;

        public IActionResult Index() =>

            // ReSharper disable once Mvc.ViewNotResolved
            View();

        public IActionResult GetContent(int id)
        {
            var content = _publishedContentQuery.Content(id);

            return new PublishedContentResult(content);
        }

        public IActionResult GetContentFromCurrentPage()
        {
            var content = CurrentPage;

            return new PublishedContentResult(content);
        }
    }

    public class PublishedContentResult : IActionResult
    {
        public PublishedContentResult(IPublishedContent content) => Content = content;

        public IPublishedContent Content { get; set; }

        public Task ExecuteResultAsync(ActionContext context) => Task.CompletedTask;
    }
}
