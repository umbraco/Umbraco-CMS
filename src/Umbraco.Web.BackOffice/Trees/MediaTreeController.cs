using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    [Authorize(Policy = AuthorizationPolicies.SectionAccessForMediaTree)]
    [Tree(Constants.Applications.Media, Constants.Trees.Media)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    [SearchableTree("searchResultFormatter", "configureMediaResult", 20)]
    public class MediaTreeController : ContentTreeControllerBase, ISearchableTree, ITreeNodeController
    {
        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly IMediaService _mediaService;
        private readonly AppCaches _appCaches;
        private readonly IEntityService _entityService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

        public MediaTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IEntityService entityService,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            ILogger<MediaTreeController> logger,
            ActionCollection actionCollection,
            IUserService userService,
            IDataTypeService dataTypeService,
            UmbracoTreeSearcher treeSearcher,
            IMediaService mediaService,
            IEventAggregator eventAggregator,
            AppCaches appCaches)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, entityService, backofficeSecurityAccessor, logger, actionCollection, userService, dataTypeService, eventAggregator, appCaches)
        {
            _treeSearcher = treeSearcher;
            _mediaService = mediaService;
            _appCaches = appCaches;
            _entityService = entityService;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
        }

        protected override int RecycleBinId => Constants.System.RecycleBinMedia;

        protected override bool RecycleBinSmells => _mediaService.RecycleBinSmells();

        private int[] _userStartNodes;
        protected override int[] UserStartNodes
            => _userStartNodes ?? (_userStartNodes = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.CalculateMediaStartNodeIds(_entityService, _appCaches));

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

        protected override ActionResult<MenuItemCollection> PerformGetMenuForNode(string id, FormCollection queryStrings)
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
                menu.Items.Add<ActionSort>(LocalizedTextService, true, opensDialog: true);
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
                return menu;
            }

            if (int.TryParse(id, out var iid) == false)
            {
                return NotFound();
            }
            var item = _entityService.Get(iid, UmbracoObjectTypes.Media);
            if (item == null)
            {
                return NotFound();
            }

            //if the user has no path access for this node, all they can do is refresh
            if (!_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.HasMediaPathAccess(item, _entityService, _appCaches))
            {
                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
                return menu;
            }


            //if the media item is in the recycle bin, we don't have a default menu and we need to show a limited menu
            if (item.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).Contains(RecycleBinId.ToInvariantString()))
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
