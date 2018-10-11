using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors
{
    internal class DashboardHelper
    {
        private readonly ISectionService _sectionService;

        public DashboardHelper(ISectionService sectionService)
        {
            if (sectionService == null) throw new ArgumentNullException("sectionService");
            _sectionService = sectionService;
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
            var tabs = new List<Tab<DashboardControl>>();
            var i = 1;

            //disable packages section dashboard
            if (section == "packages") return tabs;

            foreach (var dashboardSection in UmbracoConfig.For.DashboardSettings().Sections.Where(x => x.Areas.Contains(section)))
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
                            dashboardControl.ServerSide = true;

                        dashboardControls.Add(dashboardControl);
                    }

                    tabs.Add(new Tab<DashboardControl>
                    {
                        Id = i,
                        Alias = tab.Caption.ToSafeAlias(),
                        IsActive = i == 1,
                        Label = tab.Caption,
                        Properties = dashboardControls
                    });

                    i++;
                }
            }

            //In case there are no tabs or a user doesn't have access the empty tabs list is returned
            return tabs;
        }
    }
}
