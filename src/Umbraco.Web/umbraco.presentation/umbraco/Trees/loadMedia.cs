using System;
using System.Collections.Generic;
using umbraco.BasePages;
using umbraco.businesslogic;
using umbraco.interfaces;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;


namespace umbraco
{
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    public class loadMedia : BaseMediaTree
    {

        private int _StartNodeID;
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
			LinkableMediaDataTypes.Add(new Guid(Constants.PropertyEditors.UploadField));
		}

		public loadMedia(string application)
			: base(application)
		{
            _StartNodeID = CurrentUser.StartMediaId;
		}		

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {            

            if (this.IsDialog)
                rootNode.Action = "javascript:openMedia(-1);";
            else
                rootNode.Action = "javascript:" + ClientTools.Scripts.OpenDashboard("Media");
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
                return _StartNodeID;
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

	

    }
}
