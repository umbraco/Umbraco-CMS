using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
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

    [TestCaseSource(nameof(GraphicsEndpoints))]
    public void Graphics_Return_Not_Found_Without_Caching_When_The_Configured_File_Does_Not_Exist(
        Action<ContentSettings, string> configure,
        Func<BackOfficeGraphicsController, IActionResult> endpoint)
    {
        BackOfficeGraphicsController controller = CreateController(Configure(configure, "custom/missing.svg"));

        IActionResult result = endpoint(controller);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            Assert.That(controller.Response.Headers.CacheControl.ToString(), Is.Empty);
        });
    }

    [TestCaseSource(nameof(GraphicsEndpoints))]
    public async Task Graphics_Are_Cached_For_A_Day_With_Validators(
        Action<ContentSettings, string> configure,
        Func<BackOfficeGraphicsController, IActionResult> endpoint)
    {
        CreateWebRootFile("umbraco/custom/graphic.svg");

        HttpResponse response = await RequestAsync(Configure(configure, "custom/graphic.svg"), endpoint);

        CacheControlHeaderValue? cacheControl = response.GetTypedHeaders().CacheControl;
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(cacheControl?.Public, Is.True);
            Assert.That(cacheControl?.MaxAge, Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(response.Headers.ETag.ToString(), Is.Not.Empty);
            Assert.That(response.Headers.LastModified.ToString(), Is.Not.Empty);
        });
    }

    [TestCaseSource(nameof(GraphicsEndpoints))]
    public async Task Graphics_Return_Not_Modified_When_The_ETag_Matches(
        Action<ContentSettings, string> configure,
        Func<BackOfficeGraphicsController, IActionResult> endpoint)
    {
        CreateWebRootFile("umbraco/custom/graphic.svg");
        ContentSettings contentSettings = Configure(configure, "custom/graphic.svg");
        HttpResponse firstResponse = await RequestAsync(contentSettings, endpoint);

        HttpResponse secondResponse = await RequestAsync(contentSettings, endpoint, firstResponse.Headers.ETag);

        Assert.That(secondResponse.StatusCode, Is.EqualTo(StatusCodes.Status304NotModified));
    }

    [TestCaseSource(nameof(GraphicsEndpoints))]
    public async Task Graphics_Return_The_New_File_When_It_Has_Been_Replaced(
        Action<ContentSettings, string> configure,
        Func<BackOfficeGraphicsController, IActionResult> endpoint)
    {
        var path = CreateWebRootFile("umbraco/custom/graphic.svg");
        File.SetLastWriteTimeUtc(path, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        ContentSettings contentSettings = Configure(configure, "custom/graphic.svg");
        HttpResponse firstResponse = await RequestAsync(contentSettings, endpoint);

        File.WriteAllText(path, "a replaced graphic");
        File.SetLastWriteTimeUtc(path, new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        HttpResponse secondResponse = await RequestAsync(contentSettings, endpoint, firstResponse.Headers.ETag);

        Assert.Multiple(() =>
        {
            Assert.That(secondResponse.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(secondResponse.Headers.ETag.ToString(), Is.Not.EqualTo(firstResponse.Headers.ETag.ToString()));
        });
    }

    private static IEnumerable<TestCaseData> GraphicsEndpoints()
    {
        yield return Endpoint(nameof(BackOfficeGraphicsController.LoginBackground), (s, p) => s.LoginBackgroundImage = p, c => c.LoginBackground());
        yield return Endpoint(nameof(BackOfficeGraphicsController.Logo), (s, p) => s.BackOfficeLogo = p, c => c.Logo());
        yield return Endpoint(nameof(BackOfficeGraphicsController.LogoAlternative), (s, p) => s.BackOfficeLogoAlternative = p, c => c.LogoAlternative());
        yield return Endpoint(nameof(BackOfficeGraphicsController.LoginLogo), (s, p) => s.LoginLogoImage = p, c => c.LoginLogo());
        yield return Endpoint(nameof(BackOfficeGraphicsController.LoginLogoAlternative), (s, p) => s.LoginLogoImageAlternative = p, c => c.LoginLogoAlternative());
        yield return Endpoint(nameof(BackOfficeGraphicsController.Favicon), (s, p) => s.BackOfficeFavicon = p, c => c.Favicon());

        static TestCaseData Endpoint(
            string name,
            Action<ContentSettings, string> configure,
            Func<BackOfficeGraphicsController, IActionResult> endpoint)
            => new TestCaseData(configure, endpoint).SetArgDisplayNames(name);
    }

    private static ContentSettings Configure(Action<ContentSettings, string> configure, string path)
    {
        var contentSettings = new ContentSettings();
        configure(contentSettings, path);
        return contentSettings;
    }

    private async Task<HttpResponse> RequestAsync(
        ContentSettings contentSettings,
        Func<BackOfficeGraphicsController, IActionResult> endpoint,
        string? ifNoneMatch = null)
    {
        BackOfficeGraphicsController controller = CreateController(contentSettings);
        if (ifNoneMatch is not null)
        {
            controller.Request.Headers.IfNoneMatch = ifNoneMatch;
        }

        IActionResult result = endpoint(controller);
        await result.ExecuteResultAsync(new ActionContext(controller.HttpContext, new RouteData(), new ActionDescriptor()));
        return controller.Response;
    }

    private BackOfficeGraphicsController CreateController(ContentSettings contentSettings)
    {
        var webHostEnvironment = new Mock<IWebHostEnvironment>();
        webHostEnvironment.Setup(x => x.WebRootFileProvider).Returns(_webRootFileProvider);

        var services = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
            .AddSingleton<IActionResultExecutor<FileStreamResult>, FileStreamResultExecutor>()
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Response.Body = new MemoryStream();

        return new BackOfficeGraphicsController(
            Options.Create(contentSettings),
            Options.Create(new StaticFileOptions()),
            webHostEnvironment.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
        };
    }

    private string CreateWebRootFile(string relativePath)
    {
        var fullPath = Path.Combine(_webRootPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, "icon");
        return fullPath;
    }

    private static void AssertServesFile(IActionResult result, string expectedContentType)
    {
        Assert.That(result, Is.InstanceOf<FileStreamResult>());
        var fileResult = (FileStreamResult)result;
        fileResult.FileStream.Dispose();
        Assert.That(fileResult.ContentType, Is.EqualTo(expectedContentType));
    }
}
