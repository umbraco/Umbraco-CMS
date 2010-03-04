using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.contentitem;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Actions;


namespace umbraco
{
	public class loadLanguages : BaseTree
	{
        public loadLanguages(string application) : base(application) { }

       protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {   
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }

		public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openLanguage(id) {
	parent.right.document.location.href = 'settings/editLanguage.aspx?id=' + id;
}

function openDictionary() {
	parent.right.document.location.href = 'settings/DictionaryItemList.aspx';
}");
        }

        public override void Render(ref XmlTree tree)
        {
            foreach (Language l in Language.getAll)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = l.id.ToString(); //"language_" + l.id.ToString();
                xNode.Text = l.FriendlyName;
                xNode.Action = "javascript:openLanguage(" + l.id + ");";
                xNode.Icon = "settingLanguage.gif";
                xNode.OpenIcon = "settingLanguage.gif";

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
