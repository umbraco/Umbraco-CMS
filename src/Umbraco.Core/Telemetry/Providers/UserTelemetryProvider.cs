using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class UserTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly IUserService _userService;

        public UserTelemetryProvider(IUserService userService)
        {
            _userService = userService;
        }

        public IEnumerable<UsageInformation> GetInformation()
        {
            _userService.GetAll(1, 1, out long total);
            int userGroups = _userService.GetAllUserGroups().Count();

            yield return new UsageInformation(Constants.Telemetry.UserCount, total);
            yield return new UsageInformation(Constants.Telemetry.UserGroupCount, userGroups);
        }
    }
}
