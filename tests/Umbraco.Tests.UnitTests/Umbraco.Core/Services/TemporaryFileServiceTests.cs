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

    // '/' is invalid in a file name on every platform, unlike ':' which is only invalid on Windows.
    [TestCase("in/valid.png")]
    [TestCase("/.png")]
    public async Task Cannot_Create_Temporary_File_With_Invalid_File_Name(string fileName)
    {
        var contentSettings = new ContentSettings
        {
            AllowedUploadedFileExtensions = new HashSet<string> { "png" },
        };
        TemporaryFileService service = CreateService(contentSettings);

        Attempt<TemporaryFileModel?, TemporaryFileOperationStatus> result =
            await service.CreateAsync(new CreateTemporaryFileModel { Key = Guid.NewGuid(), FileName = fileName });

        Assert.AreEqual(TemporaryFileOperationStatus.InvalidFileName, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Temporary_File_With_Disallowed_Extension()
    {
        TemporaryFileService service = CreateService(DisallowedExtensionContentSettings());

        Attempt<TemporaryFileModel?, TemporaryFileOperationStatus> result =
            await service.CreateAsync(new CreateTemporaryFileModel { Key = Guid.NewGuid(), FileName = "file.cshtml" });

        Assert.AreEqual(TemporaryFileOperationStatus.FileExtensionNotAllowed, result.Status);
    }

    // A trailing period is discarded when the file is created, so the name comes to rest carrying the disallowed
    // extension and must be rejected on the extension rather than on the name.
    [Test]
    public async Task Cannot_Create_Temporary_File_With_Disallowed_Extension_Hidden_By_Trailing_Period()
    {
        TemporaryFileService service = CreateService(DisallowedExtensionContentSettings());

        Attempt<TemporaryFileModel?, TemporaryFileOperationStatus> result =
            await service.CreateAsync(new CreateTemporaryFileModel { Key = Guid.NewGuid(), FileName = "file.cshtml." });

        Assert.AreEqual(TemporaryFileOperationStatus.FileExtensionNotAllowed, result.Status);
    }

    [TestCase("image.png.")]
    [TestCase("image.png ")]
    public async Task Cannot_Create_Temporary_File_With_File_Name_Ending_In_Period_Or_Whitespace(string fileName)
    {
        TemporaryFileService service = CreateService(DisallowedExtensionContentSettings());

        Attempt<TemporaryFileModel?, TemporaryFileOperationStatus> result =
            await service.CreateAsync(new CreateTemporaryFileModel { Key = Guid.NewGuid(), FileName = fileName });

        Assert.AreEqual(TemporaryFileOperationStatus.InvalidFileName, result.Status);
    }

    [TestCase("image.png")]
    [TestCase("no-extension")]
    public async Task Can_Create_Temporary_File_With_Extension_That_Is_Not_Disallowed(string fileName)
    {
        TemporaryFileService service = CreateService(DisallowedExtensionContentSettings());

        Attempt<TemporaryFileModel?, TemporaryFileOperationStatus> result =
            await service.CreateAsync(new CreateTemporaryFileModel { Key = Guid.NewGuid(), FileName = fileName });

        Assert.AreEqual(TemporaryFileOperationStatus.Success, result.Status);
    }

    [TestCase("file.cshtml", "file.cshtml")] // nothing to trim
    [TestCase("file.cshtml.", "file.cshtml")] // trailing period
    [TestCase("file.cshtml..", "file.cshtml")] // repeated trailing periods
    [TestCase("file.cshtml ", "file.cshtml")] // trailing space
    [TestCase("file.cshtml\t", "file.cshtml")] // trailing tab
    [TestCase("file.cshtml\r\n", "file.cshtml")] // trailing line break
    [TestCase("file.cshtml. ", "file.cshtml")] // period then space
    [TestCase("file.cshtml .", "file.cshtml")] // space then period
    [TestCase("file.cshtml . . ", "file.cshtml")] // interleaved periods and spaces
    [TestCase("file.cshtml\u00A0", "file.cshtml")] // trailing non-breaking space
    [TestCase(" .file.cshtml", " .file.cshtml")] // leading periods and whitespace are preserved
    [TestCase("file. cshtml", "file. cshtml")] // interior periods and whitespace are preserved
    [TestCase("", "")] // empty string
    [TestCase(".", "")] // period only
    [TestCase(" . ", "")] // periods and whitespace only
    public void TrimTrailingPeriodsAndWhitespace_ReturnsExpectedResult(string input, string expected)
    {
        var result = TemporaryFileService.TrimTrailingPeriodsAndWhitespace(input);

        Assert.AreEqual(expected, result);
    }

    [Test]
    public void TrimTrailingPeriodsAndWhitespace_Returns_Original_Instance_When_Nothing_To_Trim()
    {
        var input = "file.cshtml";

        Assert.AreSame(input, TemporaryFileService.TrimTrailingPeriodsAndWhitespace(input));
    }

    private static ContentSettings DisallowedExtensionContentSettings() => new()
    {
        AllowedUploadedFileExtensions = new HashSet<string>(),
        DisallowedUploadedFileExtensions = new HashSet<string> { "cshtml" },
    };

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
