using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.UserData;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Cms.Api.Management.Controllers.UserData;

/// <summary>
/// Controller responsible for retrieving user-specific data via the API.
/// </summary>
[ApiVersion("1.0")]
public class GetUserDataController : UserDataControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserDataService _userDataService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserDataController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security operations.</param>
    /// <param name="userDataService">Service for managing user data.</param>
    /// <param name="umbracoMapper">The mapper used for mapping Umbraco objects.</param>
    public GetUserDataController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserDataService userDataService,
        IUmbracoMapper umbracoMapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userDataService = userDataService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves user-specific data for the currently authenticated user, with optional filtering and pagination.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="groups">An optional array of group names to filter the user data by group.</param>
    /// <param name="identifiers">An optional array of identifiers to filter the user data by specific keys.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return in the result set (used for pagination).</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{UserDataResponseModel}"/> representing the filtered and paginated user data.
    /// </returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<UserDataResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets user data.")]
    [EndpointDescription("Gets user-specific data stored for the current authenticated user.")]
    public async Task<IActionResult> GetUserData(CancellationToken cancellationToken, [FromQuery]string[]? groups, [FromQuery]string[]? identifiers, [FromQuery]int skip = 0, [FromQuery]int take = 100)
    {
        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);

        PagedModel<IUserData> data = await _userDataService.GetAsync(
            skip,
            take,
            new UserDataFilter { UserKeys = new[] { currentUserKey }, Groups = groups, Identifiers = identifiers });

        return Ok(new PagedViewModel<UserDataResponseModel>
        {
            Total = data.Total,
            Items = _umbracoMapper.MapEnumerable<IUserData, UserDataResponseModel>(data.Items),
        });
    }
}
