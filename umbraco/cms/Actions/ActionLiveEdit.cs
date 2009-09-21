using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	public class ActionLiveEdit : IAction
	{
		//create singleton
		private static readonly ActionLiveEdit instance = new ActionLiveEdit();
		private ActionLiveEdit() { }
		public static ActionLiveEdit Instance
		{
			get { return instance; }
		}

		#region IAction Members

		public char Letter
		{
			get { return ':'; }
		}

		public bool ShowInNotifier
		{
			get { return false; }
		}

		public bool CanBePermissionAssigned
		{
			get { return true; }
		}

		public string Icon
		{
			get { return ".sprLiveEdit"; }
		}

		public string Alias
		{
			get { return "liveEdit"; }
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionLiveEdit()", ClientTools.Scripts.GetAppActions);
			}
		}

		public string JsSource
		{
			get { return ""; }
		}

		#endregion
	}
}
