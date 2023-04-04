using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

public class CreateTemporaryFileRequestModel
{
    public required Guid Id { get; set; }

    public required IFormFile File { get; set; }
}
