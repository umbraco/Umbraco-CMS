using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Api.Management.Models;

namespace Umbraco.Cms.Api.Management.Services;

public interface IUploadFileService
{
    FormFileUploadResult TryLoad(IFormFile file);
}
