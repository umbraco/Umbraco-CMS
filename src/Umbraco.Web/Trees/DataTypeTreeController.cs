using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Search;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.DataTypes)]
    [Tree(Constants.Applications.Developer, Constants.Trees.DataTypes, null, sortOrder:1)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class DataTypeTreeController : TreeController, ISearchableTree
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var intId = id.TryConvertTo<int>();
            if (intId == false) throw new InvalidOperationException("Id must be an integer");        
            
            var nodes = new TreeNodeCollection();

            //Folders first
            nodes.AddRange(
               Services.EntityService.GetChildren(intId.Result, UmbracoObjectTypes.DataTypeContainer)
                   .OrderBy(entity => entity.Name)
                   .Select(dt =>
                   {
                       var node = CreateTreeNode(dt, Constants.ObjectTypes.DataTypeGuid, id, queryStrings, "icon-folder", dt.HasChildren());
                       node.Path = dt.Path;
                       node.NodeType = "container";
                        //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
                        node.AdditionalData["jsClickCallback"] = "javascript:void(0);";
                       return node;
                   }));

            //if the request is for folders only then just return
            if (queryStrings["foldersonly"].IsNullOrWhiteSpace() == false && queryStrings["foldersonly"] == "1") return nodes;

            //System ListView nodes
            var systemListViewDataTypeIds = GetNonDeletableSystemListViewDataTypeIds();

            nodes.AddRange(
                Services.EntityService.GetChildren(intId.Result, UmbracoObjectTypes.DataType)
                    .OrderBy(entity => entity.Name)
                    .Select(dt =>
                    {
                        var node = CreateTreeNode(dt.Id.ToInvariantString(), id, queryStrings, dt.Name, "icon-autofill", false);
                        node.Path = dt.Path;
                        if (systemListViewDataTypeIds.Contains(dt.Id))
                        {
                            node.Icon = "icon-thumbnail-list";
                        }
                        return node;
                    }));

            return nodes;
        }

        /// <summary>
        /// Get all integer identifiers for the non-deletable system datatypes.
        /// </summary>
        private static IEnumerable<int> GetNonDeletableSystemDataTypeIds()
        {
            var systemIds = new[]
            {
                Constants.System.DefaultLabelDataTypeId
            };

            return systemIds.Concat(GetNonDeletableSystemListViewDataTypeIds());
        }

        /// <summary>
        /// Get all integer identifiers for the non-deletable system listviews.
        /// </summary>
        private static IEnumerable<int> GetNonDeletableSystemListViewDataTypeIds()
        {
            return new[]
            {
                Constants.System.DefaultContentListViewDataTypeId,
                Constants.System.DefaultMediaListViewDataTypeId,
                Constants.System.DefaultMembersListViewDataTypeId
            };
        }
        
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                // root actions              
                menu.Items.Add<ActionNew>(Services.TextService.Localize($"actions/{ActionNew.Instance.Alias}"));
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize($"actions/{ActionRefresh.Instance.Alias}"), hasSeparator: true);
                return menu;
            }

            var container = Services.EntityService.Get(int.Parse(id), UmbracoObjectTypes.DataTypeContainer);
            if (container != null)
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                menu.Items.Add<ActionNew>(Services.TextService.Localize($"actions/{ActionNew.Instance.Alias}"));

                menu.Items.Add(new MenuItem("rename", Services.TextService.Localize("actions/rename"))
                {
                    Icon = "icon icon-edit"
                });

                if (container.HasChildren() == false)
                    //can delete data type
                    menu.Items.Add<ActionDelete>(Services.TextService.Localize($"actions/{ActionDelete.Instance.Alias}"));

                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize($"actions/{ActionRefresh.Instance.Alias}"), hasSeparator: true);

            }
            else
            {
                var nonDeletableSystemDataTypeIds = GetNonDeletableSystemDataTypeIds();

                if (nonDeletableSystemDataTypeIds.Contains(int.Parse(id)) == false)
                    menu.Items.Add<ActionDelete>(Services.TextService.Localize($"actions/{ActionDelete.Instance.Alias}"));

                menu.Items.Add<ActionMove>(Services.TextService.Localize($"actions/{ActionMove.Instance.Alias}"), hasSeparator: true);
            }
            
            return menu;
        }

        public IEnumerable<SearchResultItem> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
        {
            var results = Services.EntityService.GetPagedDescendantsFromRoot(UmbracoObjectTypes.DataType, pageIndex, pageSize, out totalFound, filter: query);
            return Mapper.Map<IEnumerable<SearchResultItem>>(results);
        }
    }
}
