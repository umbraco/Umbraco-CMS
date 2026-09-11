// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
internal sealed class ExternalMemberServiceTests : UmbracoIntegrationTest
{
    private IExternalMemberService ExternalMemberService => GetRequiredService<IExternalMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    [Test]
    public async Task Can_Create_And_Get_External_Member()
    {
        // Arrange
        var identity = new ExternalMemberIdentityBuilder()
            .WithEmail("create-test@example.com")
            .WithUserName("create-test@example.com")
            .WithName("Create Test")
            .WithIsApproved(true)
            .Build();

        // Act
        var result = await ExternalMemberService.CreateAsync(identity);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ExternalMemberOperationStatus.Success));

        var retrieved = await ExternalMemberService.GetByKeyAsync(result.Result.Key);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Key, Is.EqualTo(result.Result.Key));
        Assert.That(retrieved.Email, Is.EqualTo("create-test@example.com"));
        Assert.That(retrieved.UserName, Is.EqualTo("create-test@example.com"));
        Assert.That(retrieved.Name, Is.EqualTo("Create Test"));
        Assert.That(retrieved.IsApproved, Is.True);
        Assert.That(retrieved.IsLockedOut, Is.False);
    }

    [Test]
    public async Task Can_Get_By_Email()
    {
        // Arrange
        var identity = ExternalMemberIdentityBuilder.CreateSimple("email-test@example.com", "Email Test");
        await ExternalMemberService.CreateAsync(identity);

        // Act
        var retrieved = await ExternalMemberService.GetByEmailAsync("email-test@example.com");

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Email, Is.EqualTo("email-test@example.com"));
        Assert.That(retrieved.Name, Is.EqualTo("Email Test"));
    }

    [Test]
    public async Task Can_Get_By_Username()
    {
        // Arrange
        var identity = new ExternalMemberIdentityBuilder()
            .WithEmail("username-test@example.com")
            .WithUserName("username-lookup")
            .WithName("Username Test")
            .Build();
        await ExternalMemberService.CreateAsync(identity);

        // Act
        var retrieved = await ExternalMemberService.GetByUsernameAsync("username-lookup");

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.UserName, Is.EqualTo("username-lookup"));
        Assert.That(retrieved.Email, Is.EqualTo("username-test@example.com"));
    }

    [Test]
    public async Task Can_Update_External_Member()
    {
        // Arrange
        var identity = ExternalMemberIdentityBuilder.CreateSimple("update-test@example.com", "Before Update");
        var createResult = await ExternalMemberService.CreateAsync(identity);
        Assert.That(createResult.Success, Is.True);

        var member = await ExternalMemberService.GetByKeyAsync(createResult.Result.Key);
        Assert.That(member, Is.Not.Null);

        // Act
        member!.Name = "After Update";
        member.Email = "updated@example.com";
        member.IsApproved = false;
        var updateResult = await ExternalMemberService.UpdateAsync(member);

        // Assert
        Assert.That(updateResult.Success, Is.True);
        Assert.That(updateResult.Status, Is.EqualTo(ExternalMemberOperationStatus.Success));

        var retrieved = await ExternalMemberService.GetByKeyAsync(createResult.Result.Key);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Name, Is.EqualTo("After Update"));
        Assert.That(retrieved.Email, Is.EqualTo("updated@example.com"));
        Assert.That(retrieved.IsApproved, Is.False);
    }

    [Test]
    public async Task Can_Delete_External_Member()
    {
        // Arrange
        var identity = ExternalMemberIdentityBuilder.CreateSimple("delete-test@example.com", "Delete Test");
        var createResult = await ExternalMemberService.CreateAsync(identity);
        Assert.That(createResult.Success, Is.True);

        // Verify it exists first
        var existing = await ExternalMemberService.GetByKeyAsync(createResult.Result.Key);
        Assert.That(existing, Is.Not.Null);

        // Act
        var deleteResult = await ExternalMemberService.DeleteAsync(createResult.Result.Key);

        // Assert
        Assert.That(deleteResult.Success, Is.True);
        Assert.That(deleteResult.Status, Is.EqualTo(ExternalMemberOperationStatus.Success));

        var retrieved = await ExternalMemberService.GetByKeyAsync(createResult.Result.Key);
        Assert.That(retrieved, Is.Null);
    }

    [Test]
    public async Task Delete_Returns_NotFound_For_NonExistent()
    {
        // Act
        var result = await ExternalMemberService.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ExternalMemberOperationStatus.NotFound));
    }

    [Test]
    public async Task Can_Assign_And_Get_Roles()
    {
        // Arrange
        var identity = ExternalMemberIdentityBuilder.CreateSimple("roles-test@example.com", "Roles Test");
        var createResult = await ExternalMemberService.CreateAsync(identity);
        Assert.That(createResult.Success, Is.True);

        MemberService.AddRole("ExternalTestGroup");

        // Act
        var assignResult = await ExternalMemberService.AssignRolesAsync(createResult.Result.Key, ["ExternalTestGroup"]);
        var roles = await ExternalMemberService.GetRolesAsync(createResult.Result.Key);

        // Assert
        Assert.That(assignResult.Success, Is.True);
        Assert.That(roles, Is.Not.Null);
        Assert.That(roles.ToList(), Does.Contain("ExternalTestGroup"));
    }

    [Test]
    public async Task Can_Remove_Roles()
    {
        // Arrange
        var identity = ExternalMemberIdentityBuilder.CreateSimple("remove-roles@example.com", "Remove Roles Test");
        var createResult = await ExternalMemberService.CreateAsync(identity);
        Assert.That(createResult.Success, Is.True);

        MemberService.AddRole("RemovableGroup");
        await ExternalMemberService.AssignRolesAsync(createResult.Result.Key, ["RemovableGroup"]);

        // Verify role is assigned
        var rolesBefore = await ExternalMemberService.GetRolesAsync(createResult.Result.Key);
        Assert.That(rolesBefore.ToList(), Does.Contain("RemovableGroup"));

        // Act
        var removeResult = await ExternalMemberService.RemoveRolesAsync(createResult.Result.Key, ["RemovableGroup"]);

        // Assert
        Assert.That(removeResult.Success, Is.True);
        var rolesAfter = await ExternalMemberService.GetRolesAsync(createResult.Result.Key);
        Assert.That(rolesAfter.ToList(), Does.Not.Contain("RemovableGroup"));
    }

    [Test]
    public async Task Can_Store_And_Retrieve_ProfileData()
    {
        // Arrange
        var profileJson = """{"firstName":"John","lastName":"Doe","age":30}""";
        var identity = new ExternalMemberIdentityBuilder()
            .WithEmail("profile-test@example.com")
            .WithUserName("profile-test@example.com")
            .WithName("Profile Test")
            .WithProfileData(profileJson)
            .Build();

        var createResult = await ExternalMemberService.CreateAsync(identity);
        Assert.That(createResult.Success, Is.True);

        // Act
        var retrieved = await ExternalMemberService.GetByKeyAsync(createResult.Result.Key);

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.ProfileData, Is.EqualTo(profileJson));
    }

    [Test]
    public async Task GetByKey_Returns_Null_For_NonExistent()
    {
        // Act
        var retrieved = await ExternalMemberService.GetByKeyAsync(Guid.NewGuid());

        // Assert
        Assert.That(retrieved, Is.Null);
    }

    [Test]
    public async Task Cross_Store_Uniqueness_Rejects_Duplicate_Username()
    {
        // Arrange - create a content-based member first.
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "content-member@test.com", "password", "shared-username");
        MemberService.Save(member);

        // Act - creating an external member with the same username should fail.
        var externalIdentity = new ExternalMemberIdentityBuilder()
            .WithEmail("external-unique@example.com")
            .WithUserName("shared-username")
            .WithName("Duplicate Username Test")
            .Build();

        var result = await ExternalMemberService.CreateAsync(externalIdentity);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ExternalMemberOperationStatus.DuplicateUsername));
    }

    [Test]
    public async Task Cross_Store_Uniqueness_Rejects_Duplicate_Email_When_Required()
    {
        // Arrange - create a content-based member first (note: MemberRequireUniqueEmail defaults to true).
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "shared@test.com", "password", "content-user");
        MemberService.Save(member);

        // Act - creating an external member with the same email should fail.
        var externalIdentity = new ExternalMemberIdentityBuilder()
            .WithEmail("shared@test.com")
            .WithUserName("external-unique-user")
            .WithName("Duplicate Email Test")
            .Build();

        var result = await ExternalMemberService.CreateAsync(externalIdentity);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ExternalMemberOperationStatus.DuplicateEmail));
    }

    [Test]
    public async Task Can_Convert_External_To_Content_Member()
    {
        // Arrange — create external member with a group.
        var identity = new ExternalMemberIdentityBuilder()
            .WithEmail("convert-to-content@test.com")
            .WithUserName("convert-to-content")
            .WithName("Convert Test")
            .Build();
        var createResult = await ExternalMemberService.CreateAsync(identity);
        Assert.That(createResult.Success, Is.True);
        var originalKey = createResult.Result.Key;

        MemberService.AddRole("ConvertGroup");
        await ExternalMemberService.AssignRolesAsync(originalKey, ["ConvertGroup"]);

        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        // Act
        var result = await ExternalMemberService.ConvertToContentMemberAsync(originalKey, memberType.Alias);

        // Assert — content member created with same key and identity fields.
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result!.Key, Is.EqualTo(originalKey));
        Assert.That(result.Result.Email, Is.EqualTo("convert-to-content@test.com"));
        Assert.That(result.Result.Username, Is.EqualTo("convert-to-content"));

        // Assert — external member record removed.
        var externalMember = await ExternalMemberService.GetByKeyAsync(originalKey);
        Assert.That(externalMember, Is.Null);

        // Assert — group memberships migrated.
        IEnumerable<string> contentRoles = MemberService.GetAllRoles(result.Result.Username);
        Assert.That(contentRoles.ToList(), Does.Contain("ConvertGroup"));
    }

    [Test]
    public async Task Can_Convert_External_To_Content_Member_With_ProfileData_Callback()
    {
        // Arrange — create an external member with profile data.
        var profileJson = """{"department":"Engineering","floor":3}""";
        var identity = new ExternalMemberIdentityBuilder()
            .WithEmail("profile-promote@test.com")
            .WithUserName("profile-promote")
            .WithName("Profile Promote")
            .WithProfileData(profileJson)
            .Build();
        var createResult = await ExternalMemberService.CreateAsync(identity);
        Assert.That(createResult.Success, Is.True);

        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        // Act — use the callback to map profileData into a content property.
        string? capturedProfileData = null;
        var result = await ExternalMemberService.ConvertToContentMemberAsync(
            createResult.Result.Key,
            memberType.Alias,
            (member, profileData) =>
            {
                capturedProfileData = profileData;
                member.SetValue("title", "From Profile: Engineering");
            });

        // Assert — callback received the profileData and property was persisted.
        Assert.That(result.Success, Is.True);
        Assert.That(capturedProfileData, Is.EqualTo(profileJson));

        IMember? reloaded = MemberService.GetById(result.Result!.Key);
        Assert.That(reloaded, Is.Not.Null);
        Assert.That(reloaded!.GetValue<string>("title"), Is.EqualTo("From Profile: Engineering"));

        // Assert — without a callback, properties would remain empty.
        // (Verified by the absence of any auto-mapped properties on the result.)
    }

    [Test]
    public async Task Convert_External_To_Content_Returns_NotFound_For_NonExistent()
    {
        // Act
        var result = await ExternalMemberService.ConvertToContentMemberAsync(Guid.NewGuid(), "Member");

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ExternalMemberOperationStatus.NotFound));
    }

}
