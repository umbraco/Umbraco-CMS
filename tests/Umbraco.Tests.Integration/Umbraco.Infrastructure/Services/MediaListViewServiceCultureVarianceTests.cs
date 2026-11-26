using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// Verifies that the media list view orders culture-variant media by the requested culture.
/// </summary>
internal sealed class MediaListViewServiceCultureVarianceTests : ContentListViewServiceTestsBase
{
    private const string EnglishCulture = "en-US";
    private const string DanishCulture = "da-DK";

    private IMediaListViewService MediaListViewService => GetRequiredService<IMediaListViewService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IUser SuperUser { get; set; } = null!;

    [SetUp]
    public new async Task Setup()
    {
        SuperUser = await GetSuperUser();
        await LanguageService.CreateAsync(new Language(DanishCulture, "Danish"), Constants.Security.SuperUserKey);
    }

    [TestCase(EnglishCulture, new[] { "Apple", "Zebra" })]
    [TestCase(DanishCulture, new[] { "Abe", "Ørn" })]
    public async Task Can_Order_Variant_Media_By_Name_In_Requested_Culture(string orderCulture, string[] expectedNames)
    {
        // The two items sort in opposite orders in the two cultures, so a culture-blind sort
        // (on the invariant name, which follows the default culture) cannot pass for both cultures.
        IMediaType mediaType = await CreateVariantMediaTypeAsync();
        SaveVariantMedia(mediaType, englishName: "Apple", danishName: "Ørn");
        SaveVariantMedia(mediaType, englishName: "Zebra", danishName: "Abe");

        var result = await MediaListViewService.GetListViewItemsByKeyAsync(
            SuperUser,
            null,
            null,
            "name",
            orderCulture,
            Direction.Ascending,
            null,
            0,
            10);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentCollectionOperationStatus.Success, result.Status);
            Assert.That(
                result.Result!.Items.Items.Select(x => x.GetCultureName(orderCulture)),
                Is.EqualTo(expectedNames));
        });
    }

    private async Task<IMediaType> CreateVariantMediaTypeAsync()
    {
        IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("variantMedia", "Variant Media");
        mediaType.AllowedAsRoot = true;
        mediaType.Variations = ContentVariation.Culture;
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        return mediaType;
    }

    private void SaveVariantMedia(IMediaType mediaType, string englishName, string danishName)
    {
        IMedia media = MediaService.CreateMedia("ignored", Constants.System.Root, mediaType.Alias);
        media.SetCultureName(englishName, EnglishCulture);
        media.SetCultureName(danishName, DanishCulture);
        MediaService.Save(media);
    }
}
