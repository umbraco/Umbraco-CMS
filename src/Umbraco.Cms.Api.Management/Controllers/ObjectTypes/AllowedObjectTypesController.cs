using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;

namespace Umbraco.Cms.Api.Management.Controllers.ObjectTypes;

/// <summary>
/// Provides API endpoints for managing the allowed object types in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class AllowedObjectTypesController : ObjectTypesControllerBase
{
    private readonly IObjectTypePresentationFactory _objectTypePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedObjectTypesController"/> class, which manages allowed object types in the Umbraco management API.
    /// </summary>
    /// <param name="objectTypePresentationFactory">A factory used to create presentation models for object types.</param>
    public AllowedObjectTypesController(IObjectTypePresentationFactory objectTypePresentationFactory) => _objectTypePresentationFactory = objectTypePresentationFactory;

    /// <summary>
    /// Retrieves a paginated list of object types that are allowed as relation type targets.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set. Defaults to 0.</param>
    /// <param name="take">The maximum number of items to return. Defaults to 100.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{ObjectTypeResponseModel}"/> representing the paginated collection of allowed object types.
    /// </returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ObjectTypeResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of allowed object types.")]
    [EndpointDescription("Gets a paginated collection of object types that are allowed as relation type targets.")]
    public Task<IActionResult> Allowed(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        ObjectTypeResponseModel[] objectTypes = _objectTypePresentationFactory.Create().ToArray();

        return Task.FromResult<IActionResult>(Ok(new PagedViewModel<ObjectTypeResponseModel>
        {
            Total = objectTypes.Length,
            Items = objectTypes.Skip(skip).Take(take),
        }));
    }
}
