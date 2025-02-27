using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class CompositionReferenceMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public CompositionReferenceMemberTypeController(IMemberTypeService memberTypeService, IUmbracoMapper umbracoMapper)
    {
        _memberTypeService = memberTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}/composition-references")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberTypeCompositionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompositionReferences(CancellationToken cancellationToken, Guid id)
    {
        var memberType = await _memberTypeService.GetAsync(id);

        if (memberType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        IEnumerable<IMemberType> composedOf = _memberTypeService.GetComposedOf(memberType.Id);
        List<MemberTypeCompositionResponseModel> responseModels = _umbracoMapper.MapEnumerable<IMemberType, MemberTypeCompositionResponseModel>(composedOf);

        return Ok(responseModels);
    }
}
