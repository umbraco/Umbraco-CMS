using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Services.Description;
using AutoMapper;
using umbraco;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.template;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Search;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Templates)]
    [LegacyBaseTree(typeof (loadTemplates))]
    [Tree(Constants.Applications.Settings, Constants.Trees.Templates, null, sortOrder:1)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class TemplatesTreeController : TreeController, ISearchableTree
    {
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

            var found = id == Constants.System.Root.ToInvariantString() 
                ? Services.FileService.GetTemplates(-1) 
                : Services.FileService.GetTemplates(int.Parse(id));

            nodes.AddRange(found.Select(template => CreateTreeNode(
                template.Id.ToString(CultureInfo.InvariantCulture),
                //TODO: Fix parent ID stuff for templates
                "-1",
                queryStrings,
                template.Name,
                template.IsMasterTemplate ? "icon-newspaper" : "icon-newspaper-alt",
                template.IsMasterTemplate,
                GetEditorPath(template, queryStrings),
                Udi.Create(UmbracoObjectTypesExtensions.GetUdiType(Constants.ObjectTypes.TemplateTypeGuid), template.Key)
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
            var menu = new MenuItemCollection();
            //Create the normal create action
            var item = menu.Items.Add<ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
            item.NavigateToRoute(string.Format("{0}/templates/edit/{1}?create=true", queryStrings.GetValue<string>("application"), id));


            if (id == Constants.System.Root.ToInvariantString())
            {
                
                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);

                return menu;
            }

            var template = Services.FileService.GetTemplate(int.Parse(id));
            if (template == null) return new MenuItemCollection();
            var entity = FromTemplate(template);

            //don't allow delete if it has child layouts
            if (template.IsMasterTemplate == false)
            {
                //add delete option if it doesn't have children
                menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias), true);
            }

            //add refresh
            menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
            

            return menu;
        }

        private UmbracoEntity FromTemplate(ITemplate template)
        {
            return new UmbracoEntity
            {
                CreateDate = template.CreateDate,
                Id = template.Id,
                Key = template.Key,
                Name = template.Name,
                NodeObjectTypeId = new Guid(Constants.ObjectTypes.Template),
                //TODO: Fix parent/paths on templates
                ParentId = -1,
                Path = template.Path,
                UpdateDate = template.UpdateDate
            };
        }

        private string GetEditorPath(ITemplate template, FormDataCollection queryStrings)
        {
            //TODO: Rebuild the language editor in angular, then we dont need to have this at all (which is just a path to the legacy editor)

            return Services.FileService.DetermineTemplateRenderingEngine(template) == RenderingEngine.WebForms
                ? "/" + queryStrings.GetValue<string>("application") + "/framed/" +
                  Uri.EscapeDataString("settings/editTemplate.aspx?templateID=" + template.Id)
                : null;
        }

        public IEnumerable<SearchResultItem> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
        {
            var results = Services.EntityService.GetPagedDescendantsFromRoot(UmbracoObjectTypes.Template, pageIndex, pageSize, out totalFound, filter: query);
            return Mapper.Map<IEnumerable<SearchResultItem>>(results);
        }
    }
}
