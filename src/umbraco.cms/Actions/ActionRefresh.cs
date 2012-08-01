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
	public class ActionRefresh : IAction
	{
		//create singleton
#pragma warning disable 612,618
		private static readonly ActionRefresh m_instance = new ActionRefresh();
#pragma warning restore 612,618

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionRefresh() { }

		public static ActionRefresh Instance
		{
			get { return m_instance; }
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

				return ".sprRefresh";
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
