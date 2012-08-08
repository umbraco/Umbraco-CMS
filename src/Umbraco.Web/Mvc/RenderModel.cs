using System.Xml;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Mvc
{
	public class RenderModel
	{
		//public XmlNode CurrentXmlNode { get; set; }
		public INode CurrentNode { get; set; }		
		//internal page UmbracoPage { get; set; }
	}
}