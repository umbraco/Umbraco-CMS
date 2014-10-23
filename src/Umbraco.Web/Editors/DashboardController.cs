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
  
    [PluginController("UmbracoApi")]
    public class DashboardController : UmbracoAuthorizedJsonController
    {

        public IEnumerable<Tab<DashboardControl>> GetDashboard(string section)
        {
            var tabs = new List<Tab<DashboardControl>>();
            var i = 1;

            // The dashboard config can contain more than one area inserted by a package.
            foreach( var dashboardSection in UmbracoConfig.For.DashboardSettings().Sections.Where(x => x.Areas.Contains(section)))
            {
                //we need to validate access to this section
                if (DashboardSecurity.AuthorizeAccess(dashboardSection, Security.CurrentUser, Services.SectionService))
                {
                    //User is authorized
                    foreach (var dashTab in dashboardSection.Tabs)
                    {
                        //we need to validate access to this tab
                        if (DashboardSecurity.AuthorizeAccess(dashTab, Security.CurrentUser, Services.SectionService))
                        {
                            var props = new List<DashboardControl>();

                            foreach (var dashCtrl in dashTab.Controls)
                            {
                                if (DashboardSecurity.AuthorizeAccess(dashCtrl, Security.CurrentUser,
                                                                      Services.SectionService))
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
                }
            }

            //In case there are no tabs or a user doesn't have access the empty tabs list is returned
            return tabs;

        }

    }
}
