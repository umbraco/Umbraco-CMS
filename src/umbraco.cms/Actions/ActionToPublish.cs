using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when children to a document is being sent to published (by an editor without publishrights)
	/// </summary>
	public class ActionToPublish : IAction
	{
		//create singleton
#pragma warning disable 612,618
		private static readonly ActionToPublish m_instance = new ActionToPublish();
#pragma warning restore 612,618

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionToPublish() { }

		public static ActionToPublish Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'H';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionToPublish()", ClientTools.Scripts.GetAppActions);
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
				return "sendtopublish";
			}
		}

		public string Icon
		{
			get
			{
                return "outbox";
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
				//SD: Changed this to true so that any user may be able to perform this action, not just a writer
				return true;
			}
		}
		#endregion
	}
}
