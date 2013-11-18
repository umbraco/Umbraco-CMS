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
    public class cms_businesslogic_PropertyTypeGroup_Tests : BaseORMTest
    {
        const int TEST_CONTENT_TYPES_QTY = 2;

        const int TEST_PROPERTY_TYPE_GROUPS_QTY = 3;
        const int TEST_CHILD_PROPERTY_TYPE_GROUPS_QTY = 2;

        const int TEST_PROPERTY_TYPES_QTY = 3;
        const int TEST_CHILD_PROPERTY_TYPES_QTY = 2;

        // cmsDataType records are expected to be in the test database - 
        // otherwise it'd bee too much data to setup for this test suite
        const int TEST_DATA_TYPE_ID1 = -90;
        const int TEST_DATA_TYPE_ID2 = -36;
        const int TEST_DATA_TYPE_ID3 = -41;

        protected override void EnsureData() { Ensure_PropertyTypeGroup_TestData(); }

         #region Tests
        [Test(Description = "Test EnsureData()")]
        public void _1st_Test_PropertyTypeGroup_EnsureData()
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

            EnsureAll_PropertyTypeGroup_TestRecordsAreDeleted();

            Assert.That(getDto<PropertyDataDto>(_propertyData1.Id), Is.Null);
            Assert.That(getDto<PropertyDataDto>(_propertyData2.Id), Is.Null);
            Assert.That(getDto<PropertyDataDto>(_propertyData3.Id), Is.Null);
            Assert.That(getDto<PropertyTypeDto>(_propertyType1.Id), Is.Null);
            Assert.That(getDto<PropertyTypeDto>(_propertyType2.Id), Is.Null);
            Assert.That(getDto<PropertyTypeDto>(_propertyType3.Id), Is.Null);
            Assert.That(getDto<ContentTypeDto>(_contentType1.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(getDto<ContentTypeDto>(_contentType2.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(getDto<PropertyTypeGroupDto>(_propertyTypeGroup1.Id), Is.Null);
            Assert.That(getDto<PropertyTypeGroupDto>(_propertyTypeGroup2.Id), Is.Null);
            Assert.That(getDto<PropertyTypeGroupDto>(_propertyTypeGroup3.Id), Is.Null);

            Assert.That(getDto<NodeDto>(_node1.Id), Is.Null);
            Assert.That(getDto<NodeDto>(_node2.Id), Is.Null);
            Assert.That(getDto<NodeDto>(_node3.Id), Is.Null);
            Assert.That(getDto<NodeDto>(_node4.Id), Is.Null);
            Assert.That(getDto<NodeDto>(_node5.Id), Is.Null);
        }

        [Test(Description = "Constructors and Save")]
        public void _2nd_Test_PropertyTypeGroup_Constructor_I_Save()
        {
            // new PropertyTypeGroup object instance created by constructor is not loaded from the database,
            // so we're creating new PropertyTypeGroup object instance here and we're saving it in the back-end database 
            // via PropertyTypeGroup .Save method

            // create new in-memory PropertyTypeGroup object instance and save it to back-end db
            var propertyTypeGroup = new PropertyTypeGroup(0, _contentType1.NodeId, "Property Type Group" + uniqueNameSuffix);
            propertyTypeGroup.Save();
            Assert.That(propertyTypeGroup.Id, !Is.EqualTo(0));    

            var savedPropertyTypeGroup = getDto<PropertyTypeGroupDto>(propertyTypeGroup.Id);
            assertPropertyTypeGroupSetup(propertyTypeGroup, savedPropertyTypeGroup);

        }

        [Test(Description = "Constructors and Save")]
        public void _3rd_Test_PropertyTypeGroup_Constructor_II_Save_With_ParentRelationshipSet()
        {
            // new PropertyTypeGroup object instance created by constructor is not loaded from the database,
            // so we're creating new PropertyTypeGroup object instance here and we're saving it in the back-end database 
            // via PropertyTypeGroup .Save method

            var parentPropertyTypeGroup = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup1.Id);  

            // create new in-memory PropertyTypeGroup object instance and save it to back-end db
            // this object instance has parent PropertyTypeGroup _propertyTypeGroup1 created and saved above
            var propertyTypeGroup = new PropertyTypeGroup(parentPropertyTypeGroup.Id, _contentType1.NodeId, "Property Type Group" + uniqueNameSuffix, -1);
            propertyTypeGroup.Save();
            Assert.That(propertyTypeGroup.Id, !Is.EqualTo(0));    

            var savedPropertyTypeGroup = getDto<PropertyTypeGroupDto>(propertyTypeGroup.Id);
            assertPropertyTypeGroupSetup(propertyTypeGroup, savedPropertyTypeGroup);
        }

        private void assertPropertyTypeGroupSetup(PropertyTypeGroup testPropertyTypeGroup, PropertyTypeGroupDto savedPropertyTypeGroupDto)
        {
            Assert.That(testPropertyTypeGroup.Id, Is.EqualTo(savedPropertyTypeGroupDto.Id), "Id test failed");
            Assert.That(testPropertyTypeGroup.Name, Is.EqualTo(savedPropertyTypeGroupDto.Text), "Name test failed");
            Assert.That(testPropertyTypeGroup.ContentTypeId, Is.EqualTo(savedPropertyTypeGroupDto.ContentTypeNodeId), "ContentTypeId test failed");
            if (savedPropertyTypeGroupDto.ParentGroupId == null)
                Assert.That(testPropertyTypeGroup.ParentId, Is.EqualTo(0), "ParentId test failed");
            else
                Assert.That(testPropertyTypeGroup.ParentId, Is.EqualTo(savedPropertyTypeGroupDto.ParentGroupId ), "ParentId test failed");
            Assert.That(testPropertyTypeGroup.SortOrder, Is.EqualTo(savedPropertyTypeGroupDto.SortOrder), "SortOrder test failed");
        }

        [Test(Description = "Test 'public static PropertyTypeGroup GetPropertyTypeGroup(int id)' ")]
        public void Test_PropertyTypeGroup_GetPropertyTypeGroup()
        {
            var propertyTypeGroup1 = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup1.Id);  
            var propertyTypeGroup2 = getDto<PropertyTypeGroupDto>(_propertyTypeGroup1.Id);

            assertPropertyTypeGroupSetup(propertyTypeGroup1, propertyTypeGroup2);
        }

        [Test(Description = "Test 'public void Save()' method => Update")]
        public void Test_PropertyTypeGroup_Constructors_Update_via_Save()
        {
            var propertyTypeGroup = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup1.Id);
            Assert.That(propertyTypeGroup, !Is.Null);
               
            // update
            propertyTypeGroup.ParentId = 0;
            propertyTypeGroup.ContentTypeId = _contentType2.NodeId;
            propertyTypeGroup.SortOrder = -1;
            propertyTypeGroup.Name = "New Name" + uniqueNameSuffix;
            propertyTypeGroup.Save();

            var savedPropertyTypeGroup = getDto<PropertyTypeGroupDto>(propertyTypeGroup.Id);

            assertPropertyTypeGroupSetup(propertyTypeGroup, savedPropertyTypeGroup);
        }

        [Test(Description = "Test 'Delete()' method")]
        public void Test_PropertyTypeGroup_Delete()
        {
            var propertyTypeGroup = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup1.Id);
            Assert.That(propertyTypeGroup, !Is.Null);

            int id = propertyTypeGroup.Id;

            propertyTypeGroup.Delete();

            var savedPropertyTypeGroup = getDto<PropertyTypeGroupDto>(id);
            Assert.That(savedPropertyTypeGroup, Is.Null);

            initialized = false;
        }

        [Test(Description = "Test 'public IEnumerable<PropertyTypeGroup> GetPropertyTypeGroups()' method")]
        public void Test_PropertyTypeGroup_GetPropertyTypeGroups()
        {
            var parentPropertyTypeGroup = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup1.Id);
            var childGroups = parentPropertyTypeGroup.GetPropertyTypeGroups().ToArray();

            int count = independentDatabase.ExecuteScalar<int>(@"SELECT count(id) FROM cmsPropertyTypeGroup WHERE parentGroupId = @0", parentPropertyTypeGroup.Id);

            Assert.That(childGroups.Length, Is.EqualTo(count));
        }

        [Test(Description = "Test 'public static IEnumerable<PropertyTypeGroup> GetPropertyTypeGroupsFromContentType(int contentTypeId)' method")]
        public void Test_PropertyTypeGroup_GetPropertyTypeGroupsFromContentType()
        {
            var contentGroups = PropertyTypeGroup.GetPropertyTypeGroupsFromContentType(_contentType1.NodeId).ToArray();

            int count = independentDatabase.ExecuteScalar<int>(@" SELECT count(id) FROM cmsPropertyTypeGroup WHERE contenttypeNodeId = @0", _contentType1.NodeId);

            Assert.That(contentGroups.Length, Is.EqualTo(TEST_CHILD_PROPERTY_TYPE_GROUPS_QTY));
        }

        [Test(Description = "Test 'public IEnumerable<PropertyType> GetPropertyTypes()' method")]
        public void Test_PropertyTypeGroup_GetPropertyTypes()
        {
            var propertyTypeGroup = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup1.Id);
            var groupPropertyTypes = propertyTypeGroup.GetPropertyTypes().ToArray();

            int count = independentDatabase.ExecuteScalar<int>("SELECT count(id) FROM cmsPropertyType WHERE propertyTypeGroupId = @0", propertyTypeGroup.Id);


            Assert.That(groupPropertyTypes.Length, Is.EqualTo(TEST_CHILD_PROPERTY_TYPES_QTY));
        }

        [Test(Description = "Test 'public IEnumerable<PropertyType> GetPropertyTypes(List<int> contentTypeIds)' method")]
        public void Test_PropertyTypeGroup_GetPropertyTypes_by_contentTypeIds()
        {
            List<int> contentTypeIds = new List<int> { _contentType2.NodeId, _contentType1.NodeId };
            var propertyTypeGroup = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup1.Id);
            var groupPropertyTypes = propertyTypeGroup.GetPropertyTypes(contentTypeIds).ToArray();  // check this method => GetPropertyTypes

            int count = 0; 
            contentTypeIds.ForEach(x =>
                   count += independentDatabase.ExecuteScalar<int>("SELECT count(id) FROM cmsPropertyType WHERE propertyTypeGroupId = @0 and contentTypeId = @1", _propertyTypeGroup1.Id, x));

            Assert.That(groupPropertyTypes.Length, Is.EqualTo(count));
        }
        #endregion
    }
}
