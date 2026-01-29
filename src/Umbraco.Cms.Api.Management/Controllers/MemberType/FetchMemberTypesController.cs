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
public class FetchMemberTypesController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IMemberTypePresentationFactory _memberTypePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FetchMemberTypesController"/> class.
    /// </summary>
    /// <param name="memberTypeService">The member type service.</param>
    /// <param name="memberTypePresentationFactory">The member type presentation factory.</param>
    public FetchMemberTypesController(IMemberTypeService memberTypeService, IMemberTypePresentationFactory memberTypePresentationFactory)
    {
        _memberTypeService = memberTypeService;
        _memberTypePresentationFactory = memberTypePresentationFactory;
    }

    [HttpPost("fetch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FetchResponseModel<MemberTypeResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Fetch(CancellationToken cancellationToken, FetchRequestModel requestModel)
    {
        Guid[] ids = [.. requestModel.Ids.Select(x => x.Id).Distinct()];

        if (ids.Length == 0)
        {
            return Ok(new FetchResponseModel<MemberTypeResponseModel>());
        }

        IEnumerable<IMemberType> memberTypes = _memberTypeService.GetMany(ids);

        List<IMemberType> ordered = OrderByRequestedIds(memberTypes, ids);

        // Member type mapping is async via factory.
        IEnumerable<Task<MemberTypeResponseModel>> mappingTasks = ordered.Select(mt => _memberTypePresentationFactory.CreateResponseModelAsync(mt));
        MemberTypeResponseModel[] responseModels = await Task.WhenAll(mappingTasks);

        return Ok(new FetchResponseModel<MemberTypeResponseModel>
        {
            Total = responseModels.Length,
            Items = responseModels,
        });
    }
}
