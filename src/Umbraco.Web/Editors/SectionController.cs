using System.Collections.Generic;
using AutoMapper;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Trees;
using Section = Umbraco.Web.Models.ContentEditing.Section;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for using the list of sections
    /// </summary>
    [PluginController("UmbracoApi")]
    public class SectionController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<Section> GetSections()
        {

            var sections =  Services.SectionService.GetAllowedSections(Security.GetUserId());

            var sectionModels = sections.Select(Mapper.Map<Core.Models.Section, Section>).ToArray();

            //Check if there are empty dashboards or dashboards that will end up empty based on the current user's access
            //and add the meta data about them
            var dashboardHelper = new DashboardHelper(Services.SectionService);
            //this is a bit nasty since we'll be proxying via the app tree controller but we sort of have to do that
            //since tree's by nature are controllers and require request contextual data.
            var appTreeController = new ApplicationTreeController
            {
                ControllerContext = ControllerContext
            };
            var dashboards = dashboardHelper.GetDashboards(Security.CurrentUser);
            //now we can add metadata for each section so that the UI knows if there's actually anything at all to render for
            //a dashboard for a given section, then the UI can deal with it accordingly (i.e. redirect to the first tree)
            foreach (var section in sectionModels)
            {
                var hasDashboards = false;
                IEnumerable<Tab<DashboardControl>> dashboardsForSection;
                if (dashboards.TryGetValue(section.Alias, out dashboardsForSection))
                {
                    if (dashboardsForSection.Any())                    
                        hasDashboards = true;
                }

                if (hasDashboards == false)
                {
                    //get the first tree in the section and get it's root node route path
                    var sectionTrees = appTreeController.GetApplicationTrees(section.Alias, null, null).Result;
                    section.RoutePath = sectionTrees.IsContainer == false || sectionTrees.Children.Count == 0
                        ? sectionTrees.RoutePath 
                        : sectionTrees.Children[0].RoutePath;
                }
            }

            return sectionModels;
        }

        /// <summary>
        /// Returns all the sections that the user has access to
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Section> GetAllSections()
        {
            var sections = Services.SectionService.GetSections();
            var mapped = sections.Select(Mapper.Map<Core.Models.Section, Section>);
            if (Security.CurrentUser.IsAdmin())
                return mapped;

            return mapped.Where(x => Security.CurrentUser.AllowedSections.Contains(x.Alias)).ToArray();
        }

    }
}