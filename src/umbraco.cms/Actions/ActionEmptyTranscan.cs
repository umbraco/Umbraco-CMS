using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when the trash can is emptied
	/// </summary>
	public class ActionEmptyTranscan : IAction
	{
		//create singleton
		private static readonly ActionEmptyTranscan InnerInstance = new ActionEmptyTranscan();

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionEmptyTranscan() { }

		public static ActionEmptyTranscan Instance
		{
			get { return InnerInstance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'N';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionEmptyTranscan()", ClientTools.Scripts.GetAppActions);
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
				return "emptyRecycleBin";
			}
		}

		public string Icon
		{
			get
			{
				return "trash";
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
