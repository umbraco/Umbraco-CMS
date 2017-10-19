using System;
using umbraco.interfaces;
using umbraco.BasePages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace umbraco.BusinessLogic.Actions
{
    /// <summary>
    /// This action is invoked when a send to translate request occurs
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.ContentCategory)]
    public class ActionSendToTranslate : IAction
	{
		//create singleton
#pragma warning disable 612,618
		private static readonly ActionSendToTranslate m_instance = new ActionSendToTranslate();
#pragma warning restore 612,618

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionSendToTranslate() { }

		public static ActionSendToTranslate Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return '5';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionSendToTranslate()", ClientTools.Scripts.GetAppActions);
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
				return "sendToTranslate";
			}
		}

		public string Icon
		{
			get
			{
                return "chat";
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
