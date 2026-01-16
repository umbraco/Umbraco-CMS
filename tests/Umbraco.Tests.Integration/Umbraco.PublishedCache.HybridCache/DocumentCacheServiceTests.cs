// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
internal sealed partial class DocumentCacheServiceTests : UmbracoIntegrationTestWithContent
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        RegisterServices(builder);

        // Use JSON to allow easier verification of data.
        builder.Services.PostConfigure<NuCacheSettings>(options => options.NuCacheSerializerType = NuCacheSerializerType.JSON);
    }

    public static void ConfigureMessagePackSerialization(IUmbracoBuilder builder)
    {
        RegisterServices(builder);

        // Use MessagePack (the default) - don't override to JSON.
        builder.Services.PostConfigure<NuCacheSettings>(options => options.NuCacheSerializerType = NuCacheSerializerType.MessagePack);
    }

    private static void RegisterServices(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
    }

    private ISqlContext SqlContext => GetRequiredService<ISqlContext>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [Test]
    public void Rebuild_Creates_Invariant_Document_Database_Cache_Records_For_Document_Type()
    {
        // Arrange - Content is created in base class Setup()
        // The base class creates: Textpage, Subpage, Subpage2, Subpage3 (all using ContentType)

        // - publish the root page to ensure we have published and draft content
        ContentService.Publish(Textpage, ["*"]);

        // Act - Call Rebuild for the document type
        DocumentCacheService.Rebuild([ContentType.Id]);

        // Assert - Verify cmsContentNu table has records for the content items
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>();

            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            var draftDtos = dtos.Where(d => !d.Published).ToList();
            var publishedDtos = dtos.Where(d => d.Published).ToList();

            // Verify we have draft records for non-trashed content
            // Textpage, Subpage, Subpage2, Subpage3 should have draft cache entries
            Assert.That(draftDtos, Has.Count.GreaterThanOrEqualTo(4), "Expected at least 4 draft cache records");

            // Verify we have published records for published content
            // Textpage, Subpage, Subpage2, Subpage3 should have draft cache entries
            Assert.AreEqual(1, publishedDtos.Count, "Expected 1 published cache record");

            // Verify specific content items have cache entries
            var nodeIds = draftDtos.Select(d => d.NodeId).ToList();
            Assert.That(nodeIds, Does.Contain(Textpage.Id), "Textpage should have draft cache entry");
            Assert.That(nodeIds, Does.Contain(Subpage.Id), "Subpage should have draft cache entry");
            Assert.That(nodeIds, Does.Contain(Subpage2.Id), "Subpage2 should have draft cache entry");
            Assert.That(nodeIds, Does.Contain(Subpage3.Id), "Subpage3 should have draft cache entry");

            nodeIds = [.. publishedDtos.Select(d => d.NodeId)];
            Assert.That(nodeIds, Does.Contain(Textpage.Id), "Textpage should have published cache entry");
            Assert.That(nodeIds, Has.No.Member(Subpage.Id), "Subpage should have not have published cache entry");

            // Verify cache data is not empty
            var textpageDto = draftDtos.Single(d => d.NodeId == Textpage.Id);
            Assert.That(textpageDto.Data, Is.Not.Null.And.Not.Empty, "Cache data should not be empty");

            // Verify the cached data is correct across refactorings and optimizations.
            const string ExpectedJson = "{\"pd\":{\"title\":[{\"c\":\"\",\"s\":\"\",\"v\":\"Welcome to our Home page\"}],\"bodyText\":[{\"c\":\"\",\"s\":\"\",\"v\":\"This is the welcome message on the first page\"}],\"author\":[{\"c\":\"\",\"s\":\"\",\"v\":\"John Doe\"}]},\"cd\":{},\"us\":\"textpage\"}";
            Assert.That(textpageDto.Data, Is.EqualTo(ExpectedJson), "Cache data does not match expected JSON");
        }
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureMessagePackSerialization))]
    public void Rebuild_Creates_Invariant_Document_Database_Cache_Records_For_Document_Type_With_Message_Pack_Serialization()
    {
        // Arrange - Content is created in base class Setup()
        // The base class creates: Textpage using ContentType

        // - publish the root page
        ContentService.Publish(Textpage, ["*"]);

        // Act - Call Rebuild for the document type
        DocumentCacheService.Rebuild([ContentType.Id]);

        // Assert - Verify cmsContentNu table has records for the content items
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>();

            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            var publishedDtos = dtos.Where(d => d.Published).ToList();

            // Verify cache data is not empty
            var textpageDto = publishedDtos.Single(d => d.NodeId == Textpage.Id);
            Assert.That(textpageDto.RawData, Is.Not.Null.And.Not.Empty, "Cache data should not be empty");

            // Verify the cached data is correct across refactorings and optimizations.
            const string ExpectedMessagePack = "ktRif8YAAAB48A6Tg6V0aXRsZZGToKC4V2VsY29tZSB0byBvdXIgSAwA0HBhZ2WoYm9keVRleHQmAPMA2S1UaGlzIGlzIHRoZSB3MwChbWVzc2FnZSBvbhcAUWZpcnN0PABwpmF1dGhvcjoA8ASoSm9obiBEb2WAqHRleHRwYWdl";
            var rawDataString = Convert.ToBase64String(textpageDto.RawData!);
            Assert.That(rawDataString, Is.EqualTo(ExpectedMessagePack), "Cache data does not match expected MessagePack serialization");
        }
    }

    [Test]
    public async Task Rebuild_Creates_Variant_Document_Database_Cache_Records_For_Document_Type()
    {
        // Arrange - Create languages
        var langEn = new LanguageBuilder()
            .WithCultureInfo("en-US")
            .WithIsDefault(true)
            .Build();
        await LanguageService.CreateAsync(langEn, Constants.Security.SuperUserKey);

        var langDa = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);

        // Create a variant content type with a variant property
        var variantContentType = new ContentTypeBuilder()
            .WithAlias("variantPage")
            .WithName("Variant Page")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyGroup()
                .WithName("Content")
                .WithAlias("content")
                .WithSortOrder(1)
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithAlias("pageTitle")
                    .WithName("Page Title")
                    .WithVariations(ContentVariation.Culture)
                    .WithSortOrder(1)
                    .Done()
                .Done()
            .Build();
        variantContentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(variantContentType, Constants.Security.SuperUserKey);

        // Create content with culture-specific values
        var variantContent = new ContentBuilder()
            .WithContentType(variantContentType)
            .WithCultureName(langEn.IsoCode, "English Page")
            .WithCultureName(langDa.IsoCode, "Danish Page")
            .Build();
        variantContent.SetValue("pageTitle", "English Title", culture: langEn.IsoCode);
        variantContent.SetValue("pageTitle", "Danish Title", culture: langDa.IsoCode);
        ContentService.Save(variantContent);

        // Act - Rebuild the cache for the variant document type
        DocumentCacheService.Rebuild([variantContentType.Id]);

        // Assert - Verify cmsContentNu table has records for the content items
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == variantContent.Id && !x.Published);

            var dto = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql).FirstOrDefault();
            Assert.That(dto, Is.Not.Null, "Variant content should have a cache entry");
            Assert.That(dto!.Data, Is.Not.Null.And.Not.Empty, "Cache data should not be empty");

            // Verify the cached data includes the variant property with culture-specific values
            Assert.That(dto.Data, Does.Contain("\"pageTitle\":["), "Cache should include pageTitle property");
            Assert.That(dto.Data, Does.Contain("\"c\":\"en-US\""), "Cache should include English culture");
            Assert.That(dto.Data, Does.Contain("\"c\":\"da-DK\""), "Cache should include Danish culture");
            Assert.That(dto.Data, Does.Contain("\"v\":\"English Title\""), "Cache should include English title value");
            Assert.That(dto.Data, Does.Contain("\"v\":\"Danish Title\""), "Cache should include Danish title value");

            // Verify the culture data section includes both cultures
            Assert.That(dto.Data, Does.Contain("\"cd\":{"), "Cache should include culture data section");
            Assert.That(dto.Data, Does.Contain("\"en-US\":{"), "Cache should include en-US culture data");
            Assert.That(dto.Data, Does.Contain("\"da-DK\":{"), "Cache should include da-DK culture data");

            // Verify the cached data is correct across refactorings and optimizations.
            // Note: Dates are variable, so we normalize them before comparison.
            const string ExpectedJson = "{\"pd\":{\"pageTitle\":[{\"c\":\"da-DK\",\"s\":\"\",\"v\":\"Danish Title\"},{\"c\":\"en-US\",\"s\":\"\",\"v\":\"English Title\"}]},\"cd\":{\"en-US\":{\"nm\":\"English Page\",\"us\":\"english-page\",\"dt\":\"\",\"isd\":true},\"da-DK\":{\"nm\":\"Danish Page\",\"us\":\"danish-page\",\"dt\":\"\",\"isd\":true}},\"us\":\"english-page\"}";
            var actualJsonNormalized = RemoveDates(dto.Data!);

            Assert.That(actualJsonNormalized, Is.EqualTo(ExpectedJson), "Cache data does not match expected JSON");
        }
    }

    private static string RemoveDates(string input)
        => RemoveDatesFromJsonSerialization().Replace(input, @"""dt"":""""");

    [Test]
    public void Rebuild_Replaces_Existing_Document_Database_Cache_Records()
    {
        // Arrange - First rebuild to create initial records
        DocumentCacheService.Rebuild([ContentType.Id]);

        // Get initial data
        string initialData;
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == Textpage.Id && !x.Published);
            var dto = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql).FirstOrDefault();
            Assert.That(dto, Is.Not.Null);
            initialData = dto!.Data!;
        }

        // Modify content
        Textpage.SetValue("title", "Modified Title For Rebuild Test");
        ContentService.Save(Textpage);

        // Act - Rebuild again
        DocumentCacheService.Rebuild([ContentType.Id]);

        // Assert - Verify record was updated (not duplicated)
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == Textpage.Id && !x.Published);
            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            // Should have exactly one draft record (no duplicates)
            Assert.That(dtos, Has.Count.EqualTo(1), "Should have exactly one draft cache record");

            // Data should be different (updated)
            var updatedData = dtos[0].Data;
            Assert.That(updatedData, Does.Contain("Modified Title For Rebuild Test"), "Cache data should contain the modified title");
        }
    }

    [Test]
    public async Task Rebuild_Includes_Composed_Properties_In_Cache()
    {
        // Arrange - Create a composition content type with a custom property
        var compositionType = new ContentTypeBuilder()
            .WithAlias("documentComposition")
            .WithName("Document Composition")
            .AddPropertyGroup()
                .WithName("SEO")
                .WithAlias("seo")
                .WithSortOrder(1)
                .AddPropertyType()
                    .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithAlias("metaDescription")
                    .WithName("Meta Description")
                    .WithSortOrder(1)
                    .Done()
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(compositionType, Constants.Security.SuperUserKey);

        // Create a content type that uses the composition
        var composedContentType = new ContentTypeBuilder()
            .WithAlias("composedPage")
            .WithName("Composed Page")
            .AddPropertyGroup()
                .WithName("Content")
                .WithAlias("content")
                .WithSortOrder(1)
                .AddPropertyType()
                    .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithAlias("pageTitle")
                    .WithName("Page Title")
                    .WithSortOrder(1)
                    .Done()
                .Done()
            .Build();

        // Add the composition to the content type
        composedContentType.AddContentType(compositionType);
        await ContentTypeService.CreateAsync(composedContentType, Constants.Security.SuperUserKey);

        // Create content using the composed type
        var composedContent = new ContentBuilder()
            .WithName("Composed Content Item")
            .WithContentType(composedContentType)
            .WithPropertyValues(new
            {
                pageTitle = "Composed Page Title",
                metaDescription = "This is a meta description from the composition.",
            })
            .Build();
        ContentService.Save(composedContent);

        // Act - Rebuild the cache for the composed content type
        DocumentCacheService.Rebuild([composedContentType.Id]);

        // Assert - Verify the cache includes properties from both the content type AND its composition
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == composedContent.Id && !x.Published);

            var dto = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql).FirstOrDefault();
            Assert.That(dto, Is.Not.Null, "Composed content should have a cache entry");
            Assert.That(dto!.Data, Is.Not.Null.And.Not.Empty, "Cache data should not be empty");

            // Verify the cached data includes properties from the composition (metaDescription)
            Assert.That(dto.Data, Does.Contain("\"metaDescription\":["), "Cache should include metaDescription from composition");

            // Verify the cached data includes direct properties (pageTitle)
            Assert.That(dto.Data, Does.Contain("\"pageTitle\":["), "Cache should include pageTitle from direct type");

            // Verify the cached data is correct across refactorings and optimizations.
            const string ExpectedJson = "{\"pd\":{\"pageTitle\":[{\"c\":\"\",\"s\":\"\",\"v\":\"Composed Page Title\"}],\"metaDescription\":[{\"c\":\"\",\"s\":\"\",\"v\":\"This is a meta description from the composition.\"}]},\"cd\":{},\"us\":\"composed-content-item\"}";
            Assert.That(dto.Data, Is.EqualTo(ExpectedJson), "Cache data does not match expected JSON");
        }
    }

    [GeneratedRegex(@"""dt"":""[^""]+""")]
    private static partial Regex RemoveDatesFromJsonSerialization();
}
