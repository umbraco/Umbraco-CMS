using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;
using Umbraco.Cms.Search.Extension.FileToText;
using Umbraco.Cms.Search.Extension.FileToText.Configuration;
using Umbraco.Cms.Tests.Integration.Attributes;
using CoreConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Extension.FileToText.Tests.ContentIndexing;

internal class FilePropertyContentIndexerMediaTests : FilePropertyContentIndexerTestBase
{
    // the harness does not register the core notification handlers, so the media files would not be renamed when trashed
    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<MediaMovedToRecycleBinNotification, FileUploadContentDeletedNotificationHandler>();

    public static void ConfigureOmitMedia(IUmbracoBuilder builder)
        => builder.Services.Configure<IndexingOptions>(config => config.IncludeMedia = false);

    [Test]
    public async Task Can_Handle_Pdf()
        => await AssertLoremIpsumAsync(".pdf");

    [Test]
    public async Task Can_Handle_Markdown()
        => await AssertLoremIpsumAsync(".md");

    [Test]
    public async Task Can_Handle_Text()
        => await AssertLoremIpsumAsync(".txt");

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureOmitMedia))]
    public async Task Ignores_Files_When_Media_Indexing_Is_Disabled()
    {
        IMedia media = await CreateLoremIpsumFileMedia(".md");

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(media, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(0));
    }

    [TestCase(".txt")]
    [TestCase(".md")]
    [TestCase(".pdf")]
    public async Task Can_Handle_Trashed_Media_With_Recycle_Bin_Protection(string extension)
    {
        IMedia media = await AssertLoremIpsumAsync(extension, trash: true);

        var filePath = GetMediaFilePath(media);
        var suffixedFilePath = Path.ChangeExtension(filePath, CoreConstants.Conventions.Media.TrashedMediaSuffix + extension);
        Assert.Multiple(() =>
        {
            Assert.That(MediaFileExists(suffixedFilePath), Is.True, "The file of the trashed media was not suffixed.");
            Assert.That(MediaFileExists(filePath), Is.False, "The original file of the trashed media still exists.");
        });
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableRecycleBinProtection))]
    public async Task Can_Handle_Trashed_Media_Without_Recycle_Bin_Protection()
        => await AssertLoremIpsumAsync(".txt", trash: true);

    public static void ConfigureDisableRecycleBinProtection(IUmbracoBuilder builder)
        => builder.Services.Configure<ContentSettings>(config => config.EnableMediaRecycleBinProtection = false);

    private async Task<IMedia> AssertLoremIpsumAsync(string extension, bool trash = false)
    {
        IMedia media = await CreateLoremIpsumFileMedia(extension);

        if (trash)
        {
            media = await TrashMedia(media);
        }

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(media, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(1));

        IndexField field = fields.Single();
        Assert.Multiple(() =>
        {
            Assert.That(field.FieldName, Is.EqualTo($"{CoreConstants.Conventions.Media.File}{Constants.TextFieldPostfix}"));
            Assert.That(field.Culture, Is.Null);
            Assert.That(field.Segment, Is.Null);
        });

        var texts = field.Value.Texts?.ToArray();
        Assert.That(texts, Is.Not.Null);
        Assert.That(texts, Has.Length.EqualTo(1));
        Assert.That(texts.Single(), Does.StartWith(TestFileHelper.LoremIpsumStart));

        return media;
    }
}
