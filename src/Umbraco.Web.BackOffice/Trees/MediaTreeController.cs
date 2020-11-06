using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Search;
using Umbraco.Core.Security;
using Constants = Umbraco.Core.Constants;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Security;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.BackOffice.Trees
{
    //We will not allow the tree to render unless the user has access to any of the sections that the tree gets rendered
    // this is not ideal but until we change permissions to be tree based (not section) there's not much else we can do here.
    [UmbracoApplicationAuthorize(
        Constants.Applications.Content,
        Constants.Applications.Media,
        Constants.Applications.Settings,
        Constants.Applications.Packages,
        Constants.Applications.Members)]
    [Tree(Constants.Applications.Media, Constants.Trees.Media)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    [SearchableTree("searchResultFormatter", "configureMediaResult", 20)]
    public class MediaTreeController : ContentTreeControllerBase, ISearchableTree, ITreeNodeController
    {
        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly IMediaService _mediaService;
        private readonly IEntityService _entityService;
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;

        public MediaTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IEntityService entityService,
            IBackofficeSecurityAccessor backofficeSecurityAccessor,
            ILogger<MediaTreeController> logger,
            ActionCollection actionCollection,
            IUserService userService,
            IDataTypeService dataTypeService,
            UmbracoTreeSearcher treeSearcher,
            IMediaService mediaService)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, entityService, backofficeSecurityAccessor, logger, actionCollection, userService, dataTypeService)
        {
            _treeSearcher = treeSearcher;
            _mediaService = mediaService;
            _entityService = entityService;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
        }

        protected override int RecycleBinId => Constants.System.RecycleBinMedia;

        protected override bool RecycleBinSmells => _mediaService.RecycleBinSmells();

        private int[] _userStartNodes;
        protected override int[] UserStartNodes
            => _userStartNodes ?? (_userStartNodes = _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.CalculateMediaStartNodeIds(_entityService));

        /// <summary>
        /// Creates a tree node for a content item based on an UmbracoEntity
        /// </summary>
        /// <param name="e"></param>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode GetSingleTreeNode(IEntitySlim entity, string parentId, FormCollection queryStrings)
        {
            var node = CreateTreeNode(
                entity,
                Constants.ObjectTypes.Media,
                parentId,
                queryStrings,
                entity.HasChildren);

            // entity is either a container, or a media
            if (entity.IsContainer)
            {
                node.SetContainerStyle();
                node.AdditionalData.Add("isContainer", true);
            }
            else
            {
                var contentEntity = (IContentEntitySlim) entity;
                node.AdditionalData.Add("contentType", contentEntity.ContentTypeAlias);
            }

            return node;
        }

        protected override MenuItemCollection PerformGetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            //set the default
            menu.DefaultMenuAlias = ActionNew.ActionAlias;

            if (id == Constants.System.RootString)
            {
                // if the user's start node is not the root then the only menu item to display is refresh
                if (UserStartNodes.Contains(Constants.System.Root) == false)
                {
                    menu.Items.Add(new RefreshNode(LocalizedTextService, true));
                    return menu;
                }

                // root actions
                menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);
                menu.Items.Add<ActionSort>(LocalizedTextService, true);
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
                return menu;
            }

            if (int.TryParse(id, out var iid) == false)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var item = _entityService.Get(iid, UmbracoObjectTypes.Media);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //if the user has no path access for this node, all they can do is refresh
            if (!_backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.HasMediaPathAccess(item, _entityService))
            {
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
                return menu;
            }


            //if the media item is in the recycle bin, we don't have a default menu and we need to show a limited menu
            if (item.Path.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Contains(RecycleBinId.ToInvariantString()))
            {
                menu.Items.Add<ActionRestore>(LocalizedTextService, opensDialog: true);
                menu.Items.Add<ActionMove>(LocalizedTextService, opensDialog: true);
                menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true);
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));

                menu.DefaultMenuAlias = null;

            }
            else
            {
                //return a normal node menu:
                menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);
                menu.Items.Add<ActionMove>(LocalizedTextService, opensDialog: true);
                menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true);
                menu.Items.Add<ActionSort>(LocalizedTextService);
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));

                //set the default to create
                menu.DefaultMenuAlias = ActionNew.ActionAlias;
            }

            return menu;
        }

        protected override UmbracoObjectTypes UmbracoObjectType => UmbracoObjectTypes.Media;

        /// <summary>
        /// Returns true or false if the current user has access to the node based on the user's allowed start node (path) access
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override bool HasPathAccess(string id, FormCollection queryStrings)
        {
            var entity = GetEntityFromId(id);

            return HasPathAccess(entity, queryStrings);
        }

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
        {
            return _treeSearcher.ExamineSearch(query, UmbracoEntityTypes.Media, pageSize, pageIndex, out totalFound, searchFrom);
        }

    }
}
