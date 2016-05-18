using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Xml;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Web._Legacy.PackageActions
{
	public class addProxyFeedHost : IPackageAction
	{
		#region IPackageAction Members

		public bool Execute(string packageName, XmlNode xmlData)
		{
			var hostname = xmlData.Attributes["host"].Value;
			if (string.IsNullOrEmpty(hostname))
				return false;

			var xdoc = XmlHelper.OpenAsXmlDocument(SystemFiles.FeedProxyConfig);

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
					var newHostname = XmlHelper.AddTextNode(xdoc, "allow", string.Empty);
					newHostname.Attributes.Append(XmlHelper.AddAttribute(xdoc, "host", hostname));
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

			var xdoc = XmlHelper.OpenAsXmlDocument(SystemFiles.FeedProxyConfig);
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
			return PackageHelper.ParseStringToXmlNode(sample);
		}
	}
}