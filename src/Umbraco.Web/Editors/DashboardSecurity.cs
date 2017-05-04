using System;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration.Dashboard;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

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
            if (user.Id.ToString(CultureInfo.InvariantCulture) == 0.ToInvariantString())
            {
                return true;
            }

            var denyTypes = dashboardSection.AccessRights.Rules.Where(x => x.Action == AccessType.Deny).ToArray();
            var grantedTypes = dashboardSection.AccessRights.Rules.Where(x => x.Action == AccessType.Grant).ToArray();
            var grantedBySectionTypes = dashboardSection.AccessRights.Rules.Where(x => x.Action == AccessType.GrantBySection).ToArray();

            return CheckUserAccessByRules(user, sectionService, denyTypes, grantedTypes, grantedBySectionTypes);
        }

        public static bool AuthorizeAccess(IDashboardTab dashboardTab, IUser user, ISectionService sectionService)
        {
            if (user.Id.ToString(CultureInfo.InvariantCulture) == Constants.System.Root.ToInvariantString())
            {
                return true;
            }

            var denyTypes = dashboardTab.AccessRights.Rules.Where(x => x.Action == AccessType.Deny).ToArray();
            var grantedTypes = dashboardTab.AccessRights.Rules.Where(x => x.Action == AccessType.Grant).ToArray();
            var grantedBySectionTypes = dashboardTab.AccessRights.Rules.Where(x => x.Action == AccessType.GrantBySection).ToArray();

            return CheckUserAccessByRules(user, sectionService, denyTypes, grantedTypes, grantedBySectionTypes);
        }

        public static bool AuthorizeAccess(IDashboardControl dashboardTab, IUser user, ISectionService sectionService)
        {
            if (user.Id.ToString(CultureInfo.InvariantCulture) == Constants.System.Root.ToInvariantString())
            {
                return true;
            }

            var denyTypes = dashboardTab.AccessRights.Rules.Where(x => x.Action == AccessType.Deny).ToArray();
            var grantedTypes = dashboardTab.AccessRights.Rules.Where(x => x.Action == AccessType.Grant).ToArray();
            var grantedBySectionTypes = dashboardTab.AccessRights.Rules.Where(x => x.Action == AccessType.GrantBySection).ToArray();

            return CheckUserAccessByRules(user, sectionService, denyTypes, grantedTypes, grantedBySectionTypes);
        }

        public static bool CheckUserAccessByRules(IUser user, ISectionService sectionService, IAccessItem[] denyTypes, IAccessItem[] grantedTypes, IAccessItem[] grantedBySectionTypes)
        {
            var allowedSoFar = false;

            //Check if this item as any grant-by-section arguments, if so check if the user has access to any of the sections approved, if so they will
            // be allowed to see it (so far)
            if (grantedBySectionTypes.Any())
            {
                var allowedApps = sectionService.GetAllowedSections(Convert.ToInt32(user.Id))
                                                .Select(x => x.Alias)
                                                .ToArray();

                var allApprovedSections = grantedBySectionTypes.SelectMany(g => g.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
                if (allApprovedSections.Any(allowedApps.Contains))
                {
                    allowedSoFar = true;
                }
            }

            return allowedSoFar;
        }
    }
}