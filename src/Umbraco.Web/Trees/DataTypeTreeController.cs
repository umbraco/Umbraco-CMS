using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoApplicationAuthorize(Constants.Applications.Developer)]
    [Tree(Constants.Applications.Developer, Constants.Trees.DataTypes, "Data Types")]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class DataTypeTreeController : TreeController
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
                                    id,
                                    queryStrings,
                                    dt.Name,
                                    "icon-autofill",
                                    false)));
            return collection;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions              
                menu.Items.Add<CreateChildEntity, ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }
            
            //only have delete for each node
            menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));            
            return menu;
        }
    }
}