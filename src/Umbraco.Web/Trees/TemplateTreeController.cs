using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using umbraco.cms.presentation.Trees;
using Constants = Umbraco.Core.Constants;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web.Mvc;
using umbraco.cms.businesslogic.template;
using Umbraco.Web.Models.Trees;

//namespace Umbraco.Web.Trees
//{
//    [UmbracoApplicationAuthorize(Constants.Applications.Settings)]
//    [Tree(Constants.Applications.Settings, Constants.Trees.Templates, "Templates")]
//    [PluginController("UmbracoTrees")]
//    [CoreTree]
//    public class TemplateTreeController : TreeController
//    {
//        protected override Models.Trees.MenuItemCollection GetMenuForNode(string id, System.Net.Http.Formatting.FormDataCollection queryStrings)
//        {
//            return new Models.Trees.MenuItemCollection();
//        }

//        protected override Models.Trees.TreeNodeCollection GetTreeNodes(string id, System.Net.Http.Formatting.FormDataCollection queryStrings)
//        {
//            IEnumerable<Umbraco.Core.Models.EntityBase.IUmbracoEntity> templates;
//            var nodes = new TreeNodeCollection();


//            if (id == "-1")
//                templates = Services.EntityService.GetRootEntities(Core.Models.UmbracoObjectTypes.Template);
//            else
//                templates = Services.EntityService.GetChildren(int.Parse(id), Core.Models.UmbracoObjectTypes.Template);

//            foreach (var t in templates)
//            {
//                var node = CreateTreeNode(t.Id.ToString(), t.ParentId.ToString(), queryStrings, t.Name);
//                node.Icon = "icon-newspaper-alt";
//                node.HasChildren = Services.EntityService.GetChildren(t.Id, Core.Models.UmbracoObjectTypes.Template).Any();

//                if (node.HasChildren)
//                    node.Icon = "icon-newspaper";

//                nodes.Add(node);
//            }

//            return nodes;
//        }
//    }
//}
