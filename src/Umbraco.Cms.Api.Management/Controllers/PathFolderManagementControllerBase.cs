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

    protected PathFolderManagementControllerBase(
        IUmbracoMapper mapper) =>
        Mapper = mapper;

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

        Attempt<PathContainer?, TStatus> attempt = await CreateContainerAsync(folderModel);
        if (attempt.Success is false)
        {
            return OperationStatusResult(attempt.Status);
        }

        PathFolderResponseModel? viewModel = Mapper.Map<PathFolderResponseModel>(attempt.Result);
        return Ok(viewModel);
    }

    protected async Task<IActionResult> DeleteAsync(string path)
    {
        Attempt<TStatus> attempt = await DeleteContainerAsync(path);

        return attempt.Success
            ? Ok()
            : OperationStatusResult(attempt.Result!);
    }

    protected abstract Task<PathContainer?> GetContainerAsync(string path);

    protected abstract Task<Attempt<PathContainer?, TStatus>> CreateContainerAsync(PathContainer container);

    protected abstract Task<Attempt<TStatus>> DeleteContainerAsync(string path);

    protected abstract IActionResult OperationStatusResult(TStatus status);
}
