using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
/// Integration tests proving that <see cref="IMediaEditingService"/> can create,
/// update, and delete culture-variant media.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaEditingServiceCultureVarianceTests : UmbracoIntegrationTest
{
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [SetUp]
    public async Task SetupLanguages()
    {
        var langDa = new Language("da-DK", "Danish");
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);
    }

    private async Task<IMediaType> CreateCultureVariantMediaTypeAsync()
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("variantMedia", "Variant Media");
        mediaType.Variations = ContentVariation.Culture;

        foreach (var propertyType in mediaType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        mediaType.AllowedAsRoot = true;
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        return mediaType;
    }

    [Test]
    public async Task Can_Create_Culture_Variant_Media()
    {
        var mediaType = await CreateCultureVariantMediaTypeAsync();

        var createModel = new MediaCreateModel
        {
            Key = Guid.NewGuid(),
            ContentTypeKey = mediaType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "English Image" },
                new VariantModel { Culture = "da-DK", Name = "Danish Image" },
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "English Title", Culture = "en-US" },
                new PropertyValueModel { Alias = "title", Value = "Dansk Titel", Culture = "da-DK" },
            ],
        };

        var result = await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success, $"Create failed with status: {result.Status}");
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        var media = result.Result!.Content!;
        Assert.AreEqual("English Title", media.GetValue("title", "en-US"));
        Assert.AreEqual("Dansk Titel", media.GetValue("title", "da-DK"));
        Assert.AreEqual("English Image", media.GetCultureName("en-US"));
        Assert.AreEqual("Danish Image", media.GetCultureName("da-DK"));
    }

    [Test]
    public async Task Culture_Variant_Values_Survive_Roundtrip()
    {
        var mediaType = await CreateCultureVariantMediaTypeAsync();

        var mediaKey = Guid.NewGuid();
        var createModel = new MediaCreateModel
        {
            Key = mediaKey,
            ContentTypeKey = mediaType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "English Media" },
                new VariantModel { Culture = "da-DK", Name = "Danish Media" },
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "EN Title", Culture = "en-US" },
                new PropertyValueModel { Alias = "title", Value = "DA Title", Culture = "da-DK" },
            ],
        };

        var createResult = await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success, $"Create failed: {createResult.Status}");

        // Re-fetch from database to prove persistence
        var fetched = MediaService.GetById(mediaKey);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("English Media", fetched!.GetCultureName("en-US"));
        Assert.AreEqual("Danish Media", fetched.GetCultureName("da-DK"));
        Assert.AreEqual("EN Title", fetched.GetValue("title", "en-US"));
        Assert.AreEqual("DA Title", fetched.GetValue("title", "da-DK"));
    }

    [Test]
    public async Task Can_Update_Culture_Variant_Media()
    {
        var mediaType = await CreateCultureVariantMediaTypeAsync();

        var mediaKey = Guid.NewGuid();
        var createModel = new MediaCreateModel
        {
            Key = mediaKey,
            ContentTypeKey = mediaType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Original EN" },
                new VariantModel { Culture = "da-DK", Name = "Original DA" },
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Title EN", Culture = "en-US" },
                new PropertyValueModel { Alias = "title", Value = "Title DA", Culture = "da-DK" },
            ],
        };

        var createResult = await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success, $"Create failed: {createResult.Status}");

        // Update only the English culture
        var updateModel = new MediaUpdateModel
        {
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated EN" },
                new VariantModel { Culture = "da-DK", Name = "Original DA" },
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Updated Title EN", Culture = "en-US" },
                new PropertyValueModel { Alias = "title", Value = "Title DA", Culture = "da-DK" },
            ],
        };

        var updateResult = await MediaEditingService.UpdateAsync(mediaKey, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(updateResult.Success, $"Update failed: {updateResult.Status}");

        // Re-fetch and verify
        var fetched = MediaService.GetById(mediaKey);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("Updated Title EN", fetched!.GetValue("title", "en-US"));
        Assert.AreEqual("Title DA", fetched.GetValue("title", "da-DK"));
        Assert.AreEqual("Updated EN", fetched.GetCultureName("en-US"));
        Assert.AreEqual("Original DA", fetched.GetCultureName("da-DK"));
    }

    [Test]
    public async Task Can_Delete_Culture_Variant_Media()
    {
        var mediaType = await CreateCultureVariantMediaTypeAsync();

        var mediaKey = Guid.NewGuid();
        var createModel = new MediaCreateModel
        {
            Key = mediaKey,
            ContentTypeKey = mediaType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "To Delete EN" },
                new VariantModel { Culture = "da-DK", Name = "To Delete DA" },
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Delete EN", Culture = "en-US" },
                new PropertyValueModel { Alias = "title", Value = "Delete DA", Culture = "da-DK" },
            ],
        };

        var createResult = await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success, $"Create failed: {createResult.Status}");

        var deleteResult = await MediaEditingService.DeleteAsync(mediaKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(deleteResult.Success, $"Delete failed: {deleteResult.Status}");

        Assert.IsNull(MediaService.GetById(mediaKey));
    }
}
