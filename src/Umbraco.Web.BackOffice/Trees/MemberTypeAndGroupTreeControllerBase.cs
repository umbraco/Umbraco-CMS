using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Trees;

[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[CoreTree]
public abstract class MemberTypeAndGroupTreeControllerBase : TreeController
{
    private readonly IMemberTypeService _memberTypeService;

    protected MemberTypeAndGroupTreeControllerBase(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IEventAggregator eventAggregator,
        IMemberTypeService memberTypeService)
        : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
    {
        MenuItemCollectionFactory = menuItemCollectionFactory;

        _memberTypeService = memberTypeService;
    }

    [Obsolete("Use ctor injecting IMemberTypeService")]
    protected MemberTypeAndGroupTreeControllerBase(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IEventAggregator eventAggregator)
        : this(
            localizedTextService,
            umbracoApiControllerTypeCollection,
            menuItemCollectionFactory,
            eventAggregator,
            StaticServiceProvider.Instance.GetRequiredService<IMemberTypeService>())
    {
    }

    public IMenuItemCollectionFactory MenuItemCollectionFactory { get; }

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        var nodes = new TreeNodeCollection();

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
            // root actions
            menu.Items.Add(new CreateChildEntity(LocalizedTextService));
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));
            return menu;
        }

        IMemberType? memberType = _memberTypeService.Get(int.Parse(id));
        if (memberType != null)
        {
            menu.Items.Add<ActionCopy>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
        }

        // delete member type/group
        menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);

        return menu;
    }

    protected abstract IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormCollection queryStrings);
}
