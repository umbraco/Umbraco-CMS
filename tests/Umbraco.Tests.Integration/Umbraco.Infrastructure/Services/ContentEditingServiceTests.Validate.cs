using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [Test]
    public async Task Can_Validate_Valid_Invariant_Content()
    {
        var content = await CreateInvariantContent();

        var validateContentUpdateModel = new ValidateContentUpdateModel
        {
            InvariantName = "Updated Name",
            InvariantProperties =
            [
                new PropertyValueModel { Alias = "title", Value = "The updated title" },
                new PropertyValueModel { Alias = "text", Value = "The updated text" }
            ]
        };

        Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await ContentEditingService.ValidateUpdateAsync(content.Key, validateContentUpdateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    [Test]
    public async Task Will_Fail_Invalid_Invariant_Content()
    {
        var content = await CreateInvariantContent();

        var validateContentUpdateModel = new ValidateContentUpdateModel
        {
            InvariantName = "Updated Name",
            InvariantProperties =
            [
                new PropertyValueModel { Alias = "title", Value = null },
                new PropertyValueModel { Alias = "text", Value = "The updated text" }
            ]
        };

        Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await ContentEditingService.ValidateUpdateAsync(content.Key, validateContentUpdateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyValidationError, result.Status);
        Assert.AreEqual(1, result.Result.ValidationErrors.Count());
        Assert.AreEqual("#validation_invalidNull", result.Result.ValidationErrors.Single(x => x.Alias == "title").ErrorMessages[0]);
    }

    [Test]
    public async Task Can_Validate_Valid_Variant_Content()
    {
        var content = await CreateVariantContent();

        var validateContentUpdateModel = new ValidateContentUpdateModel
        {
            InvariantProperties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" }
            ],
            Variants =
            [
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "Updated English Name",
                    Properties =
                    [
                        new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title" }
                    ]
                },
                new VariantModel
                {
                    Culture = "da-DK",
                    Name = "Updated Danish Name",
                    Properties =
                    [
                        new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish title" }
                    ]
                }
            ],
        };

        Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await ContentEditingService.ValidateUpdateAsync(content.Key, validateContentUpdateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    [Test]
    public async Task Will_Fail_Invalid_Variant_Content()
    {
        var content = await CreateVariantContent();

        var validateContentUpdateModel = new ValidateContentUpdateModel
        {
            InvariantProperties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" }
            ],
            Variants =
            [
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "Updated English Name",
                    Properties =
                    [
                        new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title" }
                    ]
                },
                new VariantModel
                {
                    Culture = "da-DK",
                    Name = "Updated Danish Name",
                    Properties =
                    [
                        new PropertyValueModel { Alias = "variantTitle", Value = null }
                    ]
                }
            ],
        };

        Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await ContentEditingService.ValidateUpdateAsync(content.Key, validateContentUpdateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyValidationError, result.Status);
        Assert.AreEqual(1, result.Result.ValidationErrors.Count());
        Assert.AreEqual("#validation_invalidNull", result.Result.ValidationErrors.Single(x => x.Alias == "variantTitle" && x.Culture == "da-DK").ErrorMessages[0]);
    }

    [Test]
    public async Task Will_Succeed_For_Invalid_Variant_Content_Without_Access_To_Edited_Culture()
    {
        var content = await CreateVariantContent();

        IUser englishEditor = await CreateEnglishLanguageOnlyEditor();

        var validateContentUpdateModel = new ValidateContentUpdateModel
        {
            InvariantProperties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" }
            ],
            Variants =
            [
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "Updated English Name",
                    Properties =
                    [
                        new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title" }
                    ]
                }
            ],
        };

        Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await ContentEditingService.ValidateUpdateAsync(content.Key, validateContentUpdateModel, englishEditor.Key);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    private async Task<IUser> CreateEnglishLanguageOnlyEditor()
    {
        var enUSLanguage = await LanguageService.GetAsync("en-US");
        var userGroup = new UserGroupBuilder()
            .WithName("English Editors")
            .WithAlias("englishEditors")
            .WithAllowedLanguages([enUSLanguage.Id])
            .Build();

        var createUserGroupResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(createUserGroupResult.Success);

        var createUserAttempt = await UserService.CreateAsync(Constants.Security.SuperUserKey, new UserCreateModel
        {
            Email = "english-editor@test.com",
            Name = "Test English Editor",
            UserName = "english-editor@test.com",
            UserGroupKeys = new[] { userGroup.Key }.ToHashSet(),
        });
        Assert.IsTrue(createUserAttempt.Success);

        return await UserService.GetAsync(createUserAttempt.Result.CreatedUser.Key);
    }
}
