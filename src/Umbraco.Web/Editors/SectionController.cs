﻿using System.Collections.Generic;
using AutoMapper;
using Umbraco.Web.Mvc;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Trees;
using Section = Umbraco.Web.Models.ContentEditing.Section;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for using the list of sections
    /// </summary>
    [PluginController("UmbracoApi")]
    public class SectionController : UmbracoAuthorizedJsonController
    {
        private readonly Dashboards _dashboards;

        public SectionController(Dashboards dashboards)
        {
            _dashboards = dashboards;
        }

        public IEnumerable<Section> GetSections()
        {
            var sections =  Services.SectionService.GetAllowedSections(Security.GetUserId().ResultOr(0));

            var sectionModels = sections.Select(Mapper.Map<Core.Models.Section, Section>).ToArray();
            
            // this is a bit nasty since we'll be proxying via the app tree controller but we sort of have to do that
            // since tree's by nature are controllers and require request contextual data
            var appTreeController = new ApplicationTreeController { ControllerContext = ControllerContext };

            var dashboards = _dashboards.GetDashboards(Security.CurrentUser);

            //now we can add metadata for each section so that the UI knows if there's actually anything at all to render for
            //a dashboard for a given section, then the UI can deal with it accordingly (i.e. redirect to the first tree)
            foreach (var section in sectionModels)
            {
                var hasDashboards = dashboards.TryGetValue(section.Alias, out var dashboardsForSection) && dashboardsForSection.Any();
                if (hasDashboards) continue;

                // get the first tree in the section and get its root node route path
                var sectionRoot = appTreeController.GetApplicationTrees(section.Alias, null, null).Result;
                section.RoutePath = GetRoutePathForFirstTree(sectionRoot);
            }

            return sectionModels;
        }

        /// <summary>
        /// Returns the first non root/group node's route path
        /// </summary>
        /// <param name="rootNode"></param>
        /// <returns></returns>
        private string GetRoutePathForFirstTree(TreeRootNode rootNode)
        {
            if (!rootNode.IsContainer || !rootNode.ContainsTrees)
                return rootNode.RoutePath;

            foreach(var node in rootNode.Children)
            {
                if (node is TreeRootNode groupRoot)
                    return GetRoutePathForFirstTree(groupRoot);//recurse to get the first tree in the group
                else
                    return node.RoutePath;
            }

            return string.Empty;
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
