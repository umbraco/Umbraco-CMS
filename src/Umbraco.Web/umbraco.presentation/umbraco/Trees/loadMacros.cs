using System;
using System.Text;

using umbraco.DataLayer;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using Umbraco.Web.Trees;


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
            foreach(var macros in ApplicationContext.Current.DatabaseContext.Database.Query<dynamic>("select id, macroName from cmsMacro order by macroName"))
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = macros.id.ToString();
                xNode.Text = macros.macroName;
                xNode.Action = "javascript:openMacro(" + macros.id + ");";
                xNode.Icon = " icon-settings-alt";
                xNode.OpenIcon = "icon-settings-alt";
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
