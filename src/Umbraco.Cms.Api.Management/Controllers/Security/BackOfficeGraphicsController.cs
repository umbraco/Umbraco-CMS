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

/// <summary>
/// Provides API endpoints for managing and serving graphics used in the Umbraco back office, such as icons or images required by the administrative interface.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeGraphicsController"/> class.
    /// </summary>
    /// <param name="contentSettings">The options for configuring content settings.</param>
    /// <param name="staticFileOptions">The options for serving static files.</param>
    /// <param name="webHostEnvironment">The hosting environment for the web application.</param>
    public BackOfficeGraphicsController(IOptions<ContentSettings> contentSettings, IOptions<StaticFileOptions> staticFileOptions, IWebHostEnvironment webHostEnvironment)
    {
        _contentSettings = contentSettings;
        _webHostEnvironment = webHostEnvironment;
        _contentTypeProvider = staticFileOptions.Value.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
    }

    /// <summary>Gets the login background image.</summary>
    /// <returns>An <see cref="IActionResult"/> containing the login background image file.</returns>
    [HttpGet("login-background", Name = LoginBackGroundRouteName)]
    [EndpointSummary("Gets the login background image.")]
    [EndpointDescription("Gets the custom login background image if configured.")]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult LoginBackground() => HandleFileRequest(_contentSettings.Value.LoginBackgroundImage);

    /// <summary>
    /// Gets the custom login logo image if configured; otherwise, returns the default logo image.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing the login logo image file.</returns>
    [HttpGet("logo", Name = LogoRouteName)]
    [EndpointSummary("Gets the login logo image.")]
    [EndpointDescription("Gets the custom login logo image if configured.")]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult Logo() => HandleFileRequest(_contentSettings.Value.BackOfficeLogo);

    /// <summary>Gets the login logo image.</summary>
    /// <returns>An <see cref="IActionResult"/> containing the custom login logo image if configured.</returns>
    [HttpGet("logo-alternative", Name = LogoAlternativeRouteName)]
    [EndpointSummary("Gets the login logo image.")]
    [EndpointDescription("Gets the custom login logo image if configured.")]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult LogoAlternative() => HandleFileRequest(_contentSettings.Value.BackOfficeLogoAlternative);

    /// <summary>
    /// Gets the custom login logo image if configured.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing the login logo image file, or a not found result if not configured.</returns>
    [HttpGet("login-logo", Name = LoginLogoRouteName)]
    [EndpointSummary("Gets the login logo image.")]
    [EndpointDescription("Gets the custom login logo image if configured.")]
    [AllowAnonymous]
    [MapToApiVersion("1.0")]
    public IActionResult LoginLogo() => HandleFileRequest(_contentSettings.Value.LoginLogoImage);

    /// <summary>
    /// Retrieves the alternative login logo image for the backoffice, if one is configured.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing the alternative login logo image, or a 404 response if no image is configured.</returns>
    [HttpGet("login-logo-alternative", Name = LoginLogoAlternativeRouteName)]
    [EndpointSummary("Gets the alternative login logo image.")]
    [EndpointDescription("Gets the custom alternative login logo image if configured.")]
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
