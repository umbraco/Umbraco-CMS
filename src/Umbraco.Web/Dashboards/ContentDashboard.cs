using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    public class ContentDashboard : IDashboardSection
    {
        public string Name => "Get Started";

        public string Alias => "contentIntro";

        public string[] Sections => new [] { "content" };

        public string View => "views/dashboard/default/startupdashboardintro.html";

        public IAccessRule[] AccessRules
        {
            get
            {
                var rules = new IAccessRule[]
                {
                    new AccessRule {Type = AccessRuleType.Deny, Value = Constants.Security.TranslatorGroupAlias},
                    new AccessRule {Type = AccessRuleType.Grant, Value = Constants.Security.AdminGroupAlias}
                };
                return rules;
            }
        }
    }
}
