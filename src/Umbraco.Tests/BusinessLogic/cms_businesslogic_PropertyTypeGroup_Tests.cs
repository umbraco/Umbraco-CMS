using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.relation;
using Umbraco.Core.Models;
using umbraco.cms.businesslogic.propertytype;
using System.Collections.Generic;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_PropertyTypeGroup_Tests : BaseDatabaseFactoryTestWithContext
    {
        #region EnsureData

        const int TEST_CONTENT_TYPES_QTY = 2;
        private ContentTypeDto _contentType1;
        private ContentTypeDto _contentType2;

        const int TEST_PROPERTY_TYPE_GROUPS_QTY = 3;
        const int TEST_CHILD_PROPERTY_TYPE_GROUPS_QTY = 2;
        private PropertyTypeGroupDto _propertyTypeGroup1;
        private PropertyTypeGroupDto _propertyTypeGroup2;
        private PropertyTypeGroupDto _propertyTypeGroup3;

        const int TEST_PROPERTY_TYPES_QTY = 3;
        const int TEST_CHILD_PROPERTY_TYPES_QTY = 2;
        private PropertyTypeDto _propertyType1;
        private PropertyTypeDto _propertyType2;
        private PropertyTypeDto _propertyType3;

        // cmsDataType records are expected to be in the test database - 
        // otherwise it'd bee too much data to setup for this test suite
        const int TEST_DATA_TYPE_ID1 = -90;
        const int TEST_DATA_TYPE_ID2 = -36;
        const int TEST_DATA_TYPE_ID3 = -41;
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void EnsureData()
        {
            if (!initialized)
            {
                MakeNew_PersistsNewUmbracoNodeRow();

                _contentType1 = insertContentType(1, _node1.Id , "testMedia1");
                _contentType2 = insertContentType(1, _node2.Id, "testMedia2");

                //_propertyTypeGroup1 = insertPropertyTypeGroup(_contentType1.PrimaryKey, (int?)null, "book");
                _propertyTypeGroup1 = insertPropertyTypeGroup(_contentType1.NodeId, (int?)null, "book");
                _propertyTypeGroup2 = insertPropertyTypeGroup(_contentType2.NodeId, (int?)_propertyTypeGroup1.Id, "CD");
                _propertyTypeGroup3 = insertPropertyTypeGroup(_contentType1.NodeId, (int?)_propertyTypeGroup1.Id, "DVD");

                _propertyType1 = insertPropertyType(TEST_DATA_TYPE_ID1, _contentType1.NodeId, _propertyTypeGroup1.Id, "Test Property Type 1");
                _propertyType2 = insertPropertyType(TEST_DATA_TYPE_ID2, _contentType2.NodeId, _propertyTypeGroup2.Id, "Test Property Type 2");
                _propertyType3 = insertPropertyType(TEST_DATA_TYPE_ID3, _contentType2.NodeId, _propertyTypeGroup1.Id, "Test Property Type 3");
            }

            initialized = true;
        }

        private ContentTypeDto getTestContentTypeDto(int id)
        {
            return getPersistedTestDto<ContentTypeDto>(id, idKeyName: "pk" );
        }
        private PropertyTypeGroupDto getTestPropertyTypeGroupDto(int id)
        {
            return getPersistedTestDto<PropertyTypeGroupDto>(id);
        }
        private NodeDto getTestNodeDto(int id)
        {
            return getPersistedTestDto<NodeDto>(id);
        }
        private PropertyTypeDto getTestPropertyTypeDto(int id)
        {
            return getPersistedTestDto<PropertyTypeDto>(id);
        }
        private ContentTypeDto insertContentType(int index, int nodeId, string alias)
        {
            independentDatabase.Execute("insert into [cmsContentType] " +
                  "([nodeId] ,[alias],[icon],[thumbnail],[description],[isContainer],[allowAtRoot]) values " +
                  "(@nodeId , @alias, @icon, @thumbnail, @description, @isContainer, @allowAtRoot)",
                  new { nodeId = nodeId , alias = alias, icon = string.Format("test{0}.gif", index) , 
                        thumbnail = string.Format("test{0}.png", index), 
                        description = string.Format("test{0}", index), 
                        isContainer = false, allowAtRoot = false });
            int id = independentDatabase.ExecuteScalar<int>("select MAX(pk) from [cmsContentType]");
            return getTestContentTypeDto(id);
        }

        private PropertyTypeGroupDto insertPropertyTypeGroup(int contentNodeId, int? parentNodeId, string text)
        {
            independentDatabase.Execute("insert into [cmsPropertyTypeGroup] " +
                       " ([parentGroupId],[contenttypeNodeId],[text],[sortorder]) VALUES " +
                       " (@parentGroupId,@contenttypeNodeId,@text,@sortorder)",
                       new { parentGroupId = parentNodeId, contenttypeNodeId = contentNodeId, text = text, sortorder = 1 });
            int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsPropertyTypeGroup]");
            return getTestPropertyTypeGroupDto(id);
        }

        private PropertyTypeDto insertPropertyType(int dataTypeId, int contentNodeId, int? propertyGroupId, string text)
        {
            independentDatabase.Execute("insert into [cmsPropertyType] " +
                       " (dataTypeId, contentTypeId, propertyTypeGroupId, [Alias], [Name], helpText, sortOrder, " +
                       "  mandatory, validationRegExp, [Description]) VALUES " +
                       " (@dataTypeId, @contentTypeId, @propertyTypeGroupId, @alias, @name, @helpText, @sortOrder, @mandatory, @validationRegExp, @description)",
                       new { dataTypeId = dataTypeId, contentTypeId = contentNodeId, propertyTypeGroupId = propertyGroupId, 
                             alias = text.Replace(" ", "") , name = "text", helpText = "", sortOrder = 0, 
                             mandatory = false, validationRegExp = "", description = "" });
            int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsPropertyType]");
            return getTestPropertyTypeDto(id);
        }

        private void EnsureAllTestRecordsAreDeleted()
        {
            independentDatabase.Execute("delete from [cmsPropertyType] where id = @0", _propertyType3.Id);
            independentDatabase.Execute("delete from [cmsPropertyType] where id = @0", _propertyType2.Id);
            independentDatabase.Execute("delete from [cmsPropertyType] where id = @0", _propertyType1.Id);

            independentDatabase.Execute("delete from [cmsPropertyTypeGroup] where id = @0", _propertyTypeGroup3.Id);
            independentDatabase.Execute("delete from [cmsPropertyTypeGroup] where id = @0", _propertyTypeGroup2.Id);
            independentDatabase.Execute("delete from [cmsPropertyTypeGroup] where id = @0", _propertyTypeGroup1.Id);

            independentDatabase.Execute("delete from [cmsContentType] where pk = @0", _contentType2.PrimaryKey);
            independentDatabase.Execute("delete from [cmsContentType] where pk = @0", _contentType1.PrimaryKey);

            // nodes deletion recursively deletes all relations...
            if (_node1 != null) _node1.delete();
            if (_node2 != null) _node2.delete();
            if (_node3 != null) _node3.delete();
            if (_node4 != null) _node4.delete();
            if (_node5 != null) _node5.delete();

            initialized = false; 
        }

        #endregion

        #region Tests
        [Test(Description = "Test EnsureData()")]
        public void Test_EnsureData()
        {
            Assert.IsTrue(initialized);

            Assert.That(_contentType1, !Is.Null);
            Assert.That(_contentType2, !Is.Null);
            Assert.That(_propertyTypeGroup1, !Is.Null);
            Assert.That(_propertyTypeGroup2, !Is.Null);
            Assert.That(_propertyTypeGroup3, !Is.Null);
            Assert.That(_propertyType1, !Is.Null);
            Assert.That(_propertyType2, !Is.Null);
            Assert.That(_propertyType3, !Is.Null);            
            Assert.That(_node1, !Is.Null);
            Assert.That(_node2, !Is.Null);
            Assert.That(_node3, !Is.Null);
            Assert.That(_node4, !Is.Null);
            Assert.That(_node5, !Is.Null);

            EnsureAllTestRecordsAreDeleted();

            Assert.That(getTestPropertyTypeDto(_propertyType1.Id), Is.Null);
            Assert.That(getTestPropertyTypeDto(_propertyType2.Id), Is.Null);
            Assert.That(getTestPropertyTypeDto(_propertyType3.Id), Is.Null);
            Assert.That(getTestContentTypeDto(_contentType1.PrimaryKey), Is.Null);
            Assert.That(getTestContentTypeDto(_contentType2.PrimaryKey), Is.Null);
            Assert.That(getTestPropertyTypeGroupDto(_propertyTypeGroup1.Id), Is.Null);
            Assert.That(getTestPropertyTypeGroupDto(_propertyTypeGroup2.Id), Is.Null);
            Assert.That(getTestPropertyTypeGroupDto(_propertyTypeGroup3.Id), Is.Null);

            Assert.That(getTestNodeDto(_node1.Id), Is.Null);
            Assert.That(getTestNodeDto(_node2.Id), Is.Null);
            Assert.That(getTestNodeDto(_node3.Id), Is.Null);
            Assert.That(getTestNodeDto(_node4.Id), Is.Null);
            Assert.That(getTestNodeDto(_node5.Id), Is.Null);
        }

        [Test(Description = "Constructors and Save")]
        public void Test_PropertyTypeGroup_Constructors_and_PopulateFromDto()
        {
            // new in-memory only
            // public PropertyTypeGroup(int parentId, int contentTypeId, string name, int sortOrder)
            // public PropertyTypeGroup(int parentId, int contentTypeId, string name)

            // public void Save()

            var propertyTypeGroup1 = new PropertyTypeGroup(0, _contentType1.NodeId, "Property Type Group 1");
            propertyTypeGroup1.Save();

            var propertyTypeGroup2 = new PropertyTypeGroup(_propertyTypeGroup1.Id, _contentType2.NodeId, "Property Type Group 1", -1);
            // Save
            propertyTypeGroup2.Save();

            var savedPropertyTypeGroup1 = getTestPropertyTypeGroupDto(propertyTypeGroup1.Id);
            var savedPropertyTypeGroup2 = getTestPropertyTypeGroupDto(propertyTypeGroup2.Id);

            Assert.That(propertyTypeGroup1.Id, Is.EqualTo(savedPropertyTypeGroup1.Id));
            Assert.That(propertyTypeGroup2.Id, Is.EqualTo(savedPropertyTypeGroup2.Id));

            // update
            propertyTypeGroup1.ParentId = 0; // _propertyTypeGroup2.Id;
            propertyTypeGroup1.ContentTypeId = _contentType2.NodeId;
            propertyTypeGroup1.SortOrder = -1;
            propertyTypeGroup1.Name = "New Name";
            propertyTypeGroup1.Save();

            var savedPropertyTypeGroup3 = getTestPropertyTypeGroupDto(propertyTypeGroup1.Id);
            Assert.That(propertyTypeGroup1.Id, Is.EqualTo(savedPropertyTypeGroup3.Id), "Id test failed");
            Assert.That(propertyTypeGroup1.Name, Is.EqualTo(savedPropertyTypeGroup3.Text), "Name test failed");
            Assert.That(propertyTypeGroup1.ContentTypeId, Is.EqualTo(savedPropertyTypeGroup3.ContentTypeNodeId), "ContentTypeId test failed");
            //Assert.That(propertyTypeGroup1.ParentId, Is.EqualTo(savedPropertyTypeGroup3.ParentGroupId), "ParentId test failed");
            Assert.That(propertyTypeGroup1.ParentId, Is.EqualTo(0) , "ParentId test failed");
            Assert.That(propertyTypeGroup1.SortOrder, Is.EqualTo(savedPropertyTypeGroup3.SortOrder), "SortOrder test failed");

            int delCount1 = independentDatabase.Execute("delete from [cmsPropertyTypeGroup] where id = @0", savedPropertyTypeGroup1.Id);
            int delCount2 = independentDatabase.Execute("delete from [cmsPropertyTypeGroup] where id = @0", savedPropertyTypeGroup2.Id);

            savedPropertyTypeGroup1 = getTestPropertyTypeGroupDto(propertyTypeGroup1.Id);
            savedPropertyTypeGroup2 = getTestPropertyTypeGroupDto(propertyTypeGroup2.Id);

            Assert.That(savedPropertyTypeGroup2, Is.Null, "savedPropertyTypeGroup2 test failed");
            Assert.That(savedPropertyTypeGroup1, Is.Null, "savedPropertyTypeGroup1 test failed");
        
        }

        [Test(Description = "Test 'public static PropertyTypeGroup GetPropertyTypeGroup(int id)' ")]
        public void Test_PropertyTypeGroup_GetPropertyTypeGroup()
        {
            // public static PropertyTypeGroup GetPropertyTypeGroup(int id)
            // internal void Load()

            var propertyTypeGroup1 = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup1.Id);  
            var propertyTypeGroup2 = getTestPropertyTypeGroupDto(_propertyTypeGroup1.Id);

            Assert.That(propertyTypeGroup1.Id, Is.EqualTo(propertyTypeGroup2.Id), "Id test failed");
            Assert.That(propertyTypeGroup1.Name, Is.EqualTo(propertyTypeGroup2.Text), "Name test failed");
            Assert.That(propertyTypeGroup1.ContentTypeId, Is.EqualTo(propertyTypeGroup2.ContentTypeNodeId), "ContentTypeId test failed");
            Assert.That(propertyTypeGroup1.ParentId, Is.EqualTo(0) , "ParentId test failed");
            Assert.That(propertyTypeGroup1.SortOrder, Is.EqualTo(propertyTypeGroup2.SortOrder), "SortOrder test failed");
        }


        [Test(Description = "Test 'Delete()' method")]
        public void Test_PropertyTypeGroup_Delete()
        {
            // public void Delete()

            var propertyTypeGroup1 = new PropertyTypeGroup(0, _contentType1.NodeId, "Property Type Group 1");
            propertyTypeGroup1.Save();

            int id = propertyTypeGroup1.Id;

            var savedPropertyTypeGroup1 = getTestPropertyTypeGroupDto(id);
            Assert.That(propertyTypeGroup1.Id, Is.EqualTo(savedPropertyTypeGroup1.Id));
            
            propertyTypeGroup1.Delete();

            var savedPropertyTypeGroup2 = getTestPropertyTypeGroupDto(id);
            Assert.That(savedPropertyTypeGroup2, Is.Null);
            
        }

        [Test(Description = "Test 'public IEnumerable<PropertyTypeGroup> GetPropertyTypeGroups()' method")]
        public void Test_PropertyTypeGroup_GetPropertyTypeGroups()
        {
            // public IEnumerable<PropertyTypeGroup> GetPropertyTypeGroups()
            var propertyTypeGroup1 = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup1.Id);
            var childGroups = propertyTypeGroup1.GetPropertyTypeGroups().ToArray();

            Assert.That(childGroups.Length, Is.EqualTo(TEST_CHILD_PROPERTY_TYPE_GROUPS_QTY)); // there are two test child groups   
        }

        [Test(Description = "Test 'public static IEnumerable<PropertyTypeGroup> GetPropertyTypeGroupsFromContentType(int contentTypeId)' method")]
        public void Test_PropertyTypeGroup_GetPropertyTypeGroupsFromContentType()
        {
            // public static IEnumerable<PropertyTypeGroup> GetPropertyTypeGroupsFromContentType(int contentTypeId)
            var contentGroups = PropertyTypeGroup.GetPropertyTypeGroupsFromContentType(_contentType1.NodeId).ToArray();

            Assert.That(contentGroups.Length, Is.EqualTo(TEST_CHILD_PROPERTY_TYPE_GROUPS_QTY));
        }

        [Test(Description = "Test 'public IEnumerable<PropertyType> GetPropertyTypes()' method")]
        public void Test_PropertyTypeGroup_GetPropertyTypes()
        {
            // public IEnumerable<PropertyType> GetPropertyTypes()
            var propertyTypeGroup1 = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup1.Id);
            var groupPropertyTypes = propertyTypeGroup1.GetPropertyTypes().ToArray();

            Assert.That(groupPropertyTypes.Length, Is.EqualTo(TEST_CHILD_PROPERTY_TYPES_QTY));
        }

        [Test(Description = "Test 'public IEnumerable<PropertyType> GetPropertyTypes(List<int> contentTypeIds)' method")]
        public void Test_PropertyTypeGroup_GetPropertyTypes_by_contentTypeIds()
        {
            // public IEnumerable<PropertyType> GetPropertyTypes(List<int> contentTypeIds)

            List<int> contentTypeIds = new List<int> { _contentType2.NodeId, _contentType1.NodeId };
            var propertyTypeGroup1 = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup1.Id);
            var groupPropertyTypes = propertyTypeGroup1.GetPropertyTypes(contentTypeIds).ToArray();

            Assert.That(groupPropertyTypes.Length, Is.EqualTo(TEST_CHILD_PROPERTY_TYPES_QTY));
        }
        #endregion
    }
}
