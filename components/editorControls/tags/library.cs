using System;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;

using System.Collections.Generic;

namespace umbraco.editorControls.tags
{
    public class library
    {

        public static void addTagsToNode(int nodeId, string tags, string group)
        {
            string[] allTags = tags.Split(",".ToCharArray());
            for (int i = 0; i < allTags.Length; i++)
            {
                //if not found we'll get zero and handle that onsave instead...
                int id = GetTagId(allTags[i], group);
                if (id == 0)
                    id = AddTag(allTags[i], group);

                SqlHelper.ExecuteNonQuery("if not exists(select nodeId from cmsTagRelationShip where nodeId = @nodeId and tagId = @tagId) INSERT INTO cmsTagRelationShip(nodeId,tagId) VALUES (@nodeId, @tagId)",
    SqlHelper.CreateParameter("@nodeId", nodeId),
    SqlHelper.CreateParameter("@tagId", id)
);

            }

        }

        public static void RemoveTagFromNode(int nodeId, string tag, string group)
        {
            int tagId = GetTagId(tag, group);
            if (tagId != 0)
            {
                SqlHelper.ExecuteNonQuery("DELETE FROM cmsTagRelationship WHERE (nodeId = @nodeId and tagId = @tagId)",
                    SqlHelper.CreateParameter("@nodeId", nodeId),
                    SqlHelper.CreateParameter("@tagId", tagId));
            }
        }

        public static int AddTag(string tag, string group)
        {
            SqlHelper.ExecuteNonQuery("INSERT INTO cmsTags(tag,[group]) VALUES (@tag,@group)",
                SqlHelper.CreateParameter("@tag", tag.Trim()),
                SqlHelper.CreateParameter("@group", group));
            return GetTagId(tag, group);
        }


        public static int GetTagId(string tag, string group)
        {
            int retval = 0;
            string sql = "SELECT id FROM cmsTags where tag=@tag AND [group]=@group;";
            object result = SqlHelper.ExecuteScalar<object>(sql,
                SqlHelper.CreateParameter("@tag", tag),
                SqlHelper.CreateParameter("@group", group));

            if (result != null)
                retval = int.Parse(result.ToString());

            return retval;
        }

        public static ISqlHelper SqlHelper
        {
            get
            {
                return Application.SqlHelper;
            }
        }


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
            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationship.tagid = cmsTags.id WHERE (cmsTags.tag IN (" + GetSqlStringArray(tags) + "))";
            IRecordsReader rr = SqlHelper.ExecuteReader(sql);

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("root");
            while (rr.Read())
            {
                CMSNode cnode = new CMSNode(rr.GetInt("nodeid"));
                if (cnode != null)
                    root.AppendChild(cnode.ToXml(xmlDoc, true));
            }
            rr.Close();

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
            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id WHERE (cmsTags.tag IN (" + GetSqlStringArray(tags) + "))";
            try
            {
                IRecordsReader rr = SqlHelper.ExecuteReader(sql);

                XmlNode root = xmlDoc.CreateElement("root");
                while (rr.Read())
                {
                    Document cnode = new Document(rr.GetInt("nodeid"));

                    if (cnode != null && cnode.Published)
                        root.AppendChild(cnode.ToXml(xmlDoc, true));
                }
                rr.Close();
                xmlDoc.AppendChild(root);
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Debug, -1, ex.ToString() + " SQL: " + sql + "TAGS: " + tags);
            }
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
            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id WHERE (cmsTags.tag IN (" + GetSqlStringArray(tags) + "))";
            IRecordsReader rr = SqlHelper.ExecuteReader(sql);

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("root");
            while (rr.Read())
            {
                CMSNode cnode = new CMSNode(rr.GetInt("nodeid"));

                if (cnode != null)
                    root.AppendChild(cnode.ToXml(xmlDoc, true));
            }
            rr.Close();

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
            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id WHERE (cmsTags.tag IN (" + GetSqlStringArray(tags) + "))";
            IRecordsReader rr = SqlHelper.ExecuteReader(sql);

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("root");
            while (rr.Read())
            {
                CMSNode cnode = new CMSNode(rr.GetInt("nodeid"));

                if (cnode != null)
                    root.AppendChild(cnode.ToXml(xmlDoc, true));
            }
            rr.Close();

            xmlDoc.AppendChild(root);
            return xmlDoc.CreateNavigator().Select(".");
        }

        /// <summary>
        /// Converts the array to an SQL escaped array.
        /// </summary>
        /// <param name="commaSeparatedArray">An array of comma separated values.</param>
        /// <returns>SQL escaped array values.</returns>
        private static string GetSqlStringArray(string commaSeparatedArray)
        {
            // create array
            string[] array = commaSeparatedArray.Trim().Split(',');

            // build SQL array
            StringBuilder sqlArray = new StringBuilder();
            foreach (string item in array)
            {
                string trimmedItem = item.Trim();
                if (trimmedItem.Length > 0)
                {
                    sqlArray.Append("'").Append(SqlHelper.EscapeString(trimmedItem)).Append("',");
                }
            }

            // remove last comma
            if (sqlArray.Length > 0)
                sqlArray.Remove(sqlArray.Length - 1, 1);
            return sqlArray.ToString();
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
            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                            INNER JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                            Inner JOIN cmsContentXml ON cmsContentXml.nodeid = cmsTagRelationShip.nodeId
                            GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            IRecordsReader rr = SqlHelper.ExecuteReader(sql);

            while (rr.Read())
            {
                xmlVal += "<tag id=\"" + rr.GetInt("id").ToString() + "\" group=\"" + rr.GetString("group") + "\" nodesTagged=\"" + rr.GetInt("nodeCount").ToString() + "\">" + rr.GetString("tag") + "</tag>\n";
            }
            xmlVal += "</tags>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlVal);

            rr.Close();


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
            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                            INNER JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                            INNER JOIN cmsContentXml ON cmsContentXml.nodeid = cmsTagRelationShip.nodeId
                            WHERE cmsTags.[group] = @group
                            GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            IRecordsReader rr = SqlHelper.ExecuteReader(sql, SqlHelper.CreateParameter("@group", group));

            while (rr.Read())
            {
                xmlVal += "<tag id=\"" + rr.GetInt("id").ToString() + "\" group=\"" + rr.GetString("group") + "\" nodesTagged=\"" + rr.GetInt("nodeCount").ToString() + "\">" + rr.GetString("tag") + "</tag>\n";
            }
            xmlVal += "</tags>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlVal);

            rr.Close();


            return doc.CreateNavigator().Select(".");
        }

        /// <summary>
        /// Gets the tags from node as ITag objects.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <returns></returns>
        public static List<umbraco.interfaces.ITag> GetTagsFromNodeAsITags(int nodeId)
        {

            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                        INNER JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                        WHERE cmsTagRelationShip.nodeid = @nodeId
                        GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            List<umbraco.interfaces.ITag> tags = convertSqlToTags(sql, SqlHelper.CreateParameter("@nodeId", nodeId));

            return tags;
        }

        /// <summary>
        /// Gets the tags from group as ITag objects.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public static List<umbraco.interfaces.ITag> GetTagsFromGroupAsITags(string group)
        {

            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                            INNER JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                            INNER JOIN cmsContentXml ON cmsContentXml.nodeid = cmsTagRelationShip.nodeId
                            WHERE cmsTags.[group] = @group
                            GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            List<umbraco.interfaces.ITag> tags = convertSqlToTags(sql, SqlHelper.CreateParameter("@group", group));

            return tags;
        }

        /// <summary>
        /// Gets all the tags as ITag objects
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <returns></returns>
        public static List<umbraco.interfaces.ITag> GetTagsAsITags()
        {

            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                            INNER JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                            Inner JOIN cmsContentXml ON cmsContentXml.nodeid = cmsTagRelationShip.nodeId
                            GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            List<umbraco.interfaces.ITag> tags = convertSqlToTags(sql, SqlHelper.CreateParameter("@nodeId", "0"));

            return tags;
        }

        private static List<umbraco.interfaces.ITag> convertSqlToTags(string sql, IParameter param)
        {
            List<umbraco.interfaces.ITag> tags = new List<umbraco.interfaces.ITag>();
            IRecordsReader rr = SqlHelper.ExecuteReader(sql, param);

            while (rr.Read())
            {
                tags.Add(new Tag(
                    rr.GetInt("id"),
                    rr.GetString("tag"),
                    rr.GetString("group")));
            }

            rr.Close();
            return tags;
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
            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                        INNER JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                        WHERE cmsTagRelationShip.nodeid = @nodeId
                        GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            IRecordsReader rr = SqlHelper.ExecuteReader(sql, SqlHelper.CreateParameter("@nodeId", nodeId));

            while (rr.Read())
            {
                xmlVal += "<tag id=\"" + rr.GetInt("id").ToString() + "\" group=\"" + rr.GetString("group") + "\" nodesTagged=\"" + rr.GetInt("nodeCount").ToString() + "\">" + rr.GetString("tag") + "</tag>\n";
            }
            xmlVal += "</tags>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlVal);

            rr.Close();

            return doc.CreateNavigator().Select(".");
        }
    }

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
