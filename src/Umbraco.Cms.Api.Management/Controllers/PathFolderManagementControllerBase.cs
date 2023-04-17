using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers;

public abstract class PathFolderManagementControllerBase<TStatus> : ManagementApiControllerBase
    where TStatus : Enum
{
    protected readonly IUmbracoMapper Mapper;

    protected PathFolderManagementControllerBase(IUmbracoMapper mapper)
    {
        Mapper = mapper;
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

    protected abstract Task<PathContainer?> GetContainerAsync(string path);

    protected abstract Task<Attempt<PathContainer, TStatus>> CreateContainerAsync(PathContainer container, Guid performingUserId);

    protected abstract Task<Attempt<PathContainer, TStatus>> UpdateContainerAsync(PathContainer container, Guid performingUserId);

    protected abstract Task<TStatus> DeleteContainerAsync(string path, Guid performingUserId);

    protected abstract IActionResult OperationStatusResult(TStatus status);
}
