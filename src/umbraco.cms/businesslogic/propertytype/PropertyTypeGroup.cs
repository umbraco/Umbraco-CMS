using System;
using System.Collections.Generic;
using System.Linq;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;

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
            foreach (var item in Database.Query<PropertyTypeGroupDto>(@"SELECT id FROM cmsPropertyTypeGroup WHERE parentGroupId = @0", Id))
            {
                ptgs.Add(GetPropertyTypeGroup(item.Id));
            }
            return ptgs;
        }

        public void Save()
        {
            if (Id != 0)
            {
                Database.Execute(
                    @"UPDATE 
                        cmsPropertyTypeGroup 
                    SET 
                        parentGroupId = @parentGroupId,
                        contenttypeNodeId = @contentTypeId,
                        sortOrder = @sortOrder,                        
                        text = @name
                    WHERE
                        id = @id
                ", new {id = Id,  parentGroupId = ConvertParentId(ParentId), @contentTypeId = ContentTypeId, sortOrder = SortOrder, name = Name }
                    );
            }
            else
            {
                if (SortOrder == -1)
                    SortOrder = Database.ExecuteScalar<int>("select count(*) from cmsPropertyTypeGroup where COALESCE(parentGroupId, 0) = 0 and contenttypeNodeId = @nodeId",
                                     new {nodeId = ContentTypeId}) + 1;

                Database.Execute(
                    @"
                    INSERT INTO 
                        cmsPropertyTypeGroup
                        (parentGroupId, contenttypeNodeId, sortOrder, text)
                    VALUES 
                        (@parentGroupId, @contentTypeId, @sortOrder, @name)
                ",
                  new { parentGroupId = ConvertParentId(ParentId), contentTypeId = ContentTypeId, sortOrder = SortOrder, name = Name });

                Id = Database.ExecuteScalar<int>("SELECT MAX(id) FROM [cmsPropertyTypeGroup]");

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

            Database.Execute("DELETE FROM cmsPropertyTypeGroup WHERE id = @id", new { id = Id} );
        }

        internal void Load()
        {
            var dto = Database.FirstOrDefault<PropertyTypeGroupDto>(@" SELECT parentGroupId, contenttypeNodeId, sortOrder, text FROM cmsPropertyTypeGroup WHERE id = @id",
                   new { id = Id });
            if (dto != null)
            {
                ParentId = dto.ParentGroupId != null && dto.ParentGroupId != -1 ? (int)dto.ParentGroupId : 0;
                SortOrder = dto.SortOrder;
                ContentTypeId = dto.ContentTypeNodeId;
                Name = dto.Text;
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

            var dto = Database.Query<PropertyTypeGroupDto>(@" SELECT id FROM cmsPropertyTypeGroup WHERE contenttypeNodeId = @contentTypeId",
                   new { contentTypeId = contentTypeId });
            foreach (var item in dto)
            {
                ptgs.Add(GetPropertyTypeGroup(item.Id));
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
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        internal static UmbracoDatabase Database
        {
            get { return ApplicationContext.Current.DatabaseContext.Database; }
        }

    }
}