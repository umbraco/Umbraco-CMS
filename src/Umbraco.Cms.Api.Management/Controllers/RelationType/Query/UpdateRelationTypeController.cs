using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

[ApiVersion("1.0")]
public class UpdateRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateRelationTypeController(
        IRelationService relationService,
        IRelationTypePresentationFactory relationTypePresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _relationService = relationService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RelationTypeResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateRelationTypeRequestModel updateRelationTypeSavingViewModel)
    {
        IRelationType? persistedRelationType = _relationService.GetRelationTypeById(id);

        if (persistedRelationType is null)
        {
            ProblemDetails problemDetails = new ProblemDetailsBuilder()
                .WithTitle("Could not find relation type")
                .WithDetail($"Relation type with id {id} could not be found")
                .Build();
            return NotFound(problemDetails);
        }

        _relationTypePresentationFactory.MapUpdateModelToRelationType(updateRelationTypeSavingViewModel, persistedRelationType);

        Attempt<IRelationType, RelationTypeOperationStatus> result = await _relationService.UpdateAsync(persistedRelationType, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success ? await Task.FromResult(Ok()) : RelationTypeOperationStatusResult(result.Status);
    }
}
