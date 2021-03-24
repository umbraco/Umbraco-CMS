using System.IO;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;
using Umbraco.Core.IO;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    public class ContentDashboard : IDashboard
    {
        private readonly IContentDashboardSettings _dashboardSettings;
        public string Alias => "contentIntro";

        public string[] Sections => new[] { "content" };

        public string View => "views/dashboard/default/startupdashboardintro.html";

        public IAccessRule[] AccessRules
        {
            get
            {
                var rules = _dashboardSettings.GetAccessRulesFromConfig();

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

        public ContentDashboard(IContentDashboardSettings dashboardSettings)
        {
            _dashboardSettings = dashboardSettings;
        }
    }
}
