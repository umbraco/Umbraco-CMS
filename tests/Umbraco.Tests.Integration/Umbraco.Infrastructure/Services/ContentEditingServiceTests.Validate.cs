using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [Test]
    public async Task Can_Validate_Valid_Invariant_Content()
    {
        var content = await CreateInvariantContent();

        var validateContentUpdateModel = new ValidateContentUpdateModel
        {
            Variants =
            [
                new () { Name = "Updated Name" }
            ],
            Properties =
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
            Variants =
            [
                new () { Name = "Updated Name" }
            ],
            Properties =
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
    public async Task Can_Validate_Valid_Culture_Variant_Content()
    {
        var content = await CreateCultureVariantContent();

        var validateContentUpdateModel = new ValidateContentUpdateModel
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

        Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await ContentEditingService.ValidateUpdateAsync(content.Key, validateContentUpdateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    [Test]
    public async Task Will_Fail_Invalid_Culture_Variant_Content()
    {
        var content = await CreateCultureVariantContent();

        var validateContentUpdateModel = new ValidateContentUpdateModel
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

        Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await ContentEditingService.ValidateUpdateAsync(content.Key, validateContentUpdateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyValidationError, result.Status);
        Assert.AreEqual(1, result.Result.ValidationErrors.Count());
        Assert.AreEqual("#validation_invalidNull", result.Result.ValidationErrors.Single(x => x.Alias == "variantTitle" && x.Culture == "da-DK").ErrorMessages[0]);
    }

    [Test]
    public async Task Can_Validate_Valid_Culture_And_Segment_Variant_Content()
    {
        var content = await CreateCultureAndSegmentVariantContent(ContentVariation.Culture);

        var validateContentUpdateModel = new ValidateContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English default segment title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish default segment title", Culture = "da-DK" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English segment 1 title", Culture = "en-US", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish segment 1 title", Culture = "da-DK", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English segment 2 title", Culture = "en-US", Segment = "seg-2" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish segment 2 title", Culture = "da-DK", Segment = "seg-2" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Segment = "seg-1", Name = "Updated English segment 1 Name" },
                new VariantModel { Culture = "da-DK", Segment = "seg-1", Name = "Updated Danish segment 1 Name" },
                new VariantModel { Culture = "en-US", Segment = "seg-2", Name = "Updated English segment 2 Name" },
                new VariantModel { Culture = "da-DK", Segment = "seg-2", Name = "Updated Danish segment 2 Name" }

            ],
        };

        Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await ContentEditingService.ValidateUpdateAsync(content.Key, validateContentUpdateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
    }

    [Test]
    public async Task Will_Fail_Invalid_Culture_And_Segment_Variant_Content()
    {
        var content = await CreateCultureAndSegmentVariantContent(ContentVariation.Culture);

        var validateContentUpdateModel = new ValidateContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English default segment title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish default segment title", Culture = "da-DK" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English segment 1 title", Culture = "en-US", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish segment 1 title", Culture = "da-DK", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English segment 2 title", Culture = "en-US", Segment = "seg-2" },
                new PropertyValueModel { Alias = "variantTitle", Value = null, Culture = "da-DK", Segment = "seg-2" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Segment = "seg-1", Name = "Updated English segment 1 Name" },
                new VariantModel { Culture = "da-DK", Segment = "seg-1", Name = "Updated Danish segment 1 Name" },
                new VariantModel { Culture = "en-US", Segment = "seg-2", Name = "Updated English segment 2 Name" },
                new VariantModel { Culture = "da-DK", Segment = "seg-2", Name = "Updated Danish segment 2 Name" }
            ],
        };

        Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await ContentEditingService.ValidateUpdateAsync(content.Key, validateContentUpdateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyValidationError, result.Status);
        Assert.AreEqual(1, result.Result.ValidationErrors.Count());
        Assert.AreEqual("#validation_invalidNull", result.Result.ValidationErrors.Single(x => x.Alias == "variantTitle" && x.Culture == "da-DK" && x.Segment == "seg-2").ErrorMessages[0]);
    }

    [Test]
    public async Task Will_Succeed_For_Invalid_Variant_Content_Without_Access_To_Edited_Culture()
    {
        var content = await CreateCultureVariantContent();

        IUser englishEditor = await CreateEnglishLanguageOnlyEditor();

        var validateContentUpdateModel = new ValidateContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title", Culture = "en-US" },
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" }
            ]
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
