using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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
    private readonly IOptions<ContentSettings> _contentSettings;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BackOfficeGraphicsController(IOptions<ContentSettings> contentSettings, IOptions<StaticFileOptions> staticFileOptions, IWebHostEnvironment webHostEnvironment)
    {
        _contentSettings = contentSettings;
        _webHostEnvironment = webHostEnvironment;
        _contentTypeProvider = staticFileOptions.Value.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
    }

    [HttpGet("background", Name = nameof(BackOfficeGraphicsController) + "." + nameof(Background))]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult Background() => HandleFileRequest(_contentSettings.Value.LoginBackgroundImage);

    [HttpGet("logo", Name = nameof(BackOfficeGraphicsController) + "." + nameof(Logo))]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult Logo() => HandleFileRequest(_contentSettings.Value.LoginLogoImage);

    [HttpGet("logo-alternative", Name = nameof(BackOfficeGraphicsController) + "." + nameof(LogoAlternative))]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult LogoAlternative() => HandleFileRequest(_contentSettings.Value.LoginLogoImageAlternative);

    private IActionResult HandleFileRequest(string virtualPath)
    {
        var filePath = Path.Combine(Constants.SystemDirectories.Umbraco, virtualPath).TrimStart(Constants.CharArrays.Tilde);
        var fileInfo = _webHostEnvironment.WebRootFileProvider.GetFileInfo(filePath);

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
