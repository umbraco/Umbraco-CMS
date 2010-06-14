using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using umbraco.interfaces;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;

namespace umbraco.cms.businesslogic.Tags
{
    public class Tag : ITag
    {

        #region Constructors
        public Tag() { }
        public Tag(int id, string tag, string group, int nodeCount)
        {
            Id = id;
            TagCaption = tag;
            Group = group;
            NodeCount = nodeCount;
        } 
        #endregion

        #region Public Properties
        public int NodeCount { get; private set; }

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
        #endregion


        public static void AssociateTagToNode(int nodeId, int tagId)
        {
            SqlHelper.ExecuteNonQuery("INSERT INTO cmsTagRelationShip(nodeId,tagId) VALUES (@nodeId, @tagId)",
                            SqlHelper.CreateParameter("@nodeId", nodeId),
                            SqlHelper.CreateParameter("@tagId", tagId)
                        );
        }

        public static void AddTagsToNode(int nodeId, string tags, string group)
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

        /// <summary>
        /// Removes a tag from the database, this will also remove all relations
        /// </summary>
        /// <param name="tagId"></param>
        public static void RemoveTag(int tagId)
        {
            SqlHelper.ExecuteNonQuery("DELETE FROM cmsTagRelationship WHERE (tagid = @tagId)",
               SqlHelper.CreateParameter("@tagId", tagId));
            SqlHelper.ExecuteNonQuery("DELETE FROM cmsTags WHERE (id = @tagId)",
               SqlHelper.CreateParameter("@tagId", tagId));
        }

        /// <summary>
        /// Delete all tag associations for the node specified
        /// </summary>
        /// <param name="nodeId"></param>
        public static void RemoveTagsFromNode(int nodeId)
        {
            SqlHelper.ExecuteNonQuery("DELETE FROM cmsTagRelationship WHERE nodeId = @nodeId",
              SqlHelper.CreateParameter("@nodeId", nodeId));
        }

        /// <summary>
        /// Delete all tag associations for the node & group specified
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="group"></param>
        public static void RemoveTagsFromNode(int nodeId, string group)
        {
            SqlHelper.ExecuteNonQuery("DELETE FROM cmsTagRelationship WHERE (nodeId = @nodeId) AND EXISTS (SELECT id FROM cmsTags WHERE (cmsTagRelationship.tagId = id) AND ([group] = @group));",
               SqlHelper.CreateParameter("@nodeId", nodeId),
               SqlHelper.CreateParameter("@group", group));
        }

        /// <summary>
        /// Remove a single tag association
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="tag"></param>
        /// <param name="group"></param>
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

        public static IEnumerable<Tag> GetTags(int nodeId, string group)
        {
            var sql = @"SELECT * FROM cmsTags
                  INNER JOIN cmsTagRelationship ON cmsTagRelationShip.tagId = cmsTags.id
                  WHERE cmsTags.[group] = @group AND cmsTagRelationship.nodeid = @nodeid";

            return ConvertSqlToTags(sql, 
                SqlHelper.CreateParameter("@group", group),
                SqlHelper.CreateParameter("@nodeid", nodeId));

        }

        /// <summary>
        /// Gets the tags from node as ITag objects.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <returns></returns>
        public static IEnumerable<Tag> GetTags(int nodeId)
        {

            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                        INNER JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                        WHERE cmsTagRelationShip.nodeid = @nodeId
                        GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            return ConvertSqlToTags(sql, SqlHelper.CreateParameter("@nodeId", nodeId));

        }

        /// <summary>
        /// Gets the tags from group as ITag objects.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public static IEnumerable<Tag> GetTags(string group)
        {

            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                            INNER JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                            INNER JOIN cmsContentXml ON cmsContentXml.nodeid = cmsTagRelationShip.nodeId
                            WHERE cmsTags.[group] = @group
                            GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            return ConvertSqlToTags(sql, SqlHelper.CreateParameter("@group", group));

        }

        /// <summary>
        /// Gets all the tags as ITag objects
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <returns></returns>
        public static IEnumerable<Tag> GetTags()
        {

            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                            INNER JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                            Inner JOIN cmsContentXml ON cmsContentXml.nodeid = cmsTagRelationShip.nodeId
                            GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            return ConvertSqlToTags(sql, SqlHelper.CreateParameter("@nodeId", "0"));

        }

        public static IEnumerable<Document> GetDocumentsWithTags(string tags)
        {

            var docs = new List<Document>();
            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id 
                            INNER JOIN umbracoNode ON cmsTagRelationShip.nodeId = umbracoNode.id
                            WHERE (cmsTags.tag IN ({0})) AND nodeObjectType=@nodeType";

            using (IRecordsReader rr = SqlHelper.ExecuteReader(string.Format(sql, GetSqlStringArray(tags)),
                SqlHelper.CreateParameter("@nodeType", Document._objectType)))
            {
                while (rr.Read())
                {
                    Document cnode = new Document(rr.GetInt("nodeid"));

                    if (cnode != null && cnode.Published)
                        docs.Add(cnode);
                }
            }

            return docs;
        }

        public static IEnumerable<CMSNode> GetNodesWithTags(string tags)
        {
            var nodes = new List<CMSNode>();
            
            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id WHERE (cmsTags.tag IN (" + GetSqlStringArray(tags) + "))";
            using (IRecordsReader rr = SqlHelper.ExecuteReader(sql))
            {
                while (rr.Read())
                {
                    nodes.Add(new CMSNode(rr.GetInt("nodeid")));
                }
            }
            return nodes;
        }

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

        private static IEnumerable<Tag> ConvertSqlToTags(string sql, params IParameter[] param)
        {
            List<Tag> tags = new List<Tag>();
            using (IRecordsReader rr = SqlHelper.ExecuteReader(sql, param))
            {
                while (rr.Read())
                {
                    tags.Add(new Tag(
                        rr.GetInt("id"),
                        rr.GetString("tag"),
                        rr.GetString("group"),
                        rr.GetInt("nodeCount")));
                }
            }

            
            return tags;
        }

        private static ISqlHelper SqlHelper
        {
            get
            {
                return Application.SqlHelper;
            }
        }

    }
}
