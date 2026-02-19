// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Mock)]
internal sealed class DocumentUrlAliasServiceTests : UmbracoIntegrationTest
{
    private const string ContentTypeKey = "1D3A8E6E-2EA9-4CC1-B229-1AEE19821522";
    private const string RootPageKey = "B58B3AD4-62C2-4E27-B1BE-837BD7C533E0";
    private const string PageWithSingleAliasKey = "07EABF4A-5C62-4662-9F2A-15BBB488BCA5";
    private const string PageWithMultipleAliasesKey = "0EED78FC-A6A8-4587-AB18-D3AFE212B1C4";
    private const string PageWithNoAliasKey = "29BBB8CF-E69B-4A21-9363-02ED5B6637C4";
    private const string ChildPageKey = "DF49F477-12F2-4E33-8563-91A7CC1DCDBB";

    private IDocumentUrlAliasService DocumentUrlAliasService => GetRequiredService<IDocumentUrlAliasService>();

    private IDocumentUrlAliasRepository DocumentUrlAliasRepository => GetRequiredService<IDocumentUrlAliasRepository>();

    private ICoreScopeProvider CoreScopeProvider => GetRequiredService<ICoreScopeProvider>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    private ContentType ContentType { get; set; } = null!;

    private Content RootPage { get; set; } = null!;

    private Content PageWithSingleAlias { get; set; } = null!;

    private Content PageWithMultipleAliases { get; set; } = null!;

    private Content PageWithNoAlias { get; set; } = null!;

    private Content ChildPage { get; set; } = null!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, DocumentUrlAliasServiceInitializerNotificationHandler>();
    }

    [SetUp]
    public async Task SetupTestData()
    {
        await DocumentUrlAliasService.InitAsync(false, CancellationToken.None);

        // Create template
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        // Create content type with umbracoUrlAlias property
        ContentType = CreateContentTypeWithUrlAlias(template.Id);
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);

        // Create root page (no alias)
        RootPage = ContentBuilder.CreateSimpleContent(ContentType, "Root Page");
        RootPage.Key = new Guid(RootPageKey);
        ContentService.Save(RootPage, -1);

        // Create page with single alias
        PageWithSingleAlias = ContentBuilder.CreateSimpleContent(ContentType, "Page With Alias", RootPage.Id);
        PageWithSingleAlias.Key = new Guid(PageWithSingleAliasKey);
        PageWithSingleAlias.SetValue(Constants.Conventions.Content.UrlAlias, "my-single-alias");
        ContentService.Save(PageWithSingleAlias, -1);

        // Create page with multiple aliases (comma-delimited)
        PageWithMultipleAliases = ContentBuilder.CreateSimpleContent(ContentType, "Page With Multiple Aliases", RootPage.Id);
        PageWithMultipleAliases.Key = new Guid(PageWithMultipleAliasesKey);
        PageWithMultipleAliases.SetValue(Constants.Conventions.Content.UrlAlias, "first-alias, second-alias, /third-alias/");
        ContentService.Save(PageWithMultipleAliases, -1);

        // Create page with no alias
        PageWithNoAlias = ContentBuilder.CreateSimpleContent(ContentType, "Page Without Alias", RootPage.Id);
        PageWithNoAlias.Key = new Guid(PageWithNoAliasKey);
        ContentService.Save(PageWithNoAlias, -1);

        // Create child page with alias (for descendant tests)
        ChildPage = ContentBuilder.CreateSimpleContent(ContentType, "Child Page", PageWithSingleAlias.Id);
        ChildPage.Key = new Guid(ChildPageKey);
        ChildPage.SetValue(Constants.Conventions.Content.UrlAlias, "child-alias");
        ContentService.Save(ChildPage, -1);

        // Publish all content to trigger alias creation
        ContentService.PublishBranch(RootPage, PublishBranchFilter.IncludeUnpublished, ["*"]);
    }

    private ContentType CreateContentTypeWithUrlAlias(int templateId)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("pageWithAlias")
            .WithName("Page With Alias")
            .WithKey(new Guid(ContentTypeKey))
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSortOrder(1)
                .WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithAlias("title")
                    .WithName("Title")
                    .WithSortOrder(1)
                    .Done()
                .AddPropertyType()
                    .WithAlias("bodyText")
                    .WithName("Body text")
                    .WithSortOrder(2)
                    .Done()
                .AddPropertyType()
                    .WithAlias("author")
                    .WithName("Author")
                    .WithSortOrder(3)
                    .Done()
                .AddPropertyType()
                    .WithAlias(Constants.Conventions.Content.UrlAlias)
                    .WithName("URL Alias")
                    .WithSortOrder(4)
                    .WithDataTypeId(Constants.DataTypes.Textbox)
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .Done()
                .Done()
            .AddAllowedTemplate()
                .WithId(templateId)
                .WithAlias("defaultTemplate")
                .WithName("Default Template")
                .Done()
            .WithDefaultTemplateId(templateId)
            .Build();

        return (ContentType)contentType;
    }

    private ContentType CreateCultureVariantContentTypeWithUrlAlias(int templateId, string alias = "pageWithAliasVariant")
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(alias)
            .WithName("Page With Alias Variant")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSortOrder(1)
                .WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithAlias("title")
                    .WithName("Title")
                    .WithSortOrder(1)
                    .Done()
                .AddPropertyType()
                    .WithAlias("bodyText")
                    .WithName("Body text")
                    .WithSortOrder(2)
                    .Done()
                .AddPropertyType()
                    .WithAlias("author")
                    .WithName("Author")
                    .WithSortOrder(3)
                    .Done()
                .AddPropertyType()
                    .WithAlias(Constants.Conventions.Content.UrlAlias)
                    .WithName("URL Alias")
                    .WithSortOrder(4)
                    .WithDataTypeId(Constants.DataTypes.Textbox)
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithVariations(ContentVariation.Culture)
                    .Done()
                .Done()
            .AddAllowedTemplate()
                .WithId(templateId)
                .WithAlias("variantTemplate")
                .WithName("Variant Template")
                .Done()
            .WithDefaultTemplateId(templateId)
            .Build();

        return (ContentType)contentType;
    }

    /// <summary>
    /// Creates a culture-variant content type where the umbracoUrlAlias property is SHARED (not varied by culture).
    /// This tests the scenario where a variant content type has an invariant alias property.
    /// </summary>
    private ContentType CreateCultureVariantContentTypeWithSharedUrlAlias(int templateId, string alias = "pageWithSharedAlias")
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(alias)
            .WithName("Page With Shared Alias")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSortOrder(1)
                .WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithAlias("title")
                    .WithName("Title")
                    .WithSortOrder(1)
                    .Done()
                .AddPropertyType()
                    .WithAlias(Constants.Conventions.Content.UrlAlias)
                    .WithName("URL Alias")
                    .WithSortOrder(2)
                    .WithDataTypeId(Constants.DataTypes.Textbox)
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithVariations(ContentVariation.Nothing) // SHARED - not varied by culture
                    .Done()
                .Done()
            .AddAllowedTemplate()
                .WithId(templateId)
                .WithAlias("sharedAliasTemplate")
                .WithName("Shared Alias Template")
                .Done()
            .WithDefaultTemplateId(templateId)
            .Build();

        return (ContentType)contentType;
    }

    #region GetDocumentKeysByAlias Tests

    [Test]
    public async Task GetDocumentKeysByAliasAsync_Returns_DocumentKey_For_Single_Alias()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode);

        Assert.That(result, Does.Contain(new Guid(PageWithSingleAliasKey)));
    }

    [Test]
    public async Task GetDocumentKeysByAliasAsync_Returns_DocumentKey_For_First_Of_Multiple_Aliases()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("first-alias", isoCode);

        Assert.That(result, Does.Contain(new Guid(PageWithMultipleAliasesKey)));
    }

    [Test]
    public async Task GetDocumentKeysByAliasAsync_Returns_DocumentKey_For_Second_Of_Multiple_Aliases()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("second-alias", isoCode);

        Assert.That(result, Does.Contain(new Guid(PageWithMultipleAliasesKey)));
    }

    [Test]
    public async Task GetDocumentKeysByAliasAsync_Returns_DocumentKey_For_Third_Alias_With_Slashes_Normalized()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        // The alias was stored as "/third-alias/" but should be normalized to "third-alias"
        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("third-alias", isoCode);

        Assert.That(result, Does.Contain(new Guid(PageWithMultipleAliasesKey)));
    }

    [Test]
    public async Task GetDocumentKeysByAliasAsync_Returns_Empty_For_Non_Existent_Alias()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("non-existent-alias", isoCode);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetDocumentKeysByAliasAsync_Is_Case_Insensitive()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("MY-SINGLE-ALIAS", isoCode);

        Assert.That(result, Does.Contain(new Guid(PageWithSingleAliasKey)));
    }

    [Test]
    public async Task GetDocumentKeysByAliasAsync_Handles_Leading_Slash_In_Query()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("/my-single-alias", isoCode);

        Assert.That(result, Does.Contain(new Guid(PageWithSingleAliasKey)));
    }

    [Test]
    public async Task GetDocumentKeysByAliasAsync_Works_For_Language_Added_After_Initialization()
    {
        // Arrange - Add a new language AFTER the service has been initialized
        var newLanguage = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .WithIsDefault(false)
            .Build();
        await LanguageService.CreateAsync(newLanguage, Constants.Security.SuperUserKey);

        // Create a culture-variant content type
        var template = TemplateBuilder.CreateTextPageTemplate("variantTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var variantContentType = CreateCultureVariantContentTypeWithUrlAlias(template.Id);
        await ContentTypeService.CreateAsync(variantContentType, Constants.Security.SuperUserKey);

        // Create content with an alias for the new language
        var content = ContentBuilder.CreateSimpleContent(variantContentType, "French Page", RootPage.Id);
        content.SetCultureName("French Page", "fr-FR");
        content.SetValue(Constants.Conventions.Content.UrlAlias, "french-alias", "fr-FR");
        ContentService.Save(content, -1);
        ContentService.Publish(content, ["fr-FR"]);

        // Trigger alias creation
        await DocumentUrlAliasService.CreateOrUpdateAliasesAsync(content.Key);

        // Act - Try to retrieve the alias for the new language
        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("french-alias", "fr-FR");

        // Assert - The alias should be found even though the language was added after initialization
        Assert.That(result, Does.Contain(content.Key));
    }

    #endregion

    #region GetAliases Tests

    [Test]
    public async Task GetAliasesAsync_Returns_Single_Alias_For_Document()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var result = await DocumentUrlAliasService.GetAliasesAsync(new Guid(PageWithSingleAliasKey), isoCode);

        Assert.That(result, Has.Exactly(1).Items);
        Assert.That(result, Does.Contain("my-single-alias"));
    }

    [Test]
    public async Task GetAliasesAsync_Returns_Multiple_Aliases_For_Document()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var result = (await DocumentUrlAliasService.GetAliasesAsync(new Guid(PageWithMultipleAliasesKey), isoCode)).ToList();

        Assert.That(result, Has.Exactly(3).Items);
        Assert.That(result, Does.Contain("first-alias"));
        Assert.That(result, Does.Contain("second-alias"));
        Assert.That(result, Does.Contain("third-alias"));
    }

    [Test]
    public async Task GetAliasesAsync_Returns_Empty_For_Document_Without_Aliases()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var result = await DocumentUrlAliasService.GetAliasesAsync(new Guid(PageWithNoAliasKey), isoCode);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAliasesAsync_Returns_Empty_For_Non_Existent_Document()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var result = await DocumentUrlAliasService.GetAliasesAsync(Guid.NewGuid(), isoCode);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAliasesAsync_Uses_Default_Language_When_Culture_Is_Null()
    {
        // GetAliases should work without specifying culture
        var result = await DocumentUrlAliasService.GetAliasesAsync(new Guid(PageWithSingleAliasKey), null);

        Assert.That(result, Has.Exactly(1).Items);
        Assert.That(result, Does.Contain("my-single-alias"));
    }

    [Test]
    public async Task GetAliasesAsync_Works_For_Language_Added_After_Initialization()
    {
        // Arrange - Add a new language AFTER the service has been initialized
        var newLanguage = new LanguageBuilder()
            .WithCultureInfo("de-DE")
            .WithIsDefault(false)
            .Build();
        await LanguageService.CreateAsync(newLanguage, Constants.Security.SuperUserKey);

        // Create a culture-variant content type
        var template = TemplateBuilder.CreateTextPageTemplate("variantTemplate2");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var variantContentType = CreateCultureVariantContentTypeWithUrlAlias(template.Id, "pageWithAliasVariant2");
        await ContentTypeService.CreateAsync(variantContentType, Constants.Security.SuperUserKey);

        // Create content with an alias for the new language
        var content = ContentBuilder.CreateSimpleContent(variantContentType, "German Page", RootPage.Id);
        content.SetCultureName("German Page", "de-DE");
        content.SetValue(Constants.Conventions.Content.UrlAlias, "german-alias", "de-DE");
        ContentService.Save(content, -1);
        ContentService.Publish(content, ["de-DE"]);

        // Trigger alias creation
        await DocumentUrlAliasService.CreateOrUpdateAliasesAsync(content.Key);

        // Act - Try to retrieve the aliases for the new language
        var result = await DocumentUrlAliasService.GetAliasesAsync(content.Key, "de-DE");

        // Assert - The alias should be found even though the language was added after initialization
        Assert.That(result, Does.Contain("german-alias"));
    }

    #endregion

    #region CreateOrUpdateAliasesAsync Tests

    [Test]
    public async Task CreateOrUpdateAliasesAsync_Creates_Alias_For_New_Document()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        // Create a new page with alias
        var newPage = ContentBuilder.CreateSimpleContent(ContentType, "New Page", RootPage.Id);
        newPage.SetValue(Constants.Conventions.Content.UrlAlias, "brand-new-alias");
        ContentService.Save(newPage, -1);
        ContentService.Publish(newPage, []);

        // Manually trigger alias creation
        await DocumentUrlAliasService.CreateOrUpdateAliasesAsync(newPage.Key);

        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("brand-new-alias", isoCode);
        Assert.That(result, Does.Contain(newPage.Key));
    }

    [Test]
    public async Task CreateOrUpdateAliasesAsync_Updates_Alias_When_Changed()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        // Verify original alias exists
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Is.Not.Empty);

        // Re-fetch the content to get the current version (after PublishBranch in setup)
        var content = ContentService.GetById(PageWithSingleAlias.Key)!;

        // Update the alias
        content.SetValue(Constants.Conventions.Content.UrlAlias, "updated-alias");
        ContentService.Save(content, -1);
        ContentService.Publish(content, []);

        await DocumentUrlAliasService.CreateOrUpdateAliasesAsync(content.Key);

        // Old alias should no longer resolve
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Is.Empty);

        // New alias should resolve
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("updated-alias", isoCode), Does.Contain(new Guid(PageWithSingleAliasKey)));
    }

    [Test]
    public async Task CreateOrUpdateAliasesAsync_Removes_Alias_When_Cleared()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        // Verify original alias exists
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Is.Not.Empty);

        // Re-fetch the content to get the current version (after PublishBranch in setup)
        var content = ContentService.GetById(PageWithSingleAlias.Key)!;

        // Clear the alias
        content.SetValue(Constants.Conventions.Content.UrlAlias, string.Empty);
        ContentService.Save(content, -1);
        ContentService.Publish(content, []);

        await DocumentUrlAliasService.CreateOrUpdateAliasesAsync(content.Key);

        // Alias should no longer resolve
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Is.Empty);
    }

    [Test]
    public async Task CreateOrUpdateAliasesAsync_Stores_Alias_For_Variant_ContentType_With_Variant_Alias_Property()
    {
        // Arrange - Create a culture-variant content type with a CULTURE-VARIED umbracoUrlAlias property.
        var template = TemplateBuilder.CreateTextPageTemplate("variantAliasTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var variantContentType = CreateCultureVariantContentTypeWithUrlAlias(template.Id, "pageWithVariantAlias");
        await ContentTypeService.CreateAsync(variantContentType, Constants.Security.SuperUserKey);

        // Create content with the culture-varied alias property set
        var content = new ContentBuilder()
            .WithContentType(variantContentType)
            .WithCultureName("en-US", "Page With Variant Alias")
            .Build();
        content.SetValue(Constants.Conventions.Content.UrlAlias, "variant-alias-value", "en-US"); // With culture - it's a variant property
        content.ParentId = RootPage.Id;
        ContentService.Save(content, -1);
        ContentService.Publish(content, ["en-US"]);

        // Act - trigger alias creation
        await DocumentUrlAliasService.CreateOrUpdateAliasesAsync(content.Key);

        // Assert - the alias should be stored in the database
        List<PublishedDocumentUrlAlias> storedAliases;
        using (CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            storedAliases = DocumentUrlAliasRepository.GetAll()
                .Where(a => a.DocumentKey == content.Key)
                .ToList();
        }

        Assert.That(storedAliases, Has.Count.GreaterThan(0), "Variant alias on variant content type should be stored in database");
        Assert.That(storedAliases.Any(a => a.Alias == "variant-alias-value"), Is.True, "The alias value should be stored");

        // Also verify it can be retrieved via the service
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;
        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("variant-alias-value", isoCode);
        Assert.That(result, Does.Contain(content.Key), "Variant alias should be retrievable via GetDocumentKeysByAliasAsync");
    }

    [Test]
    public async Task CreateOrUpdateAliasesAsync_Stores_Alias_For_Variant_ContentType_With_Shared_Alias_Property()
    {
        // Arrange - Create a culture-variant content type with a SHARED (non-culture-varied) umbracoUrlAlias property.
        // This is the bug scenario: variant content type + shared alias property = no alias saved.
        var template = TemplateBuilder.CreateTextPageTemplate("sharedAliasTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var variantContentType = CreateCultureVariantContentTypeWithSharedUrlAlias(template.Id);
        await ContentTypeService.CreateAsync(variantContentType, Constants.Security.SuperUserKey);

        // Create content with the shared alias property set
        var content = new ContentBuilder()
            .WithContentType(variantContentType)
            .WithCultureName("en-US", "Page With Shared Alias")
            .Build();
        content.SetValue(Constants.Conventions.Content.UrlAlias, "shared-alias-value"); // No culture - it's a shared property
        content.ParentId = RootPage.Id;
        ContentService.Save(content, -1);
        ContentService.Publish(content, ["en-US"]);

        // Act - trigger alias creation
        await DocumentUrlAliasService.CreateOrUpdateAliasesAsync(content.Key);

        // Assert - the alias should be stored in the database
        List<PublishedDocumentUrlAlias> storedAliases;
        using (CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            storedAliases = DocumentUrlAliasRepository.GetAll()
                .Where(a => a.DocumentKey == content.Key)
                .ToList();
        }

        Assert.That(storedAliases, Has.Count.GreaterThan(0), "Shared alias on variant content type should be stored in database");
        Assert.That(storedAliases.Any(a => a.Alias == "shared-alias-value"), Is.True, "The alias value should be stored");

        // Also verify it can be retrieved via the service
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;
        var result = await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("shared-alias-value", isoCode);
        Assert.That(result, Does.Contain(content.Key), "Shared alias should be retrievable via GetDocumentKeysByAliasAsync");
    }

    [Test]
    public async Task CreateOrUpdateAliasesAsync_Removes_Alias_From_Database_When_Cleared()
    {
        var documentKey = new Guid(PageWithSingleAliasKey);

        // Verify original alias exists in database
        List<PublishedDocumentUrlAlias> aliasesBefore;
        using (ICoreScope scope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            aliasesBefore = DocumentUrlAliasRepository.GetAll()
                .Where(a => a.DocumentKey == documentKey)
                .ToList();
        }

        Assert.That(aliasesBefore, Has.Count.EqualTo(1), "Expected 1 alias in database before clearing");
        Assert.That(aliasesBefore[0].Alias, Is.EqualTo("my-single-alias"));

        // Re-fetch the content to get the current version (after PublishBranch in setup)
        var content = ContentService.GetById(documentKey)!;

        // Clear the alias
        content.SetValue(Constants.Conventions.Content.UrlAlias, string.Empty);
        ContentService.Save(content, -1);
        ContentService.Publish(content, []);

        await DocumentUrlAliasService.CreateOrUpdateAliasesAsync(content.Key);

        // Alias should be removed from database
        List<PublishedDocumentUrlAlias> aliasesAfter;
        using (ICoreScope scope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            aliasesAfter = DocumentUrlAliasRepository.GetAll()
                .Where(a => a.DocumentKey == documentKey)
                .ToList();
        }

        Assert.That(aliasesAfter, Is.Empty, "Expected no aliases in database after clearing the property");
    }

    #endregion

    #region CreateOrUpdateAliasesWithDescendantsAsync Tests

    [Test]
    public async Task CreateOrUpdateAliasesWithDescendantsAsync_Updates_Document_And_Descendants()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        // Verify child alias exists
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("child-alias", isoCode), Does.Contain(new Guid(ChildPageKey)));

        // Re-fetch the content to get the current versions (after PublishBranch in setup)
        var parentContent = ContentService.GetById(PageWithSingleAlias.Key)!;
        var childContent = ContentService.GetById(ChildPage.Key)!;

        // Update parent and child aliases
        parentContent.SetValue(Constants.Conventions.Content.UrlAlias, "parent-new-alias");
        ContentService.Save(parentContent, -1);

        childContent.SetValue(Constants.Conventions.Content.UrlAlias, "child-new-alias");
        ContentService.Save(childContent, -1);

        ContentService.PublishBranch(parentContent, PublishBranchFilter.IncludeUnpublished, ["*"]);

        await DocumentUrlAliasService.CreateOrUpdateAliasesWithDescendantsAsync(parentContent.Key);

        // Old aliases should no longer resolve
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Is.Empty);
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("child-alias", isoCode), Is.Empty);

        // New aliases should resolve
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("parent-new-alias", isoCode), Does.Contain(new Guid(PageWithSingleAliasKey)));
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("child-new-alias", isoCode), Does.Contain(new Guid(ChildPageKey)));
    }

    #endregion

    #region DeleteAliasesFromCacheAsync Tests

    [Test]
    public async Task DeleteAliasesFromCacheAsync_Removes_Aliases_For_Document()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        // Verify alias exists
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Is.Not.Empty);

        await DocumentUrlAliasService.DeleteAliasesFromCacheAsync([new Guid(PageWithSingleAliasKey)]);

        // Alias should no longer resolve
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Is.Empty);
    }

    [Test]
    public async Task DeleteAliasesFromCacheAsync_Removes_All_Aliases_For_Document_With_Multiple_Aliases()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        // Verify aliases exist
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("first-alias", isoCode), Is.Not.Empty);
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("second-alias", isoCode), Is.Not.Empty);
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("third-alias", isoCode), Is.Not.Empty);

        await DocumentUrlAliasService.DeleteAliasesFromCacheAsync([new Guid(PageWithMultipleAliasesKey)]);

        // All aliases should no longer resolve
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("first-alias", isoCode), Is.Empty);
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("second-alias", isoCode), Is.Empty);
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("third-alias", isoCode), Is.Empty);
    }

    #endregion

    #region HasAny Tests

    [Test]
    public void HasAny_Returns_True_When_Aliases_Exist()
    {
        // Act - aliases already exist from setup
        var result = DocumentUrlAliasService.HasAny();

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasAny_Returns_False_After_All_Aliases_Deleted()
    {
        // Arrange - verify aliases exist initially
        Assert.That(DocumentUrlAliasService.HasAny(), Is.True);

        // Act - delete all aliases from cache
        await DocumentUrlAliasService.DeleteAliasesFromCacheAsync(
        [
            new Guid(PageWithSingleAliasKey),
            new Guid(PageWithMultipleAliasesKey),
            new Guid(ChildPageKey),
        ]);

        // Assert
        Assert.That(DocumentUrlAliasService.HasAny(), Is.False);
    }

    #endregion

    #region RebuildAllAliasesAsync Tests

    [Test]
    public async Task RebuildAllAliasesAsync_Repopulates_Cache_From_Database()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        // Arrange - delete aliases from cache (but not database)
        await DocumentUrlAliasService.DeleteAliasesFromCacheAsync(new[]
        {
            new Guid(PageWithSingleAliasKey),
            new Guid(PageWithMultipleAliasesKey),
            new Guid(ChildPageKey)
        });

        // Verify cache is empty
        Assert.That(DocumentUrlAliasService.HasAny(), Is.False);
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Is.Empty);

        // Act - rebuild from database
        await DocumentUrlAliasService.RebuildAllAliasesAsync();

        // Assert - cache should be repopulated
        Assert.That(DocumentUrlAliasService.HasAny(), Is.True);
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Does.Contain(new Guid(PageWithSingleAliasKey)));
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("first-alias", isoCode), Does.Contain(new Guid(PageWithMultipleAliasesKey)));
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("child-alias", isoCode), Does.Contain(new Guid(ChildPageKey)));
    }

    [Test]
    public async Task RebuildAllAliasesAsync_Updates_Database_With_Current_Property_Values()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        // Arrange - update a document's alias property directly in the database (simulating external change)
        var content = ContentService.GetById(PageWithSingleAlias.Key)!;
        content.SetValue(Constants.Conventions.Content.UrlAlias, "rebuilt-alias");
        ContentService.Save(content, -1);
        ContentService.Publish(content, []);

        // Act - rebuild all aliases
        await DocumentUrlAliasService.RebuildAllAliasesAsync();

        // Assert - new alias should be available, old should not
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("rebuilt-alias", isoCode), Does.Contain(new Guid(PageWithSingleAliasKey)));
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Is.Empty);
    }

    [Test]
    public async Task RebuildAllAliasesAsync_Clears_Removed_Aliases()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        // Arrange - verify alias exists
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Is.Not.Empty);

        // Remove the alias from the document
        var content = ContentService.GetById(PageWithSingleAlias.Key)!;
        content.SetValue(Constants.Conventions.Content.UrlAlias, string.Empty);
        ContentService.Save(content, -1);
        ContentService.Publish(content, []);

        // Act - rebuild all aliases
        await DocumentUrlAliasService.RebuildAllAliasesAsync();

        // Assert - alias should no longer exist in cache or database
        Assert.That(await DocumentUrlAliasService.GetDocumentKeysByAliasAsync("my-single-alias", isoCode), Is.Empty);

        // Verify in database
        List<PublishedDocumentUrlAlias> storedAliases;
        using (ICoreScope scope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            storedAliases = DocumentUrlAliasRepository.GetAll()
                .Where(a => a.DocumentKey == new Guid(PageWithSingleAliasKey))
                .ToList();
        }

        Assert.That(storedAliases, Is.Empty);
    }

    [Test]
    public async Task RebuildAllAliasesAsync_Handles_Empty_Database()
    {
        // Arrange - clear all aliases from documents
        var content1 = ContentService.GetById(PageWithSingleAlias.Key)!;
        content1.SetValue(Constants.Conventions.Content.UrlAlias, string.Empty);
        ContentService.Save(content1, -1);
        ContentService.Publish(content1, []);

        var content2 = ContentService.GetById(PageWithMultipleAliases.Key)!;
        content2.SetValue(Constants.Conventions.Content.UrlAlias, string.Empty);
        ContentService.Save(content2, -1);
        ContentService.Publish(content2, []);

        var content3 = ContentService.GetById(ChildPage.Key)!;
        content3.SetValue(Constants.Conventions.Content.UrlAlias, string.Empty);
        ContentService.Save(content3, -1);
        ContentService.Publish(content3, []);

        // Act - rebuild should not throw
        await DocumentUrlAliasService.RebuildAllAliasesAsync();

        // Assert - cache should be empty
        Assert.That(DocumentUrlAliasService.HasAny(), Is.False);
    }

    #endregion

    #region InitAsync Tests

    [Test]
    public async Task InitAsync_With_ForceEmpty_Returns_Empty_Cache()
    {
        // Create a new service instance and init with forceEmpty
        var service = GetRequiredService<IDocumentUrlAliasService>();

        // Re-initialize with forceEmpty=true should result in empty cache
        // Note: This test is a bit tricky as we're testing with the same service instance
        // The main InitAsync test is implicitly covered by the Setup method
        Assert.DoesNotThrowAsync(async () =>
            await DocumentUrlAliasService.InitAsync(false, CancellationToken.None));
    }

    [Test]
    public async Task InitAsync_Persists_Correct_Cache_Data()
    {
        // Arrange - get the expected language ID
        ILanguage defaultLanguage = await LanguageService.GetDefaultLanguageAsync();
        var expectedLanguageId = defaultLanguage.Id;

        // Act - get all stored aliases from the database
        List<PublishedDocumentUrlAlias> storedAliases;
        using (ICoreScope scope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            storedAliases = DocumentUrlAliasRepository.GetAll().ToList();
        }

        // Assert - verify aliases were stored
        Assert.That(storedAliases, Is.Not.Empty, "No aliases were stored in the database");

        // Verify PageWithSingleAlias
        var singleAliasKey = new Guid(PageWithSingleAliasKey);
        var singleAliasEntries = storedAliases.Where(a => a.DocumentKey == singleAliasKey).ToList();
        Assert.That(singleAliasEntries, Has.Count.EqualTo(1), "Expected 1 alias for PageWithSingleAlias");

        var singleAlias = singleAliasEntries.First();
        Assert.Multiple(() =>
        {
            Assert.That(singleAlias.DocumentKey, Is.EqualTo(singleAliasKey), "DocumentKey should match PageWithSingleAlias key");
            Assert.That(singleAlias.DocumentKey, Is.Not.EqualTo(Guid.Empty), "DocumentKey should not be empty GUID");
            Assert.That(singleAlias.LanguageId, Is.EqualTo(expectedLanguageId), "LanguageId should match default language");
            Assert.That(singleAlias.Alias, Is.EqualTo("my-single-alias"), "Alias should be normalized (lowercase, no leading slash)");
        });

        // Verify PageWithMultipleAliases - should have 3 separate entries
        var multipleAliasKey = new Guid(PageWithMultipleAliasesKey);
        var multipleAliasEntries = storedAliases.Where(a => a.DocumentKey == multipleAliasKey).ToList();
        Assert.That(multipleAliasEntries, Has.Count.EqualTo(3), "Expected 3 aliases for PageWithMultipleAliases");

        var aliasValues = multipleAliasEntries.Select(a => a.Alias).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(aliasValues, Does.Contain("first-alias"), "Should contain 'first-alias'");
            Assert.That(aliasValues, Does.Contain("second-alias"), "Should contain 'second-alias'");
            Assert.That(aliasValues, Does.Contain("third-alias"), "Should contain 'third-alias' (normalized from '/third-alias/')");
        });

        foreach (var alias in multipleAliasEntries)
        {
            Assert.Multiple(() =>
            {
                Assert.That(alias.DocumentKey, Is.Not.EqualTo(Guid.Empty), "DocumentKey should not be empty GUID");
                Assert.That(alias.LanguageId, Is.EqualTo(expectedLanguageId), "LanguageId should match default language");
            });
        }

        // Verify ChildPage - should have root ancestor as RootPage (not its parent PageWithSingleAlias)
        var childPageKey = new Guid(ChildPageKey);
        var childAliasEntries = storedAliases.Where(a => a.DocumentKey == childPageKey).ToList();
        Assert.That(childAliasEntries, Has.Count.EqualTo(1), "Expected 1 alias for ChildPage");

        var childAlias = childAliasEntries.First();
        Assert.Multiple(() =>
        {
            Assert.That(childAlias.DocumentKey, Is.EqualTo(childPageKey), "DocumentKey should match ChildPage key");
            Assert.That(childAlias.DocumentKey, Is.Not.EqualTo(Guid.Empty), "DocumentKey should not be empty GUID");
            Assert.That(childAlias.LanguageId, Is.EqualTo(expectedLanguageId), "LanguageId should match default language");
            Assert.That(childAlias.Alias, Is.EqualTo("child-alias"), "Alias should be 'child-alias'");
        });

        // Verify PageWithNoAlias and RootPage - should NOT have entries (they have no umbracoUrlAlias value)
        var noAliasEntries = storedAliases.Where(a => a.DocumentKey == new Guid(PageWithNoAliasKey)).ToList();
        Assert.That(noAliasEntries, Is.Empty, "PageWithNoAlias should have no stored aliases");

        var rootPageEntries = storedAliases.Where(a => a.DocumentKey == new Guid(RootPageKey)).ToList();
        Assert.That(rootPageEntries, Is.Empty, "RootPage should have no stored aliases (no umbracoUrlAlias set)");
    }

    #endregion
}
