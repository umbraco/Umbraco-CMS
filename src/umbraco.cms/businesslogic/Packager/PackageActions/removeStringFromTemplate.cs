using System;
using System.Xml;

namespace umbraco.cms.businesslogic.packager.standardPackageActions
{
	public class removeStringFromTemplate : umbraco.interfaces.IPackageAction
	{
		#region IPackageAction Members

		public bool Execute(string packageName, XmlNode xmlData)
		{
			addStringToHtmlElement ast = new addStringToHtmlElement();
			return ast.Undo(packageName, xmlData);
		}

		public string Alias()
		{
			return "removeStringFromHtmlElement";
		}

		public bool Undo(string packageName, XmlNode xmlData)
		{
			return true;
		}

		public XmlNode SampleXml()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}