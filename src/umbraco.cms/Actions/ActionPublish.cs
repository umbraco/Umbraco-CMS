using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when a document is being published
	/// </summary>
	public class ActionPublish : IAction
	{
		//create singleton
#pragma warning disable 612,618
		private static readonly ActionPublish m_instance = new ActionPublish();
#pragma warning restore 612,618

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionPublish() { }

		public static ActionPublish Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'U';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionPublish()", ClientTools.Scripts.GetAppActions);
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
				return "publish";
			}
		}

		public string Icon
		{
			get
			{
                return "globe";
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
