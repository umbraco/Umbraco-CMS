using System;
using System.Text;
using System.Xml;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web
{

    /// <summary>
    /// Used for the new routing system
    /// </summary>
    internal class ContentStore
    {
		/// <summary>
		/// Delegate to return the current UmbracoContext
		/// </summary>
		private readonly UmbracoContext _umbracoContext;

		/// <summary>
		/// Constructor accepting a delegate to resolve the UmbracoContext
		/// </summary>
		/// <param name="umbracoContext"></param>
        public ContentStore(UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
        }

        public XmlNode GetNodeById(int nodeId)
        {
            return GetXml().GetElementById(nodeId.ToString());
        }

        public XmlNode GetNodeByRoute(string route)
        {
            string startNodeIdString = "0";
            string path = route;
            if (!route.StartsWith("/"))
            {
                int pos = route.IndexOf('/');
                startNodeIdString = route.Substring(0, pos);
                path = route.Substring(pos + 1);
            }
            int startNodeId = int.Parse(startNodeIdString);

            var xpath = CreateXpathQuery(startNodeId, path);
            //RequestContext.Current.Trace.Write(TraceCategory, "xpath='" + xpath + "'.");
            return GetXml().SelectSingleNode(xpath);
        }

        public XmlNode GetNodeByUrlAlias(int rootNodeId, string alias)
        {

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
            //RequestContext.Current.Trace.Write(TraceCategory, "xpath='" + xpath + "'.");
            return GetXml().SelectSingleNode(xpath);
        }

        public XmlNode GetNodeParent(XmlNode node)
        {
            return node.ParentNode;
        }

        public string GetNodeProperty(XmlNode node, string propertyAlias)
        {
            if (propertyAlias.StartsWith("@"))
            {
                var attribute = node.Attributes.GetNamedItem(propertyAlias.Substring(1));
                return attribute == null ? null : attribute.Value;
            }
            else
            {
                var propertyNode = node.SelectSingleNode("./" + propertyAlias);
                return propertyNode == null ? null : propertyNode.InnerText;
            }
        }

        XmlDocument GetXml()
        {
            // UmbracoModule will call LegacyRequestInitializer before anything else
            // and that will create an UmbracoContext... so we should be safe here.
            return _umbracoContext.GetXml();
        }

        static readonly char[] SlashChar = new char[] { '/' };

        // allow hideTopLevelNodeFromPath overriding at runtime
        protected virtual string CreateXpathQuery(int startNodeId, string path)
        {
            return CreateXpathQuery(startNodeId, path, GlobalSettings.HideTopLevelNodeFromPath);
        }

		

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