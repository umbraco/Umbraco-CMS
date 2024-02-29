using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

public class CreateTemporaryFileRequestModel
{
    public Guid? Id { get; set; }

    public required IFormFile File { get; set; }
}
