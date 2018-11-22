using System;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using System.Linq;

using System.Collections.Generic;
using umbraco.interfaces;

namespace umbraco.editorControls.tags
{
    [Obsolete("Use the TagService to query tags or the UmbracoHelper on the front-end and use the SetTags, RemoveTags extension methods on IContentBase to manipulate tags")]
    public class library
    {
        /// <summary>
        /// Gets everything (content, media, members) in Umbraco with the specified tags.
        /// It returns the found nodes as:
        /// <root>
        /// <node><data/>...</node>
        /// <node><data/>...</node>
        /// etc...
        /// </root>
        /// </summary>
        /// <param name="tags">Commaseparated tags.</param>
        /// <returns>
        /// A XpathNodeIterator
        ///</returns>
        public static XPathNodeIterator getEverythingWithTags(string tags)
        {
            var nodes = umbraco.cms.businesslogic.Tags.Tag.GetNodesWithTags(tags);

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("root");
            foreach (var n in nodes)
            {
                root.AppendChild(n.ToXml(xmlDoc, true));
            }

            xmlDoc.AppendChild(root);
            return xmlDoc.CreateNavigator().Select(".");
        }

        /// <summary>
        /// Gets all content nodes in Umbraco with the specified tag.
        /// It returns the found nodes as:
        /// <root>
        /// <node><data/>...</node>
        /// <node><data/>...</node>
        /// etc...
        /// </root>
        /// </summary>
        /// <param name="tags">Commaseparated tags.</param>
        /// <returns>
        /// A XpathNodeIterator
        ///</returns>
        public static XPathNodeIterator getContentsWithTags(string tags)
        {
            XmlDocument xmlDoc = new XmlDocument();

            var docs = umbraco.cms.businesslogic.Tags.Tag.GetDocumentsWithTags(tags);                       

            XmlNode root = xmlDoc.CreateElement("root");
            foreach(var d in docs)
            {                                
                root.AppendChild(d.ToXml(xmlDoc, true));
            }

            xmlDoc.AppendChild(root);
            
            return xmlDoc.CreateNavigator().Select(".");
        }

        /// <summary>
        /// Returns all members (does not care about groups and types) in Umbraco with the specified tag.
        /// It returns the found nodes as:
        /// <root>
        /// <node><data/>...</node>
        /// <node><data/>...</node>
        /// etc...
        /// </root>
        /// </summary>
        /// <param name="tags">Comma separated tags.</param>
        /// <returns>
        /// A XpathNodeIterator
        ///</returns>
        public static XPathNodeIterator getMembersWithTags(string tags)
        {
            //TODO: Implement a 'real' getter to get media items. 
            //I ported this code to the cms assembly and noticed none of these methods actually do anything different:
            // getMediaWithTags
            // getMembersWithTags
            //they just return CMSNodes!

            var nodes = umbraco.cms.businesslogic.Tags.Tag.GetNodesWithTags(tags);

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("root");
            foreach (var n in nodes)
            {
                root.AppendChild(n.ToXml(xmlDoc, true));
            }

            xmlDoc.AppendChild(root);
            return xmlDoc.CreateNavigator().Select(".");
        }

        /// <summary>
        /// Returns all media nodes in Umbraco with the specified tag.
        /// It returns the found nodes as:
        /// <root>
        /// <node><data/>...</node>
        /// <node><data/>...</node>
        /// etc...
        /// </root>
        /// </summary>
        /// <param name="tags">Commaseparated tags.</param>
        /// <returns>
        /// A XpathNodeIterator
        ///</returns>
        public static XPathNodeIterator getMediaWithTags(string tags)
        {
            //TODO: Implement a 'real' getter to get media items. 
            //I ported this code to the cms assembly and noticed none of these methods actually do anything different:
            // getMediaWithTags
            // getMembersWithTags
            //they just return CMSNodes!

            var nodes = umbraco.cms.businesslogic.Tags.Tag.GetNodesWithTags(tags);

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("root");
            foreach(var n in nodes)
            {
                root.AppendChild(n.ToXml(xmlDoc, true));
            }

            xmlDoc.AppendChild(root);
            return xmlDoc.CreateNavigator().Select(".");
        }


        /// <summary>
        /// Gets all tags
        /// It returns the found nodes as:
        /// <tags>
        /// <tag id="" nodesTagged="" group="">tagname<tag>
        /// etc...
        /// </tags>
        /// Each tag node will contain it's numeric ID, number of nodes tagged with it, and the tags group.
        /// </summary>
        /// <returns>
        /// A XpathNodeIterator
        ///</returns>
        public static XPathNodeIterator getAllTags()
        {
            string xmlVal = "<tags>\n";

            var tags = umbraco.cms.businesslogic.Tags.Tag.GetTags();

            foreach(var t in tags)
            {
                xmlVal += "<tag id=\"" + t.Id.ToString() + "\" group=\"" + t.Group + "\" nodesTagged=\"" + t.NodeCount.ToString() + "\">" + t.TagCaption + "</tag>\n";
            }
            xmlVal += "</tags>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlVal);

            return doc.CreateNavigator().Select(".");
        }

        /// <summary>
        /// Gets all tags in a specific group
        /// It returns the found nodes as:
        /// <tags>
        /// <tag id="" nodesTagged="" group="">tagname<tag>
        /// etc...
        /// </tags>
        /// Each tag node will contain it's numeric ID, number of nodes tagged with it, and the tags group.
        /// </summary>
        /// <returns>
        /// A XpathNodeIterator
        ///</returns>
        public static XPathNodeIterator getAllTagsInGroup(string group)
        {
            string xmlVal = "<tags>\n";

            var tags = umbraco.cms.businesslogic.Tags.Tag.GetTags(group);
            
            foreach (var t in tags)
            {
                xmlVal += "<tag id=\"" + t.Id.ToString() +"\" group=\"" + t.Group + "\" nodesTagged=\"" + t.NodeCount.ToString() + "\">" + t.TagCaption + "</tag>\n";
            }
            xmlVal += "</tags>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlVal);

            return doc.CreateNavigator().Select(".");
        }

        /// <summary>
        /// Gets all tags associated with a specific node ID
        /// It returns the found nodes as:
        /// <tags>
        /// <tag id="" nodesTagged="" group="">tagname<tag>
        /// etc...
        /// </tags>
        /// Each tag node will contain it's numeric ID, number of nodes tagged with it, and the tags group.
        /// </summary>
        /// <returns>
        /// A XpathNodeIterator
        ///</returns>
        public static XPathNodeIterator getTagsFromNode(string nodeId)
        {
            string xmlVal = "<tags>\n";

            var tags = umbraco.cms.businesslogic.Tags.Tag.GetTags(int.Parse(nodeId));

            foreach (var t in tags)
            {
                xmlVal += "<tag id=\"" + t.Id.ToString() + "\" group=\"" + t.Group + "\" nodesTagged=\"" + t.NodeCount.ToString() + "\">" + t.TagCaption + "</tag>\n";
            }
            xmlVal += "</tags>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlVal);

            return doc.CreateNavigator().Select(".");
        }

        #region Obsolete
        [Obsolete("use the umbraco.cms.businesslogic.Tags.Tag class instead")]
        public static void addTagsToNode(int nodeId, string tags, string group)
        {
            umbraco.cms.businesslogic.Tags.Tag.AddTagsToNode(nodeId, tags, group);
        }

        [Obsolete("use the umbraco.cms.businesslogic.Tags.Tag class instead")]
        public static void RemoveTagFromNode(int nodeId, string tag, string group)
        {
            umbraco.cms.businesslogic.Tags.Tag.RemoveTagFromNode(nodeId, tag, group);
        }

        [Obsolete("use the umbraco.cms.businesslogic.Tags.Tag class instead")]
        public static int AddTag(string tag, string group)
        {
            return umbraco.cms.businesslogic.Tags.Tag.AddTag(tag, group);
        }

        [Obsolete("use the umbraco.cms.businesslogic.Tags.Tag class instead")]
        public static int GetTagId(string tag, string group)
        {
            return umbraco.cms.businesslogic.Tags.Tag.GetTagId(tag, group);
        }

        /// <summary>
        /// Gets the tags from node as ITag objects.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <returns></returns>
        [Obsolete("Use the umbraco.cms.businesslogic.Tags.Tag class instead")]
        public static List<umbraco.interfaces.ITag> GetTagsFromNodeAsITags(int nodeId)
        {
            return umbraco.cms.businesslogic.Tags.Tag.GetTags(nodeId).Cast<ITag>().ToList();
        }

        /// <summary>
        /// Gets the tags from group as ITag objects.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        [Obsolete("Use the umbraco.cms.businesslogic.Tags.Tag class instead")]
        public static List<umbraco.interfaces.ITag> GetTagsFromGroupAsITags(string group)
        {
            return umbraco.cms.businesslogic.Tags.Tag.GetTags(group).Cast<ITag>().ToList();
        }

        /// <summary>
        /// Gets all the tags as ITag objects
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <returns></returns>
        [Obsolete("Use the umbraco.cms.businesslogic.Tags.Tag class instead")]
        public static List<umbraco.interfaces.ITag> GetTagsAsITags()
        {
            return umbraco.cms.businesslogic.Tags.Tag.GetTags().Cast<ITag>().ToList();
        }

        #endregion
        
    }

    [Obsolete("Use the TagService to query tags or the UmbracoHelper on the front-end and use the SetTags, RemoveTags extension methods on IContentBase to manipulate tags")]
    public class Tag : umbraco.interfaces.ITag
    {

        public Tag() { }
        public Tag(int id, string tag, string group)
        {
            Id = id;
            TagCaption = tag;
            Group = group;
        }

        #region ITag Members

        public int Id
        {
            get;
            set;
        }

        public string TagCaption
        {
            get;
            set;
        }

        public string Group
        {
            get;
            set;
        }

        #endregion
    }
}
