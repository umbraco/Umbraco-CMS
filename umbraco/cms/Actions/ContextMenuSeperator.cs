using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// Used simply to define context menu seperator items.
	/// This should not be used directly in any code except for creating menus.
	/// </summary>
	public class ContextMenuSeperator : IAction
	{
		//create singleton
		private static readonly ContextMenuSeperator instance = new ContextMenuSeperator();
		private ContextMenuSeperator() { }
		public static ContextMenuSeperator Instance
		{
			get { return instance; }
		}

		#region IAction Members

		public char Letter
		{
			get { return ','; }
		}

		public string JsFunctionName
		{
			get { return string.Empty; }
		}
		public string JsSource
		{
			get { return string.Empty; }
		}
		public string Alias
		{
			get { return string.Empty; }
		}
		public string Icon
		{
			get { return string.Empty; }
		}
		public bool ShowInNotifier
		{
			get { return false; }
		}
		public bool CanBePermissionAssigned
		{
			get { return false; }
		}

		#endregion
	}
}
