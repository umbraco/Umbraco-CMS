using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;

namespace Umbraco.Core.Dashboards
{
    public class ContentDashboardSettings: IContentDashboardSettings
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IUserService _userService;

        public ContentDashboardSettings(IGlobalSettings globalSettings, IUserService userService)
        {
            _globalSettings = globalSettings;
            _userService = userService;
        }

        public IAccessRule[] GetAccessRulesFromConfig()
        {
            var rules = new List<IAccessRule>();

            if (_globalSettings.AllowContentDashboardAccessToAllUsers)
            {
                var allUserGroups = _userService.GetAllUserGroups();

                foreach (var userGroup in allUserGroups)
                {
                    rules.Add(new AccessRule
                    {
                        Type = AccessRuleType.Grant,
                        Value = userGroup.Alias
                    });
                }
            }

            return rules.ToArray();
        }
    }
}
