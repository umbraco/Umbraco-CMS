using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class PublicAccessPresentationFactoryTests
{
    private Mock<IEntityService> _entityService = null!;
    private Mock<IMemberService> _memberService = null!;
    private Mock<IUmbracoMapper> _mapper = null!;
    private Mock<IMemberRoleManager> _memberRoleManager = null!;
    private Mock<IMemberPresentationFactory> _memberPresentationFactory = null!;
    private PublicAccessPresentationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _entityService = new Mock<IEntityService>();
        _memberService = new Mock<IMemberService>();
        _mapper = new Mock<IUmbracoMapper>();
        _memberRoleManager = new Mock<IMemberRoleManager>();
        _memberPresentationFactory = new Mock<IMemberPresentationFactory>();

        // Default: no roles
        _memberRoleManager.Setup(x => x.Roles).Returns(Enumerable.Empty<UmbracoIdentityRole>());

        _factory = new PublicAccessPresentationFactory(
            _entityService.Object,
            _memberService.Object,
            _mapper.Object,
            _memberRoleManager.Object,
            _memberPresentationFactory.Object);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Returns_ContentNotFound_When_ProtectedNodeKey_Cannot_Be_Resolved()
    {
        // Arrange
        var contentKey = Guid.NewGuid();
        var entry = CreateEntry(protectedNodeId: 100, loginNodeId: 200, noAccessNodeId: 300);

        _entityService.Setup(x => x.GetKey(100, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Fail());

        // Act
        var result = _factory.CreatePublicAccessResponseModel(entry, contentKey);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(PublicAccessOperationStatus.ContentNotFound, result.Status);
        Assert.IsNull(result.Result);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Returns_LoginNodeNotFound_When_LoginNodeKey_Cannot_Be_Resolved()
    {
        // Arrange
        var protectedNodeKey = Guid.NewGuid();
        var entry = CreateEntry(protectedNodeId: 100, loginNodeId: 200, noAccessNodeId: 300);

        _entityService.Setup(x => x.GetKey(100, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(protectedNodeKey));
        _entityService.Setup(x => x.GetKey(200, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Fail());

        // Act
        var result = _factory.CreatePublicAccessResponseModel(entry, protectedNodeKey);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(PublicAccessOperationStatus.LoginNodeNotFound, result.Status);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Returns_ErrorNodeNotFound_When_NoAccessNodeKey_Cannot_Be_Resolved()
    {
        // Arrange
        var protectedNodeKey = Guid.NewGuid();
        var loginNodeKey = Guid.NewGuid();
        var entry = CreateEntry(protectedNodeId: 100, loginNodeId: 200, noAccessNodeId: 300);

        _entityService.Setup(x => x.GetKey(100, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(protectedNodeKey));
        _entityService.Setup(x => x.GetKey(200, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(loginNodeKey));
        _entityService.Setup(x => x.GetKey(300, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Fail());

        // Act
        var result = _factory.CreatePublicAccessResponseModel(entry, protectedNodeKey);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(PublicAccessOperationStatus.ErrorNodeNotFound, result.Status);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Sets_IsProtectedByAncestor_False_When_ContentKey_Matches_ProtectedNode()
    {
        // Arrange
        var protectedNodeKey = Guid.NewGuid();
        var loginNodeKey = Guid.NewGuid();
        var errorNodeKey = Guid.NewGuid();
        var entry = CreateEntry(protectedNodeId: 100, loginNodeId: 200, noAccessNodeId: 300);

        SetupNodeKeyResolution(100, protectedNodeKey);
        SetupNodeKeyResolution(200, loginNodeKey);
        SetupNodeKeyResolution(300, errorNodeKey);

        // Act — contentKey is the same as the protected node key (direct protection).
        var result = _factory.CreatePublicAccessResponseModel(entry, protectedNodeKey);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);
        Assert.IsFalse(result.Result!.IsProtectedByAncestor);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Sets_IsProtectedByAncestor_True_When_ContentKey_Differs_From_ProtectedNode()
    {
        // Arrange
        var protectedNodeKey = Guid.NewGuid();
        var childContentKey = Guid.NewGuid();
        var loginNodeKey = Guid.NewGuid();
        var errorNodeKey = Guid.NewGuid();
        var entry = CreateEntry(protectedNodeId: 100, loginNodeId: 200, noAccessNodeId: 300);

        SetupNodeKeyResolution(100, protectedNodeKey);
        SetupNodeKeyResolution(200, loginNodeKey);
        SetupNodeKeyResolution(300, errorNodeKey);

        // Act — contentKey is different from the protected node key (ancestor protection).
        var result = _factory.CreatePublicAccessResponseModel(entry, childContentKey);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);
        Assert.IsTrue(result.Result!.IsProtectedByAncestor);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Populates_LoginDocument_And_ErrorDocument()
    {
        // Arrange
        var protectedNodeKey = Guid.NewGuid();
        var loginNodeKey = Guid.NewGuid();
        var errorNodeKey = Guid.NewGuid();
        var entry = CreateEntry(protectedNodeId: 100, loginNodeId: 200, noAccessNodeId: 300);

        SetupNodeKeyResolution(100, protectedNodeKey);
        SetupNodeKeyResolution(200, loginNodeKey);
        SetupNodeKeyResolution(300, errorNodeKey);

        // Act
        var result = _factory.CreatePublicAccessResponseModel(entry, protectedNodeKey);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(loginNodeKey, result.Result!.LoginDocument.Id);
        Assert.AreEqual(errorNodeKey, result.Result.ErrorDocument.Id);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Resolves_Members_From_Username_Rules()
    {
        // Arrange
        var protectedNodeKey = Guid.NewGuid();
        var loginNodeKey = Guid.NewGuid();
        var errorNodeKey = Guid.NewGuid();
        var memberKey = Guid.NewGuid();

        var entry = CreateEntry(
            protectedNodeId: 100,
            loginNodeId: 200,
            noAccessNodeId: 300,
            rules: new[]
            {
                CreateRule(Constants.Conventions.PublicAccess.MemberUsernameRuleType, "john.doe"),
            });

        SetupNodeKeyResolution(100, protectedNodeKey);
        SetupNodeKeyResolution(200, loginNodeKey);
        SetupNodeKeyResolution(300, errorNodeKey);

        var member = Mock.Of<IMember>();
        _memberService.Setup(x => x.GetByUsername("john.doe")).Returns(member);

        var memberItemModel = new MemberItemResponseModel { Id = memberKey };
        _memberPresentationFactory.Setup(x => x.CreateItemResponseModel(member)).Returns(memberItemModel);

        // Act
        var result = _factory.CreatePublicAccessResponseModel(entry, protectedNodeKey);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Result!.Members.Length);
        Assert.AreEqual(memberKey, result.Result.Members[0].Id);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Resolves_Groups_From_Role_Rules()
    {
        // Arrange
        var protectedNodeKey = Guid.NewGuid();
        var loginNodeKey = Guid.NewGuid();
        var errorNodeKey = Guid.NewGuid();
        var groupKey = Guid.NewGuid();

        var entry = CreateEntry(
            protectedNodeId: 100,
            loginNodeId: 200,
            noAccessNodeId: 300,
            rules: new[]
            {
                CreateRule(Constants.Conventions.PublicAccess.MemberRoleRuleType, "Editors"),
            });

        SetupNodeKeyResolution(100, protectedNodeKey);
        SetupNodeKeyResolution(200, loginNodeKey);
        SetupNodeKeyResolution(300, errorNodeKey);

        _memberRoleManager.Setup(x => x.Roles).Returns(new[]
        {
            new UmbracoIdentityRole("Editors") { Id = "42" },
        });

        var groupEntity = Mock.Of<IEntitySlim>();
        _entityService.Setup(x => x.GetAll(UmbracoObjectTypes.MemberGroup, It.Is<int[]>(ids => ids.Contains(42))))
            .Returns(new[] { groupEntity });

        var groupModel = new MemberGroupItemResponseModel { Id = groupKey, Name = "Editors" };
        _mapper.Setup(x => x.Map<MemberGroupItemResponseModel>(groupEntity)).Returns(groupModel);

        // Act
        var result = _factory.CreatePublicAccessResponseModel(entry, protectedNodeKey);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Result!.Groups.Length);
        Assert.AreEqual(groupKey, result.Result.Groups[0].Id);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Skips_Members_Not_Found_By_Username()
    {
        // Arrange
        var protectedNodeKey = Guid.NewGuid();
        var loginNodeKey = Guid.NewGuid();
        var errorNodeKey = Guid.NewGuid();

        var entry = CreateEntry(
            protectedNodeId: 100,
            loginNodeId: 200,
            noAccessNodeId: 300,
            rules: new[]
            {
                CreateRule(Constants.Conventions.PublicAccess.MemberUsernameRuleType, "deleted.user"),
            });

        SetupNodeKeyResolution(100, protectedNodeKey);
        SetupNodeKeyResolution(200, loginNodeKey);
        SetupNodeKeyResolution(300, errorNodeKey);

        _memberService.Setup(x => x.GetByUsername("deleted.user")).Returns((IMember?)null);

        // Act
        var result = _factory.CreatePublicAccessResponseModel(entry, protectedNodeKey);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsEmpty(result.Result!.Members);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Skips_Groups_Not_Found_In_Roles()
    {
        // Arrange
        var protectedNodeKey = Guid.NewGuid();
        var loginNodeKey = Guid.NewGuid();
        var errorNodeKey = Guid.NewGuid();

        var entry = CreateEntry(
            protectedNodeId: 100,
            loginNodeId: 200,
            noAccessNodeId: 300,
            rules: new[]
            {
                CreateRule(Constants.Conventions.PublicAccess.MemberRoleRuleType, "DeletedGroup"),
            });

        SetupNodeKeyResolution(100, protectedNodeKey);
        SetupNodeKeyResolution(200, loginNodeKey);
        SetupNodeKeyResolution(300, errorNodeKey);

        // No matching role exists
        _memberRoleManager.Setup(x => x.Roles).Returns(new[]
        {
            new UmbracoIdentityRole("Administrators") { Id = "1" },
        });

        // Act
        var result = _factory.CreatePublicAccessResponseModel(entry, protectedNodeKey);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsEmpty(result.Result!.Groups);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Handles_Mixed_Member_And_Group_Rules()
    {
        // Arrange
        var protectedNodeKey = Guid.NewGuid();
        var loginNodeKey = Guid.NewGuid();
        var errorNodeKey = Guid.NewGuid();
        var memberKey = Guid.NewGuid();
        var groupKey = Guid.NewGuid();

        var entry = CreateEntry(
            protectedNodeId: 100,
            loginNodeId: 200,
            noAccessNodeId: 300,
            rules:
            [
                CreateRule(Constants.Conventions.PublicAccess.MemberUsernameRuleType, "jane.doe"),
                CreateRule(Constants.Conventions.PublicAccess.MemberRoleRuleType, "VIPs"),
            ]);

        SetupNodeKeyResolution(100, protectedNodeKey);
        SetupNodeKeyResolution(200, loginNodeKey);
        SetupNodeKeyResolution(300, errorNodeKey);

        // Member setup
        var member = Mock.Of<IMember>();
        _memberService.Setup(x => x.GetByUsername("jane.doe")).Returns(member);
        _memberPresentationFactory.Setup(x => x.CreateItemResponseModel(member))
            .Returns(new MemberItemResponseModel { Id = memberKey });

        // Group setup
        _memberRoleManager.Setup(x => x.Roles).Returns(new[]
        {
            new UmbracoIdentityRole("VIPs") { Id = "10" },
        });
        var groupEntity = Mock.Of<IEntitySlim>();
        _entityService.Setup(x => x.GetAll(UmbracoObjectTypes.MemberGroup, It.Is<int[]>(ids => ids.Contains(10))))
            .Returns(new[] { groupEntity });
        _mapper.Setup(x => x.Map<MemberGroupItemResponseModel>(groupEntity))
            .Returns(new MemberGroupItemResponseModel { Id = groupKey, Name = "VIPs" });

        // Act
        var result = _factory.CreatePublicAccessResponseModel(entry, protectedNodeKey);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Result!.Members.Length);
        Assert.AreEqual(memberKey, result.Result.Members[0].Id);
        Assert.AreEqual(1, result.Result.Groups.Length);
        Assert.AreEqual(groupKey, result.Result.Groups[0].Id);
    }

    [Test]
    public void CreatePublicAccessResponseModel_Returns_Empty_Members_And_Groups_When_No_Rules()
    {
        // Arrange
        var protectedNodeKey = Guid.NewGuid();
        var loginNodeKey = Guid.NewGuid();
        var errorNodeKey = Guid.NewGuid();
        var entry = CreateEntry(protectedNodeId: 100, loginNodeId: 200, noAccessNodeId: 300);

        SetupNodeKeyResolution(100, protectedNodeKey);
        SetupNodeKeyResolution(200, loginNodeKey);
        SetupNodeKeyResolution(300, errorNodeKey);

        // Act
        var result = _factory.CreatePublicAccessResponseModel(entry, protectedNodeKey);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsEmpty(result.Result!.Members);
        Assert.IsEmpty(result.Result.Groups);
    }

    private void SetupNodeKeyResolution(int nodeId, Guid key)
        => _entityService.Setup(x => x.GetKey(nodeId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(key));

    private static PublicAccessEntry CreateEntry(
        int protectedNodeId,
        int loginNodeId,
        int noAccessNodeId,
        IEnumerable<PublicAccessRule>? rules = null)
        => new(Guid.NewGuid(), protectedNodeId, loginNodeId, noAccessNodeId, rules ?? Enumerable.Empty<PublicAccessRule>());

    private static PublicAccessRule CreateRule(string ruleType, string ruleValue)
        => new() { RuleType = ruleType, RuleValue = ruleValue };
}
