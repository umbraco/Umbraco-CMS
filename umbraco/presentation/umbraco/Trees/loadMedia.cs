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
    public class loadMedia : BaseMediaTree
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

		/// <summary>
		/// If the user is an admin, always return entire tree structure, otherwise
		/// return the user's start node id.
		/// </summary>
		public override int StartNodeID
		{
			get
			{
				return CurrentUser.StartMediaId;
			}
		}

		/// <summary>
		/// Adds the recycling bin node. This method should only actually add the recycle bin node when the tree is initially created and if the user
		/// actually has access to the root node.
		/// </summary>
		/// <returns></returns>
		protected XmlTreeNode CreateRecycleBin()
		{
			if (m_id == -1 && !this.IsDialog)
			{
				//create a new content recycle bin tree, initialized with it's startnodeid
				MediaRecycleBin bin = new MediaRecycleBin(this.m_app);
				bin.ShowContextMenu = this.ShowContextMenu;
				bin.id = bin.StartNodeID;
				return bin.RootNode;
			}
			return null;
		}

		public override void Render(ref XmlTree tree)
		{
			base.Render(ref tree);
			XmlTreeNode recycleBin = CreateRecycleBin();
			if (recycleBin != null)
				tree.Add(recycleBin);
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
