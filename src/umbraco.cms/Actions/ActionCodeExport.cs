using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when exporting a document type
	/// </summary>
	public class ActionCodeExport : IAction
	{
		//create singleton
		private static ActionCodeExport m_instance;

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		private ActionCodeExport() { }

		public static ActionCodeExport Instance
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = new ActionCodeExport();
				}

				return m_instance;
			}
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return '4';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionExportCode()", ClientTools.Scripts.GetAppActions);
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
				return "exportDocumentTypeAsCode";
			}
		}

		public string Icon
		{
			get
			{
				return ".sprExportDocumentType";
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
