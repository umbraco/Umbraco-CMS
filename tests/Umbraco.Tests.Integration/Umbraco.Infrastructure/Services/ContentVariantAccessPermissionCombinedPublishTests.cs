// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Mirrors <see cref="ContentVariantAccessPermissionTests"/>, but exercises the combined create-and-publish
///     and update-and-publish operations (<see cref="IContentEditingService.CreateAndPublishAsync"/> and
///     <see cref="IContentEditingService.UpdateAndPublishAsync"/>) instead of separate edit and publish calls.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ContentVariantAccessPermissionCombinedPublishTests : UmbracoIntegrationTest
{
    private const string EnglishCulture = "en-US";
    private const string DanishCulture = "da-DK";

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    [Test]
    public async Task Can_Edit_And_Publish_Both_Cultures_And_Invariant_With_Access_To_All_Languages()
    {
        var contentKey = await SetupBaselineContentAsync();
        var user = await CreateRestrictedUserAsync(
            hasAccessToAllLanguages: true,
            allowedCultures: [],
            hasAccessToInvariantForVariant: true,
            suffix: "AllLang");

        var updateResult = await UpdateAndPublishAsync(
            contentKey,
            user.Key,
            [
                new PropertyValueModel { Alias = "variantValue", Value = "Updated EN value", Culture = EnglishCulture },
                new PropertyValueModel { Alias = "variantValue", Value = "Updated DA value", Culture = DanishCulture },
                new PropertyValueModel { Alias = "invariantValue", Value = "Updated invariant value" },
            ],
            EnglishCulture,
            DanishCulture);
        Assert.IsTrue(updateResult.Success);

        var content = ContentService.GetById(contentKey)!;
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Updated EN value", content.GetValue<string>("variantValue", EnglishCulture));
            Assert.AreEqual("Updated DA value", content.GetValue<string>("variantValue", DanishCulture));
            Assert.AreEqual("Updated invariant value", content.GetValue<string>("invariantValue"));

            Assert.AreEqual("Updated EN value", content.GetValue<string>("variantValue", EnglishCulture, published: true));
            Assert.AreEqual("Updated DA value", content.GetValue<string>("variantValue", DanishCulture, published: true));
            Assert.AreEqual("Updated invariant value", content.GetValue<string>("invariantValue", published: true));
        });
    }

    [Test]
    public async Task Can_Edit_And_Publish_Both_Cultures_And_Invariant_With_Explicit_Access_To_Both_Languages()
    {
        var contentKey = await SetupBaselineContentAsync();
        var user = await CreateRestrictedUserAsync(
            hasAccessToAllLanguages: false,
            allowedCultures: [EnglishCulture, DanishCulture],
            hasAccessToInvariantForVariant: true,
            suffix: "BothLang");

        var updateResult = await UpdateAndPublishAsync(
            contentKey,
            user.Key,
            [
                new PropertyValueModel { Alias = "variantValue", Value = "Updated EN value", Culture = EnglishCulture },
                new PropertyValueModel { Alias = "variantValue", Value = "Updated DA value", Culture = DanishCulture },
                new PropertyValueModel { Alias = "invariantValue", Value = "Updated invariant value" },
            ],
            EnglishCulture,
            DanishCulture);
        Assert.IsTrue(updateResult.Success);

        var content = ContentService.GetById(contentKey)!;
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Updated EN value", content.GetValue<string>("variantValue", EnglishCulture));
            Assert.AreEqual("Updated DA value", content.GetValue<string>("variantValue", DanishCulture));
            Assert.AreEqual("Updated invariant value", content.GetValue<string>("invariantValue"));

            Assert.AreEqual("Updated EN value", content.GetValue<string>("variantValue", EnglishCulture, published: true));
            Assert.AreEqual("Updated DA value", content.GetValue<string>("variantValue", DanishCulture, published: true));
            Assert.AreEqual("Updated invariant value", content.GetValue<string>("invariantValue", published: true));
        });
    }

    [TestCase(EnglishCulture, DanishCulture)]
    [TestCase(DanishCulture, EnglishCulture)]
    public async Task Can_Edit_And_Publish_Only_The_Language_The_User_Has_Access_To(string allowedCulture, string otherCulture)
    {
        var contentKey = await SetupBaselineContentAsync();
        var user = await CreateRestrictedUserAsync(
            hasAccessToAllLanguages: false,
            allowedCultures: [allowedCulture],
            hasAccessToInvariantForVariant: true,
            suffix: $"Single-{allowedCulture}");

        var updateResult = await UpdateAndPublishAsync(
            contentKey,
            user.Key,
            [
                new PropertyValueModel { Alias = "variantValue", Value = "Updated allowed value", Culture = allowedCulture },
                new PropertyValueModel { Alias = "variantValue", Value = "Attempted other value", Culture = otherCulture },
                new PropertyValueModel { Alias = "invariantValue", Value = "Updated invariant value" },
            ],
            allowedCulture);
        Assert.IsTrue(updateResult.Success);

        var content = ContentService.GetById(contentKey)!;
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Updated allowed value", content.GetValue<string>("variantValue", allowedCulture));
            Assert.AreEqual(InitialValueFor(otherCulture), content.GetValue<string>("variantValue", otherCulture));
            Assert.AreEqual("Updated invariant value", content.GetValue<string>("invariantValue"));

            Assert.AreEqual("Updated allowed value", content.GetValue<string>("variantValue", allowedCulture, published: true));
            Assert.AreEqual("Updated invariant value", content.GetValue<string>("invariantValue", published: true));
        });
    }

    [Test]
    public async Task Cannot_Edit_A_Language_The_User_Does_Not_Have_Access_To()
    {
        var contentKey = await SetupBaselineContentAsync();
        var user = await CreateRestrictedUserAsync(
            hasAccessToAllLanguages: false,
            allowedCultures: [EnglishCulture],
            hasAccessToInvariantForVariant: true,
            suffix: "EnOnlyEdit");

        var updateResult = await UpdateAndPublishAsync(
            contentKey,
            user.Key,
            [
                new PropertyValueModel { Alias = "variantValue", Value = "Updated EN value", Culture = EnglishCulture },
                new PropertyValueModel { Alias = "variantValue", Value = "Attempted DA value", Culture = DanishCulture },
                new PropertyValueModel { Alias = "invariantValue", Value = "Updated invariant value" },
            ],
            EnglishCulture);

        // the operation reports success - the disallowed edit is silently discarded, not rejected
        Assert.IsTrue(updateResult.Success);

        var content = ContentService.GetById(contentKey)!;
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Updated EN value", content.GetValue<string>("variantValue", EnglishCulture, published: true));
            Assert.AreEqual(InitialValueFor(DanishCulture), content.GetValue<string>("variantValue", DanishCulture));
        });
    }

    [Test]
    public async Task Cannot_Edit_Invariant_Property_Without_Invariant_Access()
    {
        var contentKey = await SetupBaselineContentAsync();
        var user = await CreateRestrictedUserAsync(
            hasAccessToAllLanguages: true,
            allowedCultures: [],
            hasAccessToInvariantForVariant: false,
            suffix: "NoInvariantEdit");

        var updateResult = await UpdateAndPublishAsync(
            contentKey,
            user.Key,
            [
                new PropertyValueModel { Alias = "variantValue", Value = "Updated EN value", Culture = EnglishCulture },
                new PropertyValueModel { Alias = "variantValue", Value = "Updated DA value", Culture = DanishCulture },
                new PropertyValueModel { Alias = "invariantValue", Value = "Attempted invariant value" },
            ],
            EnglishCulture,
            DanishCulture);
        Assert.IsTrue(updateResult.Success);

        var content = ContentService.GetById(contentKey)!;
        Assert.Multiple(() =>
        {
            // language access is unaffected - both cultures update and publish normally
            Assert.AreEqual("Updated EN value", content.GetValue<string>("variantValue", EnglishCulture, published: true));
            Assert.AreEqual("Updated DA value", content.GetValue<string>("variantValue", DanishCulture, published: true));

            // the invariant edit is silently discarded
            Assert.AreEqual("Initial invariant value", content.GetValue<string>("invariantValue"));
            Assert.AreEqual("Initial invariant value", content.GetValue<string>("invariantValue", published: true));
        });
    }

    [Test]
    public async Task Cannot_Publish_Pending_Invariant_Change_Without_Invariant_Access()
    {
        var contentKey = await SetupBaselineContentAsync();

        // an authorized user makes a pending (unpublished) invariant edit. Note that every property must be
        // resent on every update - any property alias missing from the model has its value cleared - so the
        // variant values are resent unchanged here.
        var pendingEditResult = await ContentEditingService.UpdateAsync(
            contentKey,
            new ContentUpdateModel
            {
                Properties =
                [
                    new PropertyValueModel { Alias = "variantValue", Value = InitialValueFor(EnglishCulture), Culture = EnglishCulture },
                    new PropertyValueModel { Alias = "variantValue", Value = InitialValueFor(DanishCulture), Culture = DanishCulture },
                    new PropertyValueModel { Alias = "invariantValue", Value = "Pending invariant value" },
                ],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(pendingEditResult.Success);

        var restrictedUser = await CreateRestrictedUserAsync(
            hasAccessToAllLanguages: true,
            allowedCultures: [],
            hasAccessToInvariantForVariant: false,
            suffix: "NoInvariantPublish");

        // the restricted user publishes English without attempting to change any property value - only the
        // pending invariant change is at stake here
        var publishResult = await UpdateAndPublishAsync(
            contentKey,
            restrictedUser.Key,
            [
                new PropertyValueModel { Alias = "variantValue", Value = InitialValueFor(EnglishCulture), Culture = EnglishCulture },
                new PropertyValueModel { Alias = "variantValue", Value = InitialValueFor(DanishCulture), Culture = DanishCulture },
                new PropertyValueModel { Alias = "invariantValue", Value = "Pending invariant value" },
            ],
            EnglishCulture);

        // publishing the culture succeeds - it's only the pending invariant change that is not promoted
        Assert.IsTrue(publishResult.Success, $"Status: {publishResult.Status}");

        var content = ContentService.GetById(contentKey)!;
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Initial invariant value", content.GetValue<string>("invariantValue", published: true));
            Assert.AreEqual("Pending invariant value", content.GetValue<string>("invariantValue", published: false));
        });

        // an authorized user can still land the pending change afterwards - it was preserved, not lost
        var followUpPublishResult = await UpdateAndPublishAsync(
            contentKey,
            Constants.Security.SuperUserKey,
            [
                new PropertyValueModel { Alias = "variantValue", Value = InitialValueFor(EnglishCulture), Culture = EnglishCulture },
                new PropertyValueModel { Alias = "variantValue", Value = InitialValueFor(DanishCulture), Culture = DanishCulture },
                new PropertyValueModel { Alias = "invariantValue", Value = "Pending invariant value" },
            ],
            EnglishCulture);
        Assert.IsTrue(followUpPublishResult.Success);

        content = ContentService.GetById(contentKey)!;
        Assert.AreEqual("Pending invariant value", content.GetValue<string>("invariantValue", published: true));
    }

    [Test]
    public async Task HasAccessToInvariantForVariant_Is_Not_Consulted_For_Invariant_Content_Type()
    {
        var contentType = ContentTypeBuilder.CreateSimpleContentType("invariantOnlyCombined", "Invariant Only Combined");
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants = [new VariantModel { Name = "Invariant Content" }],
                Properties = [new PropertyValueModel { Alias = "title", Value = "Initial title" }],
            },
            [],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);
        var contentKey = createResult.Result.Content!.Key;

        var user = await CreateRestrictedUserAsync(
            hasAccessToAllLanguages: true,
            allowedCultures: [],
            hasAccessToInvariantForVariant: false,
            suffix: "InvariantOnlyEdit");

        var updateResult = await ContentEditingService.UpdateAndPublishAsync(
            contentKey,
            new ContentUpdateModel
            {
                Variants = [new VariantModel { Name = "Updated Invariant Content" }],
                Properties = [new PropertyValueModel { Alias = "title", Value = "Updated title" }],
            },
            [],
            user.Key);
        Assert.IsTrue(updateResult.Success);

        var content = ContentService.GetById(contentKey)!;
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Updated title", content.GetValue<string>("title", published: true));
            Assert.AreEqual("Updated Invariant Content", content.PublishName);
        });
    }

    private static string InitialValueFor(string culture) => culture == EnglishCulture ? "Initial EN value" : "Initial DA value";

    private async Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAndPublishAsync(
        Guid contentKey,
        Guid userKey,
        IEnumerable<PropertyValueModel> properties,
        params string[] culturesToPublish)
        => await ContentEditingService.UpdateAndPublishAsync(
            contentKey,
            new ContentUpdateModel { Properties = properties.ToList() },
            culturesToPublish,
            userKey);

    private async Task<Guid> SetupBaselineContentAsync()
    {
        var contentType = await SetupContentTypeAsync();

        var createResult = await ContentEditingService.CreateAndPublishAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants =
                [
                    new VariantModel { Culture = EnglishCulture, Name = "Initial English Name" },
                    new VariantModel { Culture = DanishCulture, Name = "Initial Danish Name" },
                ],
                Properties =
                [
                    new PropertyValueModel { Alias = "variantValue", Value = InitialValueFor(EnglishCulture), Culture = EnglishCulture },
                    new PropertyValueModel { Alias = "variantValue", Value = InitialValueFor(DanishCulture), Culture = DanishCulture },
                    new PropertyValueModel { Alias = "invariantValue", Value = "Initial invariant value" },
                ],
            },
            [EnglishCulture, DanishCulture],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        return createResult.Result.Content!.Key;
    }

    private async Task<IContentType> SetupContentTypeAsync()
    {
        var danish = new LanguageBuilder()
            .WithCultureInfo(DanishCulture)
            .Build();
        await LanguageService.CreateAsync(danish, Constants.Security.SuperUserKey);

        var key = Guid.NewGuid();
        var contentType = new ContentTypeBuilder()
            .WithAlias("variantInvariantAccessContent")
            .WithName("Variant Invariant Access Content")
            .WithKey(key)
            .WithContentVariation(ContentVariation.Culture)
            .AddAllowedContentType()
            .WithKey(key)
            .WithAlias("variantInvariantAccessContent")
            .Done()
            .AddPropertyType()
            .WithAlias("variantValue")
            .WithVariations(ContentVariation.Culture)
            .WithMandatory(true)
            .Done()
            .AddPropertyType()
            .WithAlias("invariantValue")
            .WithVariations(ContentVariation.Nothing)
            .WithMandatory(true)
            .Done()
            .Build();

        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        return contentType;
    }

    private async Task<IUser> CreateRestrictedUserAsync(
        bool hasAccessToAllLanguages,
        IEnumerable<string> allowedCultures,
        bool hasAccessToInvariantForVariant,
        string suffix)
    {
        var userGroup = UserGroupBuilder.CreateUserGroup(suffix: suffix);
        userGroup.HasAccessToAllLanguages = hasAccessToAllLanguages;
        userGroup.HasAccessToInvariantForVariant = hasAccessToInvariantForVariant;

        foreach (var culture in allowedCultures)
        {
            var language = await LanguageService.GetAsync(culture);
            Assert.IsNotNull(language, $"Expected language {culture} to exist.");
            userGroup.AddAllowedLanguage(language!.Id);
        }

        var createGroupResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(createGroupResult.Success);

        var user = UserService.CreateUserWithIdentity($"test-{suffix}", $"test-{suffix}@test.com".ToLowerInvariant());
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        return user;
    }
}
