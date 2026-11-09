using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Search.Extension.FileToText;
using Umbraco.Cms.Search.Extension.FileToText.Configuration;
using Umbraco.Cms.Tests.Integration.Attributes;
using CoreConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Extension.FileToText.Tests.ContentIndexing;

internal class FilePropertyContentIndexerMediaTests : FilePropertyContentIndexerTestBase
{
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

    private async Task AssertLoremIpsumAsync(string extension)
    {
        IMedia media = await CreateLoremIpsumFileMedia(extension);

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
    }
}
