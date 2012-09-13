using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Routing;
using umbraco;
using umbraco.NodeFactory;
using umbraco.interfaces;
using System.Linq;

namespace Umbraco.Web
{
	/// <summary>
    /// An IPublishedContentStore which uses the Xml cache system to return data
    /// </summary>
    internal class DefaultPublishedContentStore : IPublishedContentStore
    {

    	private IDocument ConvertToDocument(XmlNode xmlNode)
		{
			if (xmlNode == null)
				return null;

			return new Models.XmlDocument(xmlNode);
		}
		
    	public virtual IDocument GetDocumentById(UmbracoContext umbracoContext, int nodeId)
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

			//check if we can find the node in our xml cache
			var found = GetXml(umbracoContext).SelectSingleNode(xpath);

			//this check is because we allow root nodes that don't have a domain assigned, if 
			//the previous check fails, we will check if this is a root node (based purely on it having a path with only 1 '/' which it is prefixed with)
			//if it is a root node, we'll try to find it again without hiding top level nodes
			if (found == null && path.ToCharArray().Count(x => x == '/') == 1)
			{
				xpath = CreateXpathQuery(startNodeId, path, false);
				found = GetXml(umbracoContext).SelectSingleNode(xpath);
			}

        	return ConvertToDocument(found);
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