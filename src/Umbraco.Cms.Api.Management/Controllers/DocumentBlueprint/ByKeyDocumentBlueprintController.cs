using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

/// <summary>
/// Controller for managing document blueprints identified by their unique key.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes)]
public class ByKeyDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyDocumentBlueprintController"/> class, providing services for editing document blueprints by key.
    /// </summary>
    /// <param name="contentBlueprintEditingService">An instance of <see cref="IContentBlueprintEditingService"/> used to manage content blueprint editing operations.</param>
    /// <param name="umbracoMapper">An instance of <see cref="IUmbracoMapper"/> used for mapping between Umbraco models.</param>
    public ByKeyDocumentBlueprintController(IContentBlueprintEditingService contentBlueprintEditingService, IUmbracoMapper umbracoMapper)
    {
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentBlueprintResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a document blueprint.")]
    [EndpointDescription("Gets a document blueprint identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IContent? blueprint = await _contentBlueprintEditingService.GetAsync(id);
        if (blueprint == null)
        {
            return DocumentBlueprintNotFound();
        }

        return Ok(_umbracoMapper.Map<DocumentBlueprintResponseModel>(blueprint));
    }
}
