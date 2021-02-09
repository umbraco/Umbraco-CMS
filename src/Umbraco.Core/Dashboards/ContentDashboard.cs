using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards
{
    [Weight(10)]
    public class ContentDashboard : IDashboard
    {
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
