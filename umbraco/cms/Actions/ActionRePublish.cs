using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when all documents are being republished
	/// </summary>
	public class ActionRePublish : IAction
	{
		//create singleton
		private static readonly ActionRePublish m_instance = new ActionRePublish();

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionRePublish() { }

		public static ActionRePublish Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'B';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionRePublish()", ClientTools.Scripts.GetAppActions);
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
				return "republish";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprPublish";
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
