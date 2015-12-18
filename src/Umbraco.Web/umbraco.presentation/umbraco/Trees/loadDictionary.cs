using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.interfaces;
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;


namespace umbraco
{
    [Tree(Constants.Applications.Settings, Constants.Trees.Dictionary, "Dictionary", action: "openDictionary()", sortOrder: 3)]
    public class loadDictionary : BaseTree
	{
        public loadDictionary(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {   
            rootNode.NodeType = "init" + TreeAlias;			
            rootNode.NodeID = "init";
            rootNode.Action = "javascript:openDictionary()";
        }

        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionNew.Instance);
            actions.Add(ActionDelete.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionRefresh.Instance);
        }

		public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
				@"
			function openDictionary() {
				UmbClientMgr.contentFrame('settings/DictionaryItemList.aspx');
			}
			function openDictionaryItem(id) {
				UmbClientMgr.contentFrame('settings/editDictionaryItem.aspx?id=' + id);
			}");
        }

        public override void Render(ref XmlTree tree)
        {
            
            Dictionary.DictionaryItem[] tmp;
            if (this.id == this.StartNodeID)
                tmp = Dictionary.getTopMostItems;
            else
                tmp = new Dictionary.DictionaryItem(this.id).Children;

            foreach (Dictionary.DictionaryItem di in tmp.OrderBy(a => a.key))
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = di.id.ToString(); //dictionary_ + id.. 
                xNode.Text = di.key;
                xNode.Action = string.Format("javascript:openDictionaryItem({0});", di.id);
                xNode.Icon = "icon-book-alt";
                xNode.NodeType = "DictionaryItem"; //this shouldn't be like this, it should be this.TreeAlias but the ui.config file points to this name.
                xNode.Source = this.GetTreeServiceUrl(di.id);
				xNode.HasChildren = di.hasChildren;

                OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                if (xNode != null)
                {
                    tree.Add(xNode);
                    OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
                }
                
            }
        }

		
	}
    
}
