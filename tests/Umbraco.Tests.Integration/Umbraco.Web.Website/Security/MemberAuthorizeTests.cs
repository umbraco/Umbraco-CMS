using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.Website.Security
{
    public class MemberAuthorizeTests : UmbracoTestServerTestBase
    {
        private Mock<IMemberManager> _memberManagerMock = new();

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            _memberManagerMock = new Mock<IMemberManager>();
            services.Remove(new ServiceDescriptor(typeof(IMemberManager), typeof(MemberManager), ServiceLifetime.Scoped));
            services.Remove(new ServiceDescriptor(typeof(MemberManager), ServiceLifetime.Scoped));
            services.AddScoped(_ => _memberManagerMock.Object);
        }

        [Test]
        [LongRunning]
        public async Task Secure_SurfaceController_Should_Return_Redirect_WhenNotLoggedIn()
        {
            _memberManagerMock.Setup(x => x.IsLoggedIn()).Returns(false);

            var url = PrepareSurfaceControllerUrl<TestSurfaceController>(x => x.Secure());

            var response = await Client.GetAsync(url);

            var cookieAuthenticationOptions = Services.GetService<IOptions<CookieAuthenticationOptions>>();
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
            Assert.AreEqual(cookieAuthenticationOptions.Value.AccessDeniedPath.ToString(), response.Headers.Location?.AbsolutePath);
        }

        [Test]
        [LongRunning]
        public async Task Secure_SurfaceController_Should_Return_Redirect_WhenNotAuthorized()
        {
            _memberManagerMock.Setup(x => x.IsLoggedIn()).Returns(true);
            _memberManagerMock.Setup(x => x.IsMemberAuthorizedAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(false);

            var url = PrepareSurfaceControllerUrl<TestSurfaceController>(x => x.Secure());

            var response = await Client.GetAsync(url);

            var cookieAuthenticationOptions = Services.GetService<IOptions<CookieAuthenticationOptions>>();
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
            Assert.AreEqual(cookieAuthenticationOptions.Value.AccessDeniedPath.ToString(), response.Headers.Location?.AbsolutePath);
        }


        [Test]
        [LongRunning]
        public async Task Secure_ApiController_Should_Return_Unauthorized_WhenNotLoggedIn()
        {
            _memberManagerMock.Setup(x => x.IsLoggedIn()).Returns(false);
            var url = PrepareApiControllerUrl<TestApiController>(x => x.Secure());

            var response = await Client.GetAsync(url);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [LongRunning]
        public async Task Secure_ApiController_Should_Return_Forbidden_WhenNotAuthorized()
        {
            _memberManagerMock.Setup(x => x.IsLoggedIn()).Returns(true);
            _memberManagerMock.Setup(x => x.IsMemberAuthorizedAsync(
                     It.IsAny<IEnumerable<string>>(),
                    It.IsAny<IEnumerable<string>>(),
                     It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(false);

            var url = PrepareApiControllerUrl<TestApiController>(x => x.Secure());

            var response = await Client.GetAsync(url);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    public class TestSurfaceController : SurfaceController
    {
        public TestSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider)
            : base(
                umbracoContextAccessor,
                databaseFactory,
                services,
                appCaches,
                profilingLogger,
                publishedUrlProvider)
        {
        }

        [UmbracoMemberAuthorize]
        public IActionResult Secure() => NoContent();
    }

    public class TestApiController : UmbracoApiController
    {
        [UmbracoMemberAuthorize]
        public IActionResult Secure() => NoContent();
    }
}
