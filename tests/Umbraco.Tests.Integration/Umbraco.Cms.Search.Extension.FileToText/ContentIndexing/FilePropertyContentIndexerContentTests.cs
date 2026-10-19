using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Search.Extension.FileToText.Configuration;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;
using CoreConstants = Umbraco.Cms.Core.Constants;
using FileToTextConstants = Umbraco.Cms.Search.Extension.FileToText.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Extension.FileToText.Tests.ContentIndexing;

internal class FilePropertyContentIndexerContentTests : FilePropertyContentIndexerTestBase
{
    public static void ConfigureOmitContent(IUmbracoBuilder builder)
        => builder.Services.Configure<IndexingOptions>(config => config.IncludeContent = false);

    public static void ConfigureMaxFileSize(IUmbracoBuilder builder)
        => builder.Services.Configure<IndexingOptions>(config => config.MaxFileSize = 10);

    [TestCase(".pdf", ".md")]
    [TestCase(".md", ".txt")]
    [TestCase(".txt", ".pdf")]
    public async Task Can_Handle_Files_In_All_Properties(string firstFileExtension, string secondFileExtension)
    {
        IContentType contentType = await CreateFileUploadContentType();
        IContent content = await CreateFileUploadContent(contentType, firstFileExtension, secondFileExtension);

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(2));

        AssertLoremIpsumField(fields, FirstFilePropertyAlias);
        AssertLoremIpsumField(fields, SecondFilePropertyAlias);
    }

    [Test]
    public async Task Can_Handle_File_In_Some_Properties()
    {
        IContentType contentType = await CreateFileUploadContentType();
        IContent content = await CreateFileUploadContent(contentType, null, ".pdf");

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(1));

        AssertLoremIpsumField(fields, SecondFilePropertyAlias);
    }

    [Test]
    public async Task Can_Handle_No_Files()
    {
        IContentType contentType = await CreateFileUploadContentType();
        IContent content = await CreateFileUploadContent(contentType);

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Is.Empty);
    }

    [Test]
    public async Task Can_Handle_Published_Files()
    {
        IContentType contentType = await CreateFileUploadContentType();
        IContent content = await CreateFileUploadContent(contentType, ".pdf", ".md", publish: true);

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, [], true, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(2));

        AssertLoremIpsumField(fields, FirstFilePropertyAlias);
        AssertLoremIpsumField(fields, SecondFilePropertyAlias);
    }

    [Test]
    public async Task Ignores_Unpublished_Files_When_Indexing_Published()
    {
        IContentType contentType = await CreateFileUploadContentType();
        IContent content = await CreateFileUploadContent(contentType, ".pdf", ".md");

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, [], true, CancellationToken.None)).ToArray();
        Assert.That(fields, Is.Empty);
    }

    [Test]
    public async Task Can_Handle_Culture_Variant_Files()
    {
        await CreateLanguage("da-DK", "Danish (Denmark)");

        IContentType contentType = await CreateFileUploadContentType(ContentVariation.Culture);
        IContent content = await CreateCultureVariantFileUploadContent(
            contentType,
            new Dictionary<string, (string?, string?)>
            {
                ["en-US"] = (".pdf", ".md"),
                ["da-DK"] = (".txt", null)
            });

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, ["en-US", "da-DK"], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(3));

        AssertLoremIpsumField(fields, FirstFilePropertyAlias, "en-US");
        AssertLoremIpsumField(fields, SecondFilePropertyAlias, "en-US");
        AssertLoremIpsumField(fields, FirstFilePropertyAlias, "da-DK");
    }

    [Test]
    public async Task Can_Handle_Culture_Variant_Files_For_Requested_Cultures_Only()
    {
        await CreateLanguage("da-DK", "Danish (Denmark)");

        IContentType contentType = await CreateFileUploadContentType(ContentVariation.Culture);
        IContent content = await CreateCultureVariantFileUploadContent(
            contentType,
            new Dictionary<string, (string?, string?)>
            {
                ["en-US"] = (".pdf", ".md"),
                ["da-DK"] = (".txt", null)
            });

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, ["da-DK"], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(1));

        AssertLoremIpsumField(fields, FirstFilePropertyAlias, "da-DK");
    }

    [Test]
    public async Task Can_Handle_Segment_Variant_Files()
    {
        IContentType contentType = await CreateFileUploadContentType(ContentVariation.Segment);
        IContent content = await CreateSegmentVariantFileUploadContent(
            contentType,
            (".pdf", ".md"),
            new Dictionary<string, (string?, string?)>
            {
                ["vip"] = (null, ".txt")
            });

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(3));

        AssertLoremIpsumField(fields, FirstFilePropertyAlias);
        AssertLoremIpsumField(fields, SecondFilePropertyAlias);
        AssertLoremIpsumField(fields, SecondFilePropertyAlias, segment: "vip");
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureOmitContent))]
    public async Task Ignores_Files_When_Content_Indexing_Is_Disabled()
    {
        IContentType contentType = await CreateFileUploadContentType();
        IContent content = await CreateFileUploadContent(contentType, ".md", ".txt");

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(0));
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureMaxFileSize))]
    public async Task Ignores_Files_Exceeding_Max_File_Size()
    {
        IContentType contentType = await CreateFileUploadContentType();

        // the PDF test file is ~28 kb, the markdown test file is less than 1 kb
        IContent content = await CreateFileUploadContent(contentType, ".pdf", ".md");

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(1));

        AssertLoremIpsumField(fields, SecondFilePropertyAlias);
    }

    [Test]
    public async Task Can_Handle_Image_Cropper_Files()
    {
        const string imageCropperPropertyAlias = "imageCropperFile";

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("imageCropperPage")
            .WithName("Image Cropper Page")
            .AddPropertyType()
                .WithAlias(imageCropperPropertyAlias)
                .WithName("Image Cropper File")
                .WithDataTypeId(CoreConstants.DataTypes.ImageCropper)
                .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.ImageCropper)
                .WithValueStorageType(ValueStorageType.Ntext)
                .Done()
            .Build();

        Attempt<ContentTypeOperationStatus> result = await GetRequiredService<IContentTypeService>().CreateAsync(contentType, CoreConstants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True, $"Could not create the content type: {result.Result}");

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Image Cropper Content")
            .Build();
        content.SetValue(imageCropperPropertyAlias, $$"""{"src": "{{await AddLoremIpsumFileToMediaFileSystem(".pdf")}}", "crops": []}""");

        OperationResult saveResult = GetRequiredService<IContentService>().Save(content);
        Assert.That(saveResult.Success, Is.True, "Could not save the content.");

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(1));

        AssertLoremIpsumField(fields, imageCropperPropertyAlias);
    }

    [Test]
    public async Task Cannot_Handle_Files_When_Cancelled()
    {
        IContentType contentType = await CreateFileUploadContentType();
        IContent content = await CreateFileUploadContent(contentType, ".pdf", null);

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        Assert.That(
            async () => await FilePropertyContentIndexer.GetIndexFieldsAsync(content, [], false, cancellationTokenSource.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    private static void AssertLoremIpsumField(IEnumerable<IndexField> fields, string propertyAlias, string? culture = null, string? segment = null)
    {
        IndexField? field = fields.SingleOrDefault(f => f.FieldName == $"{propertyAlias}{FileToTextConstants.TextFieldPostfix}" && f.Culture == culture && f.Segment == segment);
        Assert.That(field, Is.Not.Null, $"Could not find the index field for property: {propertyAlias} (culture: {culture ?? "invariant"}, segment: {segment ?? "default"})");

        var texts = field!.Value.Texts?.ToArray();
        Assert.That(texts, Is.Not.Null);
        Assert.That(texts, Has.Length.EqualTo(1));
        Assert.That(texts.Single(), Does.StartWith(TestFileHelper.LoremIpsumStart));
    }
}
