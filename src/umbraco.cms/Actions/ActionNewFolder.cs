using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked upon creation of a document
	/// </summary>
	[Obsolete("This class is no longer used and will be removed in future versions")]
	public class ActionNewFolder : IAction
	{
		//create singleton
#pragma warning disable 612,618
		private static readonly ActionNewFolder m_instance = new ActionNewFolder();
#pragma warning restore 612,618

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionNewFolder() { }

		public static ActionNewFolder Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return '!';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionNewFolder()", ClientTools.Scripts.GetAppActions);
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
				return "createFolder";
			}
		}

		public string Icon
		{
			get
			{
                return "plus-sign-alt";
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
