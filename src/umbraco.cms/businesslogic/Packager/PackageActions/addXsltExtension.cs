using System;
using System.Xml;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.packager.standardPackageActions
{
	public class addXsltExtension : umbraco.interfaces.IPackageAction
	{
		#region IPackageAction Members

		public bool Execute(string packageName, XmlNode xmlData)
		{

			string _assembly = xmlData.Attributes["assembly"].Value;
			string _type = xmlData.Attributes["type"].Value;
			string _alias = xmlData.Attributes["extensionAlias"].Value;
			string xeConfig = SystemFiles.XsltextensionsConfig;

			XmlDocument xdoc = new XmlDocument();
			xdoc.PreserveWhitespace = true;
			xdoc = xmlHelper.OpenAsXmlDocument(xeConfig);

			XmlNode xn = xdoc.SelectSingleNode("//XsltExtensions");

			if (xn != null)
			{
				bool insertExt = true;
				if (xn.HasChildNodes)
				{
					foreach (XmlNode ext in xn.SelectNodes("//ext"))
					{
						if (ext.Attributes["alias"] != null && ext.Attributes["alias"].Value == _alias)
							insertExt = false;
					}
				}
				if (insertExt)
				{
					XmlNode newExt = umbraco.xmlHelper.addTextNode(xdoc, "ext", "");
					newExt.Attributes.Append(umbraco.xmlHelper.addAttribute(xdoc, "assembly", _assembly.Replace("/bin/", "")));
					newExt.Attributes.Append(umbraco.xmlHelper.addAttribute(xdoc, "type", _type));
					newExt.Attributes.Append(umbraco.xmlHelper.addAttribute(xdoc, "alias", _alias));
					xn.AppendChild(newExt);


					xdoc.Save(IOHelper.MapPath(xeConfig));
					return true;
				}
			}
			return false;
		}

		public string Alias()
		{
			return "addXsltExtension";
		}

		public bool Undo(string packageName, XmlNode xmlData)
		{
			string _assembly = xmlData.Attributes["assembly"].Value;
			string _type = xmlData.Attributes["type"].Value;
			string _alias = xmlData.Attributes["extensionAlias"].Value;
			string xeConfig = SystemFiles.XsltextensionsConfig;

			XmlDocument xdoc = new XmlDocument();
			xdoc.PreserveWhitespace = true;
			xdoc = xmlHelper.OpenAsXmlDocument(xeConfig);

			XmlNode xn = xdoc.SelectSingleNode("//XsltExtensions");

			if (xn != null)
			{
				bool inserted = false;
				if (xn.HasChildNodes)
				{
					foreach (XmlNode ext in xn.SelectNodes("//ext"))
					{
						if (ext.Attributes["alias"] != null && ext.Attributes["alias"].Value == _alias)
						{
							xn.RemoveChild(ext);
							inserted = true;
						}
					}
				}

				if (inserted)
				{
					xdoc.Save(IOHelper.MapPath(xeConfig));
					return true;
				}
			}
			return false;
		}

		#endregion

		public XmlNode SampleXml()
		{
			throw new NotImplementedException();
		}

	}
}