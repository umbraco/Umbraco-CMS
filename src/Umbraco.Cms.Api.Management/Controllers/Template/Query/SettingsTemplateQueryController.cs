using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template.Query;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Query;

[ApiVersion("1.0")]
public class SettingsTemplateQueryController : TemplateQueryControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    public SettingsTemplateQueryController(IContentTypeService contentTypeService)
        => _contentTypeService = contentTypeService;

    [HttpGet("settings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateQuerySettingsResponseModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<TemplateQuerySettingsResponseModel>> Settings(CancellationToken cancellationToken)
    {
        var contentTypeAliases = _contentTypeService
            .GetAll()
            .Where(contentType => contentType.IsElement == false)
            .Select(contentType => contentType.Alias)
            .ToArray();

        IEnumerable<TemplateQueryPropertyPresentationModel> properties = GetProperties();

        IEnumerable<TemplateQueryOperatorViewModel> operators = GetOperators();

        return await Task.FromResult(Ok(new TemplateQuerySettingsResponseModel
        {
            DocumentTypeAliases = contentTypeAliases,
            Properties = properties,
            Operators = operators
        }));
    }
}
