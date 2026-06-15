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
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentServiceVariantTests : UmbracoIntegrationTest
{
    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    /// <summary>
    /// Provides both happy path with correctly cased cultures, and originally failing test cases for
    /// https://github.com/umbraco/Umbraco-CMS/issues/19287, where the culture codes are provided with inconsistent casing.
    /// </summary>
    [TestCase("en-US", "en-US", "en-US")]
    [TestCase("en-us", "en-us", "en-us")]
    [TestCase("en-US", "en-US", "en-us")]
    [TestCase("en-us", "en-US", "en-US")]
    [TestCase("en-US", "en-us", "en-US")]
    public async Task Can_Save_And_Publish_With_Inconsistent_Provision_Of_Culture_Codes(string cultureNameCultureCode, string valueCultureCode, string publishCultureCode)
    {
        var contentType = await SetupVariantTest();

        IContent content = ContentService.Create("Test Item", Constants.System.Root, contentType);
        content.SetCultureName("Test item", cultureNameCultureCode);
        content.SetValue("title", "Title", valueCultureCode);
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, [publishCultureCode]);
        Assert.That(publishResult.Success, Is.True);

        content = ContentService.GetById(content.Key)!;
        Assert.Multiple(() =>
        {
            Assert.That(content.Published, Is.True);
            Assert.That(content.PublishedCultures.Count(), Is.EqualTo(1));
            Assert.That(content.PublishedCultures.FirstOrDefault(), Is.EqualTo("en-US"));
        });
    }

    [TestCase("en-US", "en-US", "en-US")]
    [TestCase("en-us", "en-us", "en-us")]
    [TestCase("en-US", "en-US", "en-us")]
    [TestCase("en-us", "en-US", "en-US")]
    [TestCase("en-US", "en-us", "en-US")]
    public async Task Can_Unpublish_With_Inconsistent_Provision_Of_Culture_Codes(string cultureNameCultureCode, string valueCultureCode, string unpublishCultureCode)
    {
        var contentType = await SetupVariantTest();

        IContent content = ContentService.Create("Test Item", Constants.System.Root, contentType);
        content.SetCultureName("Test item", cultureNameCultureCode);
        content.SetValue("title", "Title", valueCultureCode);
        ContentService.Save(content);
        // use correctly cased culture code to publish
        ContentService.Publish(content, ["en-US"]);

        var unpublishResult = ContentService.Unpublish(content, unpublishCultureCode);
        Assert.That(unpublishResult.Success, Is.True);

        content = ContentService.GetById(content.Key)!;
        Assert.Multiple(() =>
        {
            Assert.That(content.Published, Is.False);
            Assert.That(content.PublishedCultures.Count(), Is.EqualTo(0));
        });
    }

    [TestCase("en-US", "en-US", "en-US")]
    [TestCase("en-us", "en-us", "en-us")]
    [TestCase("en-US", "en-US", "en-us")]
    [TestCase("en-us", "en-US", "en-US")]
    [TestCase("en-US", "en-us", "en-US")]
    public async Task Can_Publish_Branch_With_Inconsistent_Provision_Of_Culture_Codes(string cultureNameCultureCode, string valueCultureCode, string publishCultureCode)
    {
        var contentType = await SetupVariantTest();

        IContent root = ContentService.Create("Root", Constants.System.Root, contentType);
        root.SetCultureName("Root", cultureNameCultureCode);
        root.SetValue("title", "Root Title", valueCultureCode);
        ContentService.Save(root);

        var child = ContentService.Create("Child", root.Id, contentType);
        child.SetCultureName("Child", cultureNameCultureCode);
        child.SetValue("title", "Child Title", valueCultureCode);
        ContentService.Save(child);

        var publishResult = ContentService.PublishBranch(root, PublishBranchFilter.All, [publishCultureCode]);
        Assert.That(publishResult.Count(), Is.EqualTo(2));
        Assert.That(publishResult.First().Success, Is.True);
        Assert.That(publishResult.Last().Success, Is.True);

        root = ContentService.GetById(root.Key)!;
        Assert.Multiple(() =>
        {
            Assert.That(root.Published, Is.True);
            Assert.That(root.PublishedCultures.Count(), Is.EqualTo(1));
            Assert.That(root.PublishedCultures.FirstOrDefault(), Is.EqualTo("en-US"));
        });

        child = ContentService.GetById(child.Key)!;
        Assert.Multiple(() =>
        {
            Assert.That(child.Published, Is.True);
            Assert.That(child.PublishedCultures.Count(), Is.EqualTo(1));
            Assert.That(child.PublishedCultures.FirstOrDefault(), Is.EqualTo("en-US"));
        });
    }

    private async Task<IContentType> SetupVariantTest()
    {
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

        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        return contentType;
    }
}
