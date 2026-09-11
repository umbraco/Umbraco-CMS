// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PropertyEditors;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class FileUploadPathResolverTests : UmbracoIntegrationTest
{
    private const string Prefix = "custom-prefix";

    private MediaFileManager MediaFileManager => GetRequiredService<MediaFileManager>();

    // Register a resolver that mirrors the sample: it prefixes only when the data type configuration is the
    // prefixed subclass. This lets us prove the value editor routes through IFileUploadPathResolver while a
    // standard configuration is left untouched.
    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.Services.AddUnique<IFileUploadPathResolver>(sp =>
            new ConfigGatedPrefixingResolver(sp.GetRequiredService<MediaFileManager>()));

    public static void ConfigureAllowedUploadedFileExtensions(IUmbracoBuilder builder)
        => builder.Services.Configure<ContentSettings>(config =>
            config.AllowedUploadedFileExtensions = new HashSet<string> { "txt" });

    [Test]
    public void Can_Get_Standard_Media_Path_From_Default_Resolver()
    {
        var contentKey = Guid.NewGuid();
        var propertyTypeKey = Guid.NewGuid();
        var sut = new FileUploadPathResolver(MediaFileManager);
        var expected = MediaFileManager.GetMediaPath("file.txt", contentKey, propertyTypeKey);

        // The default resolver ignores the data type configuration entirely.
        Assert.Multiple(() =>
        {
            Assert.AreEqual(expected, sut.ResolvePath("file.txt", dataTypeConfiguration: null, contentKey, propertyTypeKey));
            Assert.AreEqual(expected, sut.ResolvePath("file.txt", new FileUploadConfiguration(), contentKey, propertyTypeKey));
        });
    }

    [Test]
    public void Cannot_Prefix_Standard_File_Upload_Configuration()
    {
        var contentKey = Guid.NewGuid();
        var propertyTypeKey = Guid.NewGuid();
        var sut = new ConfigGatedPrefixingResolver(MediaFileManager);
        var expected = MediaFileManager.GetMediaPath("file.txt", contentKey, propertyTypeKey);

        // A standard (non-prefixed) configuration resolves to the standard path, so the default file upload
        // editor is unaffected when a prefixing resolver is installed for other data types.
        Assert.Multiple(() =>
        {
            Assert.AreEqual(expected, sut.ResolvePath("file.txt", new FileUploadConfiguration(), contentKey, propertyTypeKey));
            Assert.AreEqual(expected, sut.ResolvePath("file.txt", dataTypeConfiguration: null, contentKey, propertyTypeKey));
        });
    }

    [Test]
    public void Can_Prefix_Custom_File_Upload_Configuration()
    {
        var contentKey = Guid.NewGuid();
        var propertyTypeKey = Guid.NewGuid();
        var sut = new ConfigGatedPrefixingResolver(MediaFileManager);
        var expected = $"{Prefix}/{MediaFileManager.GetMediaPath("file.txt", contentKey, propertyTypeKey)}";

        var path = sut.ResolvePath("file.txt", new TestPrefixedFileUploadConfiguration { Prefix = Prefix }, contentKey, propertyTypeKey);

        Assert.AreEqual(expected, path);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowedUploadedFileExtensions))]
    public async Task Can_Store_Uploaded_File_At_Resolved_Path()
    {
        var temporaryFileId = Guid.NewGuid();
        await GetRequiredService<ITemporaryFileService>().CreateAsync(new CreateTemporaryFileModel
        {
            Key = temporaryFileId,
            FileName = "test.txt",
            OpenReadStream = () => new MemoryStream("test"u8.ToArray()),
        });

        IJsonSerializer serializer = GetRequiredService<IJsonSerializer>();
        PropertyEditorCollection propertyEditors = GetRequiredService<PropertyEditorCollection>();
        IDataEditor editor = propertyEditors[Constants.PropertyEditors.Aliases.UploadField]!;

        // Use the prefixed configuration subclass so the resolver applies the prefix, exercising the full
        // configuration round-trip through the value editor.
        var editorValue = new ContentPropertyData(
            serializer.Serialize(new FileUploadValue { TemporaryFileId = temporaryFileId }),
            new TestPrefixedFileUploadConfiguration { Prefix = Prefix })
        {
            ContentKey = Guid.NewGuid(),
            PropertyTypeKey = Guid.NewGuid(),
        };

        var storedValue = editor.GetValueEditor().FromEditor(editorValue, null) as string;

        Assert.IsNotNull(storedValue, "Expected the file upload to be processed and a path returned.");

        var relativePath = MediaFileManager.FileSystem.GetRelativePath(storedValue!);
        Assert.Multiple(() =>
        {
            Assert.That(relativePath, Does.StartWith(Prefix + "/"), "Expected the stored path to be under the resolver's prefix.");
            Assert.IsTrue(
                MediaFileManager.FileSystem.FileExists(relativePath),
                "Expected the uploaded file to physically exist at the prefixed path.");
        });
    }

    private sealed class TestPrefixedFileUploadConfiguration : FileUploadConfiguration
    {
        public string? Prefix { get; set; }
    }

    private sealed class ConfigGatedPrefixingResolver : IFileUploadPathResolver
    {
        private readonly MediaFileManager _mediaFileManager;

        public ConfigGatedPrefixingResolver(MediaFileManager mediaFileManager) => _mediaFileManager = mediaFileManager;

        public string ResolvePath(string fileName, object? dataTypeConfiguration, Guid contentKey, Guid propertyTypeKey)
        {
            var path = _mediaFileManager.GetMediaPath(fileName, contentKey, propertyTypeKey);
            if (dataTypeConfiguration is TestPrefixedFileUploadConfiguration configuration
                && string.IsNullOrWhiteSpace(configuration.Prefix) is false)
            {
                path = $"{configuration.Prefix.Trim('/')}/{path}";
            }

            return path;
        }
    }
}
