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
        public string Alias => "contentIntro";

        public string[] Sections => new[] { "content" };

        public string View => "views/dashboard/default/startupdashboardintro.html";

        public IAccessRule[] AccessRules
        {
            get
            {
                IAccessRule[] rules;
                var dashboardConfig = Path.Combine(IOHelper.MapPath(SystemDirectories.Config), "content.dashboard.access.config.js");

                if (File.Exists(dashboardConfig))
                {
                    var rawJson = File.ReadAllText(dashboardConfig);
                    rules = JsonConvert.DeserializeObject<AccessRule[]>(rawJson);
                }
                else
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
    }
}
