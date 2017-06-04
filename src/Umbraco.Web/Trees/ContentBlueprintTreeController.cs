using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// The content blueprint tree controller
    /// </summary>
    /// <remarks>
    /// This authorizes based on access to the content section even though it exists in the settings
    /// </remarks>
    [UmbracoApplicationAuthorize(Constants.Applications.Content)]
    [Tree(Constants.Applications.Settings, Constants.Trees.ContentBlueprints, null, sortOrder: 8)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class ContentBlueprintTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            //check if we're rendering the root
            if (id == Constants.System.Root.ToInvariantString())
            {
                var altStartId = string.Empty;

                if (queryStrings.HasKey(TreeQueryStringParameters.StartNodeId))
                    altStartId = queryStrings.GetValue<string>(TreeQueryStringParameters.StartNodeId);

                //check if a request has been made to render from a specific start node
                if (string.IsNullOrEmpty(altStartId) == false && altStartId != "undefined" && altStartId != Constants.System.Root.ToString(CultureInfo.InvariantCulture))
                {
                    id = altStartId;
                }

                var entities = Services.EntityService.GetChildren(Constants.System.Root, UmbracoObjectTypes.DocumentBlueprint).ToArray();

                nodes.AddRange(entities
                    .Select(entity => CreateTreeNode(entity, Constants.ObjectTypes.DocumentBlueprintGuid, id, queryStrings, "icon-blueprint", false))
                    .Where(node => node != null));

                return nodes;
            }

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions                              
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);
                return menu;
            }

            menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));

            return menu;
        }
        
    }
}