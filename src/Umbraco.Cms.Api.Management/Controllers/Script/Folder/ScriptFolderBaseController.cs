﻿using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Folder;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Script}/folder")]
[ApiExplorerSettings(GroupName = "Script")]
public class ScriptFolderBaseController : PathFolderManagementControllerBase<ScriptFolderOperationStatus>
{
    private readonly IScriptFolderService _scriptFolderService;

    public ScriptFolderBaseController(
        IUmbracoMapper mapper,
        IScriptFolderService scriptFolderService)
        : base(mapper)
    {
        _scriptFolderService = scriptFolderService;
    }

    protected override async Task<PathContainer?> GetContainerAsync(string path)
        => await _scriptFolderService.GetAsync(path);

    protected override Task<Attempt<PathContainer?, ScriptFolderOperationStatus>> CreateContainerAsync(
        PathContainer container) =>
        _scriptFolderService.CreateAsync(container);

    protected override Task<Attempt<ScriptFolderOperationStatus>> DeleteContainerAsync(string path) =>
        _scriptFolderService.DeleteAsync(path);

    protected override IActionResult OperationStatusResult(ScriptFolderOperationStatus status) =>
        status switch
        {
            ScriptFolderOperationStatus.AlreadyExists => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Folder already exists")
                .WithDetail("The folder already exists")
                .Build()),
            ScriptFolderOperationStatus.NotEmpty => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Not empty")
                .WithDetail("The folder is not empty and can therefore not be deleted.")
                .Build()),
            ScriptFolderOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Not found")
                .WithDetail("The specified folder was not found.")
                .Build()),
            ScriptFolderOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
