using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class TemporaryFileServiceTests
{
    [Test]
    public async Task Cannot_Create_Temporary_File_With_Extensionless_File_Name()
    {
        // A file name without an extension must be handled gracefully rather than throwing while
        // extracting the extension for validation.
        var contentSettings = new ContentSettings
        {
            AllowedUploadedFileExtensions = new HashSet<string> { "png" },
        };
        TemporaryFileService service = CreateService(contentSettings);

        Attempt<TemporaryFileModel?, TemporaryFileOperationStatus> result =
            await service.CreateAsync(new CreateTemporaryFileModel { Key = Guid.NewGuid(), FileName = "no-extension" });

        Assert.AreEqual(TemporaryFileOperationStatus.FileExtensionNotAllowed, result.Status);
    }

    [Test]
    public async Task Can_Create_Temporary_File_With_Allowed_Extension()
    {
        var contentSettings = new ContentSettings
        {
            AllowedUploadedFileExtensions = new HashSet<string> { "png" },
        };
        TemporaryFileService service = CreateService(contentSettings);

        Attempt<TemporaryFileModel?, TemporaryFileOperationStatus> result =
            await service.CreateAsync(new CreateTemporaryFileModel { Key = Guid.NewGuid(), FileName = "image.png" });

        Assert.AreEqual(TemporaryFileOperationStatus.Success, result.Status);
    }

    private static TemporaryFileService CreateService(ContentSettings contentSettings)
    {
        var repository = new Mock<ITemporaryFileRepository>();
        repository.Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync((TemporaryFileModel?)null);

        var runtimeMonitor = new Mock<IOptionsMonitor<RuntimeSettings>>();
        runtimeMonitor.Setup(x => x.CurrentValue).Returns(new RuntimeSettings());

        var contentMonitor = new Mock<IOptionsMonitor<ContentSettings>>();
        contentMonitor.Setup(x => x.CurrentValue).Returns(contentSettings);

        var securityValidator = new Mock<IFileStreamSecurityValidator>();
        securityValidator.Setup(x => x.IsConsideredSafe(It.IsAny<Stream>())).Returns(true);

        return new TemporaryFileService(
            repository.Object,
            runtimeMonitor.Object,
            contentMonitor.Object,
            securityValidator.Object);
    }
}
