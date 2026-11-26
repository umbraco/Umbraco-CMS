using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Verifies that rebuilding the media database cache writes the same cache data as saving the media item,
/// for both culture-variant and invariant media types.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaCultureVarianceCacheRebuildTests : UmbracoIntegrationTest
{
    private const string EnglishCulture = "en-US";
    private const string DanishCulture = "da-DK";

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IDatabaseCacheRepository DatabaseCacheRepository => GetRequiredService<IDatabaseCacheRepository>();

    private IMediaCacheService MediaCacheService => GetRequiredService<IMediaCacheService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ISqlContext SqlContext => GetRequiredService<ISqlContext>();

    [SetUp]
    public async Task SetupLanguages()
        => await LanguageService.CreateAsync(new Language(DanishCulture, "Danish"), Constants.Security.SuperUserKey);

    [Test]
    public async Task Rebuild_Writes_Culture_Names_And_Values_For_Variant_Media()
    {
        IMediaType mediaType = await CreateMediaTypeAsync(variesByCulture: true);
        IMedia media = SaveVariantMedia(mediaType);

        DeleteCacheRow(media.Id);
        MediaCacheService.Rebuild([mediaType.Id]);

        ContentData data = await GetCacheDataAsync(media.Key);
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(data.CultureInfos);
            Assert.That(data.CultureInfos!.Keys, Is.EquivalentTo(new[] { EnglishCulture, DanishCulture }));
            Assert.AreEqual("English Name", data.CultureInfos[EnglishCulture].Name);
            Assert.AreEqual("Danish Name", data.CultureInfos[DanishCulture].Name);

            Assert.That(
                PropertyValues(data, "title"),
                Is.EquivalentTo(new[] { (EnglishCulture, "English Title"), (DanishCulture, "Danish Title") }));
            Assert.That(
                PropertyValues(data, "bodyText"),
                Is.EquivalentTo(new[] { (string.Empty, "Invariant Body") }));
        });
    }

    [Test]
    public async Task Rebuild_Produces_Same_Cache_Data_As_Save_For_Variant_Media()
    {
        IMediaType mediaType = await CreateMediaTypeAsync(variesByCulture: true);
        IMedia media = SaveVariantMedia(mediaType);

        await AssertRebuildMatchesSaveAsync(mediaType, media);
    }

    [Test]
    public async Task Rebuild_Produces_Same_Cache_Data_As_Save_For_Invariant_Media()
    {
        IMediaType mediaType = await CreateMediaTypeAsync(variesByCulture: false);

        IMedia media = MediaService.CreateMedia("Invariant Name", Constants.System.Root, mediaType.Alias);
        media.SetValue("title", "Invariant Title");
        media.SetValue("bodyText", "Invariant Body");
        MediaService.Save(media);

        await AssertRebuildMatchesSaveAsync(mediaType, media);
    }

    private async Task AssertRebuildMatchesSaveAsync(IMediaType mediaType, IMedia media)
    {
        ContentData saved = await GetCacheDataAsync(media.Key);

        DeleteCacheRow(media.Id);
        MediaCacheService.Rebuild([mediaType.Id]);

        ContentData rebuilt = await GetCacheDataAsync(media.Key);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(saved.Name, rebuilt.Name);
            Assert.AreEqual(saved.UrlSegment, rebuilt.UrlSegment);
            Assert.That(CultureNames(rebuilt), Is.EquivalentTo(CultureNames(saved)));
            Assert.That(rebuilt.Properties.Keys, Is.EquivalentTo(saved.Properties.Keys));
            foreach (var alias in saved.Properties.Keys)
            {
                Assert.That(PropertyValues(rebuilt, alias), Is.EquivalentTo(PropertyValues(saved, alias)), $"Property \"{alias}\" differs.");
            }
        });
    }

    private async Task<IMediaType> CreateMediaTypeAsync(bool variesByCulture)
    {
        IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("testMedia", "Test Media");
        mediaType.AllowedAsRoot = true;
        if (variesByCulture)
        {
            mediaType.Variations = ContentVariation.Culture;
            mediaType.PropertyTypes.Single(x => x.Alias == "title").Variations = ContentVariation.Culture;
        }

        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        return mediaType;
    }

    private IMedia SaveVariantMedia(IMediaType mediaType)
    {
        IMedia media = MediaService.CreateMedia("ignored", Constants.System.Root, mediaType.Alias);
        media.SetCultureName("English Name", EnglishCulture);
        media.SetCultureName("Danish Name", DanishCulture);
        media.SetValue("title", "English Title", EnglishCulture);
        media.SetValue("title", "Danish Title", DanishCulture);
        media.SetValue("bodyText", "Invariant Body");
        MediaService.Save(media);
        return media;
    }

    private void DeleteCacheRow(int nodeId)
    {
        using var scope = ScopeProvider.CreateScope();
        ScopeAccessor.AmbientScope!.Database.Execute(
            SqlContext.Sql().Delete<ContentNuDto>().Where<ContentNuDto>(x => x.NodeId == nodeId));
        scope.Complete();
    }

    private async Task<ContentData> GetCacheDataAsync(Guid key)
    {
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        ContentCacheNode? node = await DatabaseCacheRepository.GetMediaSourceAsync(key);
        Assert.IsNotNull(node?.Data, "Expected a media cache row.");
        return node!.Data!;
    }

    private static IEnumerable<(string Culture, string? Name)> CultureNames(ContentData data)
        => data.CultureInfos?.Select(x => (x.Key, x.Value.Name)) ?? [];

    private static IEnumerable<(string Culture, string? Value)> PropertyValues(ContentData data, string alias)
        => data.Properties.TryGetValue(alias, out PropertyData[]? values)
            ? values.Select(x => (x.Culture ?? string.Empty, x.Value?.ToString()))
            : [];
}
