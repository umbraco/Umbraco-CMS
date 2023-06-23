using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class UserTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserService _userService;

    [Obsolete("Use constructor that takes IUserGroupService, scheduled for removal in V15")]
    public UserTelemetryProvider(IUserService userService)
        : this(userService, StaticServiceProvider.Instance.GetRequiredService<IUserGroupService>())
    {
    }

    [Obsolete("Use constructor that only takes IUserGroupService, scheduled for removal in V15")]
    public UserTelemetryProvider(IUserService userService, IUserGroupService userGroupService)
    {
        _userService = userService;
        _userGroupService = userGroupService;
    }

    public IEnumerable<UsageInformation> GetInformation()
    {
        _userService.GetAll(1, 1, out var total);
        var userGroups = _userGroupService.GetAllAsync(0, int.MaxValue).GetAwaiter().GetResult().Items.Count();

        yield return new UsageInformation(Constants.Telemetry.UserCount, total);
        yield return new UsageInformation(Constants.Telemetry.UserGroupCount, userGroups);
    }
}
