using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security;

[TestFixture]
public class PublicAccessCheckerTests
{
    private IHttpContextAccessor GetHttpContextAccessor(IMemberManager memberManager, out HttpContext httpContext)
    {
        var services = new ServiceCollection();
        services.AddScoped(x => memberManager);
        httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        var localHttpContext = httpContext;
        return Mock.Of<IHttpContextAccessor>(x => x.HttpContext == localHttpContext);
    }

    private PublicAccessChecker CreateSut(
        IMemberManager memberManager,
        IPublicAccessService publicAccessService,
        IContentService contentService,
        out HttpContext httpContext)
    {
        var publicAccessChecker = new PublicAccessChecker(
            GetHttpContextAccessor(memberManager, out httpContext),
            publicAccessService,
            contentService);

        return publicAccessChecker;
    }

    private ClaimsPrincipal GetLoggedInUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new(ClaimTypes.NameIdentifier, "1234"), new Claim(ClaimTypes.Name, "test@example.com") },
            "TestAuthentication"));
        return user;
    }

    private void MockGetRolesAsync(IMemberManager memberManager, IEnumerable<string> roles = null)
    {
        if (roles == null)
        {
            roles = new[] { "role1", "role2" };
        }

        Mock.Get(memberManager).Setup(x => x.GetRolesAsync(It.IsAny<MemberIdentityUser>()))
            .Returns(Task.FromResult((IList<string>)new List<string>(roles)));
    }

    private void MockGetUserAsync(IMemberManager memberManager, MemberIdentityUser memberIdentityUser)
        => Mock.Get(memberManager).Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .Returns(Task.FromResult(memberIdentityUser));

    private PublicAccessEntry GetPublicAccessEntry(string usernameRuleValue, string roleRuleValue)
        => new(
            Guid.NewGuid(),
            123,
            987,
            987,
            new List<PublicAccessRule>
            {
                new()
                {
                    RuleType = Constants.Conventions.PublicAccess.MemberUsernameRuleType,
                    RuleValue = usernameRuleValue,
                },
                new() { RuleType = Constants.Conventions.PublicAccess.MemberRoleRuleType, RuleValue = roleRuleValue },
            });

    [AutoMoqData]
    [Test]
    public async Task GivenMemberNotLoggedIn_WhenIdentityIsChecked_ThenNotLoggedInResult(
        IMemberManager memberManager,
        IPublicAccessService publicAccessService,
        IContentService contentService)
    {
        var sut = CreateSut(memberManager, publicAccessService, contentService, out var httpContext);
        httpContext.User = new ClaimsPrincipal();
        MockGetUserAsync(memberManager, new MemberIdentityUser());

        var result = await sut.HasMemberAccessToContentAsync(123);
        Assert.AreEqual(PublicAccessStatus.NotLoggedIn, result);
    }

    [AutoMoqData]
    [Test]
    public async Task GivenMemberNotLoggedIn_WhenMemberIsRequested_AndIsNull_ThenNotLoggedInResult(
        IMemberManager memberManager,
        IPublicAccessService publicAccessService,
        IContentService contentService)
    {
        var sut = CreateSut(memberManager, publicAccessService, contentService, out var httpContext);
        httpContext.User = GetLoggedInUser();
        MockGetUserAsync(memberManager, null);

        var result = await sut.HasMemberAccessToContentAsync(123);
        Assert.AreEqual(PublicAccessStatus.NotLoggedIn, result);
    }

    [AutoMoqData]
    [Test]
    public async Task GivenMemberLoggedIn_WhenMemberHasNoRolesAndWrongUsername_ThenAccessDeniedResult(
        IMemberManager memberManager,
        IPublicAccessService publicAccessService,
        IContentService contentService,
        IContent protectedNode,
        IContent loginNode,
        IContent noAccessNode,
        string username)
    {
        var sut = CreateSut(memberManager, publicAccessService, contentService, out var httpContext);

        Mock.Get(publicAccessService).Setup(x => x.GetEntryForContent(It.IsAny<IContent>()))
            .Returns(new PublicAccessEntry(
                protectedNode,
                loginNode,
                noAccessNode,
                new[]
                {
                    new PublicAccessRule(Guid.Empty, Guid.Empty)
                    {
                        RuleType = Constants.Conventions.PublicAccess.MemberUsernameRuleType,
                        RuleValue = "AnotherUsername",
                    },
                }));
        httpContext.User = GetLoggedInUser();
        MockGetUserAsync(memberManager, new MemberIdentityUser { IsApproved = true, UserName = username });
        MockGetRolesAsync(memberManager, Enumerable.Empty<string>());

        var result = await sut.HasMemberAccessToContentAsync(123);
        Assert.AreEqual(PublicAccessStatus.AccessDenied, result);
    }

    [AutoMoqData]
    [Test]
    public async Task GivenMemberLoggedIn_WhenMemberIsLockedOut_ThenLockedOutResult(
        IMemberManager memberManager,
        IPublicAccessService publicAccessService,
        IContentService contentService)
    {
        var sut = CreateSut(memberManager, publicAccessService, contentService, out var httpContext);

        httpContext.User = GetLoggedInUser();
        MockGetUserAsync(
            memberManager,
            new MemberIdentityUser { IsApproved = true, LockoutEnd = DateTime.UtcNow.AddDays(10) });
        MockGetRolesAsync(memberManager);

        var result = await sut.HasMemberAccessToContentAsync(123);
        Assert.AreEqual(PublicAccessStatus.LockedOut, result);
    }

    [AutoMoqData]
    [Test]
    public async Task GivenMemberLoggedIn_WhenMemberIsNotApproved_ThenNotApprovedResult(
        IMemberManager memberManager,
        IPublicAccessService publicAccessService,
        IContentService contentService)
    {
        var sut = CreateSut(memberManager, publicAccessService, contentService, out var httpContext);

        httpContext.User = GetLoggedInUser();
        MockGetUserAsync(memberManager, new MemberIdentityUser { IsApproved = false });
        MockGetRolesAsync(memberManager);

        var result = await sut.HasMemberAccessToContentAsync(123);
        Assert.AreEqual(PublicAccessStatus.NotApproved, result);
    }

    [AutoMoqData]
    [Test]
    public async Task GivenMemberLoggedIn_WhenMemberHasRoles_AndContentDoesNotExist_ThenAccessAcceptedResult(
        IMemberManager memberManager,
        IPublicAccessService publicAccessService,
        IContentService contentService)
    {
        var sut = CreateSut(memberManager, publicAccessService, contentService, out var httpContext);
        httpContext.User = GetLoggedInUser();
        MockGetUserAsync(memberManager, new MemberIdentityUser { IsApproved = true });
        MockGetRolesAsync(memberManager);
        Mock.Get(contentService).Setup(x => x.GetById(123)).Returns((IContent)null);

        var result = await sut.HasMemberAccessToContentAsync(123);
        Assert.AreEqual(PublicAccessStatus.AccessAccepted, result);
    }

    [AutoMoqData]
    [Test]
    public async Task GivenMemberLoggedIn_WhenMemberHasRoles_AndGetEntryForContentDoesNotExist_ThenAccessAcceptedResult(
        IMemberManager memberManager,
        IPublicAccessService publicAccessService,
        IContentService contentService,
        IContent content)
    {
        var sut = CreateSut(memberManager, publicAccessService, contentService, out var httpContext);
        httpContext.User = GetLoggedInUser();
        MockGetUserAsync(memberManager, new MemberIdentityUser { IsApproved = true });
        MockGetRolesAsync(memberManager);
        Mock.Get(contentService).Setup(x => x.GetById(123)).Returns(content);
        Mock.Get(publicAccessService).Setup(x => x.GetEntryForContent(content)).Returns((PublicAccessEntry)null);

        var result = await sut.HasMemberAccessToContentAsync(123);
        Assert.AreEqual(PublicAccessStatus.AccessAccepted, result);
    }

    [AutoMoqData]
    [Test]
    public async Task GivenMemberLoggedIn_WhenMemberHasRoles_AndEntryRulesDontMatch_ThenAccessDeniedResult(
        IMemberManager memberManager,
        IPublicAccessService publicAccessService,
        IContentService contentService,
        IContent content)
    {
        var sut = CreateSut(memberManager, publicAccessService, contentService, out var httpContext);
        httpContext.User = GetLoggedInUser();
        MockGetUserAsync(memberManager, new MemberIdentityUser { UserName = "MyUsername", IsApproved = true });
        MockGetRolesAsync(memberManager);
        Mock.Get(contentService).Setup(x => x.GetById(123)).Returns(content);
        Mock.Get(publicAccessService).Setup(x => x.GetEntryForContent(content))
            .Returns(GetPublicAccessEntry(string.Empty, string.Empty));

        var result = await sut.HasMemberAccessToContentAsync(123);
        Assert.AreEqual(PublicAccessStatus.AccessDenied, result);
    }

    [AutoMoqData]
    [Test]
    public async Task GivenMemberLoggedIn_WhenMemberHasRoles_AndUsernameRuleMatches_ThenAccessAcceptedResult(
        IMemberManager memberManager,
        IPublicAccessService publicAccessService,
        IContentService contentService,
        IContent content)
    {
        var sut = CreateSut(memberManager, publicAccessService, contentService, out var httpContext);
        httpContext.User = GetLoggedInUser();
        MockGetUserAsync(memberManager, new MemberIdentityUser { UserName = "MyUsername", IsApproved = true });
        MockGetRolesAsync(memberManager);
        Mock.Get(contentService).Setup(x => x.GetById(123)).Returns(content);
        Mock.Get(publicAccessService).Setup(x => x.GetEntryForContent(content))
            .Returns(GetPublicAccessEntry("MyUsername", string.Empty));

        var result = await sut.HasMemberAccessToContentAsync(123);
        Assert.AreEqual(PublicAccessStatus.AccessAccepted, result);
    }

    [AutoMoqData]
    [Test]
    public async Task GivenMemberLoggedIn_WhenMemberHasRoles_AndRoleRuleMatches_ThenAccessAcceptedResult(
        IMemberManager memberManager,
        IPublicAccessService publicAccessService,
        IContentService contentService,
        IContent content)
    {
        var sut = CreateSut(memberManager, publicAccessService, contentService, out var httpContext);
        httpContext.User = GetLoggedInUser();
        MockGetUserAsync(memberManager, new MemberIdentityUser { UserName = "MyUsername", IsApproved = true });
        MockGetRolesAsync(memberManager);
        Mock.Get(contentService).Setup(x => x.GetById(123)).Returns(content);
        Mock.Get(publicAccessService).Setup(x => x.GetEntryForContent(content))
            .Returns(GetPublicAccessEntry(string.Empty, "role1"));

        var result = await sut.HasMemberAccessToContentAsync(123);
        Assert.AreEqual(PublicAccessStatus.AccessAccepted, result);
    }
}
