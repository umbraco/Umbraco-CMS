using System.Xml;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.packager.standardPackageActions
{
	public class addProxyFeedHost : umbraco.interfaces.IPackageAction
	{
		#region IPackageAction Members

		public bool Execute(string packageName, XmlNode xmlData)
		{
			var hostname = xmlData.Attributes["host"].Value;
			if (string.IsNullOrEmpty(hostname))
				return false;

			var xdoc = xmlHelper.OpenAsXmlDocument(SystemFiles.FeedProxyConfig);

			xdoc.PreserveWhitespace = true;

			var xn = xdoc.SelectSingleNode("//feedProxy");
			if (xn != null)
			{
				var insert = true;

				if (xn.HasChildNodes)
				{
					foreach (XmlNode node in xn.SelectNodes("//allow"))
					{
						if (node.Attributes["host"] != null && node.Attributes["host"].Value == hostname)
							insert = false;
					}
				}

				if (insert)
				{
					var newHostname = xmlHelper.addTextNode(xdoc, "allow", string.Empty);
					newHostname.Attributes.Append(xmlHelper.addAttribute(xdoc, "host", hostname));
					xn.AppendChild(newHostname);

					xdoc.Save(IOHelper.MapPath(SystemFiles.FeedProxyConfig));

					return true;
				}
			}

			return false;
		}

		public string Alias()
		{
			return "addProxyFeedHost";
		}

		public bool Undo(string packageName, XmlNode xmlData)
		{
			var hostname = xmlData.Attributes["host"].Value;
			if (string.IsNullOrEmpty(hostname))
				return false;

			var xdoc = xmlHelper.OpenAsXmlDocument(SystemFiles.FeedProxyConfig);
			xdoc.PreserveWhitespace = true;

			var xn = xdoc.SelectSingleNode("//feedProxy");
			if (xn != null)
			{
				bool inserted = false;
				if (xn.HasChildNodes)
				{
					foreach (XmlNode node in xn.SelectNodes("//allow"))
					{
						if (node.Attributes["host"] != null && node.Attributes["host"].Value == hostname)
						{
							xn.RemoveChild(node);
							inserted = true;
						}
					}
				}

				if (inserted)
				{
					xdoc.Save(IOHelper.MapPath(SystemFiles.FeedProxyConfig));
					return true;
				}
			}

			return false;
		}

		#endregion

		public XmlNode SampleXml()
		{
			string sample = "<Action runat=\"install\" undo=\"true\" alias=\"addProxyFeedHost\" host=\"umbraco.com\"/>";
			return helper.parseStringToXmlNode(sample);
		}
	}
}