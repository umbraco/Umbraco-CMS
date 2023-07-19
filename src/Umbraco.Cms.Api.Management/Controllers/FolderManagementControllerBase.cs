using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Controllers;

public abstract class FolderManagementControllerBase<TStatus> : ManagementApiControllerBase
    where TStatus : Enum
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    protected FolderManagementControllerBase(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        => _backOfficeSecurityAccessor = backOfficeSecurityAccessor;

    protected async Task<IActionResult> GetFolderAsync(Guid key)
    {
        EntityContainer? container = await GetContainerAsync(key);
        if (container == null)
        {
            return NotFound($"Could not find the folder with id: {key}");
        }

        EntityContainer? parentContainer = await GetParentContainerAsync(container);

        // we could implement a mapper for this but it seems rather overkill at this point
        return Ok(new FolderResponseModel
        {
            Name = container.Name!,
            Id = container.Key,
            ParentId = parentContainer?.Key
        });
    }

    protected async Task<IActionResult> CreateFolderAsync<TCreatedActionController>(
        CreateFolderRequestModel createFolderRequestModel,
        Expression<Func<TCreatedActionController, string>> createdAction)
    {
        var container = new EntityContainer(ContainerObjectType) { Name = createFolderRequestModel.Name };

        if (createFolderRequestModel.Id.HasValue)
        {
            container.Key = createFolderRequestModel.Id.Value;
        }

        Attempt<EntityContainer, TStatus> result = await CreateContainerAsync(
            container,
            createFolderRequestModel.ParentId,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction(createdAction, result.Result.Key)
            : OperationStatusResult(result.Status);
    }

    protected async Task<IActionResult> UpdateFolderAsync(Guid key, UpdateFolderReponseModel updateFolderReponseModel)
    {
        EntityContainer? container = await GetContainerAsync(key);
        if (container == null)
        {
            return NotFound($"Could not find the folder with key: {key}");
        }

        container.Name = updateFolderReponseModel.Name;

        Attempt<EntityContainer, TStatus> result = await UpdateContainerAsync(container, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }

    protected async Task<IActionResult> DeleteFolderAsync(Guid key)
    {
        Attempt<EntityContainer?, TStatus> result = await DeleteContainerAsync(key, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }

    protected abstract Guid ContainerObjectType { get; }

    protected abstract Task<EntityContainer?> GetContainerAsync(Guid key);

    protected abstract Task<EntityContainer?> GetParentContainerAsync(EntityContainer container);

    protected abstract Task<Attempt<EntityContainer, TStatus>> CreateContainerAsync(EntityContainer container, Guid? parentId, Guid userKey);

    protected abstract Task<Attempt<EntityContainer, TStatus>> UpdateContainerAsync(EntityContainer container, Guid userKey);

    protected abstract Task<Attempt<EntityContainer?, TStatus>> DeleteContainerAsync(Guid id, Guid userKey);

    protected abstract IActionResult OperationStatusResult(TStatus status);
}
