using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.DataTypes)]
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

            var sysIds = GetSystemIds();

            var collection = new TreeNodeCollection();
            collection.AddRange(
                Services.DataTypeService.GetAllDataTypeDefinitions()
                    .OrderBy(x => x.Name)
                    .Select(dt => CreateTreeNode(id, queryStrings, sysIds, dt)));
            return collection;
        }

        private IEnumerable<int> GetSystemIds()
        {
            var systemIds = new[]
            {
                Constants.System.DefaultContentListViewDataTypeId, 
                Constants.System.DefaultMediaListViewDataTypeId, 
                Constants.System.DefaultMembersListViewDataTypeId
            };
            return systemIds;
        } 

        private TreeNode CreateTreeNode(string id, FormDataCollection queryStrings, IEnumerable<int> systemIds, IDataTypeDefinition dt)
        {
            var node = CreateTreeNode(
                dt.Id.ToInvariantString(),
                id,
                queryStrings,
                dt.Name,
                "icon-autofill",
                false);

            if (systemIds.Contains(dt.Id))
            {
                node.Icon = "icon-thumbnail-list";
            }

            return node;
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

            var sysIds = GetSystemIds();

            if (sysIds.Contains(int.Parse(id)) == false)
            {
                //only have delete for each node
                menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));    
            }
            
            return menu;
        }
    }
}