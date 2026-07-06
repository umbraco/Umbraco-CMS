using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    private IUserService UserService => GetRequiredService<IUserService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    [Test]
    public async Task Can_Validate_Valid_Invariant_Element()
    {
        var element = await CreateInvariantElement();

        var validateModel = new ValidateElementUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The updated title" },
                new PropertyValueModel { Alias = "text", Value = "The updated text" }
            ],
        };

        var result = await ElementEditingService.ValidateUpdateAsync(
            element.Key,
            validateModel,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    [Test]
    public async Task Will_Fail_Invalid_Invariant_Element()
    {
        var elementType = await CreateInvariantElementType();
        elementType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Initial Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The initial title" },
                new PropertyValueModel { Alias = "text", Value = "The initial text" }
            ],
        };

        var createResult = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var validateModel = new ValidateElementUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = null },
                new PropertyValueModel { Alias = "text", Value = "The updated text" }
            ],
        };

        var result = await ElementEditingService.ValidateUpdateAsync(
            createResult.Result.Content!.Key,
            validateModel,
            Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyValidationError, result.Status);
        Assert.AreEqual(1, result.Result.ValidationErrors.Count());
        Assert.AreEqual(
            "#validation_invalidNull",
            result.Result.ValidationErrors.Single(x => x.Alias == "title").ErrorMessages[0]);
    }

    [Test]
    public async Task Can_Validate_Valid_Culture_Variant_Element()
    {
        var element = await CreateCultureVariantElement();

        var validateModel = new ValidateElementUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish title", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" },
                new VariantModel { Culture = "da-DK", Name = "Updated Danish Name" }
            ],
        };

        var result = await ElementEditingService.ValidateUpdateAsync(
            element.Key,
            validateModel,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    [Test]
    public async Task Will_Fail_Invalid_Culture_Variant_Element()
    {
        var element = await CreateCultureVariantElement();

        var validateModel = new ValidateElementUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = null, Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" },
                new VariantModel { Culture = "da-DK", Name = "Updated Danish Name" }
            ],
        };

        var result = await ElementEditingService.ValidateUpdateAsync(
            element.Key,
            validateModel,
            Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyValidationError, result.Status);
        Assert.AreEqual(1, result.Result.ValidationErrors.Count());
        Assert.AreEqual(
            "#validation_invalidNull",
            result.Result.ValidationErrors.Single(x => x.Alias == "variantTitle" && x.Culture == "da-DK").ErrorMessages[0]);
    }

    [Test]
    public async Task Can_Validate_Create_Valid_Invariant_Element()
    {
        var elementType = await CreateInvariantElementType();
        elementType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "text", Value = "The text value" }
            ],
        };

        var result = await ElementEditingService.ValidateCreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    [Test]
    public async Task Will_Fail_Validate_Create_Invalid_Invariant_Element()
    {
        var elementType = await CreateInvariantElementType();
        elementType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Test Create" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = null },
                new PropertyValueModel { Alias = "text", Value = "The text value" }
            ],
        };

        var result = await ElementEditingService
            .ValidateCreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyValidationError, result.Status);
        Assert.AreEqual(1, result.Result.ValidationErrors.Count());
        Assert.AreEqual(
            "#validation_invalidNull",
            result.Result.ValidationErrors.Single(x => x.Alias == "title").ErrorMessages[0]);
    }

    [Test]
    public async Task Will_Succeed_For_Invalid_Variant_Element_Without_Access_To_Edited_Culture()
    {
        var element = await CreateCultureVariantElement();

        IUser englishEditor = await CreateEnglishLanguageOnlyEditor();

        var validateModel = new ValidateElementUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title", Culture = "en-US" },
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" }
            ],
        };

        var result = await ElementEditingService.ValidateUpdateAsync(element.Key, validateModel, englishEditor.Key);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    [Test]
    public async Task Validate_Update_Returns_NotFound_For_Non_Existing_Element()
    {
        var validateModel = new ValidateElementUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties = [],
        };

        var result = await ElementEditingService.ValidateUpdateAsync(
            Guid.NewGuid(),
            validateModel,
            Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status);
    }

    private async Task<IUser> CreateEnglishLanguageOnlyEditor()
    {
        var enUSLanguage = await LanguageService.GetAsync("en-US");
        var userGroup = new UserGroupBuilder()
            .WithName("English Editors")
            .WithAlias("englishEditors")
            .WithAllowedLanguages([enUSLanguage!.Id])
            .Build();

        var createUserGroupResult = await UserGroupService
            .CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(createUserGroupResult.Success);

        var createUserAttempt = await UserService.CreateAsync(Constants.Security.SuperUserKey, new UserCreateModel
        {
            Email = "english-editor@test.com",
            Name = "Test English Editor",
            UserName = "english-editor@test.com",
            UserGroupKeys = new[] { userGroup.Key }.ToHashSet(),
        });
        Assert.IsTrue(createUserAttempt.Success);

        return await UserService.GetAsync(createUserAttempt.Result.CreatedUser!.Key);
    }
}
