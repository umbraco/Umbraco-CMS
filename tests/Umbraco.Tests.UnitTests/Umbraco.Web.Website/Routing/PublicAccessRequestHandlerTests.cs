using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Routing;

[TestFixture]
public class PublicAccessRequestHandlerTests
{
    private const int ProtectedPageId = 1000;
    private const int LoginPageId = 1001;
    private const int NoAccessPageId = 1002;
    private const int NotFoundPageId = 1003;
    private const int UnprotectedPageId = 1004;

    private static readonly Uri _requestUri = new("http://example.com/");

    private Dictionary<int, IPublishedContent> _publishedPages = null!;
    private Dictionary<int, PublicAccessStatus> _accessStatuses = null!;
    private Func<IPublishedRequest, IPublishedContent?> _notFoundContentResolver = null!;
    private AuthenticateResult _authenticateResult = null!;
    private Mock<IPublicAccessService> _publicAccessService = null!;
    private Mock<IPublicAccessChecker> _publicAccessChecker = null!;
    private Mock<IPublishedRouter> _router = null!;

    [SetUp]
    public void SetUp()
    {
        _publishedPages = [];
        _accessStatuses = [];
        _notFoundContentResolver = request => request.PublishedContent;
        _authenticateResult = AuthenticateResult.NoResult();

        _publicAccessService = new Mock<IPublicAccessService>();
        _publicAccessService
            .Setup(x => x.IsProtected(It.IsAny<string>()))
            .Returns((string path) => path.Split(',').Contains(ProtectedPageId.ToString())
                ? Attempt<PublicAccessEntry?>.Succeed(
                    new PublicAccessEntry(Guid.NewGuid(), ProtectedPageId, LoginPageId, NoAccessPageId, []))
                : Attempt<PublicAccessEntry?>.Fail());

        _publicAccessChecker = new Mock<IPublicAccessChecker>();
        _publicAccessChecker
            .Setup(x => x.HasMemberAccessToContentAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => _accessStatuses.TryGetValue(id, out PublicAccessStatus status)
                ? status
                : PublicAccessStatus.AccessAccepted);

        _router = new Mock<IPublishedRouter>();
        _router
            .Setup(x => x.UpdateRequestAsync(It.IsAny<IPublishedRequest>(), It.IsAny<IPublishedContent?>()))
            .ReturnsAsync((IPublishedRequest request, IPublishedContent? content) =>
            {
                var builder = new PublishedRequestBuilder(request.Uri, Mock.Of<IFileService>());
                if (content is not null)
                {
                    builder.SetPublishedContent(content);
                }
                else
                {
                    builder.SetPublishedContent(_notFoundContentResolver(request));
                    builder.SetIs404();
                }

                return builder.Build();
            });
        _router
            .Setup(x => x.CreateRequestAsync(It.IsAny<Uri>()))
            .ReturnsAsync((Uri uri) => new PublishedRequestBuilder(uri, Mock.Of<IFileService>()));
    }

    [TestCase(PublicAccessStatus.NotLoggedIn, LoginPageId)]
    [TestCase(PublicAccessStatus.AccessDenied, NoAccessPageId)]
    [TestCase(PublicAccessStatus.LockedOut, NoAccessPageId)]
    [TestCase(PublicAccessStatus.NotApproved, NoAccessPageId)]
    public async Task Published_Access_Page_Is_Routed_For_Status(PublicAccessStatus status, int expectedPageId)
    {
        _accessStatuses[ProtectedPageId] = status;
        PublishPage(CreateContent(LoginPageId));
        PublishPage(CreateContent(NoAccessPageId));

        UmbracoRouteValues? result = await RewriteAsync(CreateContent(ProtectedPageId));

        Assert.IsNotNull(result);
        Assert.AreEqual(expectedPageId, result!.PublishedRequest.PublishedContent?.Id);
    }

    [TestCase(PublicAccessStatus.NotLoggedIn, LoginPageId)]
    [TestCase(PublicAccessStatus.AccessDenied, NoAccessPageId)]
    public async Task Access_Page_Within_Protected_Branch_Is_Not_Rerouted_To_Itself(PublicAccessStatus status, int accessPageId)
    {
        IPublishedContent accessPage = CreateContent(accessPageId, $"-1,{ProtectedPageId},{accessPageId}");
        _accessStatuses[accessPageId] = status;
        PublishPage(accessPage);
        UmbracoRouteValues routeValues = CreateRouteValues(CreateRequest(accessPage));

        UmbracoRouteValues? result = await RewriteAsync(routeValues);

        Assert.AreSame(routeValues, result);
        VerifyNotRerouted();
    }

    [Test]
    public async Task Unprotected_Page_Is_Not_Rerouted()
    {
        UmbracoRouteValues routeValues = CreateRouteValues(CreateRequest(CreateContent(UnprotectedPageId)));

        UmbracoRouteValues? result = await RewriteAsync(routeValues);

        Assert.AreSame(routeValues, result);
        VerifyNotRerouted();
        _publicAccessChecker.Verify(x => x.HasMemberAccessToContentAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task Protected_Page_With_Access_Accepted_Is_Not_Rerouted()
    {
        _accessStatuses[ProtectedPageId] = PublicAccessStatus.AccessAccepted;
        UmbracoRouteValues routeValues = CreateRouteValues(CreateRequest(CreateContent(ProtectedPageId)));

        UmbracoRouteValues? result = await RewriteAsync(routeValues);

        Assert.AreSame(routeValues, result);
        VerifyNotRerouted();
    }

    [Test]
    public async Task Request_Without_Published_Content_Is_Not_Checked()
    {
        UmbracoRouteValues routeValues = CreateRouteValues(CreateRequest(null));

        UmbracoRouteValues? result = await RewriteAsync(routeValues);

        Assert.AreSame(routeValues, result);
        VerifyNotRerouted();
        _publicAccessService.Verify(x => x.IsProtected(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Authenticated_Member_Is_Set_On_Request_Before_Access_Check()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "member")], "test"));
        _authenticateResult = AuthenticateResult.Success(
            new AuthenticationTicket(principal, IdentityConstants.ApplicationScheme));
        var httpContext = CreateHttpContext();
        ClaimsPrincipal? userDuringAccessCheck = null;
        _publicAccessChecker
            .Setup(x => x.HasMemberAccessToContentAsync(ProtectedPageId))
            .Callback(() => userDuringAccessCheck = httpContext.User)
            .ReturnsAsync(PublicAccessStatus.AccessAccepted);

        await CreateHandler().RewriteForPublishedContentAccessAsync(
            httpContext,
            CreateRouteValues(CreateRequest(CreateContent(ProtectedPageId))));

        Assert.AreSame(principal, userDuringAccessCheck);
    }

    [TestCase(PublicAccessStatus.NotLoggedIn)]
    [TestCase(PublicAccessStatus.AccessDenied)]
    [TestCase(PublicAccessStatus.LockedOut)]
    [TestCase(PublicAccessStatus.NotApproved)]
    public async Task Unpublished_Access_Page_Without_Not_Found_Page_Routes_As_Not_Found(PublicAccessStatus status)
    {
        _accessStatuses[ProtectedPageId] = status;

        UmbracoRouteValues? result = await RewriteAsync(CreateContent(ProtectedPageId));

        Assert.IsNotNull(result);
        Assert.IsNull(result!.PublishedRequest.PublishedContent);
        Assert.AreEqual(StatusCodes.Status404NotFound, result.PublishedRequest.ResponseStatusCode);
        Assert.AreEqual(UmbracoRouteResult.NotFound, result.PublishedRequest.GetRouteResult());
    }

    [TestCase(PublicAccessStatus.NotLoggedIn)]
    [TestCase(PublicAccessStatus.AccessDenied)]
    [TestCase(PublicAccessStatus.LockedOut)]
    [TestCase(PublicAccessStatus.NotApproved)]
    public async Task Unpublished_Access_Page_Routes_To_Not_Found_Page(PublicAccessStatus status)
    {
        _accessStatuses[ProtectedPageId] = status;
        IPublishedContent notFoundPage = CreateContent(NotFoundPageId);
        _notFoundContentResolver = _ => notFoundPage;

        UmbracoRouteValues? result = await RewriteAsync(CreateContent(ProtectedPageId));

        Assert.IsNotNull(result);
        Assert.AreEqual(NotFoundPageId, result!.PublishedRequest.PublishedContent?.Id);
    }

    private Task<UmbracoRouteValues?> RewriteAsync(IPublishedContent content)
        => RewriteAsync(CreateRouteValues(CreateRequest(content)));

    private Task<UmbracoRouteValues?> RewriteAsync(UmbracoRouteValues routeValues)
        => CreateHandler().RewriteForPublishedContentAccessAsync(CreateHttpContext(), routeValues);

    private void PublishPage(IPublishedContent content) => _publishedPages[content.Id] = content;

    private void VerifyNotRerouted()
        => _router.Verify(
            x => x.UpdateRequestAsync(It.IsAny<IPublishedRequest>(), It.IsAny<IPublishedContent?>()),
            Times.Never);

    private PublicAccessRequestHandler CreateHandler()
    {
        var contentCache = new Mock<IPublishedContentCache>();
        contentCache
            .Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int id) => _publishedPages.GetValueOrDefault(id));
        IUmbracoContext umbracoContext = Mock.Of<IUmbracoContext>(x => x.Content == contentCache.Object);
        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out umbracoContext)).Returns(true);

        var routeValuesFactory = new Mock<IUmbracoRouteValuesFactory>();
        routeValuesFactory
            .Setup(x => x.CreateAsync(It.IsAny<HttpContext>(), It.IsAny<IPublishedRequest>()))
            .ReturnsAsync((HttpContext _, IPublishedRequest request) => CreateRouteValues(request));

        return new PublicAccessRequestHandler(
            NullLogger<PublicAccessRequestHandler>.Instance,
            _publicAccessService.Object,
            _publicAccessChecker.Object,
            umbracoContextAccessor.Object,
            routeValuesFactory.Object,
            _router.Object);
    }

    private HttpContext CreateHttpContext()
    {
        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService
            .Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(() => _authenticateResult);

        return new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddSingleton(authenticationService.Object)
                .BuildServiceProvider(),
        };
    }

    private static IPublishedContent CreateContent(int id, string? path = null)
        => Mock.Of<IPublishedContent>(x => x.Id == id && x.Path == (path ?? $"-1,{id}"));

    private static IPublishedRequest CreateRequest(IPublishedContent? content)
    {
        var builder = new PublishedRequestBuilder(_requestUri, Mock.Of<IFileService>());
        builder.SetPublishedContent(content);
        return builder.Build();
    }

    private static UmbracoRouteValues CreateRouteValues(IPublishedRequest request)
        => new(
            request,
            new ControllerActionDescriptor
            {
                ControllerTypeInfo = typeof(RenderController).GetTypeInfo(),
                ControllerName = ControllerExtensions.GetControllerName<RenderController>(),
            });
}
