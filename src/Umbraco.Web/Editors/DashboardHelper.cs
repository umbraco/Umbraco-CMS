using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors
{
    internal class DashboardHelper
    {
        private readonly ApplicationContext _appContext;

        public DashboardHelper(ApplicationContext appContext)
        {
            if (appContext == null) throw new ArgumentNullException("appContext");
            _appContext = appContext;
        }

        /// <summary>
        /// Returns the dashboard models per section for the current user and it's access
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public IDictionary<string, IEnumerable<Tab<DashboardControl>>> GetDashboards(IUser currentUser)
        {
            var result = new Dictionary<string, IEnumerable<Tab<DashboardControl>>>();
            foreach (var section in _appContext.Services.SectionService.GetSections())
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

            foreach (var dashboardSection in UmbracoConfig.For.DashboardSettings().Sections.Where(x => x.Areas.Contains(section)))
            {
                //we need to validate access to this section
                if (DashboardSecurity.AuthorizeAccess(dashboardSection, currentUser, _appContext.Services.SectionService) == false)
                    continue;

                //User is authorized
                foreach (var tab in dashboardSection.Tabs)
                {
                    //we need to validate access to this tab
                    if (DashboardSecurity.AuthorizeAccess(tab, currentUser, _appContext.Services.SectionService) == false)
                        continue;

                    var dashboardControls = new List<DashboardControl>();

                    foreach (var control in tab.Controls)
                    {
                        if (DashboardSecurity.AuthorizeAccess(control, currentUser, _appContext.Services.SectionService) == false)
                            continue;

                        var dashboardControl = new DashboardControl();
                        var controlPath = control.ControlPath.Trim();
                        dashboardControl.Caption = control.PanelCaption;
                        dashboardControl.Path = IOHelper.FindFile(controlPath);
                        if (controlPath.ToLowerInvariant().EndsWith(".ascx".ToLowerInvariant()))
                            dashboardControl.ServerSide = true;

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
            var parser = new ManifestParser(appPlugins, _appContext.ApplicationCache.RuntimeCache);
            var builder = new ManifestBuilder(_appContext.ApplicationCache.RuntimeCache, parser);

            var tabs = new List<Tab<DashboardControl>>();
            var i = startTabId;

            foreach (var sectionDashboard in builder.Dashboards.Where(x => x.Value.Areas.InvariantContains(section)))
            {
                foreach (var tab in sectionDashboard.Value.Tabs)
                {
                    var dashboardControls = new List<DashboardControl>();

                    foreach (var control in tab.Value.Controls)
                    {
                        var dashboardControl = new DashboardControl();
                        var controlPath = control.Path.Trim();
                        dashboardControl.Caption = control.Caption;
                        dashboardControl.Path = IOHelper.FindFile(controlPath);
                        if (controlPath.ToLowerInvariant().EndsWith(".ascx".ToLowerInvariant()))
                            dashboardControl.ServerSide = true;

                        dashboardControls.Add(dashboardControl);
                    }

                    tabs.Add(new Tab<DashboardControl>
                    {
                        //assign the Id to the value of the index if one was defined, then we'll use the Id to sort later
                        Id = tab.Value.Index == int.MaxValue ? i : tab.Value.Index,
                        Alias = tab.Key.ToSafeAlias(),
                        Label = tab.Key,
                        Properties = dashboardControls
                    });

                    i++;
                }
            }
            return tabs;
        }
    }
}
