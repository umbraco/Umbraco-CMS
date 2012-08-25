using System.Xml;
using Umbraco.Core.Models;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Mvc
{
	public class RenderModel
	{
		//public XmlNode CurrentXmlNode { get; set; }
		internal IDocument CurrentDocument { get; set; }		
		//internal page UmbracoPage { get; set; }
	}
}