using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class MediaTypeEditingServiceTests
{
    [TestCase("pdf")]
    [TestCase("PDF")]
    [TestCase(".Pdf")]
    public async Task Provider_Support_Does_Not_Make_Pdf_An_Image(string fileExtension)
    {
        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync(fileExtension, 0, 100);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Items.Where(x => x.IsSpecificMatch).Select(x => x.MediaType.Alias),
                Is.EqualTo(new[] { Constants.Conventions.MediaTypes.ArticleAlias }));
            Assert.That(result.Items.Where(x => !x.IsSpecificMatch).Select(x => x.MediaType.Alias),
                Is.EqualTo(new[] { Constants.Conventions.MediaTypes.File }));
        });
    }

    [TestCase("eps")]
    [TestCase("webp")]
    public async Task Image_Matching_Requires_Configuration_And_Provider_Support(string fileExtension)
    {
        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync(fileExtension, 0, 100);

        AssertFileFallback(result, 1);
    }

    [TestCase("pdf")]
    [TestCase("PDF")]
    public async Task Configured_Image_Formats_Are_Respected(string configuredExtension)
    {
        var imaging = GetRequiredService<IOptionsMonitor<ContentSettings>>().CurrentValue.Imaging;
        var original = imaging.ImageFileTypes;
        imaging.ImageFileTypes = new HashSet<string> { configuredExtension };
        try
        {
            var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync("pdf", 0, 100);
            Assert.That(result.Items.Any(x => x.MediaType.Alias == Constants.Conventions.MediaTypes.Image && x.IsSpecificMatch), Is.True);

            result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync("jpg", 0, 100);
            Assert.That(result.Items.Any(x => x.MediaType.Alias == Constants.Conventions.MediaTypes.Image), Is.False);
        }
        finally
        {
            imaging.ImageFileTypes = original;
        }
    }

    [TestCase("jpg", "JPG")]
    [TestCase("png", "PnG")]
    public async Task Configured_But_Disallowed_Image_Formats_Do_Not_Match_Image(string extension, string disallowedExtension)
    {
        var settings = GetRequiredService<IOptionsMonitor<ContentSettings>>().CurrentValue;
        var original = settings.DisallowedUploadedFileExtensions;
        settings.DisallowedUploadedFileExtensions = new HashSet<string>(original) { disallowedExtension };
        try
        {
            var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync(extension, 0, 100);
            AssertFileFallback(result, 1);
        }
        finally
        {
            settings.DisallowedUploadedFileExtensions = original;
        }
    }

    [Test]
    public async Task Image_Matching_Uses_Reloaded_Content_Settings()
    {
        var service = MediaTypeEditingService;
        var monitor = GetRequiredService<IOptionsMonitor<ContentSettings>>();
        var cache = GetRequiredService<IOptionsMonitorCache<ContentSettings>>();
        var original = monitor.CurrentValue;
        var before = await service.GetMediaTypesForFileExtensionWithMatchInfoAsync("pdf", 0, 100);
        Assert.That(before.Items.Any(x => x.MediaType.Alias == Constants.Conventions.MediaTypes.Image), Is.False);
        var updated = new ContentSettings
        {
            Imaging = new ContentImagingSettings { ImageFileTypes = new HashSet<string> { "PDF" } },
        };
        try
        {
            cache.TryRemove(Options.DefaultName);
            Assert.That(cache.TryAdd(Options.DefaultName, updated), Is.True);
            Assert.That(monitor.CurrentValue, Is.SameAs(updated));

            var result = await service.GetMediaTypesForFileExtensionWithMatchInfoAsync("pdf", 0, 100);
            Assert.That(result.Items.Any(x => x.MediaType.Alias == Constants.Conventions.MediaTypes.Image && x.IsSpecificMatch), Is.True);
            result = await service.GetMediaTypesForFileExtensionWithMatchInfoAsync("jpg", 0, 100);
            Assert.That(result.Items.Any(x => x.MediaType.Alias == Constants.Conventions.MediaTypes.Image), Is.False);
        }
        finally
        {
            cache.TryRemove(Options.DefaultName);
            cache.TryAdd(Options.DefaultName, original);
        }
    }

    [Test]
    public async Task Pdf_Pagination_Preserves_Article_And_File_Matches()
    {
        var result = await MediaTypeEditingService.GetMediaTypesForFileExtensionWithMatchInfoAsync("pdf", 1, 1);

        AssertFileFallback(result, 2);
    }

    private static void AssertFileFallback(PagedModel<MediaTypeFileExtensionMatchResult> result, int expectedTotal)
    {
        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(expectedTotal));
            Assert.That(result.Items.Single().MediaType.Alias, Is.EqualTo(Constants.Conventions.MediaTypes.File));
            Assert.That(result.Items.Single().IsSpecificMatch, Is.False);
        });
    }
}
