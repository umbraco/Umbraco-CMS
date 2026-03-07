using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

    /// <summary>
    /// Controller for managing data type folders by their unique key.
    /// </summary>
[ApiVersion("1.0")]
public class ByKeyDataTypeFolderController : DataTypeFolderControllerBase
{
    /// <summary>
    /// Constructor for <see cref="Umbraco.Cms.Api.Management.Controllers.DataType.Folder.ByKeyDataTypeFolderController"/>.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features.</param>
    /// <param name="dataTypeContainerService">Service for managing data type containers.</param>
    public ByKeyDataTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDataTypeContainerService dataTypeContainerService)
        : base(backOfficeSecurityAccessor, dataTypeContainerService)
    {
    }

    /// <summary>
    /// Retrieves a data type folder by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the data type folder to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="FolderResponseModel"/> with the folder data if found; otherwise, a <see cref="ProblemDetails"/> with status 404 if not found.
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a data type folder.")]
    [EndpointDescription("Gets a data type folder identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id) => await GetFolderAsync(id);
}
