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
            var result = new List<UsageInformation>();
            _userService.GetAll(0, 0, out long total);
            int userGroups = _userService.GetAllUserGroups().Count();

            result.Add( new UsageInformation("UserCount", total));
            result.Add(new UsageInformation("UserGroupCount", userGroups));

            return result;
        }
    }
}
