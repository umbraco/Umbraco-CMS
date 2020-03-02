using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Templates)]
    [Tree(Constants.Applications.Settings, Constants.Trees.Templates, SortOrder = 6, TreeGroup = Constants.Trees.Groups.Templating)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class TemplatesTreeController : TreeController, ISearchableTree
    {
        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

        public TemplatesTreeController(
            UmbracoTreeSearcher treeSearcher,
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IRuntimeState runtimeState,
            UmbracoMapper umbracoMapper,
            IPublishedUrlProvider publishedUrlProvider,
            IMenuItemCollectionFactory menuItemCollectionFactory)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoMapper, publishedUrlProvider)
        {
            _treeSearcher = treeSearcher;
            _menuItemCollectionFactory = menuItemCollectionFactory;
        }

        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);
            //check if there are any templates
            root.HasChildren = Services.FileService.GetTemplates(-1).Any();
            return root;
        }

        /// <summary>
        /// The method called to render the contents of the tree structure
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings">
        /// All of the query string parameters passed from jsTree
        /// </param>
        /// <remarks>
        /// We are allowing an arbitrary number of query strings to be pased in so that developers are able to persist custom data from the front-end
        /// to the back end to be used in the query for model data.
        /// </remarks>
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            var found = id == Constants.System.RootString
                ? Services.FileService.GetTemplates(-1)
                : Services.FileService.GetTemplates(int.Parse(id));

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

            return nodes;
        }

        /// <summary>
        /// Returns the menu structure for the node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();

            //Create the normal create action
            var item = menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);
            item.NavigateToRoute($"{queryStrings.GetRequiredValue<string>("application")}/templates/edit/{id}?create=true");

            if (id == Constants.System.RootString)
            {
                //refresh action
                menu.Items.Add(new RefreshNode(Services.TextService, true));

                return menu;
            }

            var template = Services.FileService.GetTemplate(int.Parse(id));
            if (template == null) return menu;
            var entity = FromTemplate(template);

            //don't allow delete if it has child layouts
            if (template.IsMasterTemplate == false)
            {
                //add delete option if it doesn't have children
                menu.Items.Add<ActionDelete>(Services.TextService, true, opensDialog: true);
            }

            //add refresh
            menu.Items.Add(new RefreshNode(Services.TextService, true));


            return menu;
        }

        private EntitySlim FromTemplate(ITemplate template)
        {
            return new EntitySlim
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

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
            => _treeSearcher.EntitySearch(UmbracoObjectTypes.Template, query, pageSize, pageIndex, out totalFound, searchFrom);
    }
}
