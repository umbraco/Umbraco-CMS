using System;
using System.Xml;

namespace umbraco.cms.businesslogic.packager.standardPackageActions
{
	/// <summary>
	/// This class implements the IPackageAction Interface, used to execute code when packages are installed.
	/// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
	/// </summary>
	public class addApplication : umbraco.interfaces.IPackageAction
	{

		#region IPackageAction Members

		/// <summary>
		/// Installs a new application in umbraco.
		/// </summary>
		/// <param name="packageName">Name of the package.</param>
		/// <param name="xmlData">The XML data.</param>
		/// <example><code>
		/// <Action runat="install" [undo="false"] alias="addApplication" appName="Application Name"  appAlias="myApplication" appIcon="application.gif"/>
		/// </code></example>
		/// <returns>true if successfull</returns>
		public bool Execute(string packageName, XmlNode xmlData)
		{
			string name = xmlData.Attributes["appName"].Value;
			string alias = xmlData.Attributes["appAlias"].Value;
			string icon = xmlData.Attributes["appIcon"].Value;

			BusinessLogic.Application.MakeNew(name, alias, icon);

			return true;
		}

		public bool Undo(string packageName, XmlNode xmlData)
		{
			string alias = xmlData.Attributes["appAlias"].Value;
			BusinessLogic.Application.getByAlias(alias).Delete();
			return true;
		}
		/// <summary>
		/// Action alias.
		/// </summary>
		/// <returns></returns>
		public string Alias()
		{
			return "addApplication";
		}

		#endregion

		public XmlNode SampleXml()
		{
			throw new NotImplementedException();
		}

	}
}