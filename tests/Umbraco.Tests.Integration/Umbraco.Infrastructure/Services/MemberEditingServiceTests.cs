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
        Assert.That(member.HasIdentity, Is.True);
        Assert.That(member.Id, Is.GreaterThan(0));

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.That(member, Is.Not.Null);
        Assert.That(member.Email, Is.EqualTo("test@test.com"));
        Assert.That(member.Username, Is.EqualTo("test"));
        Assert.That(member.Name, Is.EqualTo("T. Est"));
        Assert.That(member.IsApproved, Is.True);

        Assert.That(member.GetValue<string>("title"), Is.EqualTo("The title value"));
        Assert.That(member.GetValue<string>("author"), Is.EqualTo("The author value"));

        var memberManager = GetRequiredService<IMemberManager>();
        var memberIdentityUser = await memberManager.FindByEmailAsync(member.Email);
        Assert.That(memberIdentityUser, Is.Not.Null);
        Assert.That(await memberManager.CheckPasswordAsync(memberIdentityUser, "SuperSecret123"), Is.True);

        var roles = MemberService.GetAllRoles(member.Id).ToArray();
        Assert.That(roles, Has.Length.EqualTo(1));
        Assert.That(roles[0], Is.EqualTo("RoleOne"));
    }

    [Test]
    public async Task Can_Create_Member_With_Explicit_Key()
    {
        var key = Guid.NewGuid();
        var member = await CreateMemberAsync(key);
        Assert.That(member.HasIdentity, Is.True);
        Assert.That(member.Id, Is.GreaterThan(0));
        Assert.That(member.Key, Is.EqualTo(key));

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.That(member, Is.Not.Null);
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status.ContentEditingOperationStatus, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Status.MemberEditingOperationStatus, Is.EqualTo(MemberEditingOperationStatus.Success));

        member = result.Result.Content;
        Assert.That(member, Is.Not.Null);
        Assert.That(member.Email, Is.EqualTo("test-updated@test.com"));
        Assert.That(member.Username, Is.EqualTo("test-updated"));
        Assert.That(member.Name, Is.EqualTo("T. Est Updated"));
        Assert.That(member.IsApproved, Is.False);

        Assert.That(member.GetValue<string>("title"), Is.EqualTo("The updated title value"));
        Assert.That(member.GetValue<string>("author"), Is.EqualTo("The updated author value"));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status.ContentEditingOperationStatus, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Status.MemberEditingOperationStatus, Is.EqualTo(MemberEditingOperationStatus.Success));

        var memberIdentityUser = await memberManager.FindByEmailAsync(member.Email);
        Assert.That(memberIdentityUser, Is.Not.Null);
        Assert.That(await memberManager.CheckPasswordAsync(memberIdentityUser, "NewSuperSecret123"), Is.True);
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status.ContentEditingOperationStatus, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Status.MemberEditingOperationStatus, Is.EqualTo(MemberEditingOperationStatus.Success));

        var roles = MemberService.GetAllRoles(member.Id).ToArray();
        Assert.That(roles, Has.Length.EqualTo(2));
        Assert.That(roles.Contains("RoleTwo"), Is.True);
        Assert.That(roles.Contains("RoleThree"), Is.True);
    }

    [Test]
    public async Task Can_Delete_Member()
    {
        var member = await CreateMemberAsync();
        Assert.That(member.HasIdentity, Is.True);
        Assert.That(member.Id, Is.GreaterThan(0));

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.That(member, Is.Not.Null);

        var result = await MemberEditingService.DeleteAsync(member.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status.ContentEditingOperationStatus, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Status.MemberEditingOperationStatus, Is.EqualTo(MemberEditingOperationStatus.Success));

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.That(member, Is.Null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Create_With_Property_Validation(bool addValidProperties)
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        memberType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        memberType.PropertyTypes.First(pt => pt.Alias == "author").ValidationRegExp = "^\\d*$";
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status.MemberEditingOperationStatus, Is.EqualTo(MemberEditingOperationStatus.Success));
        Assert.That(result.Status.ContentEditingOperationStatus, Is.EqualTo(addValidProperties ? ContentEditingOperationStatus.Success : ContentEditingOperationStatus.PropertyValidationError));
        Assert.That(result.Result, Is.Not.Null);

        if (addValidProperties is false)
        {
            Assert.That(result.Result.ValidationResult.ValidationErrors.Count(), Is.EqualTo(2));
            Assert.That(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "title" && v.ErrorMessages.Length == 1), Is.Not.Null);
            Assert.That(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "author" && v.ErrorMessages.Length == 1), Is.Not.Null);
        }

        // NOTE: member creation must be successful, even if the mandatory property is missing
        Assert.That(result.Result.Content!.HasIdentity, Is.True);
        Assert.That(result.Result.Content!.GetValue<string>("title"), Is.EqualTo(titleValue));
        Assert.That(result.Result.Content!.GetValue<string>("author"), Is.EqualTo(authorValue));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Update_With_Property_Validation(bool addValidProperties)
    {
        var member = await CreateMemberAsync();
        var memberType = await MemberTypeService.GetAsync(member.ContentType.Key)!;
        memberType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        memberType.PropertyTypes.First(pt => pt.Alias == "author").ValidationRegExp = "^\\d*$";
        await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);

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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status.MemberEditingOperationStatus, Is.EqualTo(MemberEditingOperationStatus.Success));
        Assert.That(result.Status.ContentEditingOperationStatus, Is.EqualTo(addValidProperties ? ContentEditingOperationStatus.Success : ContentEditingOperationStatus.PropertyValidationError));
        Assert.That(result.Result, Is.Not.Null);

        if (addValidProperties is false)
        {
            Assert.That(result.Result.ValidationResult.ValidationErrors.Count(), Is.EqualTo(2));
            Assert.That(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "title" && v.ErrorMessages.Length == 1), Is.Not.Null);
            Assert.That(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "author" && v.ErrorMessages.Length == 1), Is.Not.Null);
        }

        // NOTE: member update must be successful, even if the mandatory property is missing
        Assert.That(result.Result.Content!.GetValue<string>("title"), Is.EqualTo(titleValue));
        Assert.That(result.Result.Content!.GetValue<string>("author"), Is.EqualTo(authorValue));
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
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status.ContentEditingOperationStatus, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(result.Status.MemberEditingOperationStatus, Is.EqualTo(MemberEditingOperationStatus.Success));
        }
        else
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status.ContentEditingOperationStatus, Is.EqualTo(ContentEditingOperationStatus.NotAllowed));
            Assert.That(result.Status.MemberEditingOperationStatus, Is.EqualTo(MemberEditingOperationStatus.Success));
        }

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.That(member, Is.Not.Null);

        if (useSuperUser)
        {
            Assert.That(member.GetValue<string>("title"), Is.EqualTo("The updated title value"));
            Assert.That(member.GetValue<string>("author"), Is.EqualTo("The updated author value"));

            Assert.That(member.Email, Is.EqualTo("test-updated@test.com"));
            Assert.That(member.Username, Is.EqualTo("test-updated"));
            Assert.That(member.Name, Is.EqualTo("T. Est Updated"));
        }
        else
        {
            Assert.That(member.GetValue<string>("title"), Is.EqualTo("The title value"));
            Assert.That(member.GetValue<string>("author"), Is.EqualTo("The author value"));

            Assert.That(member.Email, Is.EqualTo("test@test.com"));
            Assert.That(member.Username, Is.EqualTo("test"));
            Assert.That(member.Name, Is.EqualTo("T. Est"));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status.ContentEditingOperationStatus, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Status.MemberEditingOperationStatus, Is.EqualTo(MemberEditingOperationStatus.Success));

        member = await MemberEditingService.GetAsync(member.Key);
        Assert.That(member, Is.Not.Null);

        Assert.That(member.GetValue<string>("title"), Is.EqualTo("The title value"));
        Assert.That(member.GetValue<string>("author"), Is.EqualTo("The updated author value"));

        Assert.That(member.Email, Is.EqualTo("test-updated@test.com"));
        Assert.That(member.Username, Is.EqualTo("test-updated"));
        Assert.That(member.Name, Is.EqualTo("T. Est Updated"));

        // IsApproved and IsLockedOut are always sensitive properties.
        Assert.That(member.IsApproved, Is.True);
        Assert.That(member.IsLockedOut, Is.False);
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
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsExternalMember_Returns_False_For_Content_Member()
    {
        // Arrange
        var member = await CreateMemberAsync();

        // Act
        var result = await MemberEditingService.IsExternalMemberAsync(member.Key);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsExternalMember_Returns_False_For_NonExistent_Key()
    {
        // Act
        var result = await MemberEditingService.IsExternalMemberAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.False);
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
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Key, Is.EqualTo(externalMember.Key));
        Assert.That(result.Email, Is.EqualTo("get-external@test.com"));
        Assert.That(result.Name, Is.EqualTo("Get External Test"));
    }

    [Test]
    public async Task GetExternalMember_Returns_Null_For_Content_Member()
    {
        // Arrange
        var member = await CreateMemberAsync();

        // Act
        var result = await MemberEditingService.GetExternalMemberAsync(member.Key);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetExternalMember_Returns_Null_For_NonExistent_Key()
    {
        // Act
        var result = await MemberEditingService.GetExternalMemberAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
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
        Assert.That(await MemberEditingService.IsExternalMemberAsync(externalMember.Key), Is.True);

        // Act
        var result = await MemberEditingService.DeleteAsync(externalMember.Key, Constants.Security.SuperUserKey);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(await MemberEditingService.IsExternalMemberAsync(externalMember.Key), Is.False);
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
        Assert.That(retrievedContent, Is.Not.Null);
    }

    private IUser SuperUser() => GetRequiredService<IUserService>().GetAsync(Constants.Security.SuperUserKey).GetAwaiter().GetResult();

    private async Task<IMember> CreateMemberAsync(Guid? key = null, bool titleIsSensitive = false)
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        memberType.SetIsSensitiveProperty("title", titleIsSensitive);
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status.ContentEditingOperationStatus, Is.EqualTo(ContentEditingOperationStatus.Success));
        Assert.That(result.Status.MemberEditingOperationStatus, Is.EqualTo(MemberEditingOperationStatus.Success));

        var member = result.Result.Content;
        Assert.That(member, Is.Not.Null);
        Assert.That(member.HasIdentity, Is.True);
        Assert.That(member.Id, Is.GreaterThan(0));

        return await MemberEditingService.GetAsync(member.Key) ?? throw new ApplicationException("Created member could not be retrieved");
    }
}
