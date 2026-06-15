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
        Assert.That(result.Id, Is.EqualTo(memberKey));
        Assert.That(result.Email, Is.EqualTo("content@test.com"));
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
        Assert.That(result.IsTwoFactorEnabled, Is.True);
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
        Assert.That(result.Kind, Is.EqualTo(MemberKind.Default));
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
        Assert.That(result.Groups.ToList(), Does.Contain(groupKey));
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
        Assert.That(result.IsApproved, Is.False);
        Assert.That(result.IsLockedOut, Is.False);
        Assert.That(result.LastLoginDate, Is.Null);
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
        Assert.That(results.Count(), Is.EqualTo(2));
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
        Assert.That(result.Id, Is.EqualTo(memberKey));
        Assert.That(result.Kind, Is.EqualTo(MemberKind.Default));
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
        Assert.That(result.Variants.Count(), Is.EqualTo(1));
        Assert.That(result.Variants.First().Name, Is.EqualTo("Test Name"));
        Assert.That(result.Variants.First().Culture, Is.Null);
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
        Assert.That(result.Kind, Is.EqualTo(MemberKind.ExternalOnly));
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
        Assert.That(result.Id, Is.EqualTo(member.Key));
        Assert.That(result.Email, Is.EqualTo("external@test.com"));
        Assert.That(result.Username, Is.EqualTo("external@test.com"));
        Assert.That(result.IsApproved, Is.True);
        Assert.That(result.IsLockedOut, Is.False);
    }

    [Test]
    public async Task CreateExternalMemberResponseModel_Has_Single_Variant_And_Empty_Values()
    {
        // Arrange
        var member = CreateExternalMember();
        _mockExternalMemberService.Setup(x => x.GetRolesAsync(member.Key)).ReturnsAsync(Enumerable.Empty<string>());

        // Act
        MemberResponseModel result = await _sut.CreateExternalMemberResponseModelAsync(member);

        // Assert — one variant with the member name, but no content values.
        Assert.That(result.Variants.Count(), Is.EqualTo(1));
        Assert.That(result.Variants.First().Name, Is.EqualTo(member.Name));
        Assert.That(result.Values.Any(), Is.False);
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
        Assert.That(result.Groups.ToList(), Does.Contain(groupKey));
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
        Assert.That(result.IsTwoFactorEnabled, Is.False);
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
        Assert.That(result.LastLoginDate!.Value.UtcDateTime, Is.EqualTo(loginDate));
        Assert.That(result.LastLockoutDate!.Value.UtcDateTime, Is.EqualTo(lockoutDate));
        Assert.That(result.LastPasswordChangeDate, Is.Null);
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
        Assert.That(result.Id, Is.EqualTo(item.Key));
        Assert.That(result.Email, Is.EqualTo("filter-content@test.com"));
        Assert.That(result.Kind, Is.EqualTo(MemberKind.Default));
        Assert.That(result.MemberType.Id, Is.EqualTo(memberTypeKey));
        Assert.That(result.MemberType.Icon, Is.EqualTo("icon-user"));
        Assert.That(result.Variants.First().Name, Is.EqualTo("Filter Content"));
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
        Assert.That(result.Id, Is.EqualTo(item.Key));
        Assert.That(result.Kind, Is.EqualTo(MemberKind.ExternalOnly));
        Assert.That(result.MemberType.Id, Is.EqualTo(Guid.Empty));
        Assert.That(result.MemberType.Icon, Is.EqualTo(string.Empty));
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
