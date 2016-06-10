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
                SqlHelper.ExecuteNonQuery(
                    @"UPDATE 
                        cmsPropertyTypeGroup 
                    SET 
                        contenttypeNodeId = @contentTypeId,
                        sortOrder = @sortOrder,                        
                        text = @name
                    WHERE
                        id = @id
                ",
                    SqlHelper.CreateParameter("@id", Id),
                    SqlHelper.CreateParameter("@contentTypeId", ContentTypeId),
                    SqlHelper.CreateParameter("@sortOrder", SortOrder),
                    SqlHelper.CreateParameter("@name", Name)
                    );
            }
            else
            {
                if (SortOrder == -1)
                    SortOrder = SqlHelper.ExecuteScalar<int>("select count(*) from cmsPropertyTypeGroup where contenttypeNodeId = @nodeId",
                        SqlHelper.CreateParameter("@nodeId", ContentTypeId)) + 1;

                SqlHelper.ExecuteNonQuery(
                    @"
                    INSERT INTO 
                        cmsPropertyTypeGroup
                        (contenttypeNodeId, sortOrder, text)
                    VALUES 
                        (@contentTypeId, @sortOrder, @name)
                ",
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
            using (var dr = SqlHelper.ExecuteReader(@" SELECT contenttypeNodeId, sortOrder, text FROM cmsPropertyTypeGroup WHERE id = @id", SqlHelper.CreateParameter("@id", Id)))
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
            using (var dr = SqlHelper.ExecuteReader(@" SELECT id FROM cmsPropertyTypeGroup WHERE contenttypeNodeId = @contentTypeId", SqlHelper.CreateParameter("@contentTypeId", contentTypeId)))
            {
                while (dr.Read())
                {
                    ptgs.Add(GetPropertyTypeGroup(dr.GetInt("id")));
                }
            }

            return ptgs;
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