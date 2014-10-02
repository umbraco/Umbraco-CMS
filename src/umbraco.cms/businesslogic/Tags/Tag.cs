using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using umbraco.interfaces;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;

namespace umbraco.cms.businesslogic.Tags
{
    [Obsolete("Use the TagService to query tags or the UmbracoHelper on the front-end and use the SetTags, RemoveTags extension methods on IContentBase to manipulate tags")]
    public class Tag : ITag
    {

        private const string SqlGetTagPropertyIdsForNode = @"SELECT cmsPropertyType.id FROM cmsPropertyType 
                INNER JOIN cmsContent ON cmsContent.contentType = cmsPropertyType.contentTypeId
                INNER JOIN cmsDataType ON cmsDataType.nodeId = cmsPropertyType.dataTypeId
                WHERE cmsContent.nodeId = {0}
                AND cmsDataType.propertyEditorAlias = '" + Constants.PropertyEditors.TagsAlias + "'";

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
            //need to check if there's a tag property type to associate with the node, this is a new change with the db change
            var tagProperties = ApplicationContext.Current.DatabaseContext.Database.Fetch<int>(
                string.Format(SqlGetTagPropertyIdsForNode, nodeId)).ToArray();
            if (tagProperties.Any() == false)
            {
                throw new InvalidOperationException("Cannot associate a tag to a node that doesn't have a Property Type assigned to a Tag Property Editor");
            }
            if (tagProperties.Length > 1)
            {
                LogHelper.Warn<Tag>("Because the legacy Tag data layer is being used, the tags being associated with the node will only be associated with the first found Property Type containing a Tag Property Editor");
            }

            if (ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND tagId=@tagId AND propertyTypeId=@propertyTypeId",
                new {tagId = tagId, nodeId = nodeId, propertyTypeId = tagProperties[0]}) == 0)
            {
                ApplicationContext.Current.DatabaseContext.Database.Insert(new TagRelationshipDto
                {
                    NodeId = nodeId,
                    TagId = tagId,
                    PropertyTypeId = tagProperties[0]
                });
            }
                        
        }

        public static void AddTagsToNode(int nodeId, string tags, string group)
        {
            //need to check if there's a tag property type to associate with the node, this is a new change with the db change
            var tagProperties = ApplicationContext.Current.DatabaseContext.Database.Fetch<int>(
                string.Format(SqlGetTagPropertyIdsForNode, nodeId)).ToArray();
            if (tagProperties.Any() == false)
            {
                throw new InvalidOperationException("Cannot associate a tag to a node that doesn't have a Property Type assigned to a Tag Property Editor");
            }
            if (tagProperties.Length > 1)
            {
                LogHelper.Warn<Tag>("Because the legacy Tag data layer is being used, the tags being associated with the node will only be associated with the first found Property Type containing a Tag Property Editor");
            }

            string[] allTags = tags.Split(",".ToCharArray());
            for (int i = 0; i < allTags.Length; i++)
            {
                //if not found we'll get zero and handle that onsave instead...
                int id = GetTagId(allTags[i], group);
                if (id == 0)
                    id = AddTag(allTags[i], group);

                //Perform a subselect insert into cmsTagRelationship using a left outer join to perform the if not exists check
                var sql = "insert into cmsTagRelationship (nodeId,tagId,propertyTypeId) select " + string.Format("{0}", nodeId) + ", " + string.Format("{0}", id) + ", " + string.Format("{0}", tagProperties[0]) + " from cmsTags ";
                //sorry, gotta do this, @params not supported in join clause
                sql += "left outer join cmsTagRelationship on (cmsTags.id = cmsTagRelationship.TagId and cmsTagRelationship.nodeId = " + string.Format("{0}", nodeId) + ") ";
                sql += "where cmsTagRelationship.tagId is null and cmsTags.id = " + string.Format("{0}", id);

                SqlHelper.ExecuteNonQuery(sql);


            }

        }

        public static void MergeTagsToNode(int nodeId, string tags, string group)
        {
            //need to check if there's a tag property type to associate with the node, this is a new change with the db change
            var tagProperties = ApplicationContext.Current.DatabaseContext.Database.Fetch<int>(
                string.Format(SqlGetTagPropertyIdsForNode, nodeId)).ToArray();
            if (tagProperties.Any() == false)
            {
                throw new InvalidOperationException("Cannot associate a tag to a node that doesn't have a Property Type assigned to a Tag Property Editor");
            }
            if (tagProperties.Length > 1)
            {
                LogHelper.Warn<Tag>("Because the legacy Tag data layer is being used, the tags being associated with the node will only be associated with the first found Property Type containing a Tag Property Editor");
            }


            //GE 2011-11-01
            //When you have a new CSV list of tags (e.g. from control) and want to push those to the DB, the only way to do this
            //is delete all the existing tags and add the new ones. 
            //On a lot of tags, or a very full cmsTagRelationship table, this will perform too slowly

            string sql = null;
            string TagSet = GetSqlSet(tags, group);

            //deletes any tags not found in csv
            sql = "delete from cmsTagRelationship where nodeId = " + string.Format("{0}", nodeId) + " ";
            sql += " and tagId in ( ";
            sql += "     select cmsTagRelationship.tagId ";
            sql += " from ";
            sql += " cmsTagRelationship ";
            sql += " left outer join ";
            sql += " (";
            sql += " select NewTags.Id from ";
            sql += " " + TagSet + " ";
            sql += " inner join cmsTags as NewTags on (TagSet.Tag = NewTags.Tag and TagSet." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group") + " = TagSet." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group") + ") ";
            sql += " ) as NewTagsSet ";
            sql += " on (cmsTagRelationship.TagId = NewTagsSet.Id and cmsTagRelationship.NodeId = " + string.Format("{0}", nodeId) + ") ";
            sql += " inner join  cmsTags as OldTags on (cmsTagRelationship.tagId = OldTags.Id) ";
            sql += " where NewTagsSet.Id is null ";
            sql += " )";
            SqlHelper.ExecuteNonQuery(sql);

            //adds any tags found in csv that aren't in cmsTag for that group
            sql = "insert into cmsTags (Tag," + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group") + @") ";
            sql += " select TagSet." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Tag") + @", TagSet." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group") + @" from ";
            sql += " " + TagSet + " ";
            sql += " left outer join cmsTags on (TagSet.Tag = cmsTags.Tag and TagSet." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group") + " = cmsTags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group") + ")";
            sql += " where cmsTags.Id is null ";
            SqlHelper.ExecuteNonQuery(sql);

            //adds any tags found in csv that aren't in tagrelationships
            sql = "insert into cmsTagRelationship (TagId,NodeId,propertyTypeId) ";
            sql += "select NewTagsSet.Id, " + string.Format("{0}", nodeId) + ", " + string.Format("{0}", tagProperties[0]) + " from  ";
            sql += "( ";
            sql += "select NewTags.Id from  ";
            sql += " " + TagSet + " ";
            sql += "inner join cmsTags as NewTags on (TagSet.Tag = NewTags.Tag and TagSet." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group") + " = TagSet." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group") + ") ";
            sql += ") as NewTagsSet ";
            sql += "left outer join cmsTagRelationship ";
            sql += "on (cmsTagRelationship.TagId = NewTagsSet.Id and cmsTagRelationship.NodeId = " + string.Format("{0}", nodeId) + ") ";
            sql += "where cmsTagRelationship.tagId is null ";
            SqlHelper.ExecuteNonQuery(sql);

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
            SqlHelper.ExecuteNonQuery("DELETE FROM cmsTagRelationship WHERE (nodeId = @nodeId) AND EXISTS (SELECT id FROM cmsTags WHERE (cmsTagRelationship.tagId = id) AND (" + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group") + " = @group));",
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
            SqlHelper.ExecuteNonQuery("INSERT INTO cmsTags(tag," + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group") + ") VALUES (@tag,@group)",
                SqlHelper.CreateParameter("@tag", tag.Trim()),
                SqlHelper.CreateParameter("@group", group));
            return GetTagId(tag, group);
        }

        public static int GetTagId(string tag, string group)
        {
            int retval = 0;
            string sql = "SELECT id FROM cmsTags where tag=@tag AND " + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group") + "=@group;";
            object result = SqlHelper.ExecuteScalar<object>(sql,
                SqlHelper.CreateParameter("@tag", tag),
                SqlHelper.CreateParameter("@group", group));

            if (result != null)
                retval = int.Parse(result.ToString());

            return retval;
        }

        public static IEnumerable<Tag> GetTags(int nodeId, string group)
        {
            var sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group") + @", count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                  INNER JOIN cmsTagRelationship ON cmsTagRelationShip.tagId = cmsTags.id
                  WHERE cmsTags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group") + @" = @group AND cmsTagRelationship.nodeid = @nodeid
                  GROUP BY cmsTags.id, cmsTags.tag, cmsTags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group");

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

            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group") + @", count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                        INNER JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                        WHERE cmsTagRelationShip.nodeid = @nodeId
                        GROUP BY cmsTags.id, cmsTags.tag, cmsTags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group");

            return ConvertSqlToTags(sql, SqlHelper.CreateParameter("@nodeId", nodeId));

        }

        /// <summary>
        /// Gets the tags from group as ITag objects.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public static IEnumerable<Tag> GetTags(string group)
        {

            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group") + @", count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                            INNER JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                            WHERE cmsTags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group") + @" = @group
                            GROUP BY cmsTags.id, cmsTags.tag, cmsTags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group");

            return ConvertSqlToTags(sql, SqlHelper.CreateParameter("@group", group));

        }

        /// <summary>
        /// Gets all the tags as ITag objects
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <returns></returns>
        public static IEnumerable<Tag> GetTags()
        {

            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group") + @", count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                            LEFT JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                            GROUP BY cmsTags.id, cmsTags.tag, cmsTags." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("group");

            return ConvertSqlToTags(sql);

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
        private static string GetSqlSet(string commaSeparatedArray, string group)
        {
            // create array
            var array = commaSeparatedArray.Trim().Split(',').ToList().ConvertAll(tag => string.Format("select '{0}' as Tag, '{1}' as " + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group"), tag.Replace("'", ""), group)).ToArray();
            return "(" + string.Join(" union ", array).Replace("  ", " ") + ") as TagSet";
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
