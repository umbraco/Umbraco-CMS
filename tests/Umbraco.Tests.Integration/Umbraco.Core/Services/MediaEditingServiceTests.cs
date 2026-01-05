using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaEditingServiceTests : UmbracoIntegrationTest
{
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IMediaType ImageMediaType { get; set; }

    private IMediaType ArticleMediaType { get; set; }

    [SetUp]
    public async Task Setup()
    {
        ImageMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Image);
        ArticleMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.ArticleAlias);
    }

    [Test]
    public async Task Cannot_Create_Media_With_Mandatory_File_Property_Without_Providing_File()
    {
        var imageModel = CreateMediaCreateModel("Image", Guid.NewGuid(), ImageMediaType.Key);
        var imageCreateAttempt = await MediaEditingService.CreateAsync(imageModel, Constants.Security.SuperUserKey);

        Assert.IsFalse(imageCreateAttempt.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyValidationError, imageCreateAttempt.Status);
    }

    [Test]
    public async Task Can_Create_Media_With_Mandatory_File_Property_With_File_Provided()
    {
        var imageModel = CreateMediaCreateModelWithFile("Image", Guid.NewGuid(), ArticleMediaType.Key);
        var imageCreateAttempt = await MediaEditingService.CreateAsync(imageModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(imageCreateAttempt.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, imageCreateAttempt.Status);
    }

    [Test]
    public async Task Can_Create_Media_With_Optional_File_Property_Without_Providing_File()
    {
        ImageMediaType.PropertyTypes.First(x => x.Alias == Constants.Conventions.Media.File).Mandatory = false;
        MediaTypeService.Save(ImageMediaType);
        var imageModel = CreateMediaCreateModel("Image", Guid.NewGuid(), ImageMediaType.Key);
        var imageCreateAttempt = await MediaEditingService.CreateAsync(imageModel, Constants.Security.SuperUserKey);

        // Assert
        Assert.IsTrue(imageCreateAttempt.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, imageCreateAttempt.Status);
    }

    [Test]
    public async Task Can_Update_Media_With_Mandatory_File_Property_With_File_Provided()
    {
        Guid articleKey = Guid.NewGuid();
        var articleModel = CreateMediaCreateModelWithFile("Article", articleKey, ArticleMediaType.Key);
        var articleCreateAttempt = await MediaEditingService.CreateAsync(articleModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(articleCreateAttempt.Success);

        var updateModel = new MediaUpdateModel
        {
            Properties =
            [
                new PropertyValueModel
                {
                    Alias = Constants.Conventions.Media.File,
                    Value = new JsonObject
                    {
                        { "src", string.Empty },
                    },
                }
            ],
            Variants = articleModel.Variants,
        };
        var articleUpdateAttempt = await MediaEditingService.ValidateUpdateAsync(articleKey, updateModel);
        Assert.IsFalse(articleUpdateAttempt.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyValidationError, articleUpdateAttempt.Status);
    }

    private static MediaCreateModel CreateMediaCreateModel(string name, Guid key, Guid mediaTypeKey)
        => new()
        {
            ContentTypeKey = mediaTypeKey,
            ParentKey = Constants.System.RootKey,
            Variants = [new() { Name = name }],
            Key = key,
        };

    private static MediaCreateModel CreateMediaCreateModelWithFile(string name, Guid key, Guid mediaTypeKey)
    {
        var model = CreateMediaCreateModel(name, key, mediaTypeKey);
        model.Properties = [
                new PropertyValueModel
                {
                    Alias = Constants.Conventions.Media.File,
                    Value = new JsonObject
                    {
                        { "src", "reference-to-file" },
                    },
                }
            ];
        return model;
    }
}
