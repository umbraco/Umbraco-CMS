using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers;

public abstract class FolderManagementControllerBase : ManagementApiControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    protected FolderManagementControllerBase(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        => _backOfficeSecurityAccessor = backOfficeSecurityAccessor;

    protected ActionResult GetFolder(Guid key)
    {
        EntityContainer? container = GetContainer(key);
        if (container == null)
        {
            return NotFound($"Could not find the folder with key: {key}");
        }

        EntityContainer? parentContainer = container.ParentId > 0
            ? GetContainer(container.ParentId)
            : null;

        // we could implement a mapper for this but it seems rather overkill at this point
        return Ok(new FolderViewModel
        {
            Name = container.Name!,
            Key = container.Key,
            ParentKey = parentContainer?.Key
        });
    }

    protected ActionResult CreateFolder<TCreatedActionController>(
        FolderCreateModel folderCreateModel,
        Expression<Func<TCreatedActionController, string>> createdAction)
    {
        EntityContainer? parentContainer = folderCreateModel.ParentKey.HasValue
            ? GetContainer(folderCreateModel.ParentKey.Value)
            : null;

        Attempt<OperationResult<OperationResultType, EntityContainer>?> result = CreateContainer(
            parentContainer?.Id ?? Constants.System.Root,
            folderCreateModel.Name,
            CurrentUserId(_backOfficeSecurityAccessor));

        if (result.Success == false)
        {
            ProblemDetails problemDetails = new ProblemDetailsBuilder()
                .WithTitle("Unable to create the folder")
                .WithDetail(result.Exception?.Message ?? FallbackProblemDetail(result.Result))
                .Build();
            return BadRequest(problemDetails);
        }

        EntityContainer container = result.Result!.Entity!;
        return CreatedAtAction(createdAction, container.Key);
    }

    protected ActionResult UpdateFolder(Guid key, FolderUpdateModel folderUpdateModel)
    {
        EntityContainer? container = GetContainer(key);
        if (container == null)
        {
            return NotFound($"Could not find the folder with key: {key}");
        }

        container.Name = folderUpdateModel.Name;

        Attempt<OperationResult?> result = SaveContainer(container, CurrentUserId(_backOfficeSecurityAccessor));
        if (result.Success == false)
        {
            ProblemDetails problemDetails = new ProblemDetailsBuilder()
                .WithTitle("Unable to update the folder")
                .WithDetail(result.Exception?.Message ?? FallbackProblemDetail(result.Result))
                .Build();
            return BadRequest(problemDetails);
        }

        return Ok();
    }

    protected ActionResult DeleteFolder(Guid key)
    {
        EntityContainer? container = GetContainer(key);
        if (container == null)
        {
            return NotFound($"Could not find the folder with key: {key}");
        }

        Attempt<OperationResult?> result = DeleteContainer(container.Id, CurrentUserId(_backOfficeSecurityAccessor));
        if (result.Success == false)
        {
            ProblemDetails problemDetails = new ProblemDetailsBuilder()
                .WithTitle("Unable to delete the folder")
                .WithDetail(result.Exception?.Message ?? FallbackProblemDetail(result.Result))
                .Build();
            return BadRequest(problemDetails);
        }

        return Ok();
    }

    private static string FallbackProblemDetail(OperationResult<OperationResultType>? result)
        => result != null ? $"The reported operation result was: {result.Result}" : "Check the log for additional details";

    protected abstract EntityContainer? GetContainer(Guid key);

    protected abstract EntityContainer? GetContainer(int containerId);

    protected abstract Attempt<OperationResult?> SaveContainer(EntityContainer container, int userId);

    protected abstract Attempt<OperationResult<OperationResultType, EntityContainer>?> CreateContainer(int parentId, string name, int userId);

    protected abstract Attempt<OperationResult?> DeleteContainer(int containerId, int userId);
}
