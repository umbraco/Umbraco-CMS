using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.Security;

[TestFixture]
public class BackOfficeGraphicsControllerTests
{
    private string _webRootPath = null!;
    private PhysicalFileProvider _webRootFileProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _webRootPath = Path.Combine(Path.GetTempPath(), "umbraco-graphics-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_webRootPath);
        _webRootFileProvider = new PhysicalFileProvider(_webRootPath);
    }

    [TearDown]
    public void TearDown()
    {
        _webRootFileProvider.Dispose();
        Directory.Delete(_webRootPath, recursive: true);
    }

    [Test]
    public void Favicon_Defaults_To_The_BackOffice_Favicon()
    {
        CreateWebRootFile("umbraco/backoffice/assets/favicon.svg");

        IActionResult result = CreateController(new ContentSettings()).Favicon();

        AssertServesFile(result, "image/svg+xml");
    }

    [TestCase("custom/favicon.png", "umbraco/custom/favicon.png", "image/png")]
    [TestCase("../favicon.ico", "favicon.ico", "image/x-icon")]
    public void Favicon_Serves_The_Configured_File_Relative_To_The_Umbraco_Folder(
        string configuredPath,
        string webRootFile,
        string expectedContentType)
    {
        CreateWebRootFile(webRootFile);

        IActionResult result = CreateController(new ContentSettings { BackOfficeFavicon = configuredPath }).Favicon();

        AssertServesFile(result, expectedContentType);
    }

    [Test]
    public void Graphics_Return_Not_Found_When_The_Configured_File_Does_Not_Exist()
    {
        IActionResult result = CreateController(new ContentSettings { BackOfficeFavicon = "custom/missing.ico" }).Favicon();

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    private BackOfficeGraphicsController CreateController(ContentSettings contentSettings)
    {
        var webHostEnvironment = new Mock<IWebHostEnvironment>();
        webHostEnvironment.Setup(x => x.WebRootFileProvider).Returns(_webRootFileProvider);

        return new BackOfficeGraphicsController(
            Options.Create(contentSettings),
            Options.Create(new StaticFileOptions()),
            webHostEnvironment.Object);
    }

    private void CreateWebRootFile(string relativePath)
    {
        var fullPath = Path.Combine(_webRootPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, "icon");
    }

    private static void AssertServesFile(IActionResult result, string expectedContentType)
    {
        Assert.That(result, Is.InstanceOf<FileStreamResult>());
        var fileResult = (FileStreamResult)result;
        fileResult.FileStream.Dispose();
        Assert.That(fileResult.ContentType, Is.EqualTo(expectedContentType));
    }
}
