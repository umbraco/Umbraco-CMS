using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Trees;

[Authorize(Policy = AuthorizationPolicies.SectionAccessForContentTree)]
[Tree(Constants.Applications.Content, Constants.Trees.Content)]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[CoreTree]
[SearchableTree("searchResultFormatter", "configureContentResult", 10)]
public class ContentTreeController : ContentTreeControllerBase, ISearchableTreeWithCulture
{
    private readonly ActionCollection _actions;
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IContentService _contentService;
    private readonly IEmailSender _emailSender;
    private readonly IEntityService _entityService;
    private readonly ILocalizationService _localizationService;
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
    private readonly IPublicAccessService _publicAccessService;
    private readonly UmbracoTreeSearcher _treeSearcher;
    private readonly IUserService _userService;

    private int[]? _userStartNodes;

    public ContentTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IEntityService entityService,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        ILogger<ContentTreeController> logger,
        ActionCollection actionCollection,
        IUserService userService,
        IDataTypeService dataTypeService,
        UmbracoTreeSearcher treeSearcher,
        ActionCollection actions,
        IContentService contentService,
        IPublicAccessService publicAccessService,
        ILocalizationService localizationService,
        IEventAggregator eventAggregator,
        IEmailSender emailSender,
        AppCaches appCaches)
        : base(
            localizedTextService,
            umbracoApiControllerTypeCollection,
            menuItemCollectionFactory,
            entityService,
            backofficeSecurityAccessor,
            logger,
            actionCollection,
            userService,
            dataTypeService,
            eventAggregator,
            appCaches)
    {
        _treeSearcher = treeSearcher;
        _actions = actions;
        _menuItemCollectionFactory = menuItemCollectionFactory;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _contentService = contentService;
        _entityService = entityService;
        _publicAccessService = publicAccessService;
        _userService = userService;
        _localizationService = localizationService;
        _emailSender = emailSender;
        _appCaches = appCaches;
    }

    protected override int RecycleBinId => Constants.System.RecycleBinContent;

    protected override bool RecycleBinSmells => _contentService.RecycleBinSmells();

    protected override int[] UserStartNodes
        => _userStartNodes ??=
            _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.CalculateContentStartNodeIds(_entityService, _appCaches) ?? Array.Empty<int>();

    protected override UmbracoObjectTypes UmbracoObjectType => UmbracoObjectTypes.Document;

    public async Task<EntitySearchResults> SearchAsync(string query, int pageSize, long pageIndex, string? searchFrom = null)
    {
        IEnumerable<SearchResultEntity> results = _treeSearcher.ExamineSearch(query, UmbracoEntityTypes.Document, pageSize, pageIndex, out var totalFound, searchFrom);
        return new EntitySearchResults(results, totalFound);
    }

    /// <inheritdoc />
    protected override TreeNode? GetSingleTreeNode(IEntitySlim entity, string parentId, FormCollection? queryStrings)
    {
        var culture = queryStrings?["culture"].ToString();

        IEnumerable<MenuItem> allowedUserOptions = GetAllowedUserMenuItemsForNode(entity);
        if (CanUserAccessNode(entity, allowedUserOptions, culture))
        {
            //Special check to see if it is a container, if so then we'll hide children.
            var isContainer = entity.IsContainer; // && (queryStrings.Get("isDialog") != "true");

            TreeNode node = CreateTreeNode(
                entity,
                Constants.ObjectTypes.Document,
                parentId,
                queryStrings,
                entity.HasChildren);

            // set container style if it is one
            if (isContainer)
            {
                node.AdditionalData.Add("isContainer", true);
                node.SetContainerStyle();
            }

            var documentEntity = (IDocumentEntitySlim)entity;

            if (!documentEntity.Variations.VariesByCulture())
            {
                if (!documentEntity.Published)
                {
                    node.SetNotPublishedStyle();
                }
                else if (documentEntity.Edited)
                {
                    node.SetHasPendingVersionStyle();
                }
            }
            else
            {
                if (!culture.IsNullOrWhiteSpace())
                {
                    if (!documentEntity.Published || !documentEntity.PublishedCultures.Contains(culture))
                    {
                        node.SetNotPublishedStyle();
                    }
                    else if (documentEntity.EditedCultures.Contains(culture))
                    {
                        node.SetHasPendingVersionStyle();
                    }
                }
            }

            node.AdditionalData.Add("variesByCulture", documentEntity.Variations.VariesByCulture());
            node.AdditionalData.Add("contentType", documentEntity.ContentTypeAlias);

            if (_publicAccessService.IsProtected(entity.Path).Success)
            {
                node.SetProtectedStyle();
            }

            return node;
        }

        return null;
    }

    protected override ActionResult<MenuItemCollection> PerformGetMenuForNode(string id, FormCollection queryStrings)
    {
        if (id == Constants.System.RootString)
        {
            MenuItemCollection menu = _menuItemCollectionFactory.Create();

            // if the user's start node is not the root then the only menu item to display is refresh
            if (UserStartNodes.Contains(Constants.System.Root) == false)
            {
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
                return menu;
            }

            //set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;

            // we need to get the default permissions as you can't set permissions on the very root node
            EntityPermission permission = _userService
                .GetPermissions(_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser, Constants.System.Root)
                .First();
            IEnumerable<MenuItem> nodeActions = _actions.FromEntityPermission(permission)
                .Select(x => new MenuItem(x));

            //these two are the standard items
            menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
            menu.Items.Add<ActionSort>(LocalizedTextService, hasSeparator: true, opensDialog: true, useLegacyIcon: false);

            //filter the standard items
            FilterUserAllowedMenuItems(menu, nodeActions);

            if (menu.Items.Any())
            {
                menu.Items.Last().SeparatorBefore = true;
            }

            // add default actions for *all* users
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));

            return menu;
        }


        //return a normal node menu:
        if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out int iid) == false)
        {
            return NotFound();
        }

        IEntitySlim? item = _entityService.Get(iid, UmbracoObjectTypes.Document);
        if (item == null)
        {
            return NotFound();
        }

        //if the user has no path access for this node, all they can do is refresh
        if (!_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.HasContentPathAccess(item, _entityService, _appCaches) ?? false)
        {
            MenuItemCollection menu = _menuItemCollectionFactory.Create();
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));
            return menu;
        }

        MenuItemCollection nodeMenu = GetAllNodeMenuItems(item);

        //if the content node is in the recycle bin, don't have a default menu, just show the regular menu
        if (item.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Contains(RecycleBinId.ToInvariantString()))
        {
            nodeMenu.DefaultMenuAlias = null;
            nodeMenu = GetNodeMenuItemsForDeletedContent(item);
        }
        else
        {
            //set the default to create
            nodeMenu.DefaultMenuAlias = ActionNew.ActionAlias;
        }

        IEnumerable<MenuItem> allowedMenuItems = GetAllowedUserMenuItemsForNode(item);
        FilterUserAllowedMenuItems(nodeMenu, allowedMenuItems);

        return nodeMenu;
    }

    /// <summary>
    ///     Returns true or false if the current user has access to the node based on the user's allowed start node (path)
    ///     access
    /// </summary>
    /// <param name="id"></param>
    /// <param name="queryStrings"></param>
    /// <returns></returns>
    protected override bool HasPathAccess(string id, FormCollection queryStrings)
    {
        IEntitySlim? entity = GetEntityFromId(id);
        return HasPathAccess(entity, queryStrings);
    }

    protected override ActionResult<IEnumerable<IEntitySlim>> GetChildEntities(string id, FormCollection queryStrings)
    {
        ActionResult<IEnumerable<IEntitySlim>> result = base.GetChildEntities(id, queryStrings);

        if (!(result.Result is null))
        {
            return result.Result;
        }

        Attempt<string> culture = queryStrings["culture"].TryConvertTo<string>();

        //if this is null we'll set it to the default.
        var cultureVal =
            (culture.Success ? culture.Result : null).IfNullOrWhiteSpace(
                _localizationService.GetDefaultLanguageIsoCode());

        // set names according to variations
        foreach (IEntitySlim entity in result.Value!)
        {
            EnsureName(entity, cultureVal);
        }

        return result;
    }

    /// <summary>
    ///     Returns a collection of all menu items that can be on a content node
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected MenuItemCollection GetAllNodeMenuItems(IUmbracoEntity item)
    {
        MenuItemCollection menu = _menuItemCollectionFactory.Create();
        AddActionNode<ActionNew>(item, menu, opensDialog: true, useLegacyIcon: false);
        AddActionNode<ActionDelete>(item, menu, opensDialog: true, useLegacyIcon: false);
        AddActionNode<ActionCreateBlueprintFromContent>(item, menu, opensDialog: true, useLegacyIcon: false);
        AddActionNode<ActionMove>(item, menu, hasSeparator: true, opensDialog: true, useLegacyIcon: false);
        AddActionNode<ActionCopy>(item, menu, opensDialog: true, useLegacyIcon: false);
        AddActionNode<ActionSort>(item, menu, hasSeparator: true, opensDialog: true, useLegacyIcon: false);
        AddActionNode<ActionAssignDomain>(item, menu, opensDialog: true, useLegacyIcon: false);
        AddActionNode<ActionRights>(item, menu, opensDialog: true, useLegacyIcon: false);
        AddActionNode<ActionProtect>(item, menu, hasSeparator: true, opensDialog: true, useLegacyIcon: false);

        if (_emailSender.CanSendRequiredEmail())
        {
            menu.Items.Add(new MenuItem("notify", LocalizedTextService)
            {
                Icon = "icon-megaphone",
                SeparatorBefore = true,
                OpensDialog = true,
                UseLegacyIcon = false
            });
        }

        if ((item is DocumentEntitySlim documentEntity && documentEntity.IsContainer) == false)
        {
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));
        }

        return menu;
    }

    /// <summary>
    ///     Returns a collection of all menu items that can be on a deleted (in recycle bin) content node
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected MenuItemCollection GetNodeMenuItemsForDeletedContent(IUmbracoEntity item)
    {
        MenuItemCollection menu = _menuItemCollectionFactory.Create();
        menu.Items.Add<ActionRestore>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
        menu.Items.Add<ActionMove>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
        menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);

        menu.Items.Add(new RefreshNode(LocalizedTextService, true));

        return menu;
    }

    /// <summary>
    ///     set name according to variations
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="culture"></param>
    private void EnsureName(IEntitySlim entity, string? culture)
    {
        if (culture == null)
        {
            if (string.IsNullOrWhiteSpace(entity.Name))
            {
                entity.Name = "[[" + entity.Id + "]]";
            }

            return;
        }

        if (!(entity is IDocumentEntitySlim docEntity))
        {
            throw new InvalidOperationException(
                $"Cannot render a tree node for a culture when the entity isn't {typeof(IDocumentEntitySlim)}, instead it is {entity.GetType()}");
        }

        // we are getting the tree for a given culture,
        // for those items that DO support cultures, we need to get the proper name, IF it exists
        // otherwise, invariant is fine (with brackets)

        if (docEntity.Variations.VariesByCulture())
        {
            if (docEntity.CultureNames.TryGetValue(culture, out var name) &&
                !string.IsNullOrWhiteSpace(name))
            {
                entity.Name = name;
            }
            else
            {
                entity.Name = "(" + entity.Name + ")";
            }
        }

        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            entity.Name = "[[" + entity.Id + "]]";
        }
    }

    private void AddActionNode<TAction>(IUmbracoEntity item, MenuItemCollection menu, bool hasSeparator = false, bool opensDialog = false, bool useLegacyIcon = true)
        where TAction : IAction
    {
        MenuItem? menuItem = menu.Items.Add<TAction>(LocalizedTextService, hasSeparator, opensDialog, useLegacyIcon);
    }

    public async Task<EntitySearchResults> SearchAsync(string query, int pageSize, long pageIndex, string? searchFrom = null, string? culture = null)
    {
        var results = _treeSearcher.ExamineSearch(query, UmbracoEntityTypes.Document, pageSize, pageIndex, out long totalFound, culture: culture, searchFrom: searchFrom);
        return new EntitySearchResults(results, totalFound);
    }
}
