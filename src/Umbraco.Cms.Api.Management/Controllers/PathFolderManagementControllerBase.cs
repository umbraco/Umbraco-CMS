using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Controllers;

public abstract class PathFolderManagementControllerBase<TStatus> : ManagementApiControllerBase
    where TStatus : Enum
{
    protected readonly IUmbracoMapper Mapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    protected PathFolderManagementControllerBase(
        IUmbracoMapper mapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        Mapper = mapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    protected async Task<IActionResult> GetFolderAsync(string path)
    {
        PathContainer? container = await GetContainerAsync(path);
        if (container == null)
        {
            return NotFound();
        }

        PathFolderResponseModel? viewModel = Mapper.Map<PathFolderResponseModel>(container);
        return Ok(viewModel);
    }

    protected async Task<IActionResult> CreateAsync(CreatePathFolderRequestModel requestModel)
    {
        PathContainer folderModel = Mapper.Map<PathContainer>(requestModel)!;

        Attempt<PathContainer?, TStatus> attempt = await CreateContainerAsync(folderModel, CurrentUserKey(_backOfficeSecurityAccessor));
        if (attempt.Success)
        {
            PathFolderResponseModel? viewModel = Mapper.Map<PathFolderResponseModel>(attempt.Result);
            return Ok(viewModel);
        }

        return OperationStatusResult(attempt.Status);
    }

    protected abstract Task<PathContainer?> GetContainerAsync(string path);

    protected abstract Task<Attempt<PathContainer?, TStatus>> CreateContainerAsync(PathContainer container, Guid performingUserId);

    protected abstract Task<Attempt<PathContainer, TStatus>> UpdateContainerAsync(PathContainer container, Guid performingUserId);

    protected abstract Task<TStatus> DeleteContainerAsync(string path, Guid performingUserId);

    protected abstract IActionResult OperationStatusResult(TStatus status);
}
