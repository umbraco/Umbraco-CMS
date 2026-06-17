// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
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

    private IExternalLoginWithKeyService ExternalLoginService => GetRequiredService<IExternalLoginWithKeyService>();

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

    [Test]
    public async Task Can_Convert_Content_To_External_Member()
    {
        // Arrange — content member with a group, a property value and an external login link.
        IMember member = await CreateContentMemberAsync("convert-to-external@test.com", "convert-to-external", title: "Engineer");

        MemberService.AddRole("ToExternalGroup");
        MemberService.AssignRoles([member.Id], ["ToExternalGroup"]);

        ExternalLoginService.Save(member.Key, new[] { new ExternalLogin("TestProvider", "provider-key-123") });

        var originalKey = member.Key;

        // Act — map the content "title" property into the external member's profile data.
        string? capturedTitle = null;
        var result = await ExternalMemberService.ConvertToExternalMemberAsync(
            originalKey,
            (identity, source) =>
            {
                capturedTitle = source.GetValue<string>("title");
                identity.ProfileData = $$"""{"title":"{{capturedTitle}}"}""";
            });

        // Assert — external member created with the same key and identity fields.
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ExternalMemberOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(originalKey, result.Result!.Key);
        Assert.AreEqual("convert-to-external@test.com", result.Result.Email);
        Assert.AreEqual("convert-to-external", result.Result.UserName);

        // Assert — callback ran against the source member and profile data was persisted.
        Assert.AreEqual("Engineer", capturedTitle);
        var externalMember = await ExternalMemberService.GetByKeyAsync(originalKey);
        Assert.IsNotNull(externalMember);
        Assert.AreEqual("""{"title":"Engineer"}""", externalMember!.ProfileData);

        // Assert — the content member is gone.
        Assert.IsNull(MemberService.GetById(originalKey));

        // Assert — group memberships migrated to the external store.
        IEnumerable<string> externalRoles = await ExternalMemberService.GetRolesAsync(originalKey);
        CollectionAssert.Contains(externalRoles.ToList(), "ToExternalGroup");
    }

    [Test]
    public async Task Convert_Content_To_External_Preserves_External_Login()
    {
        // Arrange — content member with an external login link and a token.
        IMember member = await CreateContentMemberAsync("preserve-login@test.com", "preserve-login");
        var originalKey = member.Key;

        ExternalLoginService.Save(originalKey, new[] { new ExternalLogin("TestProvider", "provider-key-abc", "user-data") });
        ExternalLoginService.Save(originalKey, new[] { new ExternalLoginToken("TestProvider", "access_token", "token-value") });

        // Act
        var result = await ExternalMemberService.ConvertToExternalMemberAsync(originalKey);

        // Assert — the login link and token survive the conversion (deleting the content member queues a
        // deferred handler that wipes the links; the second scope must re-link them).
        Assert.IsTrue(result.Success);

        var logins = ExternalLoginService.GetExternalLogins(originalKey).ToList();
        Assert.AreEqual(1, logins.Count);
        Assert.AreEqual("TestProvider", logins[0].LoginProvider);
        Assert.AreEqual("provider-key-abc", logins[0].ProviderKey);
        Assert.AreEqual("user-data", logins[0].UserData);

        var tokens = ExternalLoginService.GetExternalLoginTokens(originalKey).ToList();
        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual("access_token", tokens[0].Name);
        Assert.AreEqual("token-value", tokens[0].Value);
    }

    [Test]
    public async Task Convert_Content_To_External_Returns_NoExternalLogin_When_No_Link()
    {
        // Arrange — a password-only content member with no external login link.
        IMember member = await CreateContentMemberAsync("no-link@test.com", "no-link");
        var originalKey = member.Key;

        // Act + Assert — by default the conversion is rejected and the member is left untouched.
        var guardedResult = await ExternalMemberService.ConvertToExternalMemberAsync(originalKey);
        Assert.IsFalse(guardedResult.Success);
        Assert.AreEqual(ExternalMemberOperationStatus.NoExternalLogin, guardedResult.Status);
        Assert.IsNotNull(MemberService.GetById(originalKey), "The failed guard must not mutate the member.");

        // Act + Assert — forcing the conversion succeeds (auto-link recreates a link on next sign-in).
        var forcedResult = await ExternalMemberService.ConvertToExternalMemberAsync(originalKey, requireExternalLogin: false);
        Assert.IsTrue(forcedResult.Success);
        Assert.AreEqual(ExternalMemberOperationStatus.Success, forcedResult.Status);
        Assert.IsNull(MemberService.GetById(originalKey));
        Assert.IsNotNull(await ExternalMemberService.GetByKeyAsync(originalKey));
    }

    [Test]
    public async Task Convert_Content_To_External_Returns_NotFound_For_NonExistent()
    {
        // Act
        var result = await ExternalMemberService.ConvertToExternalMemberAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ExternalMemberOperationStatus.NotFound, result.Status);
    }

    [Test]
    public async Task ValidateConvertToExternalMember_Reports_Status_Without_Mutating()
    {
        // Arrange — member with a link (valid) and a member without one (invalid under the guard).
        IMember linkedMember = await CreateContentMemberAsync("validate-linked@test.com", "validate-linked");
        ExternalLoginService.Save(linkedMember.Key, new[] { new ExternalLogin("TestProvider", "provider-key-xyz") });

        IMember unlinkedMember = await CreateContentMemberAsync("validate-unlinked@test.com", "validate-unlinked");

        // Act
        var linkedStatus = await ExternalMemberService.ValidateConvertToExternalMemberAsync(linkedMember.Key);
        var unlinkedStatus = await ExternalMemberService.ValidateConvertToExternalMemberAsync(unlinkedMember.Key);
        var missingStatus = await ExternalMemberService.ValidateConvertToExternalMemberAsync(Guid.NewGuid());

        // Assert — correct statuses reported.
        Assert.AreEqual(ExternalMemberOperationStatus.Success, linkedStatus);
        Assert.AreEqual(ExternalMemberOperationStatus.NoExternalLogin, unlinkedStatus);
        Assert.AreEqual(ExternalMemberOperationStatus.NotFound, missingStatus);

        // Assert — validation mutated nothing: both members still exist as content members.
        Assert.IsNotNull(MemberService.GetById(linkedMember.Key));
        Assert.IsNotNull(MemberService.GetById(unlinkedMember.Key));
        Assert.IsNull(await ExternalMemberService.GetByKeyAsync(linkedMember.Key));
    }

    private async Task<IMember> CreateContentMemberAsync(string email, string username, string? title = null)
    {
        // A distinct member type alias per call, since several members may be created within one test.
        var alias = "memberType" + username.Replace("-", string.Empty);
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType(alias, alias);
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        IMember member = MemberBuilder.CreateSimpleMember(memberType, username, email, "password", username);
        if (title is not null)
        {
            member.SetValue("title", title);
        }

        MemberService.Save(member);
        return member;
    }
}
