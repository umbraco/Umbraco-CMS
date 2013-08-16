using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees.Menu;
using umbraco.BusinessLogic.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Developer, Constants.Trees.DataTypes, "Data Types")]
    [PluginController("UmbracoTrees")]
    public class DataTypeTreeController : TreeApiController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            //we only support one tree level for data types
            if (id != Constants.System.Root.ToInvariantString())
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var collection = new TreeNodeCollection();
            collection.AddRange(
                Services.DataTypeService.GetAllDataTypeDefinitions()
                        .OrderBy(x => x.Name)
                        .Select(dt =>
                                CreateTreeNode(
                                    dt.Id.ToInvariantString(),
                                    queryStrings,
                                    dt.Name,
                                    "icon-tasks",
                                    false)));
            return collection;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions              
                menu.AddMenuItem<ActionNew>();
                menu.AddMenuItem<RefreshNodeMenuItem, ActionRefresh>(true);
                return menu;
            }

            //only have delete for each node
            menu.AddMenuItem<ActionDelete>();            
            return menu;
        }
    }
}