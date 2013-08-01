using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked upon creation of a document, media, member
	/// </summary>
	public class ActionPackage : IAction
	{
		//create singleton
#pragma warning disable 612,618
		private static readonly ActionPackage m_instance = new ActionPackage();
#pragma warning restore 612,618

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionPackage() { }

		public static ActionPackage Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'X';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionPackage()", ClientTools.Scripts.GetAppActions);
			}
		}

		public string JsSource
		{
			get
			{
				return "";
			}
		}

		public string Alias
		{
			get
			{
				return "importPackage";
			}
		}

		public string Icon
		{
			get
			{
                return "gift";
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
