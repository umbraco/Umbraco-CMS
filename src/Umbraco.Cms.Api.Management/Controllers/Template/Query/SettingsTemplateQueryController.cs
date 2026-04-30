using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template.Query;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Query;

/// <summary>
/// Provides API endpoints for querying settings-related templates in the system.
/// </summary>
[ApiVersion("1.0")]
public class SettingsTemplateQueryController : TemplateQueryControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Template.Query.SettingsTemplateQueryController"/> class.
    /// </summary>
    /// <param name="contentTypeService">The service used to manage content types.</param>
    public SettingsTemplateQueryController(IContentTypeService contentTypeService)
        => _contentTypeService = contentTypeService;

    /// <summary>
    /// Retrieves the configuration settings available for template queries via the HTTP GET "settings" endpoint.
    /// The settings include document type aliases (excluding element types), available properties, and supported query operators.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an <see cref="ActionResult{TemplateQuerySettingsResponseModel}"/>,
    /// which includes the document type aliases, properties, and operators available for template queries.
    /// </returns>
    [HttpGet("settings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateQuerySettingsResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets template query settings.")]
    [EndpointDescription("Gets the available configuration settings for template queries including document type aliases, properties, and operators.")]
    public Task<ActionResult<TemplateQuerySettingsResponseModel>> Settings(CancellationToken cancellationToken)
    {
        var contentTypeAliases = _contentTypeService
            .GetAll()
            .Where(contentType => contentType.IsElement == false)
            .Select(contentType => contentType.Alias)
            .ToArray();

        IEnumerable<TemplateQueryPropertyPresentationModel> properties = GetProperties();

        IEnumerable<TemplateQueryOperatorViewModel> operators = GetOperators();

        return Task.FromResult<ActionResult<TemplateQuerySettingsResponseModel>>(Ok(new TemplateQuerySettingsResponseModel
        {
            DocumentTypeAliases = contentTypeAliases,
            Properties = properties,
            Operators = operators
        }));
    }
}
