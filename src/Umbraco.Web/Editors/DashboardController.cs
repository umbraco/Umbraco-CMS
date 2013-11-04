using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using System.Linq;
using System.Xml;
using Umbraco.Core.IO;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for getting entity objects, basic name, icon, id representation of any umbraco object
    /// </summary>
    [PluginController("UmbracoApi")]
    public class DashboardController : UmbracoAuthorizedJsonController
    {

        public IEnumerable<Tab<DashboardControl>> GetDashboard(string section)
        {
            var tabs = new List<Tab<DashboardControl>>();

            var dashboardSection = UmbracoConfig.For.DashboardSettings()
                                                .Sections.FirstOrDefault(x => x.Areas.Contains(section));
            
            //if we cannot find it for whatever reason just return an empty one.
            if (dashboardSection == null)
            {
                return tabs;
            }

            //we need to validate access to this section
            if (DashboardSecurity.AuthorizeAccess(dashboardSection, Security.CurrentUser, Services.SectionService) == false)
            {
                //return empty collection
                return tabs;
            }

            var i = 1;
            foreach (var dashTab in dashboardSection.Tabs)
            {
                //we need to validate access to this tab
                if (DashboardSecurity.AuthorizeAccess(dashTab, Security.CurrentUser, Services.SectionService))
                {
                    var props = new List<DashboardControl>();
                    
                    foreach (var dashCtrl in dashTab.Controls)
                    {
                        if (DashboardSecurity.AuthorizeAccess(dashCtrl, Security.CurrentUser, Services.SectionService))
                        {
                            var ctrl = new DashboardControl();
                            var controlPath = dashCtrl.ControlPath.Trim(' ', '\r', '\n');
                            ctrl.Path = IOHelper.FindFile(controlPath);
                            if (controlPath.ToLower().EndsWith(".ascx"))
                            {
                                ctrl.ServerSide = true;
                            }
                            props.Add(ctrl);
                        }
                    }

                    tabs.Add(new Tab<DashboardControl>
                    {
                        Id = i,
                        Alias = dashTab.Caption.ToSafeAlias(),
                        IsActive = i == 1,
                        Label = dashTab.Caption,
                        Properties = props
                    });
                    i++;
                }
            }

            return tabs;

        }

    }
}
