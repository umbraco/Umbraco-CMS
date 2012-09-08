using System;
using System.Text;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Routing;
using umbraco;
using umbraco.NodeFactory;
using umbraco.interfaces;

namespace Umbraco.Web
{
	/// <summary>
    /// An IContentStore which uses the Xml cache system to return data
    /// </summary>
    internal class XmlPublishedContentStore : IPublishedContentStore
    {

    	private IDocument ConvertToDocument(XmlNode xmlNode)
		{
			if (xmlNode == null)
				return null;

			return new Models.XmlDocument(xmlNode);
		}
		
    	public IDocument GetDocumentById(UmbracoContext umbracoContext, int nodeId)
    	{
    		if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");

    		return ConvertToDocument(GetXml(umbracoContext).GetElementById(nodeId.ToString()));
    	}

		public IDocument GetDocumentByRoute(UmbracoContext umbracoContext, string route, bool? hideTopLevelNode = null)
        {
			if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
			if (route == null) throw new ArgumentNullException("route");

			//set the default to be what is in the settings
			if (hideTopLevelNode == null)
			{
				hideTopLevelNode = GlobalSettings.HideTopLevelNodeFromPath;
			}

			//the route always needs to be lower case because we only store the urlName attribute in lower case
        	route = route.ToLowerInvariant();

            string startNodeIdString = "0";
            string path = route;
            if (!route.StartsWith("/"))
            {
                int pos = route.IndexOf('/');
                startNodeIdString = route.Substring(0, pos);
                path = route.Substring(pos + 1);
            }
            int startNodeId = int.Parse(startNodeIdString);

            var xpath = CreateXpathQuery(startNodeId, path, hideTopLevelNode.Value);

        	return ConvertToDocument(GetXml(umbracoContext).SelectSingleNode(xpath));
        }

		public IDocument GetDocumentByUrlAlias(UmbracoContext umbracoContext, int rootNodeId, string alias)
        {
			if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
			if (alias == null) throw new ArgumentNullException("alias");

			// the alias may be "foo/bar" or "/foo/bar"
            // there may be spaces as in "/foo/bar,  /foo/nil"
            // these should probably be taken care of earlier on

            alias = alias.TrimStart('/');
            var xpathBuilder = new StringBuilder();
            xpathBuilder.Append("/root");
            if (rootNodeId > 0)
                xpathBuilder.AppendFormat("//*[@isDoc and @id={0}]", rootNodeId);
            xpathBuilder.Append("//*[@isDoc and (");
            xpathBuilder.AppendFormat("contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',{0},')", alias);
            xpathBuilder.AppendFormat(" or contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',/{0},')", alias);
            xpathBuilder.Append(")]");

            var xpath = xpathBuilder.ToString();

			return ConvertToDocument(GetXml(umbracoContext).SelectSingleNode(xpath));
        }

		//public IDocument GetNodeParent(IDocument node)
		//{
		//    return node.Parent;
		//}

		public string GetDocumentProperty(UmbracoContext umbracoContext, IDocument node, string propertyAlias)
		{
			if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
			if (node == null) throw new ArgumentNullException("node");
			if (propertyAlias == null) throw new ArgumentNullException("propertyAlias");

			if (propertyAlias.StartsWith("@"))
            {
				//if it starts with an @ then its a property of the object, not a user defined property
            	var propName = propertyAlias.TrimStart('@');
				var prop = TypeHelper.GetProperty(typeof(IDocument), propName, true, false, false, false);
				if (prop == null)
					throw new ArgumentException("The property name " + propertyAlias + " was not found on type " + typeof(IDocument));
            	var val = prop.GetValue(node, null);
            	var valAsString = val == null ? "" : val.ToString();				
				return valAsString;
            }
            else
            {
            	var prop = node.GetProperty(propertyAlias);
				return prop == null ? null : Convert.ToString(prop.Value);
            	//var propertyNode = node.SelectSingleNode("./" + propertyAlias);
            	//return propertyNode == null ? null : propertyNode.InnerText;
            }
		}

		XmlDocument GetXml(UmbracoContext umbracoContext)
		{
			if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");

			return umbracoContext.GetXml();
		}

		static readonly char[] SlashChar = new char[] { '/' };

        protected string CreateXpathQuery(int startNodeId, string path, bool hideTopLevelNodeFromPath)
        {
            string xpath;

            if (path == string.Empty || path == "/")
            {
                // if url is empty
                if (startNodeId > 0)
                {
                    // if in a domain then use the root node of the domain
                    xpath = string.Format("/root//*[@isDoc and @id={0}]", startNodeId);
                }
                else
                {
                    // if not in a domain - what is the default page?
                    // let's say it is the first one in the tree, if any
                    xpath = "(/root/*[@isDoc])[1]";
                }
            }
            else
            {
                // if url is not empty, then use it to try lookup a matching page
                var urlParts = path.Split(SlashChar, StringSplitOptions.RemoveEmptyEntries);
                var xpathBuilder = new StringBuilder();
                int partsIndex = 0;

                xpathBuilder.Append("/root");
				
                if (startNodeId == 0)
                {
                    if (hideTopLevelNodeFromPath)
                        xpathBuilder.Append("/*[@isDoc]"); // first node is not in the url
                }
                else
                {
                    xpathBuilder.AppendFormat("//*[@isDoc and @id={0}]", startNodeId);
                    if (!hideTopLevelNodeFromPath)
                        xpathBuilder.AppendFormat("[@urlName='{0}']", urlParts[partsIndex++]); // first node is in the url
                }
                while (partsIndex < urlParts.Length)
                    xpathBuilder.AppendFormat("/*[@isDoc and @urlName='{0}']", urlParts[partsIndex++]);

                xpath = xpathBuilder.ToString();
            }

            return xpath;
        }
    }
}