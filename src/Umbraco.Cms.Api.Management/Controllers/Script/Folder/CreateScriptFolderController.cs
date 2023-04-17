using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Folder;

public class CreateScriptFolderController : ScriptFolderBaseController
{
    public CreateScriptFolderController(
        IUmbracoMapper mapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IScriptFolderService scriptFolderService)
        : base(mapper, backOfficeSecurityAccessor, scriptFolderService)
    {
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public Task<IActionResult> Create(CreatePathFolderRequestModel model) => CreateAsync(model);
}
