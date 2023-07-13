using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Stylesheet}/folder")]
[ApiExplorerSettings(GroupName = "Stylesheet")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessStylesheets)]
public class StylesheetFolderControllerBase : PathFolderManagementControllerBase<StylesheetFolderOperationStatus>
{
    private readonly IStylesheetFolderService _stylesheetFolderService;

    public StylesheetFolderControllerBase(
        IUmbracoMapper mapper,
        IStylesheetFolderService stylesheetFolderService)
        : base(mapper)
    {
        _stylesheetFolderService = stylesheetFolderService;
    }

    protected override Task<PathContainer?> GetContainerAsync(string path) => _stylesheetFolderService.GetAsync(path);

    protected override Task<Attempt<PathContainer?, StylesheetFolderOperationStatus>> CreateContainerAsync(PathContainer container)
        => _stylesheetFolderService.CreateAsync(container);

    protected override Task<Attempt<StylesheetFolderOperationStatus>> DeleteContainerAsync(string path)
        => _stylesheetFolderService.DeleteAsync(path);

    protected override IActionResult OperationStatusResult(StylesheetFolderOperationStatus status) =>
        status switch
        {
            StylesheetFolderOperationStatus.AlreadyExists => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Folder already exists")
                .WithDetail("The folder already exists")
                .Build()),
            StylesheetFolderOperationStatus.NotEmpty => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Not empty")
                .WithDetail("The folder is not empty and can therefore not be deleted.")
                .Build()),
            StylesheetFolderOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Not found")
                .WithDetail("The specified folder was not found.")
                .Build()),
            StylesheetFolderOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            StylesheetFolderOperationStatus.InvalidName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid name")
                .WithDetail("The name specified is not a valid name.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown stylesheet folder operation status")
        };
}
