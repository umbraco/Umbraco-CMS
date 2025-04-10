using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public class TemporaryFileServiceTests : UmbracoIntegrationTest
{
    private ITemporaryFileService TemporaryFileService => GetRequiredService<ITemporaryFileService>();

    public static void ConfigureAllowedUploadedFileExtensions(IUmbracoBuilder builder)
    {
        builder.Services.Configure<ContentSettings>(config =>
            config.AllowedUploadedFileExtensions = new HashSet<string> { "txt" });
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowedUploadedFileExtensions))]
    public async Task Can_Create_Get_And_Delete_Temporary_File()
    {
        var key = Guid.NewGuid();
        const string FileName = "test.txt";
        const string FileContents = "test";
        var model = new CreateTemporaryFileModel
        {
            FileName = FileName,
            Key = key,
            OpenReadStream = () =>
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(FileContents);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
        };
        var createAttempt = await TemporaryFileService.CreateAsync(model);
        Assert.IsTrue(createAttempt.Success);

        TemporaryFileModel? fileModel = await TemporaryFileService.GetAsync(key);
        Assert.IsNotNull(fileModel);
        Assert.AreEqual(key, fileModel.Key);
        Assert.AreEqual(FileName, fileModel.FileName);

        using (var reader = new StreamReader(fileModel.OpenReadStream()))
        {
            string fileContents = reader.ReadToEnd();
            Assert.AreEqual(FileContents, fileContents);
        }

        var deleteAttempt = await TemporaryFileService.DeleteAsync(key);
        Assert.IsTrue(createAttempt.Success);

        fileModel = await TemporaryFileService.GetAsync(key);
        Assert.IsNull(fileModel);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowedUploadedFileExtensions))]
    public async Task Cannot_Create_File_Outside_Of_Temporary_Files_Root()
    {
        var key = Guid.NewGuid();
        const string FileName = "../test.txt";
        var model = new CreateTemporaryFileModel
        {
            FileName = FileName,
            Key = key,
            OpenReadStream = () =>
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(string.Empty);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
        };
        var createAttempt = await TemporaryFileService.CreateAsync(model);
        Assert.IsFalse(createAttempt.Success);
        Assert.AreEqual(TemporaryFileOperationStatus.InvalidFileName, createAttempt.Status);
    }
}
