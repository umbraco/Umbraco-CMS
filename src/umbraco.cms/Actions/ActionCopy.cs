using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when copying a document, media, member 
	/// </summary>
	public class ActionCopy : IAction
	{
		//create singleton
#pragma warning disable 612,618
		private static readonly ActionCopy m_instance = new ActionCopy();
#pragma warning restore 612,618

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionCopy() { }

		public static ActionCopy Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{

				return 'O';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionCopy()", ClientTools.Scripts.GetAppActions);
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

				return "copy";
			}
		}

		public string Icon
		{
			get
			{

                return "documents";
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
