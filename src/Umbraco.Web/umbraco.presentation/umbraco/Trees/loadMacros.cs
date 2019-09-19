using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic;
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
using umbraco.BusinessLogic.Actions;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;


namespace umbraco
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    public class loadMacros : BaseTree
	{

        public loadMacros(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {   
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }

        /// <summary>
        /// Unused, please do not use
        /// </summary>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
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
            using (var sqlHelper = Application.SqlHelper)
            using (IRecordsReader macros = sqlHelper.ExecuteReader("select id, macroName from cmsMacro order by macroName"))
            {
                while (macros.Read())
                {
                    XmlTreeNode xNode = XmlTreeNode.Create(this);
                    xNode.NodeID = macros.GetInt("id").ToString();
                    xNode.Text = macros.GetString("macroName");
                    xNode.Action = "javascript:openMacro(" + macros.GetInt("id") + ");";
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
}
