// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class MemberPresentationFactoryTests
{
    private Mock<IUmbracoMapper> _mockMapper = null!;
    private Mock<IMemberService> _mockMemberService = null!;
    private Mock<IMemberTypeService> _mockMemberTypeService = null!;
    private Mock<ITwoFactorLoginService> _mockTwoFactorLoginService = null!;
    private Mock<IMemberGroupService> _mockMemberGroupService = null!;
    private Mock<IExternalMemberService> _mockExternalMemberService = null!;
    private MemberPresentationFactory _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockMapper = new Mock<IUmbracoMapper>();
        _mockMemberService = new Mock<IMemberService>();
        _mockMemberTypeService = new Mock<IMemberTypeService>();
        _mockTwoFactorLoginService = new Mock<ITwoFactorLoginService>();
        _mockMemberGroupService = new Mock<IMemberGroupService>();
        _mockExternalMemberService = new Mock<IExternalMemberService>();

        _sut = new MemberPresentationFactory(
            _mockMapper.Object,
            _mockMemberService.Object,
            _mockMemberTypeService.Object,
            _mockTwoFactorLoginService.Object,
            _mockMemberGroupService.Object,
            Options.Create(new DeliveryApiSettings()),
            _mockExternalMemberService.Object);
    }

    [Test]
    public async Task CreateResponseModelAsync_Maps_Member_Via_UmbracoMapper()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var member = CreateMockMember(memberKey, "content@test.com", "content");
        var mappedModel = new MemberResponseModel { Id = memberKey, Email = "content@test.com" };
        _mockMapper.Setup(x => x.Map<MemberResponseModel>(member.Object)).Returns(mappedModel);
        _mockMemberService.Setup(x => x.GetAllRoles("content")).Returns(Enumerable.Empty<string>());
        var user = CreateMockUser(hasSensitiveAccess: true);

        // Act
        MemberResponseModel result = await _sut.CreateResponseModelAsync(member.Object, user.Object);

        // Assert
        Assert.AreEqual(memberKey, result.Id);
        Assert.AreEqual("content@test.com", result.Email);
        _mockMapper.Verify(x => x.Map<MemberResponseModel>(member.Object), Times.Once);
    }

    [Test]
    public async Task CreateResponseModelAsync_Checks_TwoFactor_Status()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var member = CreateMockMember(memberKey, "2fa@test.com", "2fa-user");
        var mappedModel = new MemberResponseModel { Id = memberKey };
        _mockMapper.Setup(x => x.Map<MemberResponseModel>(member.Object)).Returns(mappedModel);
        _mockMemberService.Setup(x => x.GetAllRoles("2fa-user")).Returns(Enumerable.Empty<string>());
        _mockTwoFactorLoginService.Setup(x => x.IsTwoFactorEnabledAsync(memberKey)).ReturnsAsync(true);
        var user = CreateMockUser(hasSensitiveAccess: true);

        // Act
        MemberResponseModel result = await _sut.CreateResponseModelAsync(member.Object, user.Object);

        // Assert
        Assert.IsTrue(result.IsTwoFactorEnabled);
    }

    [Test]
    public async Task CreateResponseModelAsync_Returns_Default_Kind_For_Regular_Member()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var member = CreateMockMember(memberKey, "regular@test.com", "regular");
        var mappedModel = new MemberResponseModel { Id = memberKey };
        _mockMapper.Setup(x => x.Map<MemberResponseModel>(member.Object)).Returns(mappedModel);
        _mockMemberService.Setup(x => x.GetAllRoles("regular")).Returns(Enumerable.Empty<string>());
        var user = CreateMockUser(hasSensitiveAccess: true);

        // Act
        MemberResponseModel result = await _sut.CreateResponseModelAsync(member.Object, user.Object);

        // Assert
        Assert.AreEqual(MemberKind.Default, result.Kind);
    }

    [Test]
    public async Task CreateResponseModelAsync_Resolves_Group_Keys_From_Roles()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var groupKey = Guid.NewGuid();
        var member = CreateMockMember(memberKey, "roles@test.com", "roles-user");
        var mappedModel = new MemberResponseModel { Id = memberKey };
        _mockMapper.Setup(x => x.Map<MemberResponseModel>(member.Object)).Returns(mappedModel);
        _mockMemberService.Setup(x => x.GetAllRoles("roles-user")).Returns(new[] { "Editors" });
        var mockGroup = new Mock<IMemberGroup>();
        mockGroup.Setup(x => x.Key).Returns(groupKey);
        _mockMemberGroupService.Setup(x => x.GetByName("Editors")).Returns(mockGroup.Object);
        var user = CreateMockUser(hasSensitiveAccess: true);

        // Act
        MemberResponseModel result = await _sut.CreateResponseModelAsync(member.Object, user.Object);

        // Assert
        CollectionAssert.Contains(result.Groups.ToList(), groupKey);
    }

    [Test]
    public async Task CreateResponseModelAsync_Removes_Sensitive_Data_When_User_Lacks_Access()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var memberTypeKey = Guid.NewGuid();
        var member = CreateMockMember(memberKey, "sensitive@test.com", "sensitive-user");
        member.Setup(x => x.ContentType.Key).Returns(memberTypeKey);

        var mappedModel = new MemberResponseModel
        {
            Id = memberKey,
            IsApproved = true,
            IsLockedOut = true,
            LastLoginDate = DateTimeOffset.UtcNow,
            Values = Enumerable.Empty<MemberValueResponseModel>(),
        };
        _mockMapper.Setup(x => x.Map<MemberResponseModel>(member.Object)).Returns(mappedModel);
        _mockMemberService.Setup(x => x.GetAllRoles("sensitive-user")).Returns(Enumerable.Empty<string>());

        var mockMemberType = new Mock<IMemberType>();
        mockMemberType.Setup(x => x.PropertyTypes).Returns(Enumerable.Empty<IPropertyType>());
        _mockMemberTypeService.Setup(x => x.GetAsync(memberTypeKey)).ReturnsAsync(mockMemberType.Object);

        var user = CreateMockUser(hasSensitiveAccess: false);

        // Act
        MemberResponseModel result = await _sut.CreateResponseModelAsync(member.Object, user.Object);

        // Assert — sensitive fields are reset.
        Assert.IsFalse(result.IsApproved);
        Assert.IsFalse(result.IsLockedOut);
        Assert.IsNull(result.LastLoginDate);
    }

    [Test]
    public async Task CreateMultipleAsync_Returns_Model_Per_Member()
    {
        // Arrange
        var members = new[]
        {
            CreateMockMember(Guid.NewGuid(), "a@test.com", "a"),
            CreateMockMember(Guid.NewGuid(), "b@test.com", "b"),
        };

        foreach (var m in members)
        {
            _mockMapper.Setup(x => x.Map<MemberResponseModel>(m.Object))
                .Returns(new MemberResponseModel { Id = m.Object.Key });
            _mockMemberService.Setup(x => x.GetAllRoles(m.Object.Username)).Returns(Enumerable.Empty<string>());
        }

        var user = CreateMockUser(hasSensitiveAccess: true);

        // Act
        IEnumerable<MemberResponseModel> results = await _sut.CreateMultipleAsync(
            members.Select(m => m.Object), user.Object);

        // Assert
        Assert.AreEqual(2, results.Count());
    }

    [Test]
    public void CreateItemResponseModel_IMember_Sets_Key_And_Kind()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var member = CreateMockMember(memberKey, "item@test.com", "item-user");
        _mockMapper.Setup(x => x.Map<MemberTypeReferenceResponseModel>(member.Object))
            .Returns(new MemberTypeReferenceResponseModel());

        // Act
        MemberItemResponseModel result = _sut.CreateItemResponseModel(member.Object);

        // Assert
        Assert.AreEqual(memberKey, result.Id);
        Assert.AreEqual(MemberKind.Default, result.Kind);
    }

    [Test]
    public void CreateItemResponseModel_IMember_Includes_Name_In_Variants()
    {
        // Arrange
        var member = CreateMockMember(Guid.NewGuid(), "variants@test.com", "variants-user");
        member.Setup(x => x.Name).Returns("Test Name");
        _mockMapper.Setup(x => x.Map<MemberTypeReferenceResponseModel>(member.Object))
            .Returns(new MemberTypeReferenceResponseModel());

        // Act
        MemberItemResponseModel result = _sut.CreateItemResponseModel(member.Object);

        // Assert
        Assert.AreEqual(1, result.Variants.Count());
        Assert.AreEqual("Test Name", result.Variants.First().Name);
        Assert.IsNull(result.Variants.First().Culture);
    }

    [Test]
    public async Task CreateExternalMemberResponseModel_Returns_Model_With_ExternalOnly_Kind()
    {
        // Arrange
        var member = CreateExternalMember();
        _mockExternalMemberService.Setup(x => x.GetRolesAsync(member.Key)).ReturnsAsync(Enumerable.Empty<string>());

        // Act
        MemberResponseModel result = await _sut.CreateExternalMemberResponseModelAsync(member);

        // Assert
        Assert.AreEqual(MemberKind.ExternalOnly, result.Kind);
    }

    [Test]
    public async Task CreateExternalMemberResponseModel_Maps_Identity_Fields()
    {
        // Arrange
        var member = CreateExternalMember();
        _mockExternalMemberService.Setup(x => x.GetRolesAsync(member.Key)).ReturnsAsync(Enumerable.Empty<string>());

        // Act
        MemberResponseModel result = await _sut.CreateExternalMemberResponseModelAsync(member);

        // Assert
        Assert.AreEqual(member.Key, result.Id);
        Assert.AreEqual("external@test.com", result.Email);
        Assert.AreEqual("external@test.com", result.Username);
        Assert.IsTrue(result.IsApproved);
        Assert.IsFalse(result.IsLockedOut);
    }

    [Test]
    public async Task CreateExternalMemberResponseModel_Has_Empty_Variants_And_Values()
    {
        // Arrange
        var member = CreateExternalMember();
        _mockExternalMemberService.Setup(x => x.GetRolesAsync(member.Key)).ReturnsAsync(Enumerable.Empty<string>());

        // Act
        MemberResponseModel result = await _sut.CreateExternalMemberResponseModelAsync(member);

        // Assert
        Assert.IsFalse(result.Variants.Any());
        Assert.IsFalse(result.Values.Any());
    }

    [Test]
    public async Task CreateExternalMemberResponseModel_Resolves_Group_Keys()
    {
        // Arrange
        var member = CreateExternalMember();
        var groupKey = Guid.NewGuid();
        var mockGroup = new Mock<IMemberGroup>();
        mockGroup.Setup(x => x.Key).Returns(groupKey);

        _mockExternalMemberService.Setup(x => x.GetRolesAsync(member.Key)).ReturnsAsync(new[] { "TestGroup" });
        _mockMemberGroupService.Setup(x => x.GetByName("TestGroup")).Returns(mockGroup.Object);

        // Act
        MemberResponseModel result = await _sut.CreateExternalMemberResponseModelAsync(member);

        // Assert
        CollectionAssert.Contains(result.Groups.ToList(), groupKey);
    }

    [Test]
    public async Task CreateExternalMemberResponseModel_TwoFactor_Is_Disabled()
    {
        // Arrange
        var member = CreateExternalMember();
        _mockExternalMemberService.Setup(x => x.GetRolesAsync(member.Key)).ReturnsAsync(Enumerable.Empty<string>());

        // Act
        MemberResponseModel result = await _sut.CreateExternalMemberResponseModelAsync(member);

        // Assert
        Assert.IsFalse(result.IsTwoFactorEnabled);
    }

    [Test]
    public async Task CreateExternalMemberResponseModel_Maps_Login_Dates()
    {
        // Arrange
        var loginDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var lockoutDate = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc);
        var member = new ExternalMemberIdentity
        {
            Key = Guid.NewGuid(),
            Email = "dates@test.com",
            UserName = "dates@test.com",
            Name = "Dates Test",
            IsApproved = true,
            CreateDate = DateTime.UtcNow,
            LastLoginDate = loginDate,
            LastLockoutDate = lockoutDate,
        };
        _mockExternalMemberService.Setup(x => x.GetRolesAsync(member.Key)).ReturnsAsync(Enumerable.Empty<string>());

        // Act
        MemberResponseModel result = await _sut.CreateExternalMemberResponseModelAsync(member);

        // Assert
        Assert.AreEqual(loginDate, result.LastLoginDate!.Value.UtcDateTime);
        Assert.AreEqual(lockoutDate, result.LastLockoutDate!.Value.UtcDateTime);
        Assert.IsNull(result.LastPasswordChangeDate);
    }

    private static Mock<IMember> CreateMockMember(Guid key, string email, string username)
    {
        var contentType = new Mock<ISimpleContentType>();
        contentType.Setup(x => x.Key).Returns(Guid.NewGuid());
        contentType.Setup(x => x.Alias).Returns("Member");

        var member = new Mock<IMember>();
        member.Setup(x => x.Key).Returns(key);
        member.Setup(x => x.Email).Returns(email);
        member.Setup(x => x.Username).Returns(username);
        member.Setup(x => x.Name).Returns(username);
        member.Setup(x => x.ContentType).Returns(contentType.Object);
        return member;
    }

    private static Mock<IUser> CreateMockUser(bool hasSensitiveAccess)
    {
        var user = new Mock<IUser>();
        var groups = new List<IReadOnlyUserGroup>();
        if (hasSensitiveAccess)
        {
            var group = new Mock<IReadOnlyUserGroup>();
            group.Setup(x => x.Key).Returns(global::Umbraco.Cms.Core.Constants.Security.SensitiveDataGroupKey);
            groups.Add(group.Object);
        }

        user.Setup(x => x.Groups).Returns(groups);
        return user;
    }

    // --- CreateFilterItemResponseModel ---

    [Test]
    public void CreateFilterItemResponseModel_Maps_Content_Member_With_Type()
    {
        // Arrange
        var memberTypeKey = Guid.NewGuid();
        var item = new MemberFilterItem
        {
            Key = Guid.NewGuid(),
            Email = "filter-content@test.com",
            UserName = "filter-content",
            Name = "Filter Content",
            IsApproved = true,
            Kind = MemberKind.Default,
            MemberTypeKey = memberTypeKey,
            MemberTypeName = "Member",
            MemberTypeIcon = "icon-user",
        };

        // Act
        MemberResponseModel result = _sut.CreateFilterItemResponseModel(item);

        // Assert
        Assert.AreEqual(item.Key, result.Id);
        Assert.AreEqual("filter-content@test.com", result.Email);
        Assert.AreEqual(MemberKind.Default, result.Kind);
        Assert.AreEqual(memberTypeKey, result.MemberType.Id);
        Assert.AreEqual("icon-user", result.MemberType.Icon);
        Assert.AreEqual("Filter Content", result.Variants.First().Name);
    }

    [Test]
    public void CreateFilterItemResponseModel_Maps_External_Member_With_Empty_Type()
    {
        // Arrange
        var item = new MemberFilterItem
        {
            Key = Guid.NewGuid(),
            Email = "filter-ext@test.com",
            UserName = "filter-ext",
            Name = "Filter External",
            IsApproved = true,
            Kind = MemberKind.ExternalOnly,
            MemberTypeKey = null,
            MemberTypeName = null,
            MemberTypeIcon = null,
        };

        // Act
        MemberResponseModel result = _sut.CreateFilterItemResponseModel(item);

        // Assert
        Assert.AreEqual(item.Key, result.Id);
        Assert.AreEqual(MemberKind.ExternalOnly, result.Kind);
        Assert.AreEqual(Guid.Empty, result.MemberType.Id);
        Assert.AreEqual(string.Empty, result.MemberType.Icon);
    }

    private static ExternalMemberIdentity CreateExternalMember() => new()
    {
        Key = Guid.NewGuid(),
        Email = "external@test.com",
        UserName = "external@test.com",
        Name = "External Test",
        IsApproved = true,
        CreateDate = DateTime.UtcNow,
        SecurityStamp = Guid.NewGuid().ToString(),
    };
}
