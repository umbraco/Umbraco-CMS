using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Search.Extension.FileToText.ContentIndexing;
using Umbraco.Cms.Search.Extension.FileToText.DependencyInjection;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using CoreConstants =  Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Extension.FileToText.Tests.ContentIndexing;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal abstract class FilePropertyContentIndexerTestBase : UmbracoIntegrationTest
{
    protected const string FirstFilePropertyAlias = "firstFile";

    protected const string SecondFilePropertyAlias = "secondFile";

    protected FilePropertyContentIndexer FilePropertyContentIndexer => GetRequiredService<IEnumerable<IContentIndexer>>()
        .OfType<FilePropertyContentIndexer>()
        .Single();

    private IMediaImportService MediaImportService => GetRequiredService<IMediaImportService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private MediaFileManager MediaFileManager => GetRequiredService<MediaFileManager>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [SetUp]
    public void SetUp() => DeleteAllMediaFiles();

    [TearDown]
    public void TearDownTemplateFiles() => DeleteAllMediaFiles();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.AddFileToText();
    }

    protected async Task<IMedia> CreateLoremIpsumFileMedia(string extension)
    {
        var file = new FileInfo(TestFileHelper.LoremIpsumFilePath(extension));
        await using FileStream stream = file.OpenRead();
        return await MediaImportService.ImportAsync(file.Name, stream, null, null, CoreConstants.Security.SuperUserKey);
    }

    protected async Task CreateLanguage(string isoCode, string cultureName)
    {
        Attempt<ILanguage, LanguageOperationStatus> result = await LanguageService.CreateAsync(new Language(isoCode, cultureName), CoreConstants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True, $"Could not create the language: {result.Status}");
    }

    protected async Task<IContentType> CreateFileUploadContentType(ContentVariation variation = ContentVariation.Nothing)
    {
        IDataType uploadDataType = await DataTypeService.GetAsync(CoreConstants.DataTypes.Guids.UploadGuid)
                                   ?? throw new InvalidOperationException("Could not find the default file upload data type.");

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("fileUploadPage")
            .WithName("File Upload Page")
            .WithAllowAsRoot(true)
            .WithContentVariation(variation)
            .AddPropertyGroup()
                .WithAlias("files")
                .WithName("Files")
                .AddPropertyType()
                    .WithAlias(FirstFilePropertyAlias)
                    .WithName("First File")
                    .WithDataTypeId(uploadDataType.Id)
                    .WithPropertyEditorAlias(uploadDataType.EditorAlias)
                    .WithValueStorageType(uploadDataType.DatabaseType)
                    .WithVariations(variation)
                    .Done()
                .AddPropertyType()
                    .WithAlias(SecondFilePropertyAlias)
                    .WithName("Second File")
                    .WithDataTypeId(uploadDataType.Id)
                    .WithPropertyEditorAlias(uploadDataType.EditorAlias)
                    .WithValueStorageType(uploadDataType.DatabaseType)
                    .WithVariations(variation)
                    .Done()
                .Done()
            .Build();

        Attempt<ContentTypeOperationStatus> result = await ContentTypeService.CreateAsync(contentType, CoreConstants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True, $"Could not create the content type: {result.Result}");

        // return the persisted content type, as the built one does not support publishing for its property types
        return await ContentTypeService.GetAsync(contentType.Key)
               ?? throw new InvalidOperationException("Could not find the created content type.");
    }

    /// <summary>
    /// Creates content without any values for the file upload properties.
    /// </summary>
    protected Task<IContent> CreateFileUploadContent(IContentType contentType, bool publish = false)
        => CreateFileUploadContent(contentType, null, null, publish);

    /// <summary>
    /// Creates content with lorem ipsum files of the given extensions in the file upload properties.
    /// Pass null as extension to leave the corresponding file upload property without a value.
    /// </summary>
    protected async Task<IContent> CreateFileUploadContent(
        IContentType contentType,
        string? firstFileExtension,
        string? secondFileExtension,
        bool publish = false)
    {
        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("File Upload Content")
            .Build();

        if (firstFileExtension is not null)
        {
            content.SetValue(FirstFilePropertyAlias, await AddLoremIpsumFileToMediaFileSystem(firstFileExtension));
        }

        if (secondFileExtension is not null)
        {
            content.SetValue(SecondFilePropertyAlias, await AddLoremIpsumFileToMediaFileSystem(secondFileExtension));
        }

        SaveContent(content, publish);
        return content;
    }

    /// <summary>
    /// Creates culture variant content with lorem ipsum files of the given extensions in the file upload properties, per culture.
    /// Pass null as extension to leave the corresponding file upload property without a value for that culture.
    /// </summary>
    protected async Task<IContent> CreateCultureVariantFileUploadContent(
        IContentType contentType,
        IDictionary<string, (string? FirstFileExtension, string? SecondFileExtension)> fileExtensionsByCulture,
        bool publish = false)
    {
        var contentBuilder = new ContentBuilder().WithContentType(contentType);
        foreach (var culture in fileExtensionsByCulture.Keys)
        {
            contentBuilder.WithCultureName(culture, $"File Upload Content ({culture})");
        }

        IContent content = contentBuilder.Build();

        foreach ((var culture, (var firstFileExtension, var secondFileExtension)) in fileExtensionsByCulture)
        {
            if (firstFileExtension is not null)
            {
                content.SetValue(FirstFilePropertyAlias, await AddLoremIpsumFileToMediaFileSystem(firstFileExtension), culture);
            }

            if (secondFileExtension is not null)
            {
                content.SetValue(SecondFilePropertyAlias, await AddLoremIpsumFileToMediaFileSystem(secondFileExtension), culture);
            }
        }

        SaveContent(content, publish);
        return content;
    }

    /// <summary>
    /// Creates segment variant content with lorem ipsum files of the given extensions in the file upload properties,
    /// both for the default (no segment) value and per segment.
    /// Pass null as extension to leave the corresponding file upload property without a value for that segment.
    /// </summary>
    protected async Task<IContent> CreateSegmentVariantFileUploadContent(
        IContentType contentType,
        (string? FirstFileExtension, string? SecondFileExtension) defaultFileExtensions,
        IDictionary<string, (string? FirstFileExtension, string? SecondFileExtension)> fileExtensionsBySegment,
        bool publish = false)
    {
        IContent content = await CreateFileUploadContent(contentType, defaultFileExtensions.FirstFileExtension, defaultFileExtensions.SecondFileExtension);

        foreach ((var segment, (var firstFileExtension, var secondFileExtension)) in fileExtensionsBySegment)
        {
            if (firstFileExtension is not null)
            {
                content.SetValue(FirstFilePropertyAlias, await AddLoremIpsumFileToMediaFileSystem(firstFileExtension), segment: segment);
            }

            if (secondFileExtension is not null)
            {
                content.SetValue(SecondFilePropertyAlias, await AddLoremIpsumFileToMediaFileSystem(secondFileExtension), segment: segment);
            }
        }

        SaveContent(content, publish);
        return content;
    }

    private void SaveContent(IContent content, bool publish)
    {
        OperationResult saveResult = ContentService.Save(content);
        Assert.That(saveResult.Success, Is.True, "Could not save the content.");

        if (publish)
        {
            PublishResult publishResult = ContentService.Publish(content, ["*"]);
            Assert.That(publishResult.Success, Is.True, $"Could not publish the content: {publishResult.Result}");
        }
    }

    // adds a test file to the media file system (like the file upload property editor does) and returns the property value
    protected async Task<string> AddLoremIpsumFileToMediaFileSystem(string extension)
    {
        var file = new FileInfo(TestFileHelper.LoremIpsumFilePath(extension));
        var filePath = MediaFileManager.GetMediaPath(file.Name, Guid.NewGuid(), Guid.NewGuid());

        await using FileStream stream = file.OpenRead();
        MediaFileManager.FileSystem.AddFile(filePath, stream);

        return MediaFileManager.FileSystem.GetUrl(filePath);
    }
}
