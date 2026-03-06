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

        // Should contain the specific match and the File fallback
        Assert.IsTrue(result.Total >= 2);

        MediaTypeFileExtensionMatchResult specific = result.Items.First(r => r.MediaType.Alias == expectedMediaTypeAlias);
        Assert.IsTrue(specific.IsSpecificMatch);

        MediaTypeFileExtensionMatchResult fallback = result.Items.First(r => r.MediaType.Alias == Constants.Conventions.MediaTypes.File);
        Assert.IsFalse(fallback.IsSpecificMatch);
    }

    [TestCase("abc")]
    [TestCase("123")]
    public async Task Unknown_Extension_Returns_File_As_Fallback(string fileExtension)
    {
        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync(fileExtension, 0, 100);

        Assert.AreEqual(1, result.Total);

        MediaTypeFileExtensionMatchResult fallback = result.Items.First();
        Assert.AreEqual(Constants.Conventions.MediaTypes.File, fallback.MediaType.Alias);
        Assert.IsFalse(fallback.IsSpecificMatch);
    }

    [TestCase("jpg")]
    [TestCase(".jpg")]
    public async Task Ignores_Leading_Period_For_Allowed_Media_Types(string fileExtension)
    {
        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync(fileExtension, 0, 100);

        Assert.IsTrue(result.Total >= 2);
        Assert.IsTrue(result.Items.Any(r => r.MediaType.Alias == Constants.Conventions.MediaTypes.Image && r.IsSpecificMatch));
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

        // Should have Image (specific), Article (specific), File (fallback)
        Assert.AreEqual(3, result.Total);

        MediaTypeFileExtensionMatchResult image = result.Items.First(r => r.MediaType.Alias == Constants.Conventions.MediaTypes.Image);
        Assert.IsTrue(image.IsSpecificMatch);

        MediaTypeFileExtensionMatchResult article = result.Items.First(r => r.MediaType.Alias == Constants.Conventions.MediaTypes.ArticleAlias);
        Assert.IsTrue(article.IsSpecificMatch);

        MediaTypeFileExtensionMatchResult file = result.Items.First(r => r.MediaType.Alias == Constants.Conventions.MediaTypes.File);
        Assert.IsFalse(file.IsSpecificMatch);
    }

    [Test]
    public async Task Supports_Skip_Take_With_Match_Info()
    {
        // Configure Article to also accept .jpg so we have 3 results: Image, Article, File
        var mediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.ArticleAlias)!;
        var uploadPropertyType = mediaType.PropertyTypes.Single(pt => pt.Alias == Constants.Conventions.Media.File);

        var dataTypeService = GetRequiredService<IDataTypeService>();
        var dataType = (await dataTypeService.GetAsync(uploadPropertyType.DataTypeKey))!;
        dataType.ConfigurationData["fileExtensions"] = new[] { "pdf", "jpg" };
        await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);

        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync("jpg", 0, 1);
        Assert.AreEqual(3, result.Total);
        Assert.AreEqual(1, result.Items.Count());

        result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync("jpg", 2, 1);
        Assert.AreEqual(3, result.Total);
        Assert.AreEqual(1, result.Items.Count());
    }
}
