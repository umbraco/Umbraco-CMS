using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Search;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Web.Trees
{
    //We will not allow the tree to render unless the user has access to any of the sections that the tree gets rendered
    // this is not ideal but until we change permissions to be tree based (not section) there's not much else we can do here.
    [UmbracoApplicationAuthorize(
        Constants.Applications.Content,
        Constants.Applications.Media,
        Constants.Applications.Users,
        Constants.Applications.Settings,
        Constants.Applications.Packages,
        Constants.Applications.Members)]
    [Tree(Constants.Applications.Content, Constants.Trees.Content)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    [SearchableTree("searchResultFormatter", "configureContentResult", 10)]
    public class ContentTreeController : ContentTreeControllerBase, ISearchableTree
    {
        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly ActionCollection _actions;

        protected override int RecycleBinId => Constants.System.RecycleBinContent;

        protected override bool RecycleBinSmells => Services.ContentService.RecycleBinSmells();

        private int[] _userStartNodes;

        protected override int[] UserStartNodes
            => _userStartNodes ?? (_userStartNodes = Security.CurrentUser.CalculateContentStartNodeIds(Services.EntityService, AppCaches));

        public ContentTreeController(UmbracoTreeSearcher treeSearcher, ActionCollection actions, IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper) : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _treeSearcher = treeSearcher;
            _actions = actions;
        }

        /// <inheritdoc />
        protected override TreeNode GetSingleTreeNode(IEntitySlim entity, string parentId, FormDataCollection queryStrings)
        {
            var culture = queryStrings?["culture"];

            var allowedUserOptions = GetAllowedUserMenuItemsForNode(entity);
            if (CanUserAccessNode(entity, allowedUserOptions, culture))
            {
                //Special check to see if it is a container, if so then we'll hide children.
                var isContainer = entity.IsContainer;   // && (queryStrings.Get("isDialog") != "true");

                var node = CreateTreeNode(
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
                        node.SetNotPublishedStyle();
                    else if (documentEntity.Edited)
                        node.SetHasPendingVersionStyle();
                }
                else
                {
                    if (!culture.IsNullOrWhiteSpace())
                    {
                        if (!documentEntity.Published || !documentEntity.PublishedCultures.Contains(culture))
                            node.SetNotPublishedStyle();
                        else if (documentEntity.EditedCultures.Contains(culture))
                            node.SetHasPendingVersionStyle();
                    }
                }

                node.AdditionalData.Add("variesByCulture", documentEntity.Variations.VariesByCulture());
                node.AdditionalData.Add("contentType", documentEntity.ContentTypeAlias);

                if (Services.PublicAccessService.IsProtected(entity.Path))
                    node.SetProtectedStyle();

                return node;
            }

            return null;
        }

        protected override MenuItemCollection PerformGetMenuForNode(string id, FormDataCollection queryStrings)
        {
            if (id == Constants.System.RootString)
            {
                var menu = new MenuItemCollection();

                // if the user's start node is not the root then the only menu item to display is refresh
                if (UserStartNodes.Contains(Constants.System.Root) == false)
                {
                    menu.Items.Add(new RefreshNode(Services.TextService, true));
                    return menu;
                }

                //set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                // we need to get the default permissions as you can't set permissions on the very root node
                var assignedPermissions = Services.UserService.GetAssignedPermissions(Security.CurrentUser, Constants.System.Root);
                var nodeActions = _actions.GetByLetters(assignedPermissions)
                    .Select(x => new MenuItem(x));

                //these two are the standard items
                menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);
                menu.Items.Add<ActionSort>(Services.TextService, true, opensDialog: true);

                //filter the standard items
                FilterUserAllowedMenuItems(menu, nodeActions);

                if (menu.Items.Any())
                {
                    menu.Items.Last().SeparatorBefore = true;
                }

                // add default actions for *all* users
                menu.Items.Add(new RefreshNode(Services.TextService, true));

                return menu;
            }


            //return a normal node menu:
            int iid;
            if (int.TryParse(id, out iid) == false)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var item = Services.EntityService.Get(iid, UmbracoObjectTypes.Document);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //if the user has no path access for this node, all they can do is refresh
            if (!Security.CurrentUser.HasContentPathAccess(item, Services.EntityService, AppCaches))
            {
                var menu = new MenuItemCollection();
                menu.Items.Add(new RefreshNode(Services.TextService, true));
                return menu;
            }

            var nodeMenu = GetAllNodeMenuItems(item);            
            
            //if the content node is in the recycle bin, don't have a default menu, just show the regular menu
            if (item.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).Contains(RecycleBinId.ToInvariantString()))
            {
                nodeMenu.DefaultMenuAlias = null;
                nodeMenu = GetNodeMenuItemsForDeletedContent(item);
            }
            else
            {
                //set the default to create
                nodeMenu.DefaultMenuAlias = ActionNew.ActionAlias;
            }

            var allowedMenuItems = GetAllowedUserMenuItemsForNode(item);
            FilterUserAllowedMenuItems(nodeMenu, allowedMenuItems);

            return nodeMenu;
        }

        protected override UmbracoObjectTypes UmbracoObjectType => UmbracoObjectTypes.Document;

        /// <summary>
        /// Returns true or false if the current user has access to the node based on the user's allowed start node (path) access
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override bool HasPathAccess(string id, FormDataCollection queryStrings)
        {
            var entity = GetEntityFromId(id);
            return HasPathAccess(entity, queryStrings);
        }

        protected override IEnumerable<IEntitySlim> GetChildEntities(string id, FormDataCollection queryStrings)
        {
            var result = base.GetChildEntities(id, queryStrings).ToArray();
            var culture = queryStrings["culture"].TryConvertTo<string>();

            //if this is null we'll set it to the default.
            var cultureVal = (culture.Success ? culture.Result : null).IfNullOrWhiteSpace(Services.LocalizationService.GetDefaultLanguageIsoCode());

            // set names according to variations
            foreach (var entity in result)
            {
                EnsureName(entity, cultureVal);
            }

            return result;
        }
        /// <summary>
        /// Returns a collection of all menu items that can be on a content node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected MenuItemCollection GetAllNodeMenuItems(IUmbracoEntity item)
        {
            var menu = new MenuItemCollection();
            AddActionNode<ActionNew>(item, menu, opensDialog: true);
            AddActionNode<ActionDelete>(item, menu, opensDialog: true);
            AddActionNode<ActionCreateBlueprintFromContent>(item, menu, opensDialog: true);
            AddActionNode<ActionMove>(item, menu, true, opensDialog: true);
            AddActionNode<ActionCopy>(item, menu, opensDialog: true);
            AddActionNode<ActionSort>(item, menu, true, opensDialog: true);
            AddActionNode<ActionAssignDomain>(item, menu, opensDialog: true);
            AddActionNode<ActionRights>(item, menu, opensDialog: true);
            AddActionNode<ActionProtect>(item, menu, true, opensDialog: true);

            if (EmailSender.CanSendRequiredEmail)
            {
	            menu.Items.Add(new MenuItem("notify", Services.TextService)
	            {
	                Icon = "megaphone",
	                SeparatorBefore = true,
	                OpensDialog = true
	            });
            }

            if((item is DocumentEntitySlim documentEntity && documentEntity.IsContainer) == false)
            {
                menu.Items.Add(new RefreshNode(Services.TextService, true));
            }

            return menu;
        }

        /// <summary>
        /// Returns a collection of all menu items that can be on a deleted (in recycle bin) content node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected MenuItemCollection GetNodeMenuItemsForDeletedContent(IUmbracoEntity item)
        {
            var menu = new MenuItemCollection();
            menu.Items.Add<ActionRestore>(Services.TextService, opensDialog: true);
            menu.Items.Add<ActionMove>(Services.TextService, opensDialog: true);
            menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true);

            menu.Items.Add(new RefreshNode(Services.TextService, true));

            return menu;
        }
        
        /// <summary>
        /// set name according to variations
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="culture"></param>
        private void EnsureName(IEntitySlim entity, string culture)
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
                throw new InvalidOperationException($"Cannot render a tree node for a culture when the entity isn't {typeof(IDocumentEntitySlim)}, instead it is {entity.GetType()}");
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

        private void AddActionNode<TAction>(IUmbracoEntity item, MenuItemCollection menu, bool hasSeparator = false, bool opensDialog = false)
            where TAction : IAction
        {
            var menuItem = menu.Items.Add<TAction>(Services.TextService, hasSeparator, opensDialog);
        }

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
        {
            return _treeSearcher.ExamineSearch(query, UmbracoEntityTypes.Document, pageSize, pageIndex, out totalFound, searchFrom);
        }
    }
}
