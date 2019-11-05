using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.ContentEditing;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web.Search;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(ConstantsCore.Trees.DataTypes)]
    [Tree(ConstantsCore.Applications.Settings, ConstantsCore.Trees.DataTypes, SortOrder = 3, TreeGroup = ConstantsCore.Trees.Groups.Settings)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class DataTypeTreeController : TreeController, ISearchableTree
    {
        private readonly UmbracoTreeSearcher _treeSearcher;

        public DataTypeTreeController(UmbracoTreeSearcher treeSearcher, IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper) : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _treeSearcher = treeSearcher;
        }

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
                       var node = CreateTreeNode(dt, ConstantsCore.ObjectTypes.DataType, id, queryStrings, "icon-folder", dt.HasChildren);
                       node.Path = dt.Path;
                       node.NodeType = "container";
                       // TODO: This isn't the best way to ensure a no operation process for clicking a node but it works for now.
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
                        var node = CreateTreeNode(dt.Id.ToInvariantString(), id, queryStrings, dt.Name, ConstantsCore.Icons.DataType, false);
                        node.Path = dt.Path;
                        if (systemListViewDataTypeIds.Contains(dt.Id))
                        {
                            node.Icon = ConstantsCore.Icons.ListView;
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
                ConstantsCore.DataTypes.Boolean, // Used by the Member Type: "Member"
                ConstantsCore.DataTypes.Textarea, // Used by the Member Type: "Member"
                ConstantsCore.DataTypes.LabelBigint, // Used by the Media Type: "Image"; Used by the Media Type: "File"
                ConstantsCore.DataTypes.LabelDateTime, // Used by the Member Type: "Member"
                ConstantsCore.DataTypes.LabelDecimal, // Used by the Member Type: "Member"
                ConstantsCore.DataTypes.LabelInt, // Used by the Media Type: "Image"; Used by the Member Type: "Member"
                ConstantsCore.DataTypes.LabelString, // Used by the Media Type: "Image"; Used by the Media Type: "File"
                ConstantsCore.DataTypes.ImageCropper, // Used by the Media Type: "Image"
                ConstantsCore.DataTypes.Upload, // Used by the Media Type: "File"
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
                ConstantsCore.DataTypes.DefaultContentListView,
                ConstantsCore.DataTypes.DefaultMediaListView,
                ConstantsCore.DataTypes.DefaultMembersListView
            };
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == ConstantsCore.System.RootString)
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                // root actions
                menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);
                menu.Items.Add(new RefreshNode(Services.TextService, true));
                return menu;
            }

            var container = Services.EntityService.Get(int.Parse(id), UmbracoObjectTypes.DataTypeContainer);
            if (container != null)
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);

                menu.Items.Add(new MenuItem("rename", Services.TextService.Localize("actions/rename"))
                {
                    Icon = "icon icon-edit"
                });

                if (container.HasChildren == false)
                {
                    //can delete data type
                    menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true);
                }
                menu.Items.Add(new RefreshNode(Services.TextService, true));
            }
            else
            {
                var nonDeletableSystemDataTypeIds = GetNonDeletableSystemDataTypeIds();

                if (nonDeletableSystemDataTypeIds.Contains(int.Parse(id)) == false)
                    menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true);

                menu.Items.Add<ActionMove>(Services.TextService, hasSeparator: true, opensDialog: true);
            }

            return menu;
        }

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
            => _treeSearcher.EntitySearch(UmbracoObjectTypes.DataType, query, pageSize, pageIndex, out totalFound, searchFrom);
    }
}
