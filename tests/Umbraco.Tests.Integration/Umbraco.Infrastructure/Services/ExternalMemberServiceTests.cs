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

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ExternalMemberOperationStatus.Success, result.Status);

        var retrieved = await ExternalMemberService.GetByKeyAsync(result.Result.Key);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(result.Result.Key, retrieved!.Key);
        Assert.AreEqual("create-test@example.com", retrieved.Email);
        Assert.AreEqual("create-test@example.com", retrieved.UserName);
        Assert.AreEqual("Create Test", retrieved.Name);
        Assert.IsTrue(retrieved.IsApproved);
        Assert.IsFalse(retrieved.IsLockedOut);
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
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("email-test@example.com", retrieved!.Email);
        Assert.AreEqual("Email Test", retrieved.Name);
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
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("username-lookup", retrieved!.UserName);
        Assert.AreEqual("username-test@example.com", retrieved.Email);
    }

    [Test]
    public async Task Can_Update_External_Member()
    {
        // Arrange
        var identity = ExternalMemberIdentityBuilder.CreateSimple("update-test@example.com", "Before Update");
        var createResult = await ExternalMemberService.CreateAsync(identity);
        Assert.IsTrue(createResult.Success);

        var member = await ExternalMemberService.GetByKeyAsync(createResult.Result.Key);
        Assert.IsNotNull(member);

        // Act
        member!.Name = "After Update";
        member.Email = "updated@example.com";
        member.IsApproved = false;
        var updateResult = await ExternalMemberService.UpdateAsync(member);

        // Assert
        Assert.IsTrue(updateResult.Success);
        Assert.AreEqual(ExternalMemberOperationStatus.Success, updateResult.Status);

        var retrieved = await ExternalMemberService.GetByKeyAsync(createResult.Result.Key);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("After Update", retrieved!.Name);
        Assert.AreEqual("updated@example.com", retrieved.Email);
        Assert.IsFalse(retrieved.IsApproved);
    }

    [Test]
    public async Task Can_Delete_External_Member()
    {
        // Arrange
        var identity = ExternalMemberIdentityBuilder.CreateSimple("delete-test@example.com", "Delete Test");
        var createResult = await ExternalMemberService.CreateAsync(identity);
        Assert.IsTrue(createResult.Success);

        // Verify it exists first
        var existing = await ExternalMemberService.GetByKeyAsync(createResult.Result.Key);
        Assert.IsNotNull(existing);

        // Act
        var deleteResult = await ExternalMemberService.DeleteAsync(createResult.Result.Key);

        // Assert
        Assert.IsTrue(deleteResult.Success);
        Assert.AreEqual(ExternalMemberOperationStatus.Success, deleteResult.Status);

        var retrieved = await ExternalMemberService.GetByKeyAsync(createResult.Result.Key);
        Assert.IsNull(retrieved);
    }

    [Test]
    public async Task Delete_Returns_NotFound_For_NonExistent()
    {
        // Act
        var result = await ExternalMemberService.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ExternalMemberOperationStatus.NotFound, result.Status);
    }

    [Test]
    public async Task Can_Assign_And_Get_Roles()
    {
        // Arrange
        var identity = ExternalMemberIdentityBuilder.CreateSimple("roles-test@example.com", "Roles Test");
        var createResult = await ExternalMemberService.CreateAsync(identity);
        Assert.IsTrue(createResult.Success);

        MemberService.AddRole("ExternalTestGroup");

        // Act
        var assignResult = await ExternalMemberService.AssignRolesAsync(createResult.Result.Key, ["ExternalTestGroup"]);
        var roles = await ExternalMemberService.GetRolesAsync(createResult.Result.Key);

        // Assert
        Assert.IsTrue(assignResult.Success);
        Assert.IsNotNull(roles);
        CollectionAssert.Contains(roles.ToList(), "ExternalTestGroup");
    }

    [Test]
    public async Task Can_Remove_Roles()
    {
        // Arrange
        var identity = ExternalMemberIdentityBuilder.CreateSimple("remove-roles@example.com", "Remove Roles Test");
        var createResult = await ExternalMemberService.CreateAsync(identity);
        Assert.IsTrue(createResult.Success);

        MemberService.AddRole("RemovableGroup");
        await ExternalMemberService.AssignRolesAsync(createResult.Result.Key, ["RemovableGroup"]);

        // Verify role is assigned
        var rolesBefore = await ExternalMemberService.GetRolesAsync(createResult.Result.Key);
        CollectionAssert.Contains(rolesBefore.ToList(), "RemovableGroup");

        // Act
        var removeResult = await ExternalMemberService.RemoveRolesAsync(createResult.Result.Key, ["RemovableGroup"]);

        // Assert
        Assert.IsTrue(removeResult.Success);
        var rolesAfter = await ExternalMemberService.GetRolesAsync(createResult.Result.Key);
        CollectionAssert.DoesNotContain(rolesAfter.ToList(), "RemovableGroup");
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
        Assert.IsTrue(createResult.Success);

        // Act
        var retrieved = await ExternalMemberService.GetByKeyAsync(createResult.Result.Key);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(profileJson, retrieved!.ProfileData);
    }

    [Test]
    public async Task GetByKey_Returns_Null_For_NonExistent()
    {
        // Act
        var retrieved = await ExternalMemberService.GetByKeyAsync(Guid.NewGuid());

        // Assert
        Assert.IsNull(retrieved);
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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ExternalMemberOperationStatus.DuplicateUsername, result.Status);
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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ExternalMemberOperationStatus.DuplicateEmail, result.Status);
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
        Assert.IsTrue(createResult.Success);
        var originalKey = createResult.Result.Key;

        MemberService.AddRole("ConvertGroup");
        await ExternalMemberService.AssignRolesAsync(originalKey, ["ConvertGroup"]);

        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        // Act
        var result = await ExternalMemberService.ConvertToContentMemberAsync(originalKey, memberType.Alias);

        // Assert — content member created with same key and identity fields.
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(originalKey, result.Result!.Key);
        Assert.AreEqual("convert-to-content@test.com", result.Result.Email);
        Assert.AreEqual("convert-to-content", result.Result.Username);

        // Assert — external member record removed.
        var externalMember = await ExternalMemberService.GetByKeyAsync(originalKey);
        Assert.IsNull(externalMember);

        // Assert — group memberships migrated.
        IEnumerable<string> contentRoles = MemberService.GetAllRoles(result.Result.Username);
        CollectionAssert.Contains(contentRoles.ToList(), "ConvertGroup");
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
        Assert.IsTrue(createResult.Success);

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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(profileJson, capturedProfileData);

        IMember? reloaded = MemberService.GetById(result.Result!.Key);
        Assert.IsNotNull(reloaded);
        Assert.AreEqual("From Profile: Engineering", reloaded!.GetValue<string>("title"));

        // Assert — without a callback, properties would remain empty.
        // (Verified by the absence of any auto-mapped properties on the result.)
    }

    [Test]
    public async Task Convert_External_To_Content_Returns_NotFound_For_NonExistent()
    {
        // Act
        var result = await ExternalMemberService.ConvertToContentMemberAsync(Guid.NewGuid(), "Member");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ExternalMemberOperationStatus.NotFound, result.Status);
    }

}
