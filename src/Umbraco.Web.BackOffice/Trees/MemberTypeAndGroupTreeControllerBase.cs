using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Trees;

[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[CoreTree]
public abstract class MemberTypeAndGroupTreeControllerBase : TreeController
{
    private readonly IEntityService _entityService;
    private readonly IMemberTypeService _memberTypeService;

    [Obsolete("Use the constructor with IEntityService instead")]
    protected MemberTypeAndGroupTreeControllerBase(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IEventAggregator eventAggregator,
        IMemberTypeService memberTypeService)
        : this(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, eventAggregator, memberTypeService, StaticServiceProvider.Instance.GetRequiredService<IEntityService>())
    {
    }

    protected MemberTypeAndGroupTreeControllerBase(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IEventAggregator eventAggregator,
        IMemberTypeService memberTypeService,
        IEntityService entityService)
        : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
    {
        MenuItemCollectionFactory = menuItemCollectionFactory;

        _memberTypeService = memberTypeService;
        _entityService = entityService;
    }

    public IMenuItemCollectionFactory MenuItemCollectionFactory { get; }

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        if (!int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
        {
            throw new InvalidOperationException("Id must be an integer");
        }

        var nodes = new TreeNodeCollection();

        nodes.AddRange(
            _entityService.GetChildren(intId, UmbracoObjectTypes.MemberTypeContainer)
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

        nodes.AddRange(GetTreeNodesFromService(id, queryStrings));
        return nodes;
    }

    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
    {
        MenuItemCollection menu = MenuItemCollectionFactory.Create();

        if (id == Constants.System.RootString)
        {
            // set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;

            // root actions

            if (queryStrings["tree"].ToString() == Constants.Trees.MemberGroups)
            {
                menu.Items.Add(new CreateChildEntity(LocalizedTextService));
            }
            else
            {
                menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
            }

            menu.Items.Add(new RefreshNode(LocalizedTextService, true));

            return menu;
        }

        IMemberType? memberType = _memberTypeService.Get(int.Parse(id));
        if (memberType != null)
        {
            IEntitySlim? container = _entityService.Get(int.Parse(id, CultureInfo.InvariantCulture),
            UmbracoObjectTypes.MemberTypeContainer);

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
                    // can delete member type
                    menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
                }

                menu.Items.Add(new RefreshNode(LocalizedTextService, separatorBefore: true));
            }
            else
            {
                menu.Items.Add<ActionCopy>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
            }
        }

        // delete member type/group
        menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);

        return menu;
    }

    protected abstract IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormCollection queryStrings);
}
