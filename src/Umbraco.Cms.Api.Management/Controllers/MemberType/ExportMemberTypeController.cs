using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class ExportMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUdtFileContentFactory _fileContentFactory;

    public ExportMemberTypeController(
        IMemberTypeService memberTypeService,
        IUdtFileContentFactory fileContentFactory)
    {
        _memberTypeService = memberTypeService;
        _fileContentFactory = fileContentFactory;
    }

    [HttpGet("{id:guid}/export")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Export(
        CancellationToken cancellationToken,
        Guid id)
    {
        IMemberType? memberType = _memberTypeService.Get(id);
        if (memberType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        return _fileContentFactory.Create(memberType);
    }
}
