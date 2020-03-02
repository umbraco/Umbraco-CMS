﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Core.Security;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;
using Constants = Umbraco.Core.Constants;
using Umbraco.Web.Security;

namespace Umbraco.Web.Trees
{
    //We will not allow the tree to render unless the user has access to any of the sections that the tree gets rendered
    // this is not ideal but until we change permissions to be tree based (not section) there's not much else we can do here.
    [UmbracoApplicationAuthorize(
        Constants.Applications.Content,
        Constants.Applications.Media,
        Constants.Applications.Members)]
    [Tree(Constants.Applications.Members, Constants.Trees.Members, SortOrder = 0)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    [SearchableTree("searchResultFormatter", "configureMemberResult")]
    public class MemberTreeController : TreeController, ISearchableTree, ITreeNodeController
    {


        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

        public MemberTreeController(
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IRuntimeState runtimeState,
            UmbracoMapper umbracoMapper,
            IPublishedUrlProvider publishedUrlProvider,
            UmbracoTreeSearcher treeSearcher,
            IMenuItemCollectionFactory menuItemCollectionFactory)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoMapper, publishedUrlProvider)
        {
            _treeSearcher = treeSearcher;
            _menuItemCollectionFactory = menuItemCollectionFactory;
        }

        /// <summary>
        /// Gets an individual tree node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        public TreeNode GetTreeNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))]FormDataCollection queryStrings)
        {
            var node = GetSingleTreeNode(id, queryStrings);

            //add the tree alias to the node since it is standalone (has no root for which this normally belongs)
            node.AdditionalData["treeAlias"] = TreeAlias;
            return node;
        }

        protected TreeNode GetSingleTreeNode(string id, FormDataCollection queryStrings)
        {
            Guid asGuid;
            if (Guid.TryParse(id, out asGuid) == false)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            var member = Services.MemberService.GetByKey(asGuid);
            if (member == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            var node = CreateTreeNode(
                member.Key.ToString("N"),
                "-1",
                queryStrings,
                member.Name,
                Constants.Icons.Member,
                false,
                "",
                Udi.Create(ObjectTypes.GetUdiType(Constants.ObjectTypes.Member), member.Key));

            node.AdditionalData.Add("contentType", member.ContentTypeAlias);
            node.AdditionalData.Add("isContainer", true);

            return node;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.RootString)
            {
                nodes.Add(
                        CreateTreeNode(Constants.Conventions.MemberTypes.AllMembersListId, id, queryStrings, Services.TextService.Localize("member/allMembers"), Constants.Icons.MemberType, true,
                            queryStrings.GetRequiredValue<string>("application") + TreeAlias.EnsureStartsWith('/') + "/list/" + Constants.Conventions.MemberTypes.AllMembersListId));

                nodes.AddRange(Services.MemberTypeService.GetAll()
                        .Select(memberType =>
                            CreateTreeNode(memberType.Alias, id, queryStrings, memberType.Name, memberType.Icon.IfNullOrWhiteSpace(Constants.Icons.Member), true,
                                queryStrings.GetRequiredValue<string>("application") + TreeAlias.EnsureStartsWith('/') + "/list/" + memberType.Alias)));
            }

            //There is no menu for any of these nodes
            nodes.ForEach(x => x.MenuUrl = null);
            //All nodes are containers
            nodes.ForEach(x => x.AdditionalData.Add("isContainer", true));

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();

            if (id == Constants.System.RootString)
            {
                // root actions
                //set default
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                //Create the normal create action
                menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);

                menu.Items.Add(new RefreshNode(Services.TextService, true));
                return menu;
            }

            //add delete option for all members
            menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true);

            if (Security.CurrentUser.HasAccessToSensitiveData())
            {
                menu.Items.Add(new ExportMember(Services.TextService));
            }

            return menu;
        }

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
        {
            return _treeSearcher.ExamineSearch(query, UmbracoEntityTypes.Member, pageSize, pageIndex, out totalFound, searchFrom);
        }
    }
}
