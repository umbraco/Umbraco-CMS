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

[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
[Tree(Constants.Applications.Settings, Constants.Trees.MediaTypes, SortOrder = 1,
    TreeGroup = Constants.Trees.Groups.Settings)]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[CoreTree]
public class MediaTypeTreeController : TreeController, ISearchableTree
{
    private readonly IEntityService _entityService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
    private readonly UmbracoTreeSearcher _treeSearcher;

    public MediaTypeTreeController(ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, UmbracoTreeSearcher treeSearcher,
        IMenuItemCollectionFactory menuItemCollectionFactory, IMediaTypeService mediaTypeService,
        IEntityService entityService, IEventAggregator eventAggregator) : base(localizedTextService,
        umbracoApiControllerTypeCollection, eventAggregator)
    {
        _treeSearcher = treeSearcher;
        _menuItemCollectionFactory = menuItemCollectionFactory;
        _mediaTypeService = mediaTypeService;
        _entityService = entityService;
    }

    public async Task<EntitySearchResults> SearchAsync(string query, int pageSize, long pageIndex,
        string? searchFrom = null)
    {
        IEnumerable<SearchResultEntity?> results = _treeSearcher.EntitySearch(UmbracoObjectTypes.MediaType, query,
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

        nodes.AddRange(
            _entityService.GetChildren(intId, UmbracoObjectTypes.MediaTypeContainer)
                .OrderBy(entity => entity.Name)
                .Select(dt =>
                {
                    TreeNode node = CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, Constants.Icons.Folder,
                        dt.HasChildren, "");
                    node.Path = dt.Path;
                    node.NodeType = "container";
                    // TODO: This isn't the best way to ensure a no operation process for clicking a node but it works for now.
                    node.AdditionalData["jsClickCallback"] = "javascript:void(0);";
                    return node;
                }));

        // if the request is for folders only then just return
        if (queryStrings["foldersonly"].ToString().IsNullOrWhiteSpace() == false &&
            queryStrings["foldersonly"].ToString() == "1")
        {
            return nodes;
        }

        IEnumerable<IMediaType> mediaTypes = _mediaTypeService.GetAll();

        nodes.AddRange(
            _entityService.GetChildren(intId, UmbracoObjectTypes.MediaType)
                .OrderBy(entity => entity.Name)
                .Select(dt =>
                {
                    // since 7.4+ child type creation is enabled by a config option. It defaults to on, but can be disabled if we decide to.
                    // need this check to keep supporting sites where children have already been created.
                    var hasChildren = dt.HasChildren;
                    IMediaType? mt = mediaTypes.FirstOrDefault(x => x.Id == dt.Id);
                    TreeNode node = CreateTreeNode(dt, Constants.ObjectTypes.MediaType, id, queryStrings,
                        mt?.Icon ?? Constants.Icons.MediaType, hasChildren);

                    node.Path = dt.Path;
                    return node;
                }));

        return nodes;
    }

    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
    {
        MenuItemCollection menu = _menuItemCollectionFactory.Create();

        if (id == Constants.System.RootString)
        {
            // set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;

            // root actions
            menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
            menu.Items.Add(new RefreshNode(LocalizedTextService));
            return menu;
        }

        IEntitySlim? container = _entityService.Get(int.Parse(id, CultureInfo.InvariantCulture),
            UmbracoObjectTypes.MediaTypeContainer);
        if (container != null)
        {
            // set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;

            menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);

            menu.Items.Add(new MenuItem("rename", LocalizedTextService.Localize("actions", "rename"))
            {
                Icon = "icon-edit",
                UseLegacyIcon = false,
            });

            if (container.HasChildren == false)
            {
                // can delete doc type
                    menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
            }

            menu.Items.Add(new RefreshNode(LocalizedTextService, separatorBefore: true));
        }
        else
        {
            IMediaType? ct = _mediaTypeService.Get(int.Parse(id, CultureInfo.InvariantCulture));
            IMediaType? parent = ct == null ? null : _mediaTypeService.Get(ct.ParentId);

            menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);

            // no move action if this is a child doc type
            if (parent == null)
            {
                menu.Items.Add<ActionMove>(LocalizedTextService, hasSeparator: true, opensDialog: true, useLegacyIcon: false);
            }

            menu.Items.Add<ActionCopy>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
            if (ct?.IsSystemMediaType() == false)
            {
                menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
            }

            menu.Items.Add(new RefreshNode(LocalizedTextService, separatorBefore: true));
        }

        return menu;
    }
}
