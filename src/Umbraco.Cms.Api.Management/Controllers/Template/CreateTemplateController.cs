using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

public class CreateTemplateController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateTemplateController(ITemplateService templateService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _templateService = templateService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Create(TemplateCreateModel createModel)
    {
        ITemplate? masterTemplate = null;
        if (createModel.MasterTemplateAlias.IsNullOrWhiteSpace() == false)
        {
            masterTemplate = _templateService.GetTemplate(createModel.MasterTemplateAlias);
            if (masterTemplate == null)
            {
                return NotFound($"Could not find a master template with alias {createModel.MasterTemplateAlias}");
            }
        }

        ITemplate template = _templateService.CreateTemplateWithIdentity(
            createModel.Name,
            createModel.Alias,
            createModel.Content,
            masterTemplate,
            CurrentUserId(_backOfficeSecurityAccessor));

        return await Task.FromResult(CreatedAtAction<ByKeyTemplateController>(controller => nameof(controller.ByKey), template.Key));
    }
}
