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

[Authorize(Policy = AuthorizationPolicies.TreeAccessTemplates)]
[Tree(Constants.Applications.Settings, Constants.Trees.Templates, SortOrder = 6,
    TreeGroup = Constants.Trees.Groups.Templating)]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[CoreTree]
public class TemplatesTreeController : TreeController, ISearchableTree
{
    private readonly IFileService _fileService;
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
    private readonly UmbracoTreeSearcher _treeSearcher;

    public TemplatesTreeController(
        UmbracoTreeSearcher treeSearcher,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IFileService fileService,
        IEventAggregator eventAggregator
    ) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
    {
        _treeSearcher = treeSearcher;
        _menuItemCollectionFactory = menuItemCollectionFactory;
        _fileService = fileService;
    }

    public async Task<EntitySearchResults> SearchAsync(string query, int pageSize, long pageIndex,
        string? searchFrom = null)
    {
        IEnumerable<SearchResultEntity?> results = _treeSearcher.EntitySearch(UmbracoObjectTypes.Template, query,
            pageSize, pageIndex, out var totalFound, searchFrom);
        return new EntitySearchResults(results, totalFound);
    }

    protected override ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
    {
        ActionResult<TreeNode?> rootResult = base.CreateRootNode(queryStrings);
        if (!(rootResult.Result is null))
        {
            return rootResult;
        }

        TreeNode? root = rootResult.Value;

        if (root is not null)
        {
            //check if there are any templates
            root.HasChildren = _fileService.GetTemplates(-1)?.Any() ?? false;
        }

        return root;
    }

    /// <summary>
    ///     The method called to render the contents of the tree structure
    /// </summary>
    /// <param name="id"></param>
    /// <param name="queryStrings">
    ///     All of the query string parameters passed from jsTree
    /// </param>
    /// <remarks>
    ///     We are allowing an arbitrary number of query strings to be pased in so that developers are able to persist custom
    ///     data from the front-end
    ///     to the back end to be used in the query for model data.
    /// </remarks>
    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        var nodes = new TreeNodeCollection();

        IEnumerable<ITemplate>? found = id == Constants.System.RootString
            ? _fileService.GetTemplates(-1)
            : _fileService.GetTemplates(int.Parse(id, CultureInfo.InvariantCulture));

        if (found is not null)
        {
            nodes.AddRange(found.Select(template => CreateTreeNode(
                template.Id.ToString(CultureInfo.InvariantCulture),
                // TODO: Fix parent ID stuff for templates
                "-1",
                queryStrings,
                template.Name,
                template.IsMasterTemplate ? "icon-newspaper" : "icon-newspaper-alt",
                template.IsMasterTemplate,
                null,
                Udi.Create(ObjectTypes.GetUdiType(Constants.ObjectTypes.TemplateType), template.Key)
            )));
        }

        return nodes;
    }

    /// <summary>
    ///     Returns the menu structure for the node
    /// </summary>
    /// <param name="id"></param>
    /// <param name="queryStrings"></param>
    /// <returns></returns>
    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
    {
        MenuItemCollection menu = _menuItemCollectionFactory.Create();

        //Create the normal create action
        MenuItem? item = menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
        item?.NavigateToRoute(
            $"{queryStrings.GetRequiredValue<string>("application")}/templates/edit/{id}?create=true");

        if (id == Constants.System.RootString)
        {
                //refresh action
            menu.Items.Add(new RefreshNode(LocalizedTextService, separatorBefore: true));

            return menu;
        }

        ITemplate? template = _fileService.GetTemplate(int.Parse(id, CultureInfo.InvariantCulture));
        if (template == null)
        {
            return menu;
        }

        EntitySlim entity = FromTemplate(template);

            //don't allow delete if it has child layouts
        if (template.IsMasterTemplate == false)
        {
            //add delete option if it doesn't have children
            menu.Items.Add<ActionDelete>(LocalizedTextService, hasSeparator: true, opensDialog: true, useLegacyIcon: false);
        }

            //add refresh
        menu.Items.Add(new RefreshNode(LocalizedTextService, separatorBefore: true));

        return menu;
    }

    private EntitySlim FromTemplate(ITemplate template) =>
        new()
        {
            CreateDate = template.CreateDate,
            Id = template.Id,
            Key = template.Key,
            Name = template.Name,
            NodeObjectType = Constants.ObjectTypes.Template,
            // TODO: Fix parent/paths on templates
            ParentId = -1,
            Path = template.Path,
            UpdateDate = template.UpdateDate
        };
}
