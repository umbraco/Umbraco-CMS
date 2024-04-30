using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class MediaTypeEditingServiceTests
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
    [TestCase("abc", Constants.Conventions.MediaTypes.File)]
    [TestCase("123", Constants.Conventions.MediaTypes.File)]
    public async Task Can_Get_Default_Allowed_Media_Types(string fileExtension, string expectedMediaTypeAlias)
    {
        var allowedMediaTypes = await MediaTypeEditingService.GetMediaTypesForFileExtension(fileExtension);
        Assert.AreEqual(1, allowedMediaTypes.Count());
        Assert.AreEqual(expectedMediaTypeAlias, allowedMediaTypes.First().Alias);
    }

    [TestCase("jpg")]
    [TestCase(".jpg")]
    public async Task Ignores_Trailing_Period_For_Allowed_Media_Types(string fileExtension)
    {
        var allowedMediaTypes = await MediaTypeEditingService.GetMediaTypesForFileExtension(fileExtension);
        Assert.AreEqual(1, allowedMediaTypes.Count());
        Assert.AreEqual(Constants.Conventions.MediaTypes.Image, allowedMediaTypes.First().Alias);
    }

    [Test]
    public async Task Can_Yield_Multiple_Allowed_Media_Types()
    {
        var mediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.ArticleAlias)!;
        var uploadPropertyType = mediaType.PropertyTypes.Single(pt => pt.Alias == Constants.Conventions.Media.File);

        var dataTypeService = GetRequiredService<IDataTypeService>();
        var dataType = (await dataTypeService.GetAsync(uploadPropertyType.DataTypeKey))!;
        dataType.ConfigurationData["fileExtensions"] = new[] { "pdf", "jpg" };
        await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);

        var allowedMediaTypes = await MediaTypeEditingService.GetMediaTypesForFileExtension("jpg");
        Assert.AreEqual(2, allowedMediaTypes.Count());
        Assert.AreEqual(Constants.Conventions.MediaTypes.Image, allowedMediaTypes.First().Alias);
        Assert.AreEqual(Constants.Conventions.MediaTypes.ArticleAlias, allowedMediaTypes.Last().Alias);
    }
}
