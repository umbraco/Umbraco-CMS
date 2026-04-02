using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

/// <summary>
/// Request model used to create a temporary file.
/// </summary>
public class CreateTemporaryFileRequestModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the temporary file.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the file to be uploaded as a temporary file.
    /// </summary>
    public required IFormFile File { get; set; }
}
