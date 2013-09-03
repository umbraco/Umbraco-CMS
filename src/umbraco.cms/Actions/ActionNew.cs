using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked upon creation of a document
	/// </summary>
	public class ActionNew : IAction
	{
		//create singleton
		private static readonly ActionNew InnerInstance = new ActionNew();

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionNew() { }

		public static ActionNew Instance
		{
			get { return InnerInstance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'C';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionNew()", ClientTools.Scripts.GetAppActions);
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
				return "create";
			}
		}

		public string Icon
		{
			get
			{
                return "add";
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
