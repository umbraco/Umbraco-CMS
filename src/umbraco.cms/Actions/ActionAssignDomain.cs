using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when a domain is being assigned to a document
	/// </summary>
	public class ActionAssignDomain : IAction
	{
		//create singleton
#pragma warning disable 612,618
		private static readonly ActionAssignDomain m_instance = new ActionAssignDomain();
#pragma warning restore 612,618

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionAssignDomain() { }

		public static ActionAssignDomain Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'I';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionAssignDomain()", ClientTools.Scripts.GetAppActions);
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
				return "assignDomain";
			}
		}

		public string Icon
		{
			get
			{
                return "home";
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
				return true;
			}
		}
		#endregion
	}
}
