using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

public class DownloadCreatedPackageController : CreatedPackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public DownloadCreatedPackageController(IPackagingService packagingService, IWebHostEnvironment webHostEnvironment)
    {
        _packagingService = packagingService;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    ///     Downloads a package XML or ZIP file.
    /// </summary>
    /// <param name="key">The key of the package.</param>
    /// <returns>The XML or ZIP file of the package or not found result.</returns>
    [HttpGet("{key:guid}/download")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Download(Guid key)
    {
        PackageDefinition? package = _packagingService.GetCreatedPackageByKey(key);

        if (package is null)
        {
            return NotFound();
        }

        var filePath = package.PackagePath;
        if (_webHostEnvironment.ContentRootFileProvider.GetFileInfo(filePath) is null)
        {
            return ValidationProblem("No file found for path " + filePath);
        }

        var fileName = Path.GetFileName(filePath);
        Encoding encoding = Encoding.UTF8;

        var contentDisposition = new ContentDisposition
        {
            FileName = WebUtility.UrlEncode(fileName),
            DispositionType = DispositionTypeNames.Attachment
        };

        Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

        // Set custom header so umbRequestHelper.downloadFile can save the correct filename
        HttpContext.Response.Headers.Add("x-filename", fileName);

        var result = new FileStreamResult(
            System.IO.File.OpenRead(package.PackagePath),
            new MediaTypeHeaderValue(MediaTypeNames.Application.Octet) { Charset = encoding.WebName });

        return await Task.FromResult(result);
    }
}
