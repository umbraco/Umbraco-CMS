using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Tests.TestHelpers;
using UmbracoExamine.DataServices;

namespace Umbraco.Tests.UmbracoExamine
{
	public class TestMediaService : IMediaService
	{

		public TestMediaService()
		{
			m_Doc = XDocument.Parse(TestFiles.media);
		}

		#region IMediaService Members

		public System.Xml.Linq.XDocument GetLatestMediaByXpath(string xpath)
		{
			var xdoc = XDocument.Parse("<media></media>");
			xdoc.Root.Add(m_Doc.XPathSelectElements(xpath));
			return xdoc;
		}

		#endregion

		private XDocument m_Doc;
	}
}
