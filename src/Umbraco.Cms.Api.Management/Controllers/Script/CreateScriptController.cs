using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Script;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Script;

public class CreateScriptController : ScriptControllerBase
{
    private readonly IScriptService _scriptService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUmbracoMapper _umbracoMapper;

    public CreateScriptController(
        IScriptService scriptService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper umbracoMapper)
    {
        _scriptService = scriptService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create(CreateScriptRequestModel createRequestModel)
    {
        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);

        ScriptCreateModel createModel = _umbracoMapper.Map<ScriptCreateModel>(createRequestModel)!;

        Attempt<IScript?, ScriptOperationStatus> createAttempt = await _scriptService.CreateAsync(createModel, currentUserKey);

        return createAttempt.Success
            ? /*CreatedAtAction<ByKeyScriptController>(controller => nameof(controller.ByKey), createAttempt.Result.Key) TODO: Point to Created action */ Ok()
            : ScriptOperationStatusResult(createAttempt.Status);
    }
}
