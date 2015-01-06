using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when the content item is to be restored from the recycle bin
	/// </summary>
	public class ActionRestore : IAction
	{
		//create singleton
#pragma warning disable 612,618
		private static readonly ActionRestore m_instance = new ActionRestore();
#pragma warning restore 612,618

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionRestore() { }

		public static ActionRestore Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'V';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return null;
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

				return "restore";
			}
		}

		public string Icon
		{
			get
			{

				return "undo";
			}
		}

		public bool ShowInNotifier
		{
			get
			{

				return true;
			}
		}

		public bool CanBePermissionAssigned
		{
			get
			{

				return true;
			}
		}

		#endregion
	}
}