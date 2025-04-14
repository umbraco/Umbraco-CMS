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
    protected IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    protected IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    protected IMediaType ImageMediaType { get; set; }

    [SetUp]
    public async Task Setup()
    {
        ImageMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Image);
    }

    [Test]
    public async Task Cannot_Create_Media_With_Mandatory_Property()
    {
        var imageModel = CreateMediaCreateModel("Image", new Guid(), ImageMediaType.Key);
        var imageCreateAttempt = await MediaEditingService.CreateAsync(imageModel, Constants.Security.SuperUserKey);

        // Assert
        Assert.IsFalse(imageCreateAttempt.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyValidationError, imageCreateAttempt.Status);
    }

    [Test]
    public async Task Can_Create_Media_Without_Mandatory_Property()
    {
        ImageMediaType.PropertyTypes.First(x => x.Alias == "umbracoFile").Mandatory = false;
        MediaTypeService.Save(ImageMediaType);
        var imageModel = CreateMediaCreateModel("Image", new Guid(), ImageMediaType.Key);
        var imageCreateAttempt = await MediaEditingService.CreateAsync(imageModel, Constants.Security.SuperUserKey);

        // Assert
        Assert.IsTrue(imageCreateAttempt.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, imageCreateAttempt.Status);
    }

    private MediaCreateModel CreateMediaCreateModel(string name, Guid key, Guid mediaTypeKey)
        => new()
        {
            ContentTypeKey = mediaTypeKey,
            ParentKey = Constants.System.RootKey,
            InvariantName = name,
            Key = key,
        };
}
