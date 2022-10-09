using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     A utility class for determine dashboard security
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly DashboardCollection _dashboardCollection;

    private readonly ILocalizedTextService _localizedText;

    // TODO: Unit test all this!!! :/
    private readonly ISectionService _sectionService;

    public DashboardService(ISectionService sectionService, DashboardCollection dashboardCollection, ILocalizedTextService localizedText)
    {
        _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
        _dashboardCollection = dashboardCollection ?? throw new ArgumentNullException(nameof(dashboardCollection));
        _localizedText = localizedText ?? throw new ArgumentNullException(nameof(localizedText));
    }

    /// <inheritdoc />
    public IEnumerable<Tab<IDashboard>> GetDashboards(string section, IUser? currentUser)
    {
        var tabs = new List<Tab<IDashboard>>();
        var tabId = 0;

        foreach (IDashboard dashboard in _dashboardCollection.Where(x => x.Sections.InvariantContains(section)))
        {
            // validate access
            if (currentUser is null || !CheckUserAccessByRules(currentUser, _sectionService, dashboard.AccessRules))
            {
                continue;
            }

            if (dashboard.View?.InvariantEndsWith(".ascx") ?? false)
            {
                throw new NotSupportedException("Legacy UserControl (.ascx) dashboards are no longer supported.");
            }

            var dashboards = new List<IDashboard> { dashboard };
            tabs.Add(new Tab<IDashboard>
            {
                Id = tabId++,
                Label = _localizedText.Localize("dashboardTabs", dashboard.Alias),
                Alias = dashboard.Alias,
                Properties = dashboards,
            });
        }

        return tabs;
    }

    /// <inheritdoc />
    public IDictionary<string, IEnumerable<Tab<IDashboard>>> GetDashboards(IUser? currentUser) => _sectionService
        .GetSections().ToDictionary(x => x.Alias, x => GetDashboards(x.Alias, currentUser));

    private static (IAccessRule[], IAccessRule[], IAccessRule[]) GroupRules(IEnumerable<IAccessRule> rules)
    {
        IAccessRule[]? denyRules = null, grantRules = null, grantBySectionRules = null;

        IEnumerable<IGrouping<AccessRuleType, IAccessRule>> groupedRules = rules.GroupBy(x => x.Type);
        foreach (IGrouping<AccessRuleType, IAccessRule> group in groupedRules)
        {
            IAccessRule[] a = group.ToArray();
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
                    throw new NotSupportedException($"The '{group.Key}'-AccessRuleType is not supported.");
            }
        }

        return (denyRules ?? Array.Empty<IAccessRule>(), grantRules ?? Array.Empty<IAccessRule>(),
            grantBySectionRules ?? Array.Empty<IAccessRule>());
    }

    private bool CheckUserAccessByRules(IUser user, ISectionService sectionService, IEnumerable<IAccessRule> rules)
    {
        if (user.Id == Constants.Security.SuperUserId)
        {
            return true;
        }

        (IAccessRule[] denyRules, IAccessRule[] grantRules, IAccessRule[] grantBySectionRules) = GroupRules(rules);

        var hasAccess = true;
        string[]? assignedUserGroups = null;

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
                var wantedSections = grantBySectionRules.SelectMany(g =>
                    g.Value?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries) ??
                    Array.Empty<string>()).ToArray();

                if (wantedSections.Intersect(allowedSections).Any())
                {
                    hasAccess = true;
                }
            }

            // if not already granted access, check if this item as any grant arguments.
            // if so check if the user is in one of the user groups approved, if so they will be allowed to see it (so far)
            if (hasAccess == false && grantRules.Any())
            {
                assignedUserGroups = user.Groups.Select(x => x.Alias).ToArray();
                var wantedUserGroups = grantRules.SelectMany(g =>
                    g.Value?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries) ??
                    Array.Empty<string>()).ToArray();

                if (wantedUserGroups.Intersect(assignedUserGroups).Any())
                {
                    hasAccess = true;
                }
            }
        }

        // No need to check denyRules if there aren't any, just return current state
        if (denyRules.Length == 0)
        {
            return hasAccess;
        }

        // check if this item has any deny arguments, if so check if the user is in one of the denied user groups, if so they will
        // be denied to see it no matter what
        assignedUserGroups ??= user.Groups.Select(x => x.Alias).ToArray();
        var deniedUserGroups = denyRules.SelectMany(g =>
                g.Value?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries) ??
                Array.Empty<string>())
            .ToArray();

        if (deniedUserGroups.Intersect(assignedUserGroups).Any())
        {
            hasAccess = false;
        }

        return hasAccess;
    }
}
