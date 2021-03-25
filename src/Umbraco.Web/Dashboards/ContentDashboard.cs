using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Services;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    public class ContentDashboard : IDashboard
    {
        private readonly IContentDashboardSettings _dashboardSettings;
        private readonly IUserService _userService;
        private IAccessRule[] _accessRulesFromConfig;

        public string Alias => "contentIntro";

        public string[] Sections => new[] { "content" };

        public string View => "views/dashboard/default/startupdashboardintro.html";

        public IAccessRule[] AccessRules
        {
            get
            {
                var rules = AccessRulesFromConfig;

                if (rules.Length == 0)
                {
                    rules = new IAccessRule[]
                    {
                        new AccessRule {Type = AccessRuleType.Deny, Value = Constants.Security.TranslatorGroupAlias},
                        new AccessRule {Type = AccessRuleType.Grant, Value = Constants.Security.AdminGroupAlias}
                    };
                }

                return rules;
            }
        }

        private IAccessRule[] AccessRulesFromConfig
        {
            get
            {
                if (_accessRulesFromConfig is null)
                {
                    var rules = new List<IAccessRule>();

                    if (_dashboardSettings.AllowContentDashboardAccessToAllUsers)
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

                    _accessRulesFromConfig = rules.ToArray();
                }

                return _accessRulesFromConfig;
            }
        }

        public ContentDashboard(IContentDashboardSettings dashboardSettings, IUserService userService)
        {
            _dashboardSettings = dashboardSettings;
            _userService = userService;
        }
    }
}
