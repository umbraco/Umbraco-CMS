using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

public class DownloadCreatedPackageController : CreatedPackageControllerBase
{
    private readonly IPackagingService _packagingService;

    public DownloadCreatedPackageController(IPackagingService packagingService) => _packagingService = packagingService;

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
        PackageDefinition? package = await _packagingService.GetCreatedPackageByKeyAsync(key);

        if (package is null)
        {
            return NotFound();
        }

        Stream? fileStream = _packagingService.GetPackageFileStream(package);
        if (fileStream is null)
        {
            return NotFound();
        }

        var fileName = Path.GetFileName(package.PackagePath);
        Encoding encoding = Encoding.UTF8;

        var contentDisposition = new ContentDisposition
        {
            FileName = WebUtility.UrlEncode(fileName),
            DispositionType = DispositionTypeNames.Attachment
        };

        Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

        var result = new FileStreamResult(
            fileStream,
            new MediaTypeHeaderValue(MediaTypeNames.Application.Octet) { Charset = encoding.WebName });

        return result;
    }
}
