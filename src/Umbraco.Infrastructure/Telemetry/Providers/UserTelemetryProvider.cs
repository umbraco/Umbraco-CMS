using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <summary>
/// Provides telemetry data about user activities and interactions within the system.
/// </summary>
public class UserTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserTelemetryProvider"/> class, which provides telemetry data related to users.
    /// </summary>
    /// <param name="userService">The service used to manage and retrieve user information.</param>
    /// <param name="userGroupService">The service used to manage and retrieve user group information.</param>
    public UserTelemetryProvider(IUserService userService, IUserGroupService userGroupService)
    {
        _userService = userService;
        _userGroupService = userGroupService;
    }

    /// <summary>
    /// Retrieves telemetry information about the number of users and user groups in the system.
    /// </summary>
    /// <returns>
    /// An enumerable collection of <see cref="UsageInformation"/> instances, where each instance contains telemetry data such as the total user count and user group count.
    /// </returns>
    public IEnumerable<UsageInformation> GetInformation()
    {
        _userService.GetAll(1, 1, out var total);
        var userGroups = _userGroupService.GetAllAsync(0, int.MaxValue).GetAwaiter().GetResult().Items.Count();

        yield return new UsageInformation(Constants.Telemetry.UserCount, total);
        yield return new UsageInformation(Constants.Telemetry.UserGroupCount, userGroups);
    }
}
