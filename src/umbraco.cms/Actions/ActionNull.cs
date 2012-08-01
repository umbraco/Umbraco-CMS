using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This permission is assigned to a node when there are no permissions assigned to the node.
	/// This is used internally to assign no permissions to a node for a user and shouldn't be used in code.
	/// </summary>
	public class ActionNull : IAction
	{
		//create singleton
		private static readonly ActionNull instance = new ActionNull();
		private ActionNull() { }
		public static ActionNull Instance
		{
			get { return instance; }
		}

		#region IAction Members

		public char Letter
		{
			get { return '-'; }
		}

		public bool ShowInNotifier
		{
			get { return false; }
		}

		public bool CanBePermissionAssigned
		{
			get { return false; }
		}

		public string Icon
		{
			get { return string.Empty; }
		}

		public string Alias
		{
			get { return string.Empty; }
		}

		public string JsFunctionName
		{
			get { return string.Empty; }
		}

		public string JsSource
		{
			get { return string.Empty; }
		}

		#endregion
	}
}
