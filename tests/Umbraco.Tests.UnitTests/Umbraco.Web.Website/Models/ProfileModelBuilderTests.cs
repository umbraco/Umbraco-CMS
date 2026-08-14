// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Website.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Models;

[TestFixture]
public class ProfileModelBuilderTests
{
    private Mock<IMemberTypeService> _mockMemberTypeService = null!;
    private Mock<IMemberService> _mockMemberService = null!;
    private Mock<IMemberManager> _mockMemberManager = null!;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor = null!;

    [SetUp]
    public void SetUp()
    {
        _mockMemberTypeService = new Mock<IMemberTypeService>();
        _mockMemberService = new Mock<IMemberService>();
        _mockMemberManager = new Mock<IMemberManager>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var services = new ServiceCollection();
        services.AddSingleton(_mockMemberManager.Object);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    }

    private ProfileModelBuilder CreateSut() =>
        new(
            _mockMemberTypeService.Object,
            _mockMemberService.Object,
            Mock.Of<IShortStringHelper>(),
            _mockHttpContextAccessor.Object);

    [Test]
    public async Task GivenNoLoggedInMember_WhenBuildForCurrentMember_ThenReturnsNull()
    {
        // Arrange
        _mockMemberManager
            .Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((MemberIdentityUser?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.BuildForCurrentMemberAsync();

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task GivenALoggedInContentMember_WhenBuildForCurrentMember_ThenReturnsPopulatedModel()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var user = new MemberIdentityUser
        {
            Key = memberKey,
            Name = "Test Member",
            Email = "test@example.com",
            UserName = "test@example.com",
            MemberTypeAlias = "Member",
            IsApproved = true,
        };

        _mockMemberManager
            .Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var mockMemberType = new Mock<IMemberType>();
        mockMemberType.Setup(x => x.PropertyTypes).Returns(Enumerable.Empty<IPropertyType>());
        _mockMemberTypeService.Setup(x => x.Get("Member")).Returns(mockMemberType.Object);

        var mockMember = new Mock<IMember>();
        mockMember.Setup(x => x.Key).Returns(memberKey);
        _mockMemberService.Setup(x => x.GetById(memberKey)).Returns(mockMember.Object);

        var sut = CreateSut();

        // Act
        var result = await sut.BuildForCurrentMemberAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test Member", result!.Name);
        Assert.AreEqual("test@example.com", result.Email);
        Assert.AreEqual(memberKey, result.Key);
    }

    [Test]
    public async Task GivenAnExternalOnlyMember_WhenBuildForCurrentMember_ThenReturnsModelWithIdentityFieldsOnly()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var user = new MemberIdentityUser
        {
            Key = memberKey,
            Name = "External Member",
            Email = "external@example.com",
            UserName = "external@example.com",
            IsApproved = true,
            IsExternalOnly = true,
        };

        _mockMemberManager
            .Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var sut = CreateSut();

        // Act
        var result = await sut.BuildForCurrentMemberAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("External Member", result!.Name);
        Assert.AreEqual("external@example.com", result.Email);
        Assert.AreEqual(memberKey, result.Key);
        Assert.IsTrue(result.IsApproved);
    }

    [Test]
    public async Task GivenAnExternalOnlyMember_WhenBuildForCurrentMember_ThenDoesNotCallMemberService()
    {
        // Arrange
        var user = new MemberIdentityUser
        {
            Key = Guid.NewGuid(),
            Name = "External",
            Email = "external@example.com",
            UserName = "external@example.com",
            IsExternalOnly = true,
        };

        _mockMemberManager
            .Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var sut = CreateSut();

        // Act
        await sut.BuildForCurrentMemberAsync();

        // Assert — neither IMemberService nor IMemberTypeService should be called.
        _mockMemberService.Verify(x => x.GetById(It.IsAny<Guid>()), Times.Never);
        _mockMemberTypeService.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GivenAnExternalOnlyMember_WhenBuildForCurrentMember_ThenCustomPropertiesAreEmpty()
    {
        // Arrange
        var user = new MemberIdentityUser
        {
            Key = Guid.NewGuid(),
            Name = "External",
            Email = "external@example.com",
            UserName = "external@example.com",
            IsExternalOnly = true,
        };

        _mockMemberManager
            .Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var sut = CreateSut().WithCustomProperties(true);

        // Act
        var result = await sut.BuildForCurrentMemberAsync();

        // Assert — MemberProperties should be null/empty since no content properties exist.
        Assert.IsNotNull(result);
        Assert.IsTrue(result!.MemberProperties == null || result.MemberProperties.Count == 0);
    }
}
