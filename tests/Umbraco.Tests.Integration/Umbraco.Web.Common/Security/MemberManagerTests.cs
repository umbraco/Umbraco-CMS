using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.Common.Security;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MemberManagerTests : UmbracoIntegrationTest
{
    private IMemberManager MemberManager => GetRequiredService<IMemberManager>();

    private IMemberEditingService MemberEditingService => GetRequiredService<IMemberEditingService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberGroupService MemberGroupService => GetRequiredService<IMemberGroupService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    [Test]
    public async Task AsPublishedMember_Returns_PublishedMember_For_Valid_MemberIdentityUser()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var createdMember = await CreateMemberAsync(memberKey);

        var memberIdentityUser = new MemberIdentityUser
        {
            Id = createdMember.Id.ToString(),
            Key = createdMember.Key,
            UserName = createdMember.Username,
            Email = createdMember.Email,
            Name = createdMember.Name,
            MemberTypeAlias = createdMember.ContentType.Alias,
        };

        // Act
        var publishedMember = MemberManager.AsPublishedMember(memberIdentityUser);

        // Assert
        Assert.IsNotNull(publishedMember);
        Assert.AreEqual(createdMember.Id, publishedMember.Id);
        Assert.AreEqual(createdMember.Key, publishedMember.Key);
        Assert.AreEqual(createdMember.Name, publishedMember.Name);
        Assert.AreEqual(PublishedItemType.Member, publishedMember.ItemType);
        Assert.AreEqual("The title value", publishedMember.Value("title"));
        Assert.IsNotNull(publishedMember.ContentType);
        Assert.AreEqual(createdMember.ContentType.Alias, publishedMember.ContentType.Alias);
    }

    [Test]
    public void AsPublishedMember_Returns_Null_For_Null_User()
    {
        // Act
        var publishedMember = MemberManager.AsPublishedMember(null!);

        // Assert
        Assert.IsNull(publishedMember);
    }

    [Test]
    public void AsPublishedMember_Returns_Null_For_NonExistent_Member()
    {
        // Arrange
        var memberIdentityUser = new MemberIdentityUser
        {
            Id = "99999",
            Key = Guid.NewGuid(),
            UserName = "nonexistent",
            Email = "nonexistent@test.com",
            Name = "Non Existent",
        };

        // Act
        var publishedMember = MemberManager.AsPublishedMember(memberIdentityUser);

        // Assert
        Assert.IsNull(publishedMember);
    }

    private async Task<IMember> CreateMemberAsync(Guid? key = null)
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        MemberService.AddRole("TestRole");
        var group = MemberGroupService.GetByName("TestRole");

        var createModel = new MemberCreateModel
        {
            Key = key,
            Email = "test@test.com",
            Username = "testuser",
            Password = "SuperSecret123",
            IsApproved = true,
            ContentTypeKey = memberType.Key,
            Roles = [group.Key],
            Variants = [new() { Name = "Test User" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "author", Value = "The author value" }
            ]
        };

        var result = await MemberEditingService.CreateAsync(createModel, SuperUser());
        Assert.IsTrue(result.Success, "Member should be created successfully");
        return result.Result.Content!;
    }

    private IUser SuperUser() => UserService.GetAsync(Constants.Security.SuperUserKey).GetAwaiter().GetResult()!;
}
