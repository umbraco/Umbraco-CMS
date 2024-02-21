using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers;

public abstract class FolderManagementControllerBase<TTreeEntity> : ManagementApiControllerBase
    where TTreeEntity : ITreeEntity
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityTypeContainerService<TTreeEntity> _treeEntityTypeContainerService;

    protected FolderManagementControllerBase(IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IEntityTypeContainerService<TTreeEntity> treeEntityTypeContainerService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _treeEntityTypeContainerService = treeEntityTypeContainerService;
    }

    protected async Task<IActionResult> GetFolderAsync(Guid key)
    {
        EntityContainer? container = await _treeEntityTypeContainerService.GetAsync(key);
        if (container == null)
        {
            return OperationStatusResult(
                EntityContainerOperationStatus.NotFound,
                problemDetailsBuilder => NotFound(problemDetailsBuilder
                    .WithTitle($"Could not find the folder with id: {key}")
                    .Build()));
        }

        // we could implement a mapper for this but it seems rather overkill at this point
        return Ok(new FolderResponseModel
        {
            Name = container.Name!,
            Id = container.Key
        });
    }

    protected async Task<IActionResult> CreateFolderAsync<TCreatedActionController>(
        CreateFolderRequestModel createFolderRequestModel,
        Expression<Func<TCreatedActionController, string>> createdAction)
    {
        Attempt<EntityContainer?, EntityContainerOperationStatus> result = await _treeEntityTypeContainerService.CreateAsync(
            createFolderRequestModel.Id,
            createFolderRequestModel.Name,
            createFolderRequestModel.Parent?.Id,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId(createdAction, result.Result!.Key)
            : OperationStatusResult(result.Status);
    }

    protected async Task<IActionResult> UpdateFolderAsync(Guid key, UpdateFolderResponseModel updateFolderResponseModel)
    {
        Attempt<EntityContainer?, EntityContainerOperationStatus> result = await _treeEntityTypeContainerService.UpdateAsync(
            key,
            updateFolderResponseModel.Name,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }

    protected async Task<IActionResult> DeleteFolderAsync(Guid key)
    {
        Attempt<EntityContainer?, EntityContainerOperationStatus> result = await _treeEntityTypeContainerService.DeleteAsync(key, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }

    protected IActionResult OperationStatusResult(EntityContainerOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            EntityContainerOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("The folder could not be found")
                .Build()),
            EntityContainerOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The parent folder could not be found")
                .Build()),
            EntityContainerOperationStatus.DuplicateName => BadRequest(problemDetailsBuilder
                .WithTitle("The name is already used")
                .WithDetail("The folder name must be unique on this parent.")
                .Build()),
            EntityContainerOperationStatus.DuplicateKey => BadRequest(problemDetailsBuilder
                .WithTitle("The id is already used")
                .WithDetail("The folder id must be unique.")
                .Build()),
            EntityContainerOperationStatus.NotEmpty => BadRequest(problemDetailsBuilder
                .WithTitle("The folder is not empty")
                .WithDetail("The folder must be empty to perform this action.")
                .Build()),
            EntityContainerOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the folder operation.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown folder operation status.")
                .Build()),
        });
}
