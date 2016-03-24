using System;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Web._Legacy.PackageActions
{
	/// <summary>
	/// This class implements the IPackageAction Interface, used to execute code when packages are installed.
	/// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
	/// </summary>
	public class publishRootDocument : IPackageAction
	{
		#region IPackageAction Members

		/// <summary>
		/// Executes the specified package action.
		/// </summary>
		/// <param name="packageName">Name of the package.</param>
		/// <param name="xmlData">The XML data.</param>
		/// <example>
		/// <Action runat="install" alias="publishRootDocument" documentName="News"  />
		/// </example>
		/// <returns>True if executed succesfully</returns>
		public bool Execute(string packageName, XmlNode xmlData)
		{

			string documentName = xmlData.Attributes["documentName"].Value;

			int parentDocid = 0;

			global::umbraco.cms.businesslogic.web.Document[] rootDocs = global::umbraco.cms.businesslogic.web.Document.GetRootDocuments();

			foreach (global::umbraco.cms.businesslogic.web.Document rootDoc in rootDocs)
			{
				if (rootDoc.Text.Trim() == documentName.Trim() && rootDoc != null && rootDoc.ContentType != null)
				{

					rootDoc.PublishWithChildrenWithResult(ApplicationContext.Current.Services.UserService.GetUserById(0));


					break;
				}
			}
			return true;
		}

		//this has no undo.
		/// <summary>
		/// This action has no undo.
		/// </summary>
		/// <param name="packageName">Name of the package.</param>
		/// <param name="xmlData">The XML data.</param>
		/// <returns></returns>
		public bool Undo(string packageName, XmlNode xmlData)
		{
			return true;
		}

		/// <summary>
		/// Action alias
		/// </summary>
		/// <returns></returns>
		public string Alias()
		{
			return "publishRootDocument";
		}
		#endregion

		public XmlNode SampleXml()
		{
			throw new NotImplementedException();
		}

	}
}