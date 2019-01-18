using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration.Dashboard;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Services;

namespace Umbraco.Web.Editors
{
    public class Dashboards
    {
        private readonly ISectionService _sectionService;
        private readonly IDashboardSection _dashboardSection;
        private readonly ManifestParser _manifestParser;

        public Dashboards(ISectionService sectionService, IDashboardSection dashboardSection, ManifestParser manifestParser)
        {
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
            _dashboardSection = dashboardSection;
            _manifestParser = manifestParser;
        }

        /// <summary>
        /// Gets all dashboards, organized by section, for a user.
        /// </summary>
        public IDictionary<string, IEnumerable<Tab<DashboardControl>>> GetDashboards(IUser currentUser)
        {
            return _sectionService.GetSections().ToDictionary(x => x.Alias, x => GetDashboards(x.Alias, currentUser));
        }

        /// <summary>
        /// Returns dashboards for a specific section, for a user.
        /// </summary>
        public IEnumerable<Tab<DashboardControl>> GetDashboards(string section, IUser currentUser)
        {
            var tabId = 1;
            var configDashboards = GetDashboardsFromConfig(ref tabId, section, currentUser);
            var pluginDashboards = GetDashboardsFromPlugins(ref tabId, section, currentUser);

            // merge dashboards
            // both collections contain tab.alias -> controls
            var dashboards = configDashboards;

            // until now, it was fine to have duplicate tab.aliases in configDashboard
            // so... the rule should be - just merge whatever we get, don't be clever
            dashboards.AddRange(pluginDashboards);

            // re-sort by id
            dashboards.Sort((tab1, tab2) => tab1.Id > tab2.Id ? 1 : 0);

            // re-assign ids (why?)
            var i = 1;
            foreach (var tab in dashboards)
            {
                tab.Id = i++;
                tab.IsActive = tab.Id == 1;
            }

            return configDashboards;
        }

        // note:
        // in dashboard.config we have 'sections' which define 'tabs' for 'areas'
        // and 'areas' are the true UI sections - and each tab can have more than
        // one control
        // in a manifest, we directly have 'dashboards' which map to a unique
        // control in a tab

        // gets all tabs & controls from the config file
        private List<Tab<DashboardControl>> GetDashboardsFromConfig(ref int tabId, string section, IUser currentUser)
        {
            var tabs = new List<Tab<DashboardControl>>();

            // disable packages section dashboard
            if (section == "packages") return tabs;

            foreach (var dashboardSection in _dashboardSection.Sections.Where(x => x.Areas.InvariantContains(section)))
            {
                // validate access to this section
                if (!DashboardSecurity.AuthorizeAccess(dashboardSection, currentUser, _sectionService))
                    continue;

                foreach (var tab in dashboardSection.Tabs)
                {
                    // validate access to this tab
                    if (!DashboardSecurity.AuthorizeAccess(tab, currentUser, _sectionService))
                        continue;

                    var dashboardControls = new List<DashboardControl>();

                    foreach (var control in tab.Controls)
                    {
                        // validate access to this control
                        if (!DashboardSecurity.AuthorizeAccess(control, currentUser, _sectionService))
                            continue;

                        // create and add control
                        var dashboardControl = new DashboardControl
                        {
                            Caption = control.PanelCaption,
                            Path = IOHelper.FindFile(control.ControlPath.Trim())
                        };

                        if (dashboardControl.Path.InvariantEndsWith(".ascx"))
                            throw new NotSupportedException("Legacy UserControl (.ascx) dashboards are no longer supported.");

                        dashboardControls.Add(dashboardControl);
                    }

                    // create and add tab
                    tabs.Add(new Tab<DashboardControl>
                    {
                        Id = tabId++,
                        Alias = tab.Caption.ToSafeAlias(),
                        Label = tab.Caption,
                        Properties = dashboardControls
                    });
                }
            }

            return tabs;
        }

        private List<Tab<DashboardControl>> GetDashboardsFromPlugins(ref int tabId, string section, IUser currentUser)
        {
            var tabs = new List<Tab<DashboardControl>>();

            foreach (var dashboard in _manifestParser.Manifest.Dashboards.Where(x => x.Sections.InvariantContains(section)).OrderBy(x => x.Weight))
            {
                // validate access
                if (!DashboardSecurity.CheckUserAccessByRules(currentUser, _sectionService, dashboard.AccessRules))
                    continue;

                var dashboardControl = new DashboardControl
                {
                    Caption = "",
                    Path = IOHelper.FindFile(dashboard.View.Trim())
                };

                if (dashboardControl.Path.InvariantEndsWith(".ascx"))
                    throw new NotSupportedException("Legacy UserControl (.ascx) dashboards are no longer supported.");

                tabs.Add(new Tab<DashboardControl>
                {
                    Id = tabId++,
                    Alias = dashboard.Alias.ToSafeAlias(),
                    Label = dashboard.Name,
                    Properties = new[] { dashboardControl }
                });
            }

            return tabs;
        }
    }
}
