using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Dashboards;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Services
{
    internal class DashboardService : IDashboardService
    {
        private readonly ISectionService _sectionService;
        private readonly DashboardCollection _dashboardCollection;

        public DashboardService(ISectionService sectionService, DashboardCollection dashboardCollection)
        {
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
            _dashboardCollection = dashboardCollection ?? throw new ArgumentNullException(nameof(dashboardCollection));
        }


        /// <inheritdoc />
        public IEnumerable<Tab<IDashboardSection>> GetDashboards(string section, IUser currentUser)
        {
            var tabs = new List<Tab<IDashboardSection>>();
            var tabId = 0;

            foreach (var dashboard in _dashboardCollection.Where(x => x.Sections.InvariantContains(section)))
            {
                // validate access
                if (!CheckUserAccessByRules(currentUser, _sectionService, dashboard.AccessRules))
                    continue;

                if (dashboard.View.InvariantEndsWith(".ascx"))
                    throw new NotSupportedException("Legacy UserControl (.ascx) dashboards are no longer supported.");

                var dashboards = new List<IDashboardSection> {dashboard};

                tabs.Add(new Tab<IDashboardSection>
                {
                    Id = tabId++,
                    Label = dashboard.Name,
                    Alias = dashboard.Alias,
                    Properties = dashboards
                });
            }

            return tabs;
        }

        /// <inheritdoc />
        public IDictionary<string, IEnumerable<Tab<IDashboardSection>>> GetDashboards(IUser currentUser)
        {
            return _sectionService.GetSections().ToDictionary(x => x.Alias, x => GetDashboards(x.Alias, currentUser));
        }

        private bool CheckUserAccessByRules(IUser user, ISectionService sectionService, IEnumerable<IAccessRule> rules)
        {
            if (user.Id == Constants.Security.SuperUserId)
                return true;

            var (denyRules, grantRules, grantBySectionRules) = GroupRules(rules);

            var hasAccess = true;
            string[] assignedUserGroups = null;

            // if there are no grant rules, then access is granted by default, unless denied
            // otherwise, grant rules determine if access can be granted at all
            if (grantBySectionRules.Length > 0 || grantRules.Length > 0)
            {
                hasAccess = false;

                // check if this item has any grant-by-section arguments.
                // if so check if the user has access to any of the sections approved, if so they will be allowed to see it (so far)
                if (grantBySectionRules.Length > 0)
                {
                    var allowedSections = sectionService.GetAllowedSections(user.Id).Select(x => x.Alias).ToArray();
                    var wantedSections = grantBySectionRules.SelectMany(g => g.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

                    if (wantedSections.Intersect(allowedSections).Any())
                        hasAccess = true;
                }

                // if not already granted access, check if this item as any grant arguments.
                // if so check if the user is in one of the user groups approved, if so they will be allowed to see it (so far)
                if (hasAccess == false && grantRules.Any())
                {
                    assignedUserGroups = user.Groups.Select(x => x.Alias).ToArray();
                    var wantedUserGroups = grantRules.SelectMany(g => g.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

                    if (wantedUserGroups.Intersect(assignedUserGroups).Any())
                        hasAccess = true;
                }
            }

            if (!hasAccess || denyRules.Length == 0)
                return true;

            // check if this item has any deny arguments, if so check if the user is in one of the denied user groups, if so they will
            // be denied to see it no matter what
            assignedUserGroups = assignedUserGroups ?? user.Groups.Select(x => x.Alias).ToArray();
            var deniedUserGroups = denyRules.SelectMany(g => g.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            if (deniedUserGroups.Intersect(assignedUserGroups).Any())
                hasAccess = false;

            return hasAccess;
        }

        private static (IAccessRule[], IAccessRule[], IAccessRule[]) GroupRules(IEnumerable<IAccessRule> rules)
        {
            IAccessRule[] denyRules = null, grantRules = null, grantBySectionRules = null;

            var groupedRules = rules.GroupBy(x => x.Type);
            foreach (var group in groupedRules)
            {
                var a = group.ToArray();
                switch (group.Key)
                {
                    case AccessRuleType.Deny:
                        denyRules = a;
                        break;
                    case AccessRuleType.Grant:
                        grantRules = a;
                        break;
                    case AccessRuleType.GrantBySection:
                        grantBySectionRules = a;
                        break;
                    default:
                        throw new NotSupportedException($"The '{group.Key.ToString()}'-AccessRuleType is not supported.");
                }
            }

            return (denyRules ?? Array.Empty<IAccessRule>(), grantRules ?? Array.Empty<IAccessRule>(), grantBySectionRules ?? Array.Empty<IAccessRule>());
        }
    }
}
