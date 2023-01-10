using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

public class UpdateTemplateController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly ITemplateContentParserService _templateContentParserService;

    public UpdateTemplateController(
        ITemplateService templateService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ITemplateContentParserService templateContentParserService)
    {
        _templateService = templateService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _templateContentParserService = templateContentParserService;
    }

    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid key, TemplateUpdateModel updateModel)
    {
        ITemplate? template = _templateService.GetTemplate(key);
        if (template == null)
        {
            return NotFound();
        }

        template = _umbracoMapper.Map(updateModel, template);

        var masterTemplateAlias = _templateContentParserService.MasterTemplateAlias(updateModel.Content);
        _templateService.SetMasterTemplate(template, masterTemplateAlias);
        _templateService.SaveTemplate(template, CurrentUserId(_backOfficeSecurityAccessor));

        return await Task.FromResult(Ok());
    }
}
