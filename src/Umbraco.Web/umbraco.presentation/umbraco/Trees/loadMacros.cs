using System;
using System.Text;
using umbraco.businesslogic;
using umbraco.cms.presentation.Trees;
using umbraco.DataLayer;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;


namespace umbraco
{
	/// <summary>
	/// Handles loading of the cache application into the developer application tree
	/// </summary>
    [Tree(Constants.Applications.Developer, "macros", "Macros", sortOrder: 2)]
    public class loadMacros : BaseTree
	{

        public loadMacros(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {   
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }

        protected static ISqlHelper SqlHelper
        {
            get { return umbraco.BusinessLogic.Application.SqlHelper; }
        }

        /// <summary>
        /// Renders the JS.
        /// </summary>
        /// <param name="Javascript">The javascript.</param>
        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openMacro(id) {
	UmbClientMgr.contentFrame('developer/macros/editMacro.aspx?macroID=' + id);
}
");
        }

        /// <summary>
        /// This will call the normal Render method by passing the converted XmlTree to an XmlDocument.
        /// TODO: need to update this render method to do everything that the obsolete render method does and remove the obsolete method
        /// </summary>
        /// <param name="tree"></param>
        public override void Render(ref XmlTree tree)
        {
            var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<MacroDto>(
                "select id, macroName from cmsMacro order by macroName");
            foreach (var dto in dtos)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = dto.Id.ToString();
                xNode.Text = dto.Name;
                xNode.Action = "javascript:openMacro(" + dto.Id + ");";
                xNode.Icon = "developerMacro.gif";
                xNode.OpenIcon = "developerMacro.gif";
                OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                if (xNode != null)
                {
                    tree.Add(xNode);
                }
                OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);                
            }            
        }
	}
}
