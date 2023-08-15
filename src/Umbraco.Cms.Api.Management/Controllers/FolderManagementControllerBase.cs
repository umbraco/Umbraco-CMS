using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers;

public abstract class FolderManagementControllerBase<TEntityType> : ManagementApiControllerBase
    where TEntityType : ITreeEntity
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityTypeContainerService<TEntityType> _entityTypeContainerService;

    protected FolderManagementControllerBase(IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IEntityTypeContainerService<TEntityType> entityTypeContainerService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _entityTypeContainerService = entityTypeContainerService;
    }

    protected async Task<IActionResult> GetFolderAsync(Guid key)
    {
        EntityContainer? container = await _entityTypeContainerService.GetAsync(key);
        if (container == null)
        {
            return NotFound(new ProblemDetailsBuilder()
                .WithTitle($"Could not find the folder with id: {key}")
                .Build());
        }

        EntityContainer? parentContainer = await _entityTypeContainerService.GetParentAsync(container);

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
        Attempt<EntityContainer?, EntityContainerOperationStatus> result = await _entityTypeContainerService.CreateAsync(
            createFolderRequestModel.Id,
            createFolderRequestModel.Name,
            createFolderRequestModel.ParentId,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction(createdAction, result.Result!.Key)
            : OperationStatusResult(result.Status);
    }

    protected async Task<IActionResult> UpdateFolderAsync(Guid key, UpdateFolderResponseModel updateFolderResponseModel)
    {
        Attempt<EntityContainer?, EntityContainerOperationStatus> result = await _entityTypeContainerService.UpdateAsync(
            key,
            updateFolderResponseModel.Name,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }

    protected async Task<IActionResult> DeleteFolderAsync(Guid key)
    {
        Attempt<EntityContainer?, EntityContainerOperationStatus> result = await _entityTypeContainerService.DeleteAsync(key, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }

    protected IActionResult OperationStatusResult(EntityContainerOperationStatus status)
        => status switch
        {
            EntityContainerOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("The folder could not be found")
                .Build()),
            EntityContainerOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("The parent folder could not be found")
                .Build()),
            EntityContainerOperationStatus.DuplicateName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("The name is already used")
                .WithDetail("The folder name must be unique on this parent.")
                .Build()),
            EntityContainerOperationStatus.DuplicateKey => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("The id is already used")
                .WithDetail("The folder id must be unique.")
                .Build()),
            EntityContainerOperationStatus.NotEmpty => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("The folder is not empty")
                .WithDetail("The folder must be empty to perform this action.")
                .Build()),
            EntityContainerOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the folder operation.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown folder operation status.")
                .Build()),
        };
}
