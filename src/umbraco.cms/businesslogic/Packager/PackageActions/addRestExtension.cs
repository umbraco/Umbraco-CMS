using System;
using System.Xml;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.packager.standardPackageActions
{
	public class addRestExtension : umbraco.interfaces.IPackageAction
	{
		#region IPackageAction Members

		public bool Execute(string packageName, XmlNode xmlData)
		{

			XmlNodeList _newExts = xmlData.SelectNodes("//ext");

			if (_newExts.Count > 0)
			{

				string reConfig = SystemFiles.RestextensionsConfig;

				XmlDocument xdoc = new XmlDocument();
				xdoc.PreserveWhitespace = true;
				xdoc = xmlHelper.OpenAsXmlDocument(reConfig);


				XmlNode xn = xdoc.SelectSingleNode("//RestExtensions");

				if (xn != null)
				{
					for (int i = 0; i < _newExts.Count; i++)
					{
						XmlNode newExt = _newExts[i];
						string _alias = newExt.Attributes["alias"].Value;

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
							xn.AppendChild(xdoc.ImportNode(newExt, true));
						}
					}

					xdoc.Save(IOHelper.MapPath(reConfig));
					return true;
				}
			}
			return false;
		}

		public string Alias()
		{
			return "addRestExtension";
		}

		public bool Undo(string packageName, XmlNode xmlData)
		{

			XmlNodeList _newExts = xmlData.SelectNodes("//ext");

			if (_newExts.Count > 0)
			{
				string reConfig = SystemFiles.RestextensionsConfig;

				XmlDocument xdoc = new XmlDocument();
				xdoc.PreserveWhitespace = true;
				xdoc.Load(reConfig);

				XmlNode xn = xdoc.SelectSingleNode("//RestExtensions");

				if (xn != null)
				{
					bool inserted = false;

					for (int i = 0; i < _newExts.Count; i++)
					{
						XmlNode newExt = _newExts[i];
						string _alias = newExt.Attributes["alias"].Value;
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
					}

					if (inserted)
					{
						xdoc.Save(IOHelper.MapPath(reConfig));
						return true;
					}
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