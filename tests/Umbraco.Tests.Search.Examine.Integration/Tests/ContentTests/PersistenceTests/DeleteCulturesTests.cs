using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Search.Core.Persistence;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.PersistenceTests;

[TestFixture]
public class DeleteCulturesTests : TestBase
{
    private IIndexDocumentRepository IndexDocumentRepository => GetRequiredService<IIndexDocumentRepository>();

    private IIndexDocumentService IndexDocumentService => GetRequiredService<IIndexDocumentService>();

    [TestCase(true)]
    [TestCase(false)]
    public async Task DeletingLanguage_RemovesVariantDocumentFromCache(bool publish)
    {
        await CreateVariantContent(publish);

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            IndexDocument? doc = await IndexDocumentRepository.GetAsync(RootKey, publish);
            Assert.That(doc, Is.Not.Null);
            Assert.That(doc!.Fields.Any(f => f.Culture == "da-DK"), Is.True);
        }

        await LanguageService.DeleteAsync("da-DK", Constants.Security.SuperUserKey);
        await WaitForUpdatesAsync();

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // The variant document should be re-created by the rebuild (without the deleted culture's fields,
            // since the language no longer exists in the system)
            IndexDocument? docAfter = await IndexDocumentRepository.GetAsync(RootKey, publish);
            Assert.That(docAfter, Is.Not.Null, "Document should be re-created by rebuild");
            Assert.That(docAfter!.Fields.Any(f => f.Culture == "da-DK"), Is.False, "Deleted culture fields should not be present after rebuild");
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task DeletingLanguage_PreservesInvariantDocumentCache(bool publish)
    {
        await CreateInvariantAndVariantContent(publish);

        IndexField[] invariantFieldsBefore;
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            IndexDocument? invariantDoc = await IndexDocumentRepository.GetAsync(ChildKey, publish);
            Assert.That(invariantDoc, Is.Not.Null);
            invariantFieldsBefore = invariantDoc!.Fields;
        }

        await LanguageService.DeleteAsync("da-DK", Constants.Security.SuperUserKey);
        await WaitForUpdatesAsync();

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            // The invariant document should still be in cache with the same fields (not wiped by the
            // language deletion). This is the key assertion - previously, ClearDocumentIndexCache()
            // would have wiped it too, forcing an expensive re-collection from content services.
            IndexDocument? invariantDocAfter = await IndexDocumentRepository.GetAsync(ChildKey, publish);
            Assert.That(invariantDocAfter, Is.Not.Null, "Invariant document cache should be preserved after language deletion");
            Assert.That(invariantDocAfter!.Fields.Length, Is.EqualTo(invariantFieldsBefore.Length), "Invariant document fields should be unchanged");
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task DeletingLanguage_PreservesDocumentCacheForNonDeletedCultures(bool publish)
    {
        await CreateVariantContentWithThreeCultures(publish);

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            IndexDocument? doc = await IndexDocumentRepository.GetAsync(RootKey, publish);
            Assert.That(doc, Is.Not.Null);
            Assert.That(doc!.Fields.Any(f => f.Culture == "fr-FR"), Is.True);
        }

        // Delete ja-JP only; en-US and fr-FR should remain
        await LanguageService.DeleteAsync("ja-JP", Constants.Security.SuperUserKey);
        await WaitForUpdatesAsync();

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            IndexDocument? docAfter = await IndexDocumentRepository.GetAsync(RootKey, publish);
            Assert.That(docAfter, Is.Not.Null, "Document should be re-created by rebuild");
            Assert.That(docAfter!.Fields.Any(f => f.Culture == "ja-JP"), Is.False, "Deleted culture fields should not be present after rebuild");
            Assert.That(docAfter.Fields.Any(f => f.Culture == "fr-FR"), Is.True, "Non-deleted culture fields should still be present");
        }
    }

    [Test]
    public async Task DeleteAllPresentCultures_CleansUpIndexDocuments()
    {
        var documentKey = Guid.NewGuid();

        // Insert a document that only has culture-specific fields (no invariant)
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            var document = new IndexDocument
            {
                Key = documentKey,
                Published = false,
                Fields =
                [
                    new IndexField("title", new IndexValue { Texts = ["Danish Title"] }, "da-DK", null),
                ],
            };
            await IndexDocumentRepository.AddAsync(document);
        }

        // Verify the index document
        using (IScope scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            IndexDocument? doc = await IndexDocumentRepository.GetAsync(documentKey, false);
            Assert.That(doc, Is.Not.Null, "Document should exist after adding");

            Sql<ISqlContext> sql = scope.Database.SqlContext.Sql()
                .Select<IndexDocumentDto>()
                .From<IndexDocumentDto>()
                .Where<IndexDocumentDto>(x => x.Key == documentKey);
            List<IndexDocumentDto> documents = await scope.Database.FetchAsync<IndexDocumentDto>(sql);
            Assert.That(documents, Has.Count.EqualTo(1), "Document should exist");
        }

        // Remove the only culture, which should also clean up document
        await IndexDocumentService.DeleteCulturesAsync(["da-DK"]);

        // Verify the document is gone (not just that GetAsync returns null)
        using (IScope scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            Sql<ISqlContext> sql = scope.Database.SqlContext.Sql()
                .Select<IndexDocumentDto>()
                .From<IndexDocumentDto>()
                .Where<IndexDocumentDto>(x => x.Key == documentKey);
            List<IndexDocumentDto> documents = await scope.Database.FetchAsync<IndexDocumentDto>(sql);
            Assert.That(documents, Has.Count.EqualTo(0), "Document should be cleaned up");
        }
    }

    [Test]
    public async Task DeleteCultures_CanHandleMultipleSqlPages()
    {
        var documentKeys = new List<Guid>();

        // Insert a document that only has culture-specific fields (no invariant)
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            for (var i = 0; i < 1100; i++)
            {
                // Create a mix of published and unpublished documents for multiple cultures (including invariant).
                var published = i % 2 == 0;
                documentKeys.Add(Guid.NewGuid());
                var document = new IndexDocument
                {
                    Key = documentKeys.Last(),
                    Published = published,
                    Fields =
                    [
                        new IndexField("title", new IndexValue { Texts = ["Invariant Title"] }, null, null),
                        new IndexField("title", new IndexValue { Texts = ["Danish Title"] }, "da-DK", null),
                        new IndexField("title", new IndexValue { Texts = ["English Title"] }, "en-US", null),
                    ],
                };
                await IndexDocumentRepository.AddAsync(document);
            }
        }

        // Remove the culture
        await IndexDocumentService.DeleteCulturesAsync(["da-DK"]);

        // Verify that all items have been cleaned up
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            foreach (Guid documentKey in documentKeys)
            {
                var published = documentKeys.IndexOf(documentKey) % 2 == 0;
                IndexDocument? docAfter = await IndexDocumentRepository.GetAsync(documentKey, published);
                Assert.That(docAfter, Is.Not.Null, "Document should still exist after removing the language");
                Assert.Multiple(() =>
                {
                    Assert.That(docAfter!.Fields.Any(f => f.Culture == "da-DK"), Is.False);
                    Assert.That(docAfter.Fields.Any(f => f.Culture == "en-US"), Is.True);
                    Assert.That(docAfter.Fields.Any(f => f.Culture == null), Is.True);
                });
            }
        }
    }

    private async Task CreateVariantContent(bool publish)
    {
        ILanguage langDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        ILanguage langJp = new LanguageBuilder()
            .WithCultureInfo("ja-JP")
            .Build();

        await LanguageService.CreateAsync(langDk, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langJp, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("variantType")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
                .WithAlias("title")
                .WithVariations(ContentVariation.Culture)
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "Root EN")
            .WithCultureName("da-DK", "Root DA")
            .WithCultureName("ja-JP", "Root JP")
            .Build();

        root.SetValue("title", "English Title", "en-US");
        root.SetValue("title", "Danish Title", "da-DK");
        root.SetValue("title", "Japanese Title", "ja-JP");

        var indexAlias = GetIndexAlias(publish);
        await WaitForIndexing(indexAlias, () =>
        {
            ContentService.Save(root);
            if (publish)
            {
                ContentService.Publish(root, ["*"]);
            }

            return Task.CompletedTask;
        });
    }

    private async Task CreateVariantContentWithThreeCultures(bool publish)
    {
        ILanguage langDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        ILanguage langJp = new LanguageBuilder()
            .WithCultureInfo("ja-JP")
            .Build();
        ILanguage langFr = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();

        await LanguageService.CreateAsync(langDk, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langJp, Constants.Security.SuperUserKey);
        await LanguageService.CreateAsync(langFr, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("variantType")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
                .WithAlias("title")
                .WithVariations(ContentVariation.Culture)
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "Root EN")
            .WithCultureName("da-DK", "Root DA")
            .WithCultureName("ja-JP", "Root JP")
            .WithCultureName("fr-FR", "Root FR")
            .Build();

        root.SetValue("title", "English Title", "en-US");
        root.SetValue("title", "Danish Title", "da-DK");
        root.SetValue("title", "Japanese Title", "ja-JP");
        root.SetValue("title", "French Title", "fr-FR");

        var indexAlias = GetIndexAlias(publish);
        await WaitForIndexing(indexAlias, () =>
        {
            ContentService.Save(root);
            if (publish)
            {
                ContentService.Publish(root, ["*"]);
            }

            return Task.CompletedTask;
        });
    }

    private async Task CreateInvariantAndVariantContent(bool publish)
    {
        ILanguage langDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();

        await LanguageService.CreateAsync(langDk, Constants.Security.SuperUserKey);

        // Variant content type
        IContentType variantType = new ContentTypeBuilder()
            .WithAlias("variantType")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
                .WithAlias("title")
                .WithVariations(ContentVariation.Culture)
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(variantType, Constants.Security.SuperUserKey);

        // Invariant content type
        IContentType invariantType = new ContentTypeBuilder()
            .WithAlias("invariantType")
            .WithContentVariation(ContentVariation.Nothing)
            .AddPropertyType()
                .WithAlias("title")
                .WithVariations(ContentVariation.Nothing)
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(invariantType, Constants.Security.SuperUserKey);

        var indexAlias = GetIndexAlias(publish);

        // Create variant content
        Content variantRoot = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(variantType)
            .WithCultureName("en-US", "Variant EN")
            .WithCultureName("da-DK", "Variant DA")
            .Build();

        variantRoot.SetValue("title", "English Title", "en-US");
        variantRoot.SetValue("title", "Danish Title", "da-DK");

        await WaitForIndexing(indexAlias, () =>
        {
            ContentService.Save(variantRoot);
            if (publish)
            {
                ContentService.Publish(variantRoot, ["*"]);
            }

            return Task.CompletedTask;
        });

        // Create invariant content
        Content invariantRoot = new ContentBuilder()
            .WithKey(ChildKey)
            .WithContentType(invariantType)
            .Build();

        invariantRoot.Name = "Invariant Content";
        invariantRoot.SetValue("title", "Invariant Title");

        await WaitForIndexing(indexAlias, () =>
        {
            ContentService.Save(invariantRoot);
            if (publish)
            {
                ContentService.Publish(invariantRoot, ["*"]);
            }

            return Task.CompletedTask;
        });
    }

    private static async Task WaitForUpdatesAsync() => await Task.Delay(4000);
}
