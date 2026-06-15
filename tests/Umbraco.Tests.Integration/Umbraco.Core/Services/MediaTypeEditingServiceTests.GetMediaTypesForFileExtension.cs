using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class MediaTypeEditingServiceTests
{
    [TestCase("jpg", Constants.Conventions.MediaTypes.Image)]
    [TestCase("png", Constants.Conventions.MediaTypes.Image)]
    [TestCase("svg", Constants.Conventions.MediaTypes.VectorGraphicsAlias)]
    [TestCase("pdf", Constants.Conventions.MediaTypes.ArticleAlias)]
    [TestCase("doc", Constants.Conventions.MediaTypes.ArticleAlias)]
    [TestCase("docx", Constants.Conventions.MediaTypes.ArticleAlias)]
    [TestCase("mp4", Constants.Conventions.MediaTypes.VideoAlias)]
    [TestCase("webm", Constants.Conventions.MediaTypes.VideoAlias)]
    [TestCase("ogv", Constants.Conventions.MediaTypes.VideoAlias)]
    [TestCase("mp3", Constants.Conventions.MediaTypes.AudioAlias)]
    [TestCase("weba", Constants.Conventions.MediaTypes.AudioAlias)]
    [TestCase("oga", Constants.Conventions.MediaTypes.AudioAlias)]
    [TestCase("opus", Constants.Conventions.MediaTypes.AudioAlias)]
    public async Task Can_Get_Default_Allowed_Media_Types_As_Specific_Match(string fileExtension, string expectedMediaTypeAlias)
    {
        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync(fileExtension, 0, 100);

        // Should contain the specific match and the File fallback.
        Assert.That(result.Total, Is.GreaterThanOrEqualTo(2));

        MediaTypeFileExtensionMatchResult specific = result.Items.First(r => r.MediaType.Alias == expectedMediaTypeAlias);
        Assert.That(specific.IsSpecificMatch, Is.True);

        MediaTypeFileExtensionMatchResult fallback = result.Items.First(r => r.MediaType.Alias == Constants.Conventions.MediaTypes.File);
        Assert.That(fallback.IsSpecificMatch, Is.False);
    }

    [TestCase("abc")]
    [TestCase("123")]
    public async Task Unknown_Extension_Returns_File_As_Fallback(string fileExtension)
    {
        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync(fileExtension, 0, 100);

        Assert.That(result.Total, Is.EqualTo(1));

        MediaTypeFileExtensionMatchResult fallback = result.Items.First();
        Assert.That(fallback.MediaType.Alias, Is.EqualTo(Constants.Conventions.MediaTypes.File));
        Assert.That(fallback.IsSpecificMatch, Is.False);
    }

    [TestCase("jpg")]
    [TestCase(".jpg")]
    public async Task Ignores_Leading_Period_For_Allowed_Media_Types(string fileExtension)
    {
        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync(fileExtension, 0, 100);

        Assert.That(result.Total, Is.GreaterThanOrEqualTo(2));
        Assert.That(result.Items.Any(r => r.MediaType.Alias == Constants.Conventions.MediaTypes.Image && r.IsSpecificMatch), Is.True);
    }

    [Test]
    public async Task Returns_Multiple_Specific_Matches_And_Fallback()
    {
        // Configure Article to also accept .jpg
        var mediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.ArticleAlias)!;
        var uploadPropertyType = mediaType.PropertyTypes.Single(pt => pt.Alias == Constants.Conventions.Media.File);

        var dataTypeService = GetRequiredService<IDataTypeService>();
        var dataType = (await dataTypeService.GetAsync(uploadPropertyType.DataTypeKey))!;
        dataType.ConfigurationData["fileExtensions"] = new[] { "pdf", "jpg" };
        await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);

        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync("jpg", 0, 100);

        // Should have Image (specific), Article (specific), File (fallback).
        Assert.That(result.Total, Is.EqualTo(3));

        MediaTypeFileExtensionMatchResult image = result.Items.First(r => r.MediaType.Alias == Constants.Conventions.MediaTypes.Image);
        Assert.That(image.IsSpecificMatch, Is.True);

        MediaTypeFileExtensionMatchResult article = result.Items.First(r => r.MediaType.Alias == Constants.Conventions.MediaTypes.ArticleAlias);
        Assert.That(article.IsSpecificMatch, Is.True);

        MediaTypeFileExtensionMatchResult file = result.Items.First(r => r.MediaType.Alias == Constants.Conventions.MediaTypes.File);
        Assert.That(file.IsSpecificMatch, Is.False);
    }

    [Test]
    public async Task Supports_Skip_Take_With_Match_Info()
    {
        // Configure Article to also accept .jpg so we have 3 results: Image, Article, File.
        var mediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.ArticleAlias)!;
        var uploadPropertyType = mediaType.PropertyTypes.Single(pt => pt.Alias == Constants.Conventions.Media.File);

        var dataTypeService = GetRequiredService<IDataTypeService>();
        var dataType = (await dataTypeService.GetAsync(uploadPropertyType.DataTypeKey))!;
        dataType.ConfigurationData["fileExtensions"] = new[] { "pdf", "jpg" };
        await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);

        // First page: should be a specific match.
        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync("jpg", 0, 1);
        Assert.That(result.Total, Is.EqualTo(3));
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Items.First().IsSpecificMatch, Is.True);

        // Last page: should be the File fallback (specific matches come before fallbacks).
        result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync("jpg", 2, 1);
        Assert.That(result.Total, Is.EqualTo(3));
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Items.First().MediaType.Alias, Is.EqualTo(Constants.Conventions.MediaTypes.File));
        Assert.That(result.Items.First().IsSpecificMatch, Is.False);
    }

    [TestCase("PDF")]
    [TestCase("Pdf")]
    [TestCase("pdf")]
    public async Task Extension_Matching_Is_Case_Insensitive(string fileExtension)
    {
        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync(fileExtension, 0, 100);

        Assert.That(
            result.Items.Any(r => r.MediaType.Alias == Constants.Conventions.MediaTypes.ArticleAlias && r.IsSpecificMatch), Is.True,
            $"Article should be a specific match for extension '{fileExtension}'");
    }

    [Test]
#pragma warning disable CS0618 // Type or member is obsolete
    public async Task GetMediaTypesForFileExtension_Excludes_Fallbacks_When_Specific_Match_Exists()
    {
        // The obsolete GetMediaTypesForFileExtensionAsync should preserve its original behavior:
        // only return fallback types (like File) when there are NO specific extension matches.
        // For .pdf, Article is a specific match, so File should NOT be returned.
        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionAsync("pdf", 0, 100);

        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(result.Items.First().Alias, Is.EqualTo(Constants.Conventions.MediaTypes.ArticleAlias));
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
