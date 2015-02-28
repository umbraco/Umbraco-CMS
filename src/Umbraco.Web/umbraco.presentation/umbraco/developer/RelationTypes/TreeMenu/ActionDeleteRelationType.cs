using umbraco.interfaces;

namespace umbraco.cms.presentation.developer.RelationTypes.TreeMenu
{
	/// <summary>
	/// Delete a Relation Type - an Umbraco tree context menu action
	/// </summary>
	public class ActionDeleteRelationType : IAction
	{
		/// <summary>
		/// Private field for the singleton instance
		/// </summary>
		private static readonly ActionDeleteRelationType instance = new ActionDeleteRelationType();

		/// <summary>
		/// Gets a singleton instance of this action
		/// </summary>
		public static ActionDeleteRelationType Instance
		{
			get { return instance; }
		}

		#region IAction Members

		/// <summary>
		/// Gets a string alias used to identify this action
		/// </summary>
		public string Alias
		{
			get { return "delete"; }
		}

		/// <summary>
		/// Gets a unique char to associate with this action
		/// </summary>
		public char Letter
		{
			get { return '¤'; }
		}

		/// <summary>
		/// Gets a value indicating whether the Umbraco notification area is used ?
		/// </summary>
		public bool ShowInNotifier
		{
			get { return false; }
		}

		/// <summary>
		/// Gets a value indicating whether this action can be configured for use only by specific members
		/// </summary>
		public bool CanBePermissionAssigned
		{
			get { return false; } // Since this tree is in the developer section, no further granular permissions are required
		}

		/// <summary>
		/// Gets an icon to be used for the right click action 
		/// </summary>
		public string Icon
		{
            get { return "delete"; } // delete refers to an existing sprite
		}

		/// <summary>
		/// Gets a string for the javascript source
		/// </summary>
		public string JsSource
		{
			get { return "/umbraco/developer/RelationTypes/TreeMenu/ActionDeleteRelationType.js"; }
		}

		/// <summary>
		/// Gets a javascript string to execute when this action is fired
		/// </summary>
		public string JsFunctionName
		{
			get { return "javascript:actionDeleteRelationType(UmbClientMgr.mainTree().getActionNode().nodeId,UmbClientMgr.mainTree().getActionNode().nodeName);"; }
		}

		#endregion
	}
}
