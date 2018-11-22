using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
    /// <summary>
	/// This action is invoked when a node reloads its children
	/// Concerns only the tree itself and thus you should not handle
	/// this action from without umbraco.
	/// </summary>
    [LegacyActionMenuItem("umbracoMenuActions", "RefreshNode")]
	public class ActionRefresh : IAction
	{
		//create singleton
		private static readonly ActionRefresh InnerInstance = new ActionRefresh();

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionRefresh() { }

		public static ActionRefresh Instance
		{
			get { return InnerInstance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{

				return 'L';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionRefresh()", ClientTools.Scripts.GetAppActions);
			}
		}

		public string JsSource
		{
			get
			{

				return null;
			}
		}

		public string Alias
		{
			get
			{

				return "refreshNode";
			}
		}

		public string Icon
		{
			get
			{

                return "refresh";
			}
		}

		public bool ShowInNotifier
		{
			get
			{

				return false;
			}
		}
		public bool CanBePermissionAssigned
		{
			get
			{

				return false;
			}
		}
		#endregion
	}
}
