using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

public class UploadSingleFileRequestModel
{
    public required Guid Key { get; set; }
    public required IFormFile File { get; set; }
}
