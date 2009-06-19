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
using umbraco.BusinessLogic.Actions;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;


namespace umbraco
{
    public class loadMedia : BaseTree
    {

		/// <summary>
		/// Create the linkable data types list and add the DataTypeUploadField guid to it.
		/// By default, any media type that is to be "linkable" in the WYSIWYG editor must contain
		/// a DataTypeUploadField data type which will ouput the value for the link, however, if 
		/// a developer wants the WYSIWYG editor to link to a custom media type, they will either have
		/// to create their own media tree and inherit from this one and override the GetLinkValue 
		/// or add another GUID to the LinkableMediaDataType list on application startup that matches
		/// the GUID of a custom data type. The order of GUIDs will determine the output value.
		/// </summary>
		static loadMedia()
		{
			LinkableMediaDataTypes = new List<Guid>();
			LinkableMediaDataTypes.Add(new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));
		}

		public loadMedia(string application)
			: base(application)
		{
						
		}

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {            
			//TODO: SD: Find out what openMedia does!?
            if (this.IsDialog)
                rootNode.Action = "javascript:openMedia(-1);";
            else
                rootNode.Action = ClientTools.Scripts.OpenDashboard("Media");
        }

        protected override void CreateRootNodeActions(ref List<IAction> actions)
		{
			actions.Clear();
			actions.Add(ActionNew.Instance);
			actions.Add(ContextMenuSeperator.Instance);
			actions.Add(ActionSort.Instance);
			actions.Add(ContextMenuSeperator.Instance);
			actions.Add(ActionRefresh.Instance);
		}

        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionNew.Instance);
            actions.Add(ActionMove.Instance);
            actions.Add(ActionDelete.Instance);
            actions.Add(ActionSort.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionRefresh.Instance);
        }

		public override int StartNodeID
		{
			get
			{
				UmbracoEnsuredPage page = new UmbracoEnsuredPage();
				return page.getUser().StartMediaId;
			}
		}

        public override void RenderJS(ref StringBuilder Javascript)
        {
            if (!string.IsNullOrEmpty(this.FunctionToCall))
            {
                Javascript.Append("function openMedia(id) {\n");
                Javascript.Append(this.FunctionToCall + "(id)\n");
                Javascript.Append("}\n");
            }
            else if (HttpContext.Current.Request.QueryString["isDialog"] == null)
            {
                Javascript.Append(
					@"
function openMedia(id) {
	" + ClientTools.Scripts.GetContentFrame() + ".location.href = '/umbraco/editMedia.aspx?id=' + id;" + @"
}
");
            }
            else
            {
				//TODO: SD: Find out how what this does...?
                Javascript.Append(
                    @"
function openMedia(id) {
	if (parent.opener)
		parent.opener.dialogHandler(id);
	else
		parent.dialogHandler(id);	
}
");
            }
        }

        public override void Render(ref XmlTree tree)
        {
            Media[] docs;

            if (m_id == -1)
                docs = Media.GetRootMedias();
            else
                docs = new Media(m_id).Children;
            
            foreach (Media dd in docs)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = dd.Id.ToString();
                xNode.Text = dd.Text;

                // Check for dialog behaviour
                if (!this.IsDialog)
                {
                    if (!this.ShowContextMenu)
                        xNode.Menu = null;
                    xNode.Action = "javascript:openMedia(" + dd.Id + ");";
                }
                else
                {
                    if (this.ShowContextMenu)
                        xNode.Menu = new List<IAction>(new IAction[] { ActionRefresh.Instance });
                    else
                        xNode.Menu = null;
                    if (this.DialogMode == TreeDialogModes.fulllink)
                    {
                        string nodeLink = GetLinkValue(dd, dd.Id.ToString());
                        if (!String.IsNullOrEmpty(nodeLink))
                        {
                            xNode.Action = "javascript:openMedia('" + nodeLink + "');";
                        }
                        else
                        {
                            xNode.Action = null;
                            xNode.DimNode();
                        }
                    }
                    else
                    {
                        xNode.Action = "javascript:openMedia('" + dd.Id.ToString() + "');";
                    }
                }

				xNode.HasChildren = dd.HasChildren;
                if (this.IsDialog)
                    xNode.Source = GetTreeDialogUrl(dd.Id);
                else
                    xNode.Source = GetTreeServiceUrl(dd.Id);                    

                if (dd.ContentType != null)
                {
                    xNode.Icon = dd.ContentType.IconUrl;
                    xNode.OpenIcon = dd.ContentType.IconUrl;                    
                }

                base.OnBeforeNodeRender(ref tree, ref xNode, EventArgs.Empty);
                if (xNode != null)
                {
                    tree.Add(xNode);
                }
                base.OnAfterNodeRender(ref tree, ref xNode, EventArgs.Empty);
            }
        }

		/// <summary>
		/// Returns the value for a link in WYSIWYG mode, by default only media items that have a 
		/// DataTypeUploadField are linkable, however, a custom tree can be created which overrides
		/// this method, or another GUID for a custom data type can be added to the LinkableMediaDataTypes
		/// list on application startup.
		/// </summary>
		/// <param name="dd"></param>
		/// <param name="nodeLink"></param>
		/// <returns></returns>
        public virtual string GetLinkValue(Media dd, string nodeLink)
        {

			foreach (Property p in dd.getProperties)
			{				
				Guid currId = p.PropertyType.DataTypeDefinition.DataType.Id;
				if (LinkableMediaDataTypes.Contains(currId) &&  !String.IsNullOrEmpty(p.Value.ToString()))
				{
					return p.Value.ToString();
				}
			}
            return "";
        }

		/// <summary>
		/// By default, any media type that is to be "linkable" in the WYSIWYG editor must contain
		/// a DataTypeUploadField data type which will ouput the value for the link, however, if 
		/// a developer wants the WYSIWYG editor to link to a custom media type, they will either have
		/// to create their own media tree and inherit from this one and override the GetLinkValue 
		/// or add another GUID to the LinkableMediaDataType list on application startup that matches
		/// the GUID of a custom data type. The order of property types on the media item definition will determine the output value.
		/// </summary>
		public static List<Guid> LinkableMediaDataTypes { get; private set; }

    }
}
