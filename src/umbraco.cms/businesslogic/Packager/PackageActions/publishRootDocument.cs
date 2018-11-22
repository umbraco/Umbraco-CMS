using System;
using System.Xml;

namespace umbraco.cms.businesslogic.packager.standardPackageActions
{
	/// <summary>
	/// This class implements the IPackageAction Interface, used to execute code when packages are installed.
	/// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
	/// </summary>
	public class publishRootDocument : umbraco.interfaces.IPackageAction
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

			web.Document[] rootDocs = web.Document.GetRootDocuments();

			foreach (web.Document rootDoc in rootDocs)
			{
				if (rootDoc.Text.Trim() == documentName.Trim() && rootDoc != null && rootDoc.ContentType != null)
				{

					rootDoc.PublishWithChildrenWithResult(umbraco.BusinessLogic.User.GetUser(0));


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
