using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration.Dashboard;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Services;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// A utility class for determine dashboard security
    /// </summary>
    internal class DashboardSecurity
    {
        //TODO: Unit test all this!!! :/

        public static bool AuthorizeAccess(ISection dashboardSection, IUser user, ISectionService sectionService)
        {
            return CheckUserAccessByRules(user, sectionService, dashboardSection.AccessRights.Rules);
        }

        public static bool AuthorizeAccess(IDashboardTab dashboardTab, IUser user, ISectionService sectionService)
        {
            return CheckUserAccessByRules(user, sectionService, dashboardTab.AccessRights.Rules);
        }

        public static bool AuthorizeAccess(IDashboardControl dashboardControl, IUser user, ISectionService sectionService)
        {
            return CheckUserAccessByRules(user, sectionService, dashboardControl.AccessRights.Rules);
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
                        throw new Exception("panic");
                }
            }

            return (denyRules ?? Array.Empty<IAccessRule>(), grantRules ?? Array.Empty<IAccessRule>(), grantBySectionRules ?? Array.Empty<IAccessRule>());
        }

        public static bool CheckUserAccessByRules(IUser user, ISectionService sectionService, IEnumerable<IAccessRule> rules)
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
                return false;

            // check if this item has any deny arguments, if so check if the user is in one of the denied user groups, if so they will
            // be denied to see it no matter what
            assignedUserGroups = assignedUserGroups ?? user.Groups.Select(x => x.Alias).ToArray();
            var deniedUserGroups = denyRules.SelectMany(g => g.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            if (deniedUserGroups.Intersect(assignedUserGroups).Any())
                hasAccess = false;

            return hasAccess;
        }
    }
}
