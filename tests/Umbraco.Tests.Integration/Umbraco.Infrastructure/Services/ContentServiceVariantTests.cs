// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
internal sealed class ContentServiceVariantTests : UmbracoIntegrationTest
{
    private IContentService ContentService => GetRequiredService<IContentService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    /// <summary>
    /// Provides a happy path test case where culture codes are provided exactly matching the language ISO codes.
    /// </summary>
    [Test]
    public async Task Can_Publish_With_Consistent_Provision_Of_Culture_Codes()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = ContentService.Create("Test Item", Constants.System.Root, contentType);
        content.SetCultureName("Test item", "en-US");
        content.SetValue("title", "Title", "en-US");

        Assert.AreEqual("en-US", content.AvailableCultures.FirstOrDefault());

        ContentService.Save(content);
        var publishResult = ContentService.Publish(content, ["en-US"]);
        Assert.IsTrue(publishResult.Success);
    }

    /// <summary>
    /// Provides an originally failing test case for https://github.com/umbraco/Umbraco-CMS/issues/19287, where the culture
    /// codes are provided with inconsistent casing.
    /// </summary>
    [Test]
    public async Task Can_Publish_With_Inconsistent_Provision_Of_Culture_Codes_When_Setting_Properties()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = ContentService.Create("Test Item", Constants.System.Root, contentType);
        content.SetCultureName("Test item", "en-us");
        content.SetValue("title", "Title", "en-us");

        Assert.AreEqual("en-US", content.AvailableCultures.FirstOrDefault());

        ContentService.Save(content);
        var publishResult = ContentService.Publish(content, ["en-US"]);
        Assert.IsTrue(publishResult.Success);
    }

    [Test]
    public async Task Can_Publish_With_Inconsistent_Provision_Of_Culture_Codes_When_Publishing()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = ContentService.Create("Test Item", Constants.System.Root, contentType);
        content.SetCultureName("Test item", "en-US");
        content.SetValue("title", "Title", "en-US");
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-us"]);
        Assert.IsTrue(publishResult.Success);
    }

    [Test]
    public async Task Can_Unpublish_With_Inconsistent_Provision_Of_Culture_Codes()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = ContentService.Create("Test Item", Constants.System.Root, contentType);
        content.SetCultureName("Test item", "en-US");
        content.SetValue("title", "Title", "en-US");
        ContentService.Save(content);

        var unpublishResult = ContentService.Unpublish(content, "en-us");
        Assert.IsTrue(unpublishResult.Success);
    }

    [Test]
    public async Task Can_Publish_Branch_With_Inconsistent_Provision_Of_Culture_Codes()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = ContentService.Create("Test Item", Constants.System.Root, contentType);
        content.SetCultureName("Test item", "en-US");
        content.SetValue("title", "Title", "en-US");
        ContentService.Save(content);

        var publishResult = ContentService.PublishBranch(content, PublishBranchFilter.All, ["en-us"]);
        Assert.AreEqual(1, publishResult.Count());
        Assert.IsTrue(publishResult.First().Success);
    }

    private async Task<(ILanguage LangEn, ILanguage LangDa, IContentType contentType)> SetupVariantTest()
    {
        var langEn = await LanguageService.GetAsync("en-US");

        var langDa = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);

        var key = Guid.NewGuid();
        var contentType = new ContentTypeBuilder()
            .WithAlias("variantContent")
            .WithName("Variant Content")
            .WithKey(key)
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithAlias("title")
                    .WithName("Title")
                    .WithVariations(ContentVariation.Culture)
                .Done()
            .Done()
            .Build();

        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        return (langEn, langDa, contentType);
    }
}
