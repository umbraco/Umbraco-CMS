using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[Category("Slow")]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MemberEditingServiceTests : UmbracoIntegrationTest
{
    private IMemberEditingService MemberEditingService => GetRequiredService<IMemberEditingService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IMemberGroupService MemberGroupService => GetRequiredService<IMemberGroupService>();

    private IExternalMemberService ExternalMemberService => GetRequiredService<IExternalMemberService>();

    [Test]
    public async Task Can_Create_Member()
    {
        var member = await CreateMemberAsync();
        Assert.IsTrue(member.HasIdentity);
        Assert.Greater(member.Id, 0);

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.IsNotNull(member);
        Assert.AreEqual("test@test.com", member.Email);
        Assert.AreEqual("test", member.Username);
        Assert.AreEqual("T. Est", member.Name);
        Assert.IsTrue(member.IsApproved);

        Assert.AreEqual("The title value", member.GetValue<string>("title"));
        Assert.AreEqual("The author value", member.GetValue<string>("author"));

        var memberManager = GetRequiredService<IMemberManager>();
        var memberIdentityUser = await memberManager.FindByEmailAsync(member.Email);
        Assert.IsNotNull(memberIdentityUser);
        Assert.IsTrue(await memberManager.CheckPasswordAsync(memberIdentityUser, "SuperSecret123"));

        var roles = MemberService.GetAllRoles(member.Id).ToArray();
        Assert.AreEqual(1, roles.Length);
        Assert.AreEqual("RoleOne", roles[0]);
    }

    [Test]
    public async Task Can_Create_Member_With_Explicit_Key()
    {
        var key = Guid.NewGuid();
        var member = await CreateMemberAsync(key);
        Assert.IsTrue(member.HasIdentity);
        Assert.Greater(member.Id, 0);
        Assert.AreEqual(key, member.Key);

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.IsNotNull(member);
    }

    [Test]
    public async Task Can_Update_Member()
    {
        var member = await CreateMemberAsync();

        var updateModel = new MemberUpdateModel
        {
            Email = "test-updated@test.com",
            Username = "test-updated",
            IsApproved = false,
            Variants = [new VariantModel { Name = "T. Est Updated" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The updated title value" },
                new PropertyValueModel { Alias = "author", Value = "The updated author value" }
            ]
        };

        var result = await MemberEditingService.UpdateAsync(member.Key, updateModel, SuperUser());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
        Assert.AreEqual(MemberEditingOperationStatus.Success, result.Status.MemberEditingOperationStatus);

        member = result.Result.Content;
        Assert.IsNotNull(member);
        Assert.AreEqual("test-updated@test.com", member.Email);
        Assert.AreEqual("test-updated", member.Username);
        Assert.AreEqual("T. Est Updated", member.Name);
        Assert.IsFalse(member.IsApproved);

        Assert.AreEqual("The updated title value", member.GetValue<string>("title"));
        Assert.AreEqual("The updated author value", member.GetValue<string>("author"));
    }

    [Test]
    public async Task Can_Change_Member_Password()
    {
        var member = await CreateMemberAsync();
        var memberManager = GetRequiredService<IMemberManager>();

        var updateModel = new MemberUpdateModel
        {
            Email = member.Email,
            Username = member.Username,
            IsApproved = true,
            Variants = [new VariantModel { Name = member.Name }],
            NewPassword = "NewSuperSecret123"
        };

        var result = await MemberEditingService.UpdateAsync(member.Key, updateModel, SuperUser());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
        Assert.AreEqual(MemberEditingOperationStatus.Success, result.Status.MemberEditingOperationStatus);

        var memberIdentityUser = await memberManager.FindByEmailAsync(member.Email);
        Assert.IsNotNull(memberIdentityUser);
        Assert.IsTrue(await memberManager.CheckPasswordAsync(memberIdentityUser, "NewSuperSecret123"));
    }

    [Test]
    public async Task Can_Change_Member_Roles()
    {
        MemberService.AddRole("RoleTwo");
        MemberService.AddRole("RoleThree");
        var groups = new[] { MemberGroupService.GetByName("RoleTwo"), MemberGroupService.GetByName("RoleThree") };

        var member = await CreateMemberAsync();

        var updateModel = new MemberUpdateModel
        {
            Email = member.Email,
            Username = member.Username,
            IsApproved = true,
            Variants = [new VariantModel { Name = member.Name }],
            Roles = groups.Select(x => x.Key),
        };

        var result = await MemberEditingService.UpdateAsync(member.Key, updateModel, SuperUser());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
        Assert.AreEqual(MemberEditingOperationStatus.Success, result.Status.MemberEditingOperationStatus);

        var roles = MemberService.GetAllRoles(member.Id).ToArray();
        Assert.AreEqual(2, roles.Length);
        Assert.IsTrue(roles.Contains("RoleTwo"));
        Assert.IsTrue(roles.Contains("RoleThree"));
    }

    [Test]
    public async Task Can_Delete_Member()
    {
        var member = await CreateMemberAsync();
        Assert.IsTrue(member.HasIdentity);
        Assert.Greater(member.Id, 0);

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.IsNotNull(member);

        var result = await MemberEditingService.DeleteAsync(member.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
        Assert.AreEqual(MemberEditingOperationStatus.Success, result.Status.MemberEditingOperationStatus);

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.IsNull(member);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Create_With_Property_Validation(bool addValidProperties)
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        memberType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        memberType.PropertyTypes.First(pt => pt.Alias == "author").ValidationRegExp = "^\\d*$";
        MemberTypeService.Save(memberType);

        var titleValue = addValidProperties ? "The title value" : null;
        var authorValue = addValidProperties ? "12345" : "This is not a number";

        var createModel = new MemberCreateModel
        {
            Email = "test@test.com",
            Username = "test",
            Password = "SuperSecret123",
            IsApproved = true,
            ContentTypeKey = memberType.Key,
            Variants = [new VariantModel { Name = "T. Est" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = titleValue },
                new PropertyValueModel { Alias = "author", Value = authorValue }
            ]
        };

        var result = await MemberEditingService.CreateAsync(createModel, SuperUser());

        // success is expected regardless of property level validation - the validation error status is communicated in the attempt status (see below)
        Assert.IsTrue(result.Success);
        Assert.AreEqual(MemberEditingOperationStatus.Success, result.Status.MemberEditingOperationStatus);
        Assert.AreEqual(addValidProperties ? ContentEditingOperationStatus.Success : ContentEditingOperationStatus.PropertyValidationError, result.Status.ContentEditingOperationStatus);
        Assert.IsNotNull(result.Result);

        if (addValidProperties is false)
        {
            Assert.AreEqual(2, result.Result.ValidationResult.ValidationErrors.Count());
            Assert.IsNotNull(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "title" && v.ErrorMessages.Length == 1));
            Assert.IsNotNull(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "author" && v.ErrorMessages.Length == 1));
        }

        // NOTE: member creation must be successful, even if the mandatory property is missing
        Assert.IsTrue(result.Result.Content!.HasIdentity);
        Assert.AreEqual(titleValue, result.Result.Content!.GetValue<string>("title"));
        Assert.AreEqual(authorValue, result.Result.Content!.GetValue<string>("author"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Update_With_Property_Validation(bool addValidProperties)
    {
        var member = await CreateMemberAsync();
        var memberType = await MemberTypeService.GetAsync(member.ContentType.Key)!;
        memberType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        memberType.PropertyTypes.First(pt => pt.Alias == "author").ValidationRegExp = "^\\d*$";
        await MemberTypeService.SaveAsync(memberType, Constants.Security.SuperUserKey);

        var titleValue = addValidProperties ? "The title value" : null;
        var authorValue = addValidProperties ? "12345" : "This is not a number";

        var updateModel = new MemberUpdateModel
        {
            Email = member.Email,
            Username = member.Username,
            IsApproved = true,
            Variants = [new VariantModel { Name = member.Name }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = titleValue },
                new PropertyValueModel { Alias = "author", Value = authorValue }
            ]
        };

        var result = await MemberEditingService.UpdateAsync(member.Key, updateModel, SuperUser());

        // success is expected regardless of property level validation - the validation error status is communicated in the attempt status (see below)
        Assert.IsTrue(result.Success);
        Assert.AreEqual(MemberEditingOperationStatus.Success, result.Status.MemberEditingOperationStatus);
        Assert.AreEqual(addValidProperties ? ContentEditingOperationStatus.Success : ContentEditingOperationStatus.PropertyValidationError, result.Status.ContentEditingOperationStatus);
        Assert.IsNotNull(result.Result);

        if (addValidProperties is false)
        {
            Assert.AreEqual(2, result.Result.ValidationResult.ValidationErrors.Count());
            Assert.IsNotNull(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "title" && v.ErrorMessages.Length == 1));
            Assert.IsNotNull(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "author" && v.ErrorMessages.Length == 1));
        }

        // NOTE: member update must be successful, even if the mandatory property is missing
        Assert.AreEqual(titleValue, result.Result.Content!.GetValue<string>("title"));
        Assert.AreEqual(authorValue, result.Result.Content!.GetValue<string>("author"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Update_Sensitive_Properties_Without_Access(bool useSuperUser)
    {
        // this user does NOT have access to sensitive data
        var user = UserBuilder.CreateUser();
        UserService.Save(user);

        var member = await CreateMemberAsync(titleIsSensitive: true);

        var updateModel = new MemberUpdateModel
        {
            Email = "test-updated@test.com",
            Username = "test-updated",
            IsApproved = member.IsApproved,
            Variants = [new VariantModel { Name = "T. Est Updated" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The updated title value" },
                new PropertyValueModel { Alias = "author", Value = "The updated author value" }
            ]
        };

        var result = await MemberEditingService.UpdateAsync(member.Key, updateModel, useSuperUser ? SuperUser() : user);
        if (useSuperUser)
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
            Assert.AreEqual(MemberEditingOperationStatus.Success, result.Status.MemberEditingOperationStatus);
        }
        else
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status.ContentEditingOperationStatus);
            Assert.AreEqual(MemberEditingOperationStatus.Success, result.Status.MemberEditingOperationStatus);
        }

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.IsNotNull(member);

        if (useSuperUser)
        {
            Assert.AreEqual("The updated title value", member.GetValue<string>("title"));
            Assert.AreEqual("The updated author value", member.GetValue<string>("author"));

            Assert.AreEqual("test-updated@test.com", member.Email);
            Assert.AreEqual("test-updated", member.Username);
            Assert.AreEqual("T. Est Updated", member.Name);
        }
        else
        {
            Assert.AreEqual("The title value", member.GetValue<string>("title"));
            Assert.AreEqual("The author value", member.GetValue<string>("author"));

            Assert.AreEqual("test@test.com", member.Email);
            Assert.AreEqual("test", member.Username);
            Assert.AreEqual("T. Est", member.Name);
        }
    }

    [Test]
    public async Task Sensitive_Properties_Are_Retained_When_Updating_Without_Access()
    {
        // this user does NOT have access to sensitive data
        var user = UserBuilder.CreateUser();
        UserService.Save(user);

        var member = await CreateMemberAsync(titleIsSensitive: true);

        var updateModel = new MemberUpdateModel
        {
            Email = "test-updated@test.com",
            Username = "test-updated",
            Variants = [new VariantModel { Name = "T. Est Updated" }],
            Properties =
            [
                new PropertyValueModel { Alias = "author", Value = "The updated author value" }
            ]
        };

        var result = await MemberEditingService.UpdateAsync(member.Key, updateModel, user);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
        Assert.AreEqual(MemberEditingOperationStatus.Success, result.Status.MemberEditingOperationStatus);

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.IsNotNull(member);

        Assert.AreEqual("The title value", member.GetValue<string>("title"));
        Assert.AreEqual("The updated author value", member.GetValue<string>("author"));

        Assert.AreEqual("test-updated@test.com", member.Email);
        Assert.AreEqual("test-updated", member.Username);
        Assert.AreEqual("T. Est Updated", member.Name);

        // IsApproved and IsLockedOut are always sensitive properties.
        Assert.IsTrue(member.IsApproved);
        Assert.IsFalse(member.IsLockedOut);
    }

    [Test]
    public async Task IsExternalMember_Returns_True_For_External_Member()
    {
        // Arrange
        var externalMember = new ExternalMemberIdentityBuilder()
            .WithEmail("external@test.com")
            .WithUserName("external@test.com")
            .Build();
        await ExternalMemberService.CreateAsync(externalMember);

        // Act
        var result = await MemberEditingService.IsExternalMemberAsync(externalMember.Key);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task IsExternalMember_Returns_False_For_Content_Member()
    {
        // Arrange
        var member = await CreateMemberAsync();

        // Act
        var result = await MemberEditingService.IsExternalMemberAsync(member.Key);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task IsExternalMember_Returns_False_For_NonExistent_Key()
    {
        // Act
        var result = await MemberEditingService.IsExternalMemberAsync(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task GetExternalMember_Returns_Member_For_External_Member()
    {
        // Arrange
        var externalMember = new ExternalMemberIdentityBuilder()
            .WithEmail("get-external@test.com")
            .WithUserName("get-external@test.com")
            .WithName("Get External Test")
            .Build();
        await ExternalMemberService.CreateAsync(externalMember);

        // Act
        var result = await MemberEditingService.GetExternalMemberAsync(externalMember.Key);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(externalMember.Key, result!.Key);
        Assert.AreEqual("get-external@test.com", result.Email);
        Assert.AreEqual("Get External Test", result.Name);
    }

    [Test]
    public async Task GetExternalMember_Returns_Null_For_Content_Member()
    {
        // Arrange
        var member = await CreateMemberAsync();

        // Act
        var result = await MemberEditingService.GetExternalMemberAsync(member.Key);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task GetExternalMember_Returns_Null_For_NonExistent_Key()
    {
        // Act
        var result = await MemberEditingService.GetExternalMemberAsync(Guid.NewGuid());

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task Can_Delete_External_Member()
    {
        // Arrange
        var externalMember = new ExternalMemberIdentityBuilder()
            .WithEmail("delete-external@test.com")
            .WithUserName("delete-external@test.com")
            .Build();
        await ExternalMemberService.CreateAsync(externalMember);

        // Verify it exists.
        Assert.IsTrue(await MemberEditingService.IsExternalMemberAsync(externalMember.Key));

        // Act
        var result = await MemberEditingService.DeleteAsync(externalMember.Key, Constants.Security.SuperUserKey);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsFalse(await MemberEditingService.IsExternalMemberAsync(externalMember.Key));
    }

    [Test]
    public async Task Delete_External_Member_Does_Not_Affect_Content_Members()
    {
        // Arrange — create both a content member and an external member.
        var contentMember = await CreateMemberAsync();
        var externalMember = new ExternalMemberIdentityBuilder()
            .WithEmail("ext-only@test.com")
            .WithUserName("ext-only@test.com")
            .Build();
        await ExternalMemberService.CreateAsync(externalMember);

        // Act — delete the external member.
        await MemberEditingService.DeleteAsync(externalMember.Key, Constants.Security.SuperUserKey);

        // Assert — content member still exists.
        var retrievedContent = await MemberEditingService.GetAsync(contentMember.Key);
        Assert.IsNotNull(retrievedContent);
    }

    private IUser SuperUser() => GetRequiredService<IUserService>().GetAsync(Constants.Security.SuperUserKey).GetAwaiter().GetResult();

    private async Task<IMember> CreateMemberAsync(Guid? key = null, bool titleIsSensitive = false)
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        memberType.SetIsSensitiveProperty("title", titleIsSensitive);
        MemberTypeService.Save(memberType);
        MemberService.AddRole("RoleOne");
        var group = MemberGroupService.GetByName("RoleOne");

        var createModel = new MemberCreateModel
        {
            Key = key,
            Email = "test@test.com",
            Username = "test",
            Password = "SuperSecret123",
            IsApproved = true,
            ContentTypeKey = memberType.Key,
            Roles = new [] { group.Key },
            Variants = [new VariantModel { Name = "T. Est" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "author", Value = "The author value" }
            ]
        };

        var result = await MemberEditingService.CreateAsync(createModel, SuperUser());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
        Assert.AreEqual(MemberEditingOperationStatus.Success, result.Status.MemberEditingOperationStatus);

        var member = result.Result.Content;
        Assert.IsNotNull(member);
        Assert.IsTrue(member.HasIdentity);
        Assert.Greater(member.Id, 0);

        return await MemberEditingService.GetAsync(member.Key) ?? throw new ApplicationException("Created member could not be retrieved");
    }
}
