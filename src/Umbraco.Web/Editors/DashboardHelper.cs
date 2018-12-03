using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Dashboard;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors
{
    public class DashboardHelper
    {
        private readonly ISectionService _sectionService;
        private readonly IDashboardSection _dashboardSection;
        private readonly ManifestParser _manifestParser;

        public DashboardHelper(ISectionService sectionService, IDashboardSection dashboardSection, ManifestParser manifestParser)
        {
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
            _dashboardSection = dashboardSection;
            _manifestParser = manifestParser;
        }

        /// <summary>
        /// Returns the dashboard models per section for the current user and it's access
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public IDictionary<string, IEnumerable<Tab<DashboardControl>>> GetDashboards(IUser currentUser)
        {
            var result = new Dictionary<string, IEnumerable<Tab<DashboardControl>>>();
            foreach (var section in _sectionService.GetSections())
            {
                result[section.Alias] = GetDashboard(section.Alias, currentUser);
            }
            return result;
        }

        /// <summary>
        /// Returns the dashboard model for the given section based on the current user and it's access
        /// </summary>
        /// <param name="section"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public IEnumerable<Tab<DashboardControl>> GetDashboard(string section, IUser currentUser)
        {
            var configDashboards = GetDashboardsFromConfig(1, section, currentUser);
            var pluginDashboards = GetDashboardsFromPlugins(configDashboards.Count + 1, section, currentUser);

            //now we need to merge them, the plugin ones would replace anything matched in the config one where the tab alias matches
            var added = new List<Tab<DashboardControl>>(); //to track the ones we'll add
            foreach (var configDashboard in configDashboards)
            {
                var matched = pluginDashboards.Where(x => string.Equals(x.Alias, configDashboard.Alias, StringComparison.InvariantCultureIgnoreCase)).ToList();
                foreach (var tab in matched)
                {
                    configDashboard.Label = tab.Label; //overwrite
                    configDashboard.Properties = configDashboard.Properties.Concat(tab.Properties).ToList(); //combine
                    added.Add(tab); //track this
                }
            }

            //now add the plugin dashboards to the config dashboards that have not already been added
            var toAdd = pluginDashboards.Where(pluginDashboard => added.Contains(pluginDashboard) == false).ToList();
            configDashboards.AddRange(toAdd);

            //last thing is to re-sort and ID the tabs
            configDashboards.Sort((tab, tab1) => tab.Id > tab1.Id ? 1 : 0);
            for (var index = 0; index < configDashboards.Count; index++)
            {
                var tab = configDashboards[index];
                tab.Id = (index + 1);
                if (tab.Id == 1)
                    tab.IsActive = true;
            }

            return configDashboards;
        }

        private List<Tab<DashboardControl>> GetDashboardsFromConfig(int startTabId, string section, IUser currentUser)
        {
            var tabs = new List<Tab<DashboardControl>>();
            var i = startTabId;

            //disable packages section dashboard
            if (section == "packages") return tabs;

            foreach (var dashboardSection in _dashboardSection.Sections.Where(x => x.Areas.Contains(section)))
            {
                //we need to validate access to this section
                if (DashboardSecurity.AuthorizeAccess(dashboardSection, currentUser, _sectionService) == false)
                    continue;

                //User is authorized
                foreach (var tab in dashboardSection.Tabs)
                {
                    //we need to validate access to this tab
                    if (DashboardSecurity.AuthorizeAccess(tab, currentUser, _sectionService) == false)
                        continue;

                    var dashboardControls = new List<DashboardControl>();

                    foreach (var control in tab.Controls)
                    {
                        if (DashboardSecurity.AuthorizeAccess(control, currentUser, _sectionService) == false)
                            continue;

                        var dashboardControl = new DashboardControl();
                        var controlPath = control.ControlPath.Trim();
                        dashboardControl.Caption = control.PanelCaption;
                        dashboardControl.Path = IOHelper.FindFile(controlPath);
                        if (controlPath.ToLowerInvariant().EndsWith(".ascx".ToLowerInvariant()))
                            throw new NotSupportedException("Legacy UserControl (.ascx) dashboards are no longer supported");

                        dashboardControls.Add(dashboardControl);
                    }

                    tabs.Add(new Tab<DashboardControl>
                    {
                        Id = i,
                        Alias = tab.Caption.ToSafeAlias(),
                        Label = tab.Caption,
                        Properties = dashboardControls
                    });

                    i++;
                }
            }

            //In case there are no tabs or a user doesn't have access the empty tabs list is returned
            return tabs;
        }

        private List<Tab<DashboardControl>> GetDashboardsFromPlugins(int startTabId, string section, IUser currentUser)
        {
            //TODO: Need to integrate the security with the manifest dashboards

            var appPlugins = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.AppPlugins));

            var tabs = new List<Tab<DashboardControl>>();
            var i = startTabId;

            foreach (var dashboard in _manifestParser.Manifest.Dashboards.Where(x => x.Sections.InvariantContains(section)))
            {
                var dashboardControls = new List<DashboardControl>();
                var view = dashboard.View.Trim();
                var dashboardControl = new DashboardControl
                {
                    Path = IOHelper.FindFile(view)
                };

                if (view.ToLowerInvariant().EndsWith(".ascx".ToLowerInvariant()))
                    throw new NotSupportedException("Legacy UserControl (.ascx) dashboards are no longer supported");

                dashboardControls.Add(dashboardControl);

                tabs.Add(new Tab<DashboardControl>
                {
                    //assign the Id to the value of the index if one was defined, then we'll use the Id to sort later
                    Id = dashboard.Weight == int.MaxValue ? i : dashboard.Weight,
                    Alias = dashboard.Alias.ToSafeAlias(),
                    Label = dashboard.Name,
                    Properties = dashboardControls
                });

                i++;
            }
            return tabs;
        }
    }
}
