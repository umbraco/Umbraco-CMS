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
        public int ParentId { get; set; }
        public int ContentTypeId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }

        public PropertyTypeGroup()
        {
        }

        public PropertyTypeGroup(int parentId, int contentTypeId, string name, int sortOrder)
        {
            ParentId = parentId;
            ContentTypeId = contentTypeId;
            Name = name;
            SortOrder = sortOrder;
        }

        public PropertyTypeGroup(int parentId, int contentTypeId, string name)
        {
            ParentId = parentId;
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

        //TODO: Verify support for master doctypes / mixins!
        public IEnumerable<PropertyTypeGroup> GetPropertyTypeGroups()
        {
            var ptgs = new List<PropertyTypeGroup>();
            using (var dr = SqlHelper.ExecuteReader(@"SELECT id FROM cmsPropertyTypeGroup WHERE parentGroupId = @id", SqlHelper.CreateParameter("@id", Id)))
            {
                while (dr.Read())
                {
                    ptgs.Add(GetPropertyTypeGroup(dr.GetInt("id")));
                }
            }

            return ptgs;
        }

        public void Save()
        {
            if (Id != 0)
            {
                SqlHelper.ExecuteNonQuery(
                    @"UPDATE 
                        cmsPropertyTypeGroup 
                    SET 
                        parentGroupId = @parentGroupId,
                        contenttypeNodeId = @contentTypeId,
                        sortOrder = @sortOrder,                        
                        text = @name
                    WHERE
                        id = @id
                ",
                    SqlHelper.CreateParameter("@id", Id),
                    SqlHelper.CreateParameter("@parentGroupId", ConvertParentId(ParentId)),
                    SqlHelper.CreateParameter("@contentTypeId", ContentTypeId),
                    SqlHelper.CreateParameter("@sortOrder", SortOrder),
                    SqlHelper.CreateParameter("@name", Name)
                    );
            }
            else
            {
                if (SortOrder == -1)
                    SortOrder = SqlHelper.ExecuteScalar<int>("select count(*) from cmsPropertyTypeGroup where COALESCE(parentGroupId, 0) = 0 and contenttypeNodeId = @nodeId",
                        SqlHelper.CreateParameter("@nodeId", ContentTypeId)) + 1;

                SqlHelper.ExecuteNonQuery(
                    @"
                    INSERT INTO 
                        cmsPropertyTypeGroup
                        (parentGroupId, contenttypeNodeId, sortOrder, text)
                    VALUES 
                        (@parentGroupId, @contentTypeId, @sortOrder, @name)
                ",
                    SqlHelper.CreateParameter("@parentGroupId", ConvertParentId(ParentId)),
                    SqlHelper.CreateParameter("@contentTypeId", ContentTypeId),
                    SqlHelper.CreateParameter("@sortOrder", SortOrder),
                    SqlHelper.CreateParameter("@name", Name)
                    );
                Id = SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM [cmsPropertyTypeGroup]");

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

            SqlHelper.ExecuteNonQuery("DELETE FROM cmsPropertyTypeGroup WHERE id = @id", SqlHelper.CreateParameter("@id", Id));
        }

        internal void Load()
        {
            using (var dr = SqlHelper.ExecuteReader(@" SELECT parentGroupId, contenttypeNodeId, sortOrder, text FROM cmsPropertyTypeGroup WHERE id = @id", SqlHelper.CreateParameter("@id", Id)))
            {
                if (dr.Read())
                {
                    // if no parent, the value should just be null. The GetInt helper method returns -1 if value is null so we need to check
                    ParentId = dr.GetInt("parentGroupId") != -1 ? dr.GetInt("parentGroupId") : 0;
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
            using (var dr = SqlHelper.ExecuteReader(@" SELECT id FROM cmsPropertyTypeGroup WHERE contenttypeNodeId = @contentTypeId", SqlHelper.CreateParameter("@contentTypeId", contentTypeId)))
            {
                while (dr.Read())
                {
                    ptgs.Add(GetPropertyTypeGroup(dr.GetInt("id")));
                }
            }

            return ptgs;
        }

        private object ConvertParentId(int parentId)
        {
            if (parentId == 0)
                return DBNull.Value;

            return parentId;
        }
        
        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }
    }
}