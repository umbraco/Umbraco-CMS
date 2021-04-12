using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security
{
    [TestFixture]
    public class PublicAccessCheckerTests
    {
        private PublicAccessChecker CreateSut(out HttpContext httpContext, out IMemberManager memberManager)
        {
            memberManager = Mock.Of<IMemberManager>();
            IPublicAccessService publicAccessService = Mock.Of<IPublicAccessService>();
            IContentService contentService = Mock.Of<IContentService>();

            var services = new ServiceCollection();
            IMemberManager localMemberManager = memberManager;
            services.AddScoped<IMemberManager>(x => localMemberManager);
            httpContext = new DefaultHttpContext
            {
                RequestServices = services.BuildServiceProvider()
            };

            HttpContext localHttpContext = httpContext;
            var publicAccessChecker = new PublicAccessChecker(
                Mock.Of<IHttpContextAccessor>(x => x.HttpContext == localHttpContext),
                publicAccessService,
                contentService);

            return publicAccessChecker;
        }

        [Test]
        public async Task GivenMemberNotLoggedIn_WhenIdentityIsChecked_ThenNotLoggedInResponse()
        {
            PublicAccessChecker sut = CreateSut(out HttpContext httpContext, out IMemberManager memberManager);
            httpContext.User = new ClaimsPrincipal();
            Mock.Get(memberManager).Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new MemberIdentityUser()));

            var result = await sut.HasMemberAccessToContentAsync(123);
            Assert.AreEqual(PublicAccessStatus.NotLoggedIn, result);
        }

        [Test]
        public async Task GivenMemberNotLoggedIn_WhenMemberIsRequested_AndIsNull_ThenNotLoggedInResponse()
        {
            PublicAccessChecker sut = CreateSut(out HttpContext httpContext, out IMemberManager memberManager);
            httpContext.User = new ClaimsPrincipal(Mock.Of<IIdentity>(x => x.IsAuthenticated == true));
            Mock.Get(memberManager).Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult((MemberIdentityUser)null));

            var result = await sut.HasMemberAccessToContentAsync(123);
            Assert.AreEqual(PublicAccessStatus.NotLoggedIn, result);
        }
    }
}
