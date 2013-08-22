using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using umbraco;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using Examine.LuceneEngine;

namespace UmbracoExamine
{
    /// <summary>
    /// Static methods to help query umbraco xml
    /// </summary>
    public static class ContentExtensions
    {

        /// <summary>
        /// Converts a content node to XDocument
        /// </summary>
        /// <param name="node"></param>
        /// <param name="cacheOnly">true if data is going to be returned from cache</param>
        /// <returns></returns>
        /// <remarks>
        /// If the type of node is not a Document, the cacheOnly has no effect, it will use the API to return
        /// the xml. 
        /// </remarks>
		[SecuritySafeCritical]
		[Obsolete("This method is no longer used and will be removed in future versions")]
        public static XDocument ToXDocument(this Content node, bool cacheOnly)
        {
            if (cacheOnly && node.GetType().Equals(typeof(Document)))
            {
                var umbXml = LegacyLibrary.GetXmlNodeById(node.Id.ToString());
                if (umbXml != null)
                {
                    return umbXml.ToXDocument();    
                }
            }

            //this will also occur if umbraco hasn't cached content yet....

            //if it's not a using cache and it's not cacheOnly, then retrieve the Xml using the API
            return node.ToXDocument();
        }

        /// <summary>
        /// Converts a content node to Xml
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
		[SecuritySafeCritical] 
       	[Obsolete("This method is no longer used and will be removed in future versions")]
        private static XDocument ToXDocument(this Content node)
        {
            var xDoc = new XmlDocument();
            var xNode = xDoc.CreateNode(XmlNodeType.Element, "node", "");
            node.XmlPopulate(xDoc, ref xNode, false);

            if (xNode.Attributes["nodeTypeAlias"] == null)
            {
                //we'll add the nodeTypeAlias ourselves                                
                XmlAttribute d = xDoc.CreateAttribute("nodeTypeAlias");
                d.Value = node.ContentType.Alias;
                xNode.Attributes.Append(d);
            }

            return new XDocument(xNode.ToXElement());
        }

    }
}
