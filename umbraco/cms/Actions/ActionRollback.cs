using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when copying a document is being rolled back
	/// </summary>
	public class ActionRollback : IAction
	{
		//create singleton
		private static readonly ActionRollback m_instance = new ActionRollback();

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionRollback() { }

		public static ActionRollback Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{

				return 'K';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionRollback()", ClientTools.Scripts.GetAppActions);
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

				return "rollback";
			}
		}

		public string Icon
		{
			get
			{

				return ".sprRollback";
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
