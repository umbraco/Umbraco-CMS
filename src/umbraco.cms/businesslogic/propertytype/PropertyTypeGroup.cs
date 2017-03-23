using System;
using System.Collections.Generic;
using System.Linq;
using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco.cms.businesslogic.propertytype
{
    public class PropertyTypeGroup
    {
        public int Id { get; set; }
        public int ContentTypeId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }

        public PropertyTypeGroup()
        {
        }

        public PropertyTypeGroup(int contentTypeId, string name, int sortOrder)
        {
            ContentTypeId = contentTypeId;
            Name = name;
            SortOrder = sortOrder;
        }

        public PropertyTypeGroup(int contentTypeId, string name)
        {
            ContentTypeId = contentTypeId;
            Name = name;
            SortOrder = -1; // we set this to -1 so in the save method we can get the current highest sortorder in case it's not sat after init (ie. if you want to force a sortOrder)
        }

        public IEnumerable<PropertyType> GetPropertyTypes(List<int> contentTypeIds)
        {
            return PropertyType.GetPropertyTypesByGroup(Id, contentTypeIds);
        }

        public IEnumerable<PropertyType> GetPropertyTypes()
        {
            return PropertyType.GetPropertyTypesByGroup(Id);
        }

        // note: this is used to delete all groups that inherit from a group, when the group is deleted,
        // see the Delete method in this class - but it is all done in an obsolete way which does not
        // take compositions in account + we delete the group but do not re-allocate the properties, etc.
        // ALL THIS should be either removed, or refactored to use the new APIs - so... returning nothing
        // from now on, which is just another way of being broken.
        public IEnumerable<PropertyTypeGroup> GetPropertyTypeGroups()
        {
            return Enumerable.Empty<PropertyTypeGroup>();
        }

        public void Save()
        {
            if (Id != 0)
            {
                using (var sqlHelper = Application.SqlHelper)
                    sqlHelper.ExecuteNonQuery(
                        @"UPDATE 
                            cmsPropertyTypeGroup 
                        SET 
                            contenttypeNodeId = @contentTypeId,
                            sortOrder = @sortOrder,                        
                            text = @name
                        WHERE
                            id = @id
                    ",
                        sqlHelper.CreateParameter("@id", Id),
                        sqlHelper.CreateParameter("@contentTypeId", ContentTypeId),
                        sqlHelper.CreateParameter("@sortOrder", SortOrder),
                        sqlHelper.CreateParameter("@name", Name)
                    );
            }
            else
            {
                if (SortOrder == -1)
                    using (var sqlHelper = Application.SqlHelper)
                        SortOrder = sqlHelper.ExecuteScalar<int>("select count(*) from cmsPropertyTypeGroup where contenttypeNodeId = @nodeId",
                        sqlHelper.CreateParameter("@nodeId", ContentTypeId)) + 1;

                using (var sqlHelper = Application.SqlHelper)
                    sqlHelper.ExecuteNonQuery(
                        @"
                        INSERT INTO 
                            cmsPropertyTypeGroup
                            (contenttypeNodeId, sortOrder, text)
                        VALUES 
                            (@contentTypeId, @sortOrder, @name)
                    ",
                        sqlHelper.CreateParameter("@contentTypeId", ContentTypeId),
                        sqlHelper.CreateParameter("@sortOrder", SortOrder),
                        sqlHelper.CreateParameter("@name", Name)
                    );

                using (var sqlHelper = Application.SqlHelper)
                    Id = sqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM [cmsPropertyTypeGroup]");

            }
        }

        public void Delete()
        {
            // update all PropertyTypes using this group
            foreach (var pt in GetPropertyTypes())
            {
                pt.PropertyTypeGroup = 0;
                pt.Save();
            }

            foreach (var ptg in GetPropertyTypeGroups())
                ptg.Delete();

            using (var sqlHelper = Application.SqlHelper)
                sqlHelper.ExecuteNonQuery("DELETE FROM cmsPropertyTypeGroup WHERE id = @id", sqlHelper.CreateParameter("@id", Id));
        }

        internal void Load()
        {
            using (var sqlHelper = Application.SqlHelper)
            using (var dr = sqlHelper.ExecuteReader(@" SELECT contenttypeNodeId, sortOrder, text FROM cmsPropertyTypeGroup WHERE id = @id", sqlHelper.CreateParameter("@id", Id)))
            {
                if (dr.Read())
                {
                    SortOrder = dr.GetInt("sortOrder");
                    ContentTypeId = dr.GetInt("contenttypeNodeId");
                    Name = dr.GetString("text");
                }
            }
        }

        public static PropertyTypeGroup GetPropertyTypeGroup(int id)
        {
            var ptg = new PropertyTypeGroup { Id = id };
            ptg.Load();
            return ptg;
        }

        public static IEnumerable<PropertyTypeGroup> GetPropertyTypeGroupsFromContentType(int contentTypeId)
        {
            var ptgs = new List<PropertyTypeGroup>();
            using (var sqlHelper = Application.SqlHelper)
            using (var dr = sqlHelper.ExecuteReader(@" SELECT id FROM cmsPropertyTypeGroup WHERE contenttypeNodeId = @contentTypeId", sqlHelper.CreateParameter("@contentTypeId", contentTypeId)))
            {
                while (dr.Read())
                {
                    ptgs.Add(GetPropertyTypeGroup(dr.GetInt("id")));
                }
            }

            return ptgs;
        }

        /// <summary>
        /// Unused, please do not use
        /// </summary>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }
    }
}