using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.Services.OperationStatus;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Services;

public interface IUploadFileService
{
    Task<Attempt<UdtFileUpload, UdtFileUploadOperationStatus>> UploadUdtFileAsync(IFormFile file);
}
