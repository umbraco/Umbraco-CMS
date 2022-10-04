using Microsoft.AspNetCore.Http;
using Umbraco.Cms.ManagementApi.Models;

namespace Umbraco.Cms.ManagementApi.Services;

public interface IUploadFileService
{
    FormFileUploadResult TryLoad(IFormFile file);
}
