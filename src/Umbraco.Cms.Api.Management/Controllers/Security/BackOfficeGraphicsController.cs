using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

[ApiVersion("1.0")]
[VersionedApiBackOfficeRoute(Common.Security.Paths.BackOfficeApi.EndpointTemplate + "/graphics")]
[ApiExplorerSettings(IgnoreApi = true)]
public class BackOfficeGraphicsController : Controller
{
    public const string LogoRouteName = nameof(BackOfficeGraphicsController) + "." + nameof(Logo);
    public const string LogoAlternativeRouteName = nameof(BackOfficeGraphicsController) + "." + nameof(LogoAlternative);
    public const string LoginBackGroundRouteName = nameof(BackOfficeGraphicsController) + "." + nameof(LoginBackground);
    public const string LoginLogoRouteName = nameof(BackOfficeGraphicsController) + "." + nameof(LoginLogo);
    public const string LoginLogoAlternativeRouteName = nameof(BackOfficeGraphicsController) + "." + nameof(LoginLogoAlternative);

    private readonly IOptions<ContentSettings> _contentSettings;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BackOfficeGraphicsController(IOptions<ContentSettings> contentSettings, IOptions<StaticFileOptions> staticFileOptions, IWebHostEnvironment webHostEnvironment)
    {
        _contentSettings = contentSettings;
        _webHostEnvironment = webHostEnvironment;
        _contentTypeProvider = staticFileOptions.Value.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
    }

    [HttpGet("login-background", Name = LoginBackGroundRouteName)]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult LoginBackground() => HandleFileRequest(_contentSettings.Value.LoginBackgroundImage);

    [HttpGet("logo", Name = LogoRouteName)]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult Logo() => HandleFileRequest(_contentSettings.Value.BackOfficeLogo);

    [HttpGet("logo-alternative", Name = LogoAlternativeRouteName)]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult LogoAlternative() => HandleFileRequest(_contentSettings.Value.BackOfficeLogoAlternative);

    [HttpGet("login-logo", Name = LoginLogoRouteName)]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult LoginLogo() => HandleFileRequest(_contentSettings.Value.LoginLogoImage);

    [HttpGet("login-logo-alternative", Name = LoginLogoAlternativeRouteName)]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult LoginLogoAlternative() => HandleFileRequest(_contentSettings.Value.LoginLogoImageAlternative);

    private IActionResult HandleFileRequest(string virtualPath)
    {
        var filePath = $"{Constants.SystemDirectories.Umbraco}/{virtualPath}".TrimStart(Constants.CharArrays.Tilde);
        IFileInfo fileInfo = _webHostEnvironment.WebRootFileProvider.GetFileInfo(filePath);

        if (fileInfo.PhysicalPath is null)
        {
            return NotFound();
        }

        if (_contentTypeProvider.TryGetContentType(fileInfo.PhysicalPath, out var contentType))
        {
            Stream fileStream = fileInfo.CreateReadStream();
            return File(fileStream, contentType);
        }

        return StatusCode(StatusCodes.Status412PreconditionFailed);
    }
}
