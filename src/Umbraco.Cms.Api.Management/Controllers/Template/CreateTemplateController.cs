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
    private readonly ITemplateContentParserService _templateContentParserService;

    public CreateTemplateController(
        ITemplateService templateService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ITemplateContentParserService templateContentParserService)
    {
        _templateService = templateService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _templateContentParserService = templateContentParserService;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Create(TemplateCreateModel createModel)
    {
        var masterTemplateAlias = _templateContentParserService.MasterTemplateAlias(createModel.Content);
        ITemplate? masterTemplate = null;
        if (masterTemplateAlias.IsNullOrWhiteSpace() == false)
        {
            masterTemplate = _templateService.GetTemplate(masterTemplateAlias);
            if (masterTemplate == null)
            {
                return NotFound($"Could not find a master template with alias {masterTemplateAlias}");
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
