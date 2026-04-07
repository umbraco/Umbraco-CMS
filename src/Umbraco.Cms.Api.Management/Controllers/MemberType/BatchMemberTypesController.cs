using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

/// <summary>
/// Provides an API controller for retrieving the full details for multiple member types by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchMemberTypesController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IMemberTypePresentationFactory _memberTypePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchMemberTypesController"/> class.
    /// </summary>
    /// <param name="memberTypeService">The member type service.</param>
    /// <param name="memberTypePresentationFactory">The member type presentation factory.</param>
    public BatchMemberTypesController(IMemberTypeService memberTypeService, IMemberTypePresentationFactory memberTypePresentationFactory)
    {
        _memberTypeService = memberTypeService;
        _memberTypePresentationFactory = memberTypePresentationFactory;
    }

    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<MemberTypeResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple member types.")]
    [EndpointDescription("Gets multiple member types identified by the provided Ids.")]
    public async Task<IActionResult> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Guid[] requestedIds = [.. ids];

        if (requestedIds.Length == 0)
        {
            return Ok(new BatchResponseModel<MemberTypeResponseModel>());
        }

        IEnumerable<IMemberType> memberTypes = _memberTypeService.GetMany(requestedIds);

        List<IMemberType> ordered = OrderByRequestedIds(memberTypes, requestedIds);

        // Member type mapping is async via factory.
        IEnumerable<Task<MemberTypeResponseModel>> mappingTasks = ordered.Select(mt => _memberTypePresentationFactory.CreateResponseModelAsync(mt));
        MemberTypeResponseModel[] responseModels = await Task.WhenAll(mappingTasks);

        return Ok(new BatchResponseModel<MemberTypeResponseModel>
        {
            Total = responseModels.Length,
            Items = responseModels,
        });
    }
}
