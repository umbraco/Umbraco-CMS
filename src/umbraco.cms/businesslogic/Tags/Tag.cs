using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using umbraco.interfaces;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using System.Runtime.CompilerServices;

namespace umbraco.cms.businesslogic.Tags
{
    public class Tag : ITag
    {
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        internal static UmbracoDatabase Database
        {
            get { return ApplicationContext.Current.DatabaseContext.Database; }
        }
        
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
            ApplicationContext.Current.DatabaseContext.Database.Insert(
                new TagRelationshipDto() { NodeId = nodeId, TagId = tagId });
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

                //Perform a subselect insert into cmsTagRelationship using a left outer join to perform the if not exists check
                string sql = "insert into cmsTagRelationship (nodeId,tagId) select " + string.Format("{0}", nodeId) + ", " + string.Format("{0}", id) + " from cmsTags ";
                //sorry, gotta do this, @params not supported in join clause
                sql += "left outer join cmsTagRelationship on (cmsTags.id = cmsTagRelationship.TagId and cmsTagRelationship.nodeId = " + string.Format("{0}", nodeId) + ") ";
                sql += "where cmsTagRelationship.tagId is null and cmsTags.id = " + string.Format("{0}", id);

                ApplicationContext.Current.DatabaseContext.Database.Execute(sql); 
            }

        }

        public static void MergeTagsToNode(int nodeId, string tags, string group)
        {
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
            sql += " inner join cmsTags as NewTags on (TagSet.Tag = NewTags.Tag and TagSet.[Group] = TagSet.[Group]) ";
            sql += " ) as NewTagsSet ";
            sql += " on (cmsTagRelationship.TagId = NewTagsSet.Id and cmsTagRelationship.NodeId = " + string.Format("{0}", nodeId) + ") ";
            sql += " inner join  cmsTags as OldTags on (cmsTagRelationship.tagId = OldTags.Id) ";
            sql += " where NewTagsSet.Id is null ";
            sql += " )";
            ApplicationContext.Current.DatabaseContext.Database.Execute(sql); 

            //adds any tags found in csv that aren't in cmsTag for that group
            sql = "insert into cmsTags (Tag,[Group]) ";
            sql += " select TagSet.[Tag], TagSet.[Group] from ";
            sql += " " + TagSet + " ";
            sql += " left outer join cmsTags on (TagSet.Tag = cmsTags.Tag and TagSet.[Group] = cmsTags.[Group])";
            sql += " where cmsTags.Id is null ";
            ApplicationContext.Current.DatabaseContext.Database.Execute(sql); 

            //adds any tags found in csv that aren't in tagrelationships
            sql = "insert into cmsTagRelationship (TagId,NodeId) ";
            sql += "select NewTagsSet.Id, " + string.Format("{0}", nodeId) + " from  ";
            sql += "( ";
            sql += "select NewTags.Id from  ";
            sql += " " + TagSet + " ";
            sql += "inner join cmsTags as NewTags on (TagSet.Tag = NewTags.Tag and TagSet.[Group] = TagSet.[Group]) ";
            sql += ") as NewTagsSet ";
            sql += "left outer join cmsTagRelationship ";
            sql += "on (cmsTagRelationship.TagId = NewTagsSet.Id and cmsTagRelationship.NodeId = " + string.Format("{0}", nodeId) + ") ";
            sql += "where cmsTagRelationship.tagId is null ";
            ApplicationContext.Current.DatabaseContext.Database.Execute(sql); 

        }

        /// <summary>
        /// Removes a tag from the database, this will also remove all relations
        /// </summary>
        /// <param name="tagId"></param>
        public static void RemoveTag(int tagId)
        {
            ApplicationContext.Current.DatabaseContext.Database.Delete<TagRelationshipDto>("WHERE tagid = @0", tagId);
            ApplicationContext.Current.DatabaseContext.Database.Delete<TagDto>("WHERE id = @0", tagId);   
        }

        /// <summary>
        /// Delete all tag associations for the node specified
        /// </summary>
        /// <param name="nodeId"></param>
        public static void RemoveTagsFromNode(int nodeId)
        {
            ApplicationContext.Current.DatabaseContext.Database.Delete<TagRelationshipDto>("WHERE nodeId = @0", nodeId);
        }

        /// <summary>
        /// Delete all tag associations for the node & group specified
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="group"></param>
        public static void RemoveTagsFromNode(int nodeId, string group)
        {
            ApplicationContext.Current.DatabaseContext.Database.Execute(
                "DELETE FROM cmsTagRelationship WHERE (nodeId = @0) AND EXISTS (SELECT id FROM cmsTags WHERE (cmsTagRelationship.tagId = id) AND ([group] = @1)",
                   nodeId, group); 
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
                ApplicationContext.Current.DatabaseContext.Database.Delete<TagRelationshipDto>("WHERE (nodeId = @0 and tagId = @1)", nodeId, tagId);
            }
        }

        /// <summary>
        /// Adds new Tag
        /// </summary>
        /// <param name="tag">tag name</param>
        /// <param name="group">group name</param>
        /// <returns>Tag.Id</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int AddTag(string tag, string group)
        {
            Database.Execute("insert into [cmsTags] ([Tag], [Group]) values (@0, @1)", tag, group);
            object id = Database.ExecuteScalar<object>("select Max(id) from cmsTags");
            if (id == null) throw new ArgumentNullException(string.Format("Tag addition failed, Tag = '{0}', GroupName = '{1}'", tag, group));
            return (int)id;
        }

        public static int GetTagId(string tag, string group)
        {
            var tagDto = Database.Fetch<TagDto>("where tag=@0 AND [group]=@1", tag, group);
            if (tagDto == null) return 0;
            return tagDto[0].Id;  
        }

        public static IEnumerable<Tag> GetTags(int nodeId, string group)
        {
            var sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                  INNER JOIN cmsTagRelationship ON cmsTagRelationShip.tagId = cmsTags.id
                  WHERE cmsTags.[group] = @0 AND cmsTagRelationship.nodeid = @1
                  GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            return ConvertSqlToTags(sql, group, nodeId);

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
                        WHERE cmsTagRelationShip.nodeid = @0
                        GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            return ConvertSqlToTags(sql, nodeId);

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
                            WHERE cmsTags.[group] = @0
                            GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            return ConvertSqlToTags(sql, group);

        }

        /// <summary>
        /// Gets all the tags as ITag objects
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <returns></returns>
        public static IEnumerable<Tag> GetTags()
        {

            string sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                            LEFT JOIN cmsTagRelationShip ON cmsTagRelationShip.tagid = cmsTags.id
                            GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";

            return ConvertSqlToTags(sql);

        }

        public static IEnumerable<Document> GetDocumentsWithTags(string tags)
        {

            var docs = new List<Document>();
            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id 
                            INNER JOIN umbracoNode ON cmsTagRelationShip.nodeId = umbracoNode.id
                            WHERE (cmsTags.tag IN ({0})) AND nodeObjectType=@nodeType";
            foreach (var id in ApplicationContext.Current.DatabaseContext.Database.Query<int>(string.Format(sql, GetSqlStringArray(tags),
                                                   new { nodeType = Document._objectType})))
            {
                Document cnode = new Document(id);

                if (cnode != null && cnode.Published)
                     yield return cnode; 
            }
        }

        public static IEnumerable<CMSNode> GetNodesWithTags(string tags)
        {
            var nodes = new List<CMSNode>();

            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id WHERE (cmsTags.tag IN (" + GetSqlStringArray(tags) + "))";
            foreach (var id in ApplicationContext.Current.DatabaseContext.Database.Query<int>(sql))
                yield return new CMSNode(id);         
        }

        private static string GetSqlSet(string commaSeparatedArray, string group)
        {
            // create array
            var array = commaSeparatedArray.Trim().Split(',').ToList().ConvertAll(tag => string.Format("select '{0}' as Tag, '{1}' as [Group]", tag.Replace("'", ""), group)).ToArray();
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
                    sqlArray.Append("'").Append(escapeString(trimmedItem)).Append("',");
                }
            }

            // remove last comma
            if (sqlArray.Length > 0)
                sqlArray.Remove(sqlArray.Length - 1, 1);
            return sqlArray.ToString();
        }
        internal class TagDtoExt : TagDto
        {
            public int NodeCount { get; set; }
        }

        //private static IEnumerable<Tag> ConvertSqlToTags(string sql, params IParameter[] param)
        private static IEnumerable<Tag> ConvertSqlToTags(string sql, params object[] param)
        {
            foreach (var tagDto in ApplicationContext.Current.DatabaseContext.Database.Query<TagDtoExt>(sql, param))
            {
                yield return
                    new Tag(tagDto.Id, tagDto.Tag, tagDto.Group, tagDto.NodeCount);      
            }

        }

        private static string escapeString(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("'", "''");
        }


    }
}
