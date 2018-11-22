using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when a user logs out
	/// </summary>
	[Obsolete("This should not be used and will be removed from the codebase in future versions")]
	public class ActionQuit : IAction
	{
		//create singleton
#pragma warning disable 612,618
		private static readonly ActionQuit m_instance = new ActionQuit();
#pragma warning restore 612,618

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionQuit() { }

		public static ActionQuit Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'Q';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionQuit()", ClientTools.Scripts.GetAppActions);
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
				return "logout";
			}
		}

		public string Icon
		{
			get
			{
                return "signout";
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
