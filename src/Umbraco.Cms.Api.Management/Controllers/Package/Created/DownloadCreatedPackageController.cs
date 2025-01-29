using System.Net;
using System.Net.Mime;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

[ApiVersion("1.0")]
public class DownloadCreatedPackageController : CreatedPackageControllerBase
{
    private readonly IPackagingService _packagingService;

    public DownloadCreatedPackageController(IPackagingService packagingService) => _packagingService = packagingService;

    /// <summary>
    ///     Downloads a package XML or ZIP file.
    /// </summary>
    /// <param name="id">The id of the package.</param>
    /// <returns>The XML or ZIP file of the package or not found result.</returns>
    [HttpGet("{id:guid}/download")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Download(CancellationToken cancellationToken, Guid id)
    {
        PackageDefinition? package = await _packagingService.GetCreatedPackageByKeyAsync(id);

        if (package is null)
        {
            return CreatedPackageNotFound();
        }

        Stream? fileStream = _packagingService.GetPackageFileStream(package);
        if (fileStream is null)
        {
            return CreatedPackageFileStreamNotFound();
        }

        var fileName = Path.GetFileName(package.PackagePath);
        Encoding encoding = Encoding.UTF8;

        var contentDisposition = new ContentDisposition
        {
            FileName = WebUtility.UrlEncode(fileName),
            DispositionType = DispositionTypeNames.Attachment
        };

        Response.Headers.Append("Content-Disposition", contentDisposition.ToString());

        var mediaType = fileName.InvariantEndsWith(".zip")
            ? MediaTypeNames.Application.Zip
            : MediaTypeNames.Text.Xml;

        var result = new FileStreamResult(
            fileStream,
            new MediaTypeHeaderValue(mediaType) { Charset = encoding.WebName });

        return result;
    }
}
