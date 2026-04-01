// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing.SeedProfiles;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Base class for ContentEditingService integration tests that use the snapshot pattern.
///     Gets a shared host from <see cref="ServiceTestsSetUpFixture"/> and swaps to a
///     seeded database per test via <see cref="Testing.Fixtures.TestDatabaseSwapper"/>.
/// </summary>
[TestFixture]
[DatabaseSeedProfile(typeof(InstalledSchemaWithContentSeed))]
public abstract class ContentEditingServiceSnapshotTestBase
{
    private ServiceTestsSetUpFixture Fixture => ServiceTestsSetUpFixture.Instance;

    protected IServiceProvider Services => Fixture.SharedServices;

    protected T GetRequiredService<T>() => Services.GetRequiredService<T>();

    protected IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    protected IContentBlueprintEditingService ContentBlueprintEditingService => GetRequiredService<IContentBlueprintEditingService>();

    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    protected IUserService UserService => GetRequiredService<IUserService>();

    protected IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    protected IFileService FileService => GetRequiredService<IFileService>();

    protected IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    protected IIOHelper IOHelper => GetRequiredService<IIOHelper>();

    protected ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    // Content from the seed profile — populated after each database swap for
    // tests that use InstalledSchemaWithContentSeed
    protected Content Textpage { get; private set; }

    protected Content Subpage { get; private set; }

    protected Content Subpage2 { get; private set; }

    protected Content Subpage3 { get; private set; }

    protected Content Trashed { get; private set; }

    protected ContentType ContentType { get; private set; }

    [SetUp]
    public virtual async Task SetUp()
    {
        // Swap to a seeded database per test (snapshot-aware)
        var seedAttr = GetType().GetCustomAttribute<DatabaseSeedProfileAttribute>();
        if (seedAttr is not null)
        {
            var profile = (ITestDatabaseSeedProfile)Activator.CreateInstance(seedAttr.SeedProfileType)!;
            await Fixture.SwapToSeededDatabaseAsync(profile);
        }
        else
        {
            await Fixture.SwapToFreshDatabaseAsync();
        }

        // Re-set StaticServiceProvider for obsolete constructors
        StaticServiceProvider.Instance = Services;

        // Reset ContentSettings to defaults — previous tests may have mutated them
        // and since we share a host, the mutations persist across tests
        var contentSettings = GetRequiredService<IOptionsMonitor<ContentSettings>>().CurrentValue;
        contentSettings.DisableDeleteWhenReferenced = ContentSettings.StaticDisableDeleteWhenReferenced;
        contentSettings.DisableUnpublishWhenReferenced = ContentSettings.StaticDisableUnpublishWhenReferenced;

        // Re-fetch seeded content from the freshly-restored database
        if (seedAttr?.SeedProfileType == typeof(InstalledSchemaWithContentSeed))
        {
            PopulateSeededContent();
        }
    }

    private void PopulateSeededContent()
    {
        ContentType = (ContentType)ContentTypeService.Get("umbTextpage");
        Textpage = (Content)ContentService.GetById(InstalledSchemaWithContentSeed.TextpageKey);
        Subpage = (Content)ContentService.GetById(InstalledSchemaWithContentSeed.SubPageKey);
        Subpage2 = (Content)ContentService.GetById(InstalledSchemaWithContentSeed.SubPage2Key);
        Subpage3 = (Content)ContentService.GetById(InstalledSchemaWithContentSeed.SubPage3Key);
        Trashed = (Content)ContentService.GetById(InstalledSchemaWithContentSeed.TrashedKey);
    }

    #region Helper methods migrated from ContentEditingServiceTestsBase

    protected IContentType CreateInvariantContentType(params ITemplate[] templates)
    {
        var contentTypeBuilder = new ContentTypeBuilder()
            .WithAlias("invariantTest")
            .WithName("Invariant Test")
            .WithContentVariation(ContentVariation.Nothing)
            .AddPropertyType()
                .WithAlias("title")
                .WithName("Title")
                .WithMandatory(true)
                .WithVariations(ContentVariation.Nothing)
                .Done()
            .AddPropertyType()
                .WithAlias("text")
                .WithName("Text")
                .WithVariations(ContentVariation.Nothing)
                .Done()
            .AddPropertyType()
                .WithAlias("label")
                .WithName("Label")
                .WithDataTypeId(Constants.DataTypes.LabelString)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
                .WithVariations(ContentVariation.Nothing)
                .Done();

        foreach (var template in templates)
        {
            contentTypeBuilder
                .AddAllowedTemplate()
                .WithId(template.Id)
                .WithAlias(template.Alias)
                .WithName(template.Name ?? template.Alias)
                .Done();
        }

        if (templates.Any())
        {
            contentTypeBuilder.WithDefaultTemplateId(templates.First().Id);
        }

        var contentType = contentTypeBuilder.Build();
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        return contentType;
    }

    protected async Task<IContentType> CreateVariantContentType(ContentVariation variation = ContentVariation.Culture, bool variantTitleAsMandatory = true)
    {
        var language = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("cultureVariationTest")
            .WithName("Culture Variation Test")
            .WithContentVariation(variation)
            .AddPropertyType()
            .WithAlias("variantTitle")
            .WithName("Variant Title")
            .WithMandatory(variantTitleAsMandatory)
            .WithVariations(variation)
            .Done()
            .AddPropertyType()
                .WithAlias("invariantTitle")
                .WithName("Invariant Title")
                .WithVariations(ContentVariation.Nothing)
                .Done()
            .AddPropertyType()
                .WithAlias("variantLabel")
                .WithName("Variant Label")
                .WithDataTypeId(Constants.DataTypes.LabelString)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
                .WithVariations(variation)
                .Done()
            .Build();
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);
        return contentType;
    }

    protected async Task<IContent> CreateInvariantContent(params ITemplate[] templates)
    {
        var contentType = CreateInvariantContentType(templates);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Initial Name" }
            ],
            TemplateKey = templates.FirstOrDefault()?.Key,
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The initial title" },
                new PropertyValueModel { Alias = "text", Value = "The initial text" }
            ],
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    protected async Task<IContent> CreateCultureVariantContent()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The initial invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial Danish title", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Initial English Name" },
                new VariantModel { Culture = "da-DK", Name = "Initial Danish Name" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    protected async Task<IContent> CreateSegmentVariantContent()
    {
        var contentType = await CreateVariantContentType(ContentVariation.Segment);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The initial invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial default title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial seg-1 title", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial seg-2 title", Segment = "seg-2" }
            ],
            Variants =
            [
                new VariantModel { Segment = null, Name = "The Name" },
                new VariantModel { Segment = "seg-1", Name = "The Name" },
                new VariantModel { Segment = "seg-2", Name = "The Name" }
            ],
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    protected async Task<IContent> CreateCultureAndSegmentVariantContent(ContentVariation otherTitleVariation)
    {
        var contentType = await CreateVariantContentType(ContentVariation.CultureAndSegment);
        var propertyType = contentType.PropertyTypes.First(pt => pt.Alias == "invariantTitle");
        propertyType.Alias = "otherTitle";
        propertyType.Variations = otherTitleVariation;
        ContentTypeService.Save(contentType);

        IEnumerable<PropertyValueModel> otherTitleValues = otherTitleVariation switch
        {
            ContentVariation.Culture =>
            [
                new PropertyValueModel { Alias = "otherTitle", Value = "The initial other English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "otherTitle", Value = "The initial other Danish title", Culture = "da-DK" },
            ],
            ContentVariation.Segment =>
            [
                new PropertyValueModel { Alias = "otherTitle", Value = "The initial other default title" },
                new PropertyValueModel { Alias = "otherTitle", Value = "The initial other seg-1 title", Segment = "seg-1" },
                new PropertyValueModel { Alias = "otherTitle", Value = "The initial other seg-2 title", Segment = "seg-2" }
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(otherTitleVariation))
        };

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Properties = otherTitleValues.Union([
                new () { Alias = "variantTitle", Value = "The initial title in English", Culture = "en-US" },
                new () { Alias = "variantTitle", Value = "The initial seg-1 title in English", Culture = "en-US", Segment = "seg-1" },
                new () { Alias = "variantTitle", Value = "The initial seg-2 title in English", Culture = "en-US", Segment = "seg-2" },
                new () { Alias = "variantTitle", Value = "The initial title in Danish", Culture = "da-DK" },
                new () { Alias = "variantTitle", Value = "The initial seg-1 title in Danish", Culture = "da-DK", Segment = "seg-1" },
                new () { Alias = "variantTitle", Value = "The initial seg-2 title in Danish", Culture = "da-DK", Segment = "seg-2" }
            ]),
            Variants =
            [
                new VariantModel { Name = "The Name", Culture = "en-US", Segment = null },
                new VariantModel { Name = "The Name", Culture = "en-US", Segment = "seg-1" },
                new VariantModel { Name = "The Name", Culture = "en-US", Segment = "seg-2" },
                new VariantModel { Name = "The Name", Culture = "da-DK", Segment = null },
                new VariantModel { Name = "The Name", Culture = "da-DK", Segment = "seg-1" },
                new VariantModel { Name = "The Name", Culture = "da-DK", Segment = "seg-2" },
            ],
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    #endregion
}
