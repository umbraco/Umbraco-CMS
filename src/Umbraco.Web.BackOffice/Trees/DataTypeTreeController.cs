using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Trees;

[Authorize(Policy = AuthorizationPolicies.TreeAccessDataTypes)]
[Tree(Constants.Applications.Settings, Constants.Trees.DataTypes, SortOrder = 3,
    TreeGroup = Constants.Trees.Groups.Settings)]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[CoreTree]
public class DataTypeTreeController : TreeController, ISearchableTree
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IEntityService _entityService;
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
    private readonly UmbracoTreeSearcher _treeSearcher;


    public DataTypeTreeController(ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, UmbracoTreeSearcher treeSearcher,
        IMenuItemCollectionFactory menuItemCollectionFactory, IEntityService entityService,
        IDataTypeService dataTypeService, IEventAggregator eventAggregator) : base(localizedTextService,
        umbracoApiControllerTypeCollection, eventAggregator)
    {
        _treeSearcher = treeSearcher;
        _menuItemCollectionFactory = menuItemCollectionFactory;
        _entityService = entityService;
        _dataTypeService = dataTypeService;
    }

    public async Task<EntitySearchResults> SearchAsync(string query, int pageSize, long pageIndex,
        string? searchFrom = null)
    {
        IEnumerable<SearchResultEntity?> results = _treeSearcher.EntitySearch(UmbracoObjectTypes.DataType, query,
            pageSize, pageIndex, out var totalFound, searchFrom);
        return new EntitySearchResults(results, totalFound);
    }

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        if (!int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
        {
            throw new InvalidOperationException("Id must be an integer");
        }

        var nodes = new TreeNodeCollection();

        //Folders first
        nodes.AddRange(
            _entityService.GetChildren(intId, UmbracoObjectTypes.DataTypeContainer)
                .OrderBy(entity => entity.Name)
                .Select(dt =>
                {
                    TreeNode node = CreateTreeNode(dt, Constants.ObjectTypes.DataType, id, queryStrings,
                        Constants.Icons.Folder, dt.HasChildren);
                    node.Path = dt.Path;
                    node.NodeType = "container";
                    // TODO: This isn't the best way to ensure a no operation process for clicking a node but it works for now.
                    node.AdditionalData["jsClickCallback"] = "javascript:void(0);";
                    return node;
                }));

        //if the request is for folders only then just return
        if (queryStrings["foldersonly"].ToString().IsNullOrWhiteSpace() == false && queryStrings["foldersonly"] == "1")
        {
            return nodes;
        }

        //System ListView nodes
        IEnumerable<int> systemListViewDataTypeIds = GetNonDeletableSystemListViewDataTypeIds();

        IEntitySlim[] children = _entityService.GetChildren(intId, UmbracoObjectTypes.DataType).ToArray();
        var dataTypes = _dataTypeService.GetAll(children.Select(c => c.Id).ToArray()).ToDictionary(dt => dt.Id);

        nodes.AddRange(
            children
                .OrderBy(entity => entity.Name)
                .Select(dt =>
                {
                    IDataType dataType = dataTypes[dt.Id];
                    TreeNode node = CreateTreeNode(dt.Id.ToInvariantString(), id, queryStrings, dt.Name,
                        dataType.Editor?.Icon, false);
                    node.Path = dt.Path;
                    return node;
                })
        );

        return nodes;
    }

    /// <summary>
    ///     Get all integer identifiers for the non-deletable system datatypes.
    /// </summary>
    private static IEnumerable<int> GetNonDeletableSystemDataTypeIds()
    {
        var systemIds = new[]
        {
            Constants.DataTypes.Boolean, // Used by the Member Type: "Member"
            Constants.DataTypes.Textarea, // Used by the Member Type: "Member"
            Constants.DataTypes.LabelBigint, // Used by the Media Type: "Image"; Used by the Media Type: "File"
            Constants.DataTypes.LabelDateTime, // Used by the Member Type: "Member"
            Constants.DataTypes.LabelDecimal, // Used by the Member Type: "Member"
            Constants.DataTypes.LabelInt, // Used by the Media Type: "Image"; Used by the Member Type: "Member"
            Constants.DataTypes.LabelString, // Used by the Media Type: "Image"; Used by the Media Type: "File"
            Constants.DataTypes.ImageCropper, // Used by the Media Type: "Image"
            Constants.DataTypes.Upload // Used by the Media Type: "File"
        };

        return systemIds.Concat(GetNonDeletableSystemListViewDataTypeIds());
    }

    /// <summary>
    ///     Get all integer identifiers for the non-deletable system listviews.
    /// </summary>
    private static IEnumerable<int> GetNonDeletableSystemListViewDataTypeIds() =>
        new[]
        {
            Constants.DataTypes.DefaultContentListView, Constants.DataTypes.DefaultMediaListView,
            Constants.DataTypes.DefaultMembersListView
        };

    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
    {
        MenuItemCollection menu = _menuItemCollectionFactory.Create();

        if (id == Constants.System.RootString)
        {
            //set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;

            // root actions
            menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));
            return menu;
        }

        IEntitySlim? container = _entityService.Get(int.Parse(id, CultureInfo.InvariantCulture),
            UmbracoObjectTypes.DataTypeContainer);
        if (container != null)
        {
                //set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;

            menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);

            menu.Items.Add(new MenuItem("rename", LocalizedTextService.Localize("actions", "rename"))
            {
                Icon = "icon-edit",
                UseLegacyIcon = false,
            });

            if (container.HasChildren == false)
            {
                //can delete data type
                menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
            }

            menu.Items.Add(new RefreshNode(LocalizedTextService, true));
        }
        else
        {
            IEnumerable<int> nonDeletableSystemDataTypeIds = GetNonDeletableSystemDataTypeIds();

            if (nonDeletableSystemDataTypeIds.Contains(int.Parse(id, CultureInfo.InvariantCulture)) == false)
            {
                menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
            }

                menu.Items.Add<ActionMove>(LocalizedTextService, hasSeparator: true, opensDialog: true, useLegacyIcon: false);
                menu.Items.Add<ActionCopy>(LocalizedTextService, hasSeparator: true, opensDialog: true, useLegacyIcon: false);
            }

        return menu;
    }
}
