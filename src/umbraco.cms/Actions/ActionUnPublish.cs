using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{



	/// <summary>
	/// This action is invoked when a document is being unpublished
	/// </summary>
	public class ActionUnPublish : IAction
	{
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionUnPublish m_instance = new ActionUnPublish();
#pragma warning restore 612,618

        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance).
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionUnPublish() { }

        public static ActionUnPublish Instance
        {
            get { return m_instance; }
        }

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'Z';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return "";
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
				return "unpublish";
			}
		}

		public string Icon
		{
			get
			{
                return "circle-dotted";
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
