using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
/// Integration tests proving that media supports culture/segment variance
/// without publishing. Tests the repository persistence, hydration, and
/// slim entity loading of culture-variant media.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaServiceCultureVarianceTests : UmbracoIntegrationTest
{
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    [SetUp]
    public async Task SetupLanguages()
    {
        // Create Danish language (en-US exists by default)
        var langDa = new Language("da-DK", "Danish");
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);
    }

    private async Task<IMediaType> CreateCultureVariantMediaTypeAync()
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("variantMedia", "Variant Media");
        mediaType.Variations = ContentVariation.Culture;

        // Set all properties to vary by culture
        foreach (var propertyType in mediaType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        mediaType.AllowedAsRoot = true;
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        return mediaType;
    }

    [Test]
    public async Task Can_Create_Media_With_Culture_Variant_Properties()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        var media = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media.SetCultureName("English Image", "en-US");
        media.SetCultureName("Danish Image", "da-DK");
        media.SetValue("title", "English Title", "en-US");
        media.SetValue("title", "Dansk Titel", "da-DK");

        var saveResult = MediaService.Save(media);
        Assert.IsTrue(saveResult.Success, $"Save failed: {saveResult.Result?.Result}");

        Assert.AreEqual("English Title", media.GetValue("title", "en-US"));
        Assert.AreEqual("Dansk Titel", media.GetValue("title", "da-DK"));
        Assert.AreEqual("English Image", media.GetCultureName("en-US"));
        Assert.AreEqual("Danish Image", media.GetCultureName("da-DK"));
    }

    [Test]
    public async Task Culture_Variant_Values_Survive_Roundtrip_Through_Repository()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        var media = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media.SetCultureName("English Media", "en-US");
        media.SetCultureName("Danish Media", "da-DK");
        media.SetValue("title", "EN Title", "en-US");
        media.SetValue("title", "DA Title", "da-DK");
        media.SetValue("bodyText", "EN Body", "en-US");
        media.SetValue("bodyText", "DA Body", "da-DK");

        MediaService.Save(media);

        // Re-fetch from database to prove values were persisted
        var fetched = MediaService.GetById(media.Key);
        Assert.IsNotNull(fetched);

        // Verify culture names survived round-trip
        Assert.AreEqual("English Media", fetched!.GetCultureName("en-US"));
        Assert.AreEqual("Danish Media", fetched.GetCultureName("da-DK"));

        // Verify culture property values survived round-trip
        Assert.AreEqual("EN Title", fetched.GetValue("title", "en-US"));
        Assert.AreEqual("DA Title", fetched.GetValue("title", "da-DK"));
        Assert.AreEqual("EN Body", fetched.GetValue("bodyText", "en-US"));
        Assert.AreEqual("DA Body", fetched.GetValue("bodyText", "da-DK"));
    }

    [Test]
    public async Task Can_Update_Single_Culture_Without_Affecting_Other()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        var media = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media.SetCultureName("English", "en-US");
        media.SetCultureName("Danish", "da-DK");
        media.SetValue("title", "Original EN", "en-US");
        media.SetValue("title", "Original DA", "da-DK");
        MediaService.Save(media);

        // Re-fetch, update only EN
        var fetched = MediaService.GetById(media.Key)!;
        fetched.SetCultureName("Updated English", "en-US");
        fetched.SetValue("title", "Updated EN", "en-US");
        MediaService.Save(fetched);

        // Re-fetch again and verify
        var refetched = MediaService.GetById(media.Key)!;
        Assert.AreEqual("Updated EN", refetched.GetValue("title", "en-US"));
        Assert.AreEqual("Original DA", refetched.GetValue("title", "da-DK"));
        Assert.AreEqual("Updated English", refetched.GetCultureName("en-US"));
        Assert.AreEqual("Danish", refetched.GetCultureName("da-DK"));
    }

    [Test]
    public async Task Invariant_Media_Still_Works_With_No_Culture()
    {
        // Use a standard invariant media type
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("invariantMedia", "Invariant Media");
        mediaType.AllowedAsRoot = true;
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var media = MediaService.CreateMedia("My Invariant Media", Constants.System.Root, mediaType.Alias);
        media.SetValue("title", "Invariant Title");
        MediaService.Save(media);

        var fetched = MediaService.GetById(media.Key);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("My Invariant Media", fetched!.Name);
        Assert.AreEqual("Invariant Title", fetched.GetValue("title"));
    }

    [Test]
    public async Task Slim_Entity_Has_Culture_Names_For_Variant_Media()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        var media = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media.SetCultureName("EN Slim", "en-US");
        media.SetCultureName("DA Slim", "da-DK");
        media.SetValue("title", "Title EN", "en-US");
        media.SetValue("title", "Title DA", "da-DK");
        MediaService.Save(media);

        // Fetch as slim entity via EntityService using the typed GetAll (which goes through the media-specific path)
        var entities = EntityService.GetAll(UmbracoObjectTypes.Media, new[] { media.Key }).ToList();
        Assert.AreEqual(1, entities.Count);
        Assert.IsInstanceOf<IMediaEntitySlim>(entities[0]);

        var mediaSlim = (IMediaEntitySlim)entities[0];
        Assert.IsTrue(mediaSlim.CultureNames.Count > 0, "CultureNames should be populated for variant media");
        Assert.AreEqual("EN Slim", mediaSlim.CultureNames["en-US"]);
        Assert.AreEqual("DA Slim", mediaSlim.CultureNames["da-DK"]);
    }

    [Test]
    public async Task Slim_Entity_Has_Empty_Culture_Names_For_Invariant_Media()
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("invariantMedia", "Invariant Media");
        mediaType.AllowedAsRoot = true;
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var media = MediaService.CreateMedia("Invariant Media", Constants.System.Root, mediaType.Alias);
        media.SetValue("title", "Title");
        MediaService.Save(media);

        var entity = EntityService.Get(media.Key);
        Assert.IsNotNull(entity);
        Assert.IsInstanceOf<IMediaEntitySlim>(entity);

        var mediaSlim = (IMediaEntitySlim)entity!;
        Assert.AreEqual(0, mediaSlim.CultureNames.Count, "CultureNames should be empty for invariant media");
    }

    [Test]
    public async Task Single_Slim_Entity_By_Key_Has_Culture_Names_For_Variant_Media()
    {
        IMedia media = await CreateAndSaveVariantMediaAsync();

        IEntitySlim? entity = EntityService.Get(media.Key, UmbracoObjectTypes.Media);

        AssertHasCultureNames(entity);
    }

    [Test]
    public async Task Single_Slim_Entity_By_Id_Has_Culture_Names_For_Variant_Media()
    {
        IMedia media = await CreateAndSaveVariantMediaAsync();

        IEntitySlim? entity = EntityService.Get(media.Id, UmbracoObjectTypes.Media);

        AssertHasCultureNames(entity);
    }

    private async Task<IMedia> CreateAndSaveVariantMediaAsync()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        var media = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media.SetCultureName("EN Single", "en-US");
        media.SetCultureName("DA Single", "da-DK");
        MediaService.Save(media);
        return media;
    }

    private static void AssertHasCultureNames(IEntitySlim? entity)
    {
        Assert.IsNotNull(entity);
        Assert.IsInstanceOf<IMediaEntitySlim>(entity);

        var mediaSlim = (IMediaEntitySlim)entity!;
        Assert.AreEqual(ContentVariation.Culture, mediaSlim.Variations);
        Assert.AreEqual(2, mediaSlim.CultureNames.Count);
        Assert.AreEqual("EN Single", mediaSlim.CultureNames["en-US"]);
        Assert.AreEqual("DA Single", mediaSlim.CultureNames["da-DK"]);
    }

    [Test]
    public async Task Culture_Variant_Media_Can_Be_Deleted()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        var media = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media.SetCultureName("To Delete EN", "en-US");
        media.SetCultureName("To Delete DA", "da-DK");
        media.SetValue("title", "Delete Me EN", "en-US");
        media.SetValue("title", "Delete Me DA", "da-DK");
        MediaService.Save(media);

        // Delete should not throw (culture variation records are cleaned up)
        Assert.DoesNotThrow(() => MediaService.Delete(media));

        // Verify gone
        var fetched = MediaService.GetById(media.Key);
        Assert.IsNull(fetched);
    }

    [Test]
    public async Task GetAll_Returns_Variant_Media_With_Culture_Info()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        var media1 = MediaService.CreateMedia("test1", Constants.System.Root, mediaType.Alias);
        media1.SetCultureName("Media 1 EN", "en-US");
        media1.SetCultureName("Media 1 DA", "da-DK");
        media1.SetValue("title", "Title 1 EN", "en-US");
        media1.SetValue("title", "Title 1 DA", "da-DK");
        MediaService.Save(media1);

        var media2 = MediaService.CreateMedia("test2", Constants.System.Root, mediaType.Alias);
        media2.SetCultureName("Media 2 EN", "en-US");
        media2.SetCultureName("Media 2 DA", "da-DK");
        media2.SetValue("title", "Title 2 EN", "en-US");
        media2.SetValue("title", "Title 2 DA", "da-DK");
        MediaService.Save(media2);

        // Fetch all via GetAll (batch path)
        var all = MediaService.GetByIds(new[] { media1.Key, media2.Key }).ToList();
        Assert.AreEqual(2, all.Count);

        var first = all.First(m => m.Key == media1.Key);
        Assert.AreEqual("Media 1 EN", first.GetCultureName("en-US"));
        Assert.AreEqual("Title 1 DA", first.GetValue("title", "da-DK"));

        var second = all.First(m => m.Key == media2.Key);
        Assert.AreEqual("Media 2 DA", second.GetCultureName("da-DK"));
        Assert.AreEqual("Title 2 EN", second.GetValue("title", "en-US"));
    }

    // ── Name uniqueness tests ───────────────────────────────────────────

    [Test]
    public async Task Invariant_Name_Is_Made_Unique_Among_Siblings()
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("invariantMedia", "Invariant Media");
        mediaType.AllowedAsRoot = true;
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var media1 = MediaService.CreateMedia("My Image", Constants.System.Root, mediaType.Alias);
        media1.SetValue("title", "Title 1");
        MediaService.Save(media1);

        // Second media with exact same name — should be made unique
        var media2 = MediaService.CreateMedia("My Image", Constants.System.Root, mediaType.Alias);
        media2.SetValue("title", "Title 2");
        MediaService.Save(media2);

        // The second should have been renamed to avoid collision
        Assert.AreEqual("My Image", media1.Name);
        Assert.AreEqual("My Image (1)", media2.Name);
    }

    [Test]
    public async Task Variant_Invariant_Name_Is_Derived_From_Default_Culture()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        var media = MediaService.CreateMedia("placeholder", Constants.System.Root, mediaType.Alias);
        media.SetCultureName("English Name", "en-US");
        media.SetCultureName("Danish Name", "da-DK");
        media.SetValue("title", "EN", "en-US");
        media.SetValue("title", "DA", "da-DK");
        MediaService.Save(media);

        // en-US is the default language — invariant name should come from it
        Assert.AreEqual("English Name", media.Name);
    }

    [Test]
    public async Task Variant_Culture_Names_Are_Made_Unique_Among_Siblings()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        // Create first media with culture names
        var media1 = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media1.SetCultureName("Photo", "en-US");
        media1.SetCultureName("Foto", "da-DK");
        media1.SetValue("title", "Title 1 EN", "en-US");
        media1.SetValue("title", "Title 1 DA", "da-DK");
        MediaService.Save(media1);

        // Create second media with the same culture names — should be made unique per culture
        var media2 = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media2.SetCultureName("Photo", "en-US");
        media2.SetCultureName("Foto", "da-DK");
        media2.SetValue("title", "Title 2 EN", "en-US");
        media2.SetValue("title", "Title 2 DA", "da-DK");
        MediaService.Save(media2);

        // First media keeps original names
        Assert.AreEqual("Photo", media1.GetCultureName("en-US"));
        Assert.AreEqual("Foto", media1.GetCultureName("da-DK"));

        // Second media should have suffixed names
        Assert.AreEqual("Photo (1)", media2.GetCultureName("en-US"));
        Assert.AreEqual("Foto (1)", media2.GetCultureName("da-DK"));
    }

    [Test]
    public async Task Variant_Culture_Names_Only_Clash_Within_Same_Culture()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        var media1 = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media1.SetCultureName("Photo", "en-US");
        media1.SetCultureName("Bild", "da-DK");
        media1.SetValue("title", "T1 EN", "en-US");
        media1.SetValue("title", "T1 DA", "da-DK");
        MediaService.Save(media1);

        // Same EN name, different DA name — only EN should clash
        var media2 = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media2.SetCultureName("Photo", "en-US");
        media2.SetCultureName("Foto", "da-DK");
        media2.SetValue("title", "T2 EN", "en-US");
        media2.SetValue("title", "T2 DA", "da-DK");
        MediaService.Save(media2);

        Assert.AreEqual("Photo (1)", media2.GetCultureName("en-US"), "EN name should be suffixed");
        Assert.AreEqual("Foto", media2.GetCultureName("da-DK"), "DA name should NOT be suffixed (no clash)");
    }

    [Test]
    public async Task Variant_Culture_Name_Uniqueness_Survives_Roundtrip()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        var media1 = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media1.SetCultureName("Shared Name", "en-US");
        media1.SetCultureName("Delt Navn", "da-DK");
        media1.SetValue("title", "T1 EN", "en-US");
        media1.SetValue("title", "T1 DA", "da-DK");
        MediaService.Save(media1);

        var media2 = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media2.SetCultureName("Shared Name", "en-US");
        media2.SetCultureName("Delt Navn", "da-DK");
        media2.SetValue("title", "T2 EN", "en-US");
        media2.SetValue("title", "T2 DA", "da-DK");
        MediaService.Save(media2);

        // Re-fetch from database
        var fetched = MediaService.GetById(media2.Key)!;
        Assert.AreEqual("Shared Name (1)", fetched.GetCultureName("en-US"));
        Assert.AreEqual("Delt Navn (1)", fetched.GetCultureName("da-DK"));
    }

    [Test]
    public async Task Update_Does_Not_Suffix_Own_Name()
    {
        var mediaType = await CreateCultureVariantMediaTypeAync();

        var media = MediaService.CreateMedia("test", Constants.System.Root, mediaType.Alias);
        media.SetCultureName("My Photo", "en-US");
        media.SetCultureName("Mit Foto", "da-DK");
        media.SetValue("title", "EN", "en-US");
        media.SetValue("title", "DA", "da-DK");
        MediaService.Save(media);

        // Update without changing names — should NOT suffix
        var fetched = MediaService.GetById(media.Key)!;
        fetched.SetValue("title", "Updated EN", "en-US");
        MediaService.Save(fetched);

        var refetched = MediaService.GetById(media.Key)!;
        Assert.AreEqual("My Photo", refetched.GetCultureName("en-US"), "Name should not change on update without rename");
        Assert.AreEqual("Mit Foto", refetched.GetCultureName("da-DK"), "Name should not change on update without rename");
    }

    [Test]
    public async Task Switching_Media_Type_To_Culture_Variant_Moves_Invariant_Name_To_Default_Culture()
    {
        IMedia media = await CreateInvariantMediaAsync("Existing Photo");

        await SetMediaTypeVariationAsync(media.ContentType.Key, ContentVariation.Culture);

        IMedia fetched = MediaService.GetById(media.Key)!;
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Existing Photo", fetched.GetCultureName("en-US"));
            Assert.IsNull(fetched.GetCultureName("da-DK"));
        });
    }

    [Test]
    public async Task Can_Save_Existing_Media_After_Media_Type_Is_Switched_To_Culture_Variant()
    {
        IMedia media = await CreateInvariantMediaAsync("Existing Photo");

        await SetMediaTypeVariationAsync(media.ContentType.Key, ContentVariation.Culture);

        IMedia fetched = MediaService.GetById(media.Key)!;
        Attempt<OperationResult?> saveResult = MediaService.Save(fetched);

        IMedia refetched = MediaService.GetById(media.Key)!;
        Assert.Multiple(() =>
        {
            Assert.IsTrue(saveResult.Success);
            Assert.AreEqual("Existing Photo", refetched.Name);
            Assert.AreEqual("Existing Photo", refetched.GetCultureName("en-US"));
        });
    }

    [Test]
    public async Task Switching_Media_Type_Back_To_Invariant_Keeps_Invariant_Name()
    {
        IMedia media = await CreateInvariantMediaAsync("Existing Photo");

        await SetMediaTypeVariationAsync(media.ContentType.Key, ContentVariation.Culture);
        await SetMediaTypeVariationAsync(media.ContentType.Key, ContentVariation.Nothing);

        IMedia fetched = MediaService.GetById(media.Key)!;
        Assert.AreEqual("Existing Photo", fetched.Name);
    }

    private async Task<IMedia> CreateInvariantMediaAsync(string name)
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("switchingMedia", "Switching Media");
        mediaType.AllowedAsRoot = true;
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var media = MediaService.CreateMedia(name, Constants.System.Root, mediaType.Alias);
        MediaService.Save(media);
        return media;
    }

    private async Task SetMediaTypeVariationAsync(Guid mediaTypeKey, ContentVariation variation)
    {
        IMediaType mediaType = MediaTypeService.Get(mediaTypeKey)!;
        mediaType.Variations = variation;
        var result = await MediaTypeService.UpdateAsync(mediaType, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Updating the media type variation failed: {result.Result}");
    }
}
