using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.relation;
//using Umbraco.Core.Models;
using umbraco.cms.businesslogic;
using System.Collections.Generic;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.datatype;
using Umbraco.Tests.BusinessLogic;

namespace Umbraco.Tests.ORM
{
    [TestFixture]
    public class cms_businesslogic_PropertyType_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_PropertyType_TestData(); }

        [Test(Description = "Test EnsureData()")]
        public void _1st_Test_PropertyType_EnsureData()
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
            Assert.That(_propertyData1, !Is.Null);
            Assert.That(_propertyData2, !Is.Null);
            Assert.That(_propertyData3, !Is.Null);
            Assert.That(_node1, !Is.Null);
            Assert.That(_node2, !Is.Null);
            Assert.That(_node3, !Is.Null);
            Assert.That(_node4, !Is.Null);
            Assert.That(_node5, !Is.Null);

            EnsureAll_PropertyType_TestRecordsAreDeleted();

            Assert.That(TRAL.GetDto<PropertyDataDto>(_propertyData1.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyDataDto>(_propertyData2.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyDataDto>(_propertyData3.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeDto>(_propertyType1.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeDto>(_propertyType2.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeDto>(_propertyType3.Id), Is.Null);
            Assert.That(TRAL.GetDto<ContentTypeDto>(_contentType1.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(TRAL.GetDto<ContentTypeDto>(_contentType2.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeGroupDto>(_propertyTypeGroup1.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeGroupDto>(_propertyTypeGroup2.Id), Is.Null);
            Assert.That(TRAL.GetDto<PropertyTypeGroupDto>(_propertyTypeGroup3.Id), Is.Null);

            Assert.That(TRAL.GetDto<NodeDto>(_node1.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node2.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node3.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node4.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node5.Id), Is.Null);
        }

        [Test(Description = "Test 'public PropertyType(int id)' constructor")]
        public void _2nd_Test_PropertyType_Constructors()
        {
            var testPropertyType = new PropertyType(_propertyType1.Id);
            Assert.That(testPropertyType, !Is.Null);  
            var savedPropertyTypeDto = TRAL.GetDto<PropertyTypeDto>(_propertyType1.Id);
            Assert.That(savedPropertyTypeDto, !Is.Null);  

            assertPropertyTypeSetup(testPropertyType, savedPropertyTypeDto);  
        }

        private void assertPropertyTypeSetup(PropertyType testPropertyType, PropertyTypeDto savedPropertyTypeDto, bool tabIdIsZero = false)
        {
            Assert.That(testPropertyType.Id, Is.EqualTo(savedPropertyTypeDto.Id), "Id test failed");
            Assert.That(testPropertyType.DataTypeDefinition.Id, Is.EqualTo(savedPropertyTypeDto.DataTypeId), "DataTypeId test failed");
            if (!tabIdIsZero)
            Assert.That(testPropertyType.TabId, Is.EqualTo(savedPropertyTypeDto.PropertyTypeGroupId), "testPropertyTypeGroupId test failed");
            else
                Assert.That(testPropertyType.TabId, Is.EqualTo(0), "testPropertyTypeGroupId test failed");
            if (!tabIdIsZero)
                Assert.That(testPropertyType.PropertyTypeGroup, Is.EqualTo(savedPropertyTypeDto.PropertyTypeGroupId), "testPropertyTypeGroup test failed");
            else
                Assert.That(testPropertyType.PropertyTypeGroup, Is.EqualTo(0), "testPropertyTypeGroup test failed");
            Assert.That(testPropertyType.Mandatory, Is.EqualTo(savedPropertyTypeDto.Mandatory), "Mandatory property test failed");
            Assert.That(testPropertyType.ValidationRegExp, Is.EqualTo(savedPropertyTypeDto.ValidationRegExp), "ValidationRegExp test failed");
            Assert.That(testPropertyType.Description, Is.EqualTo(savedPropertyTypeDto.Description), "Description test failed");
            Assert.That(testPropertyType.SortOrder, Is.EqualTo(savedPropertyTypeDto.SortOrder), "SortOrder test failed");
            Assert.That(testPropertyType.Alias, Is.EqualTo(savedPropertyTypeDto.Alias), "Alias test failed");
            Assert.That(testPropertyType.Name, Is.EqualTo(savedPropertyTypeDto.Name), "Name test failed");
        }

        const int TEST_DATA_TYPE_ID1 = -90;
        const int TEST_DATA_TYPE_ID2 = -36;
        const int TEST_DATA_TYPE_ID3 = -41;

        [Test(Description = "Test 'public static PropertyType MakeNew(DataTypeDefinition dt, ContentType ct, string name, string alias)' method")]
        public void Test_PropertyType_MakeNew()
        {
            var dataType = new DataTypeDefinition(TEST_DATA_TYPE_ID2);
            Assert.That(dataType, !Is.Null);
 
            var contentType = new ContentType(_contentType1.NodeId);
            Assert.That(contentType, !Is.Null);

            Assert.Throws<ArgumentNullException>(() => { PropertyType.MakeNew(dataType, contentType, "Test Property Type 123", string.Empty); }, "MakeNew: Alias can't be empty");
            Assert.Throws<ArgumentNullException>(() => { PropertyType.MakeNew(dataType, contentType, string.Empty, "testPropertyType123"); }, "MakeNew: Name can't be empty");
            Assert.Throws<ArgumentException>(() => { PropertyType.MakeNew(dataType, contentType, "Test Property Type 123", "123 testPropertyType123"); }, "MakeNew: Alias must start with a letter");  

            var propertyType = PropertyType.MakeNew(dataType, contentType, "Test Property Type 123", "testPropertyType123");
            Assert.That(propertyType, !Is.Null, "Object instance !Is.Null test failed");

            var savedPropertyTypeDto = TRAL.GetDto<PropertyTypeDto>(propertyType.Id);

            assertPropertyTypeSetup(propertyType, savedPropertyTypeDto, tabIdIsZero:true); 
        }

        [Test(Description = "Test 'public void delete()' method")]
        public void Test_PropertyType_Delete()
        {
            //public void delete()
            // TODO - see https://groups.google.com/d/msg/umbraco-dev/9qLYrQrTQ8o/Uljx446Bv1YJ
            //private void CleanPropertiesOnDeletion(int contentTypeId)
            //
            //private void CleanAllPropertiesOnDeletion()
            int id = -1;
            var propertyType = new PropertyType(_propertyType1.Id);  

            id = propertyType.Id; 
            propertyType.delete();  

            var savedPropertyTypeDto = TRAL.GetDto<PropertyTypeDto>(id);

            Assert.That(savedPropertyTypeDto, Is.Null, "Delete test failed");

            initialized = false;
        }

        [Test(Description = "Test 'DataTypeDefinition DataTypeDefinition .set' property")]
        public void Test_PropertyType_DataTypeDefinition_Set()
        { 
            var newValue = new DataTypeDefinition(TEST_DATA_TYPE_ID3);
            var expectedValue = newValue.Id;
            TRAL.Setter_Persists_Ext<PropertyType, DataTypeDefinition, int>(
                    n => n.DataTypeDefinition,
                    n => n.DataTypeDefinition = newValue,
                    "cmsPropertyType",
                    "dataTypeId",
                    expectedValue,
                    "id",
                    _propertyType1.Id,
                    useSecondGetter: true,
                    getter2: n => n.DataTypeDefinition.Id,
                    oldValue2: _propertyType1.DataTypeId
                );
        }

        [Obsolete("TabId is marked as obsolete - this test to be deleted when .TabId will be deleted")] 
        [Test(Description = "Test 'public int TabId .set => PropertyTypeGroup' property")]
        public void Test_PropertyType_TabId_Set()
        {
            var newValue = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup3.Id);
            var expectedValue = newValue.Id;
            TRAL.Setter_Persists_Ext<PropertyType, int, int>(
                    n => n.TabId,
                    n => n.TabId = newValue.Id,
                    "cmsPropertyType",
                    "propertyTypeGroupId",
                    expectedValue,
                    "id",
                    _propertyType1.Id
                );
        }

        [Test(Description = "Test 'public int PropertyTypeGroup .set' property")]
        public void Test_PropertyType_PropertyTypeGroup_Set()
        {
            var newValue = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup3.Id);
            var expectedValue = newValue.Id;
            TRAL.Setter_Persists_Ext<PropertyType, int, int>(
                    n => n.PropertyTypeGroup,
                    n => n.PropertyTypeGroup = newValue.Id,
                    "cmsPropertyType",
                    "propertyTypeGroupId",
                    expectedValue,
                    "id",
                    _propertyType1.Id
                );
        }
        
        [Test(Description = "Test 'public bool Mandatory .set' property")]
        public void Test_PropertyType_Mandatory_Set()
        {
            var propertyType = new PropertyType(_propertyType1.Id);
            var oldValue = propertyType.Mandatory;

            var newValue = !oldValue;
            var expectedValue = !oldValue;
            TRAL.Setter_Persists_Ext<PropertyType, bool, bool>(
                    n => n.Mandatory,
                    n => n.Mandatory = newValue,
                    "cmsPropertyType",
                    "Mandatory",
                    expectedValue,
                    "id",
                    _propertyType1.Id
                );
        }

        [Test(Description = "Test 'public string ValidationRegExp .set' property")]
        public void Test_PropertyType_ValidationRegExp_Set()
        {
            var newValue = @"[a-b]\w[1-9]";
            var expectedValue = @"[a-b]\w[1-9]";
            TRAL.Setter_Persists_Ext<PropertyType, string, string>(
                    n => n.ValidationRegExp,
                    n => n.ValidationRegExp = newValue,
                    "cmsPropertyType",
                    "ValidationRegExp",
                    expectedValue,
                    "id",
                    _propertyType1.Id
                );
        }

        [Test(Description = "Test 'public string Description .set' property")]
        public void Test_PropertyType_Description_Set()
        {
            var newValue = "New Description";
            var expectedValue = "New Description";
            TRAL.Setter_Persists_Ext<PropertyType, string, string>(
                    n => n.Description,
                    n => n.Description = newValue,
                    "cmsPropertyType",
                    "Description",
                    expectedValue,
                    "id",
                    _propertyType1.Id
                );
        }

        [Test(Description = "Test 'public string SortOrder .set' property")]
        public void Test_PropertyType_SortOrder_Set()
        {
            var newValue = 7;
            var expectedValue = 7;
            TRAL.Setter_Persists_Ext<PropertyType, int, int>(
                    n => n.SortOrder,
                    n => n.SortOrder = newValue,
                    "cmsPropertyType",
                    "SortOrder",
                    expectedValue,
                    "id",
                    _propertyType1.Id
                );
        }

        [Test(Description = "Test 'public string Alias .set' property")]
        public void Test_PropertyType_Alias_Set()
        {
            var newValue      = "theNewAliasValue";
            var expectedValue = "theNewAliasValue";
            TRAL.Setter_Persists_Ext<PropertyType, string, string>(
                    n => n.Alias,
                    n => n.Alias = newValue,
                    "cmsPropertyType",
                    "Alias",
                    expectedValue,
                    "id",
                    _propertyType1.Id
                );
        }

        [Test(Description = "Test 'public string Name .set' property")]
        public void Test_PropertyType_Name_Set()
        {
            var newValue = "The New Name Value";
            var expectedValue = "The New Name Value";
            TRAL.Setter_Persists_Ext<PropertyType, string, string>(
                    n => n.Name,
                    n => n.Name = newValue,
                    "cmsPropertyType",
                    "Name",
                    expectedValue,
                    "id",
                    _propertyType1.Id
                );
        }

        [Test(Description = "Test 'public static PropertyType[] GetAll()' method")]
        public void Test_PropertyType_GetAll()
        {
            var all = PropertyType.GetAll();
            int count = TRAL.Property.CountAllPropertyTypes;

            Assert.That(all.Length, Is.EqualTo(count), "GetAll test failed");
        }

        [Test(Description = "Test 'public static IEnumerable<PropertyType> GetPropertyTypes()' method")]
        public void Test_PropertyType_GetPropertyTypes()
        {
            var all = PropertyType.GetPropertyTypes().ToArray() ;
            int count = TRAL.Property.CountAllPropertyTypes;

            Assert.That(all.Length, Is.EqualTo(count), "GetPropertyTypes test failed");
        }

        [Test(Description = "Test 'public static PropertyType GetPropertyType(int id)' method")]
        public void Test_PropertyType_GetPropertyType()
        {
            var propertyType = PropertyType.GetPropertyType(_propertyType1.Id);
            Assert.That(propertyType, !Is.Null, "Object instance !Is.Null test failed");

            var savedPropertyTypeDto = TRAL.GetDto<PropertyTypeDto>(propertyType.Id);

            assertPropertyTypeSetup(propertyType, savedPropertyTypeDto); 
        } 

        [Test(Description = "Test 'public static IEnumerable<PropertyType> GetPropertyTypesByGroup(int groupId)' method")]
        public void Test_PropertyType_GetPropertyTypesByGroup_1()
        {
            var all = PropertyType.GetPropertyTypesByGroup(_propertyTypeGroup1.Id).ToArray() ;
            int count = TRAL.Property.CountPropertyTypesByGroupId(_propertyTypeGroup1.Id);

            Assert.That(all.Length, Is.EqualTo(count));  // there are two properties saved for _propertyTypeGroup1.Id
        }

        [Test(Description = "Test 'public static IEnumerable<PropertyType> GetPropertyTypesByGroup(int groupId, List<int> contentTypeIds)' method")]
        public void Test_PropertyType_GetPropertyTypesByGroup_2()
        {
            var all1 = PropertyType.GetPropertyTypesByGroup(_propertyTypeGroup1.Id, new List<int> { _contentType1.NodeId }).ToArray();
            var all2 = PropertyType.GetPropertyTypesByGroup(_propertyTypeGroup1.Id, new List<int> { _contentType2.NodeId }).ToArray();
            var all3 = PropertyType.GetPropertyTypesByGroup(_propertyTypeGroup1.Id, new List<int> { _contentType1.NodeId, _contentType2.NodeId }).ToArray();

            Assert.That(all1.Length, Is.EqualTo(1), "GetPropertyTypesByGroup_2 Test1 failed");  // there is one property saved for _propertyTypeGroup1.Id & _contentType1.NodeId
            Assert.That(all2.Length, Is.EqualTo(1), "GetPropertyTypesByGroup_2 Test2 failed");  // there is one property saved for _propertyTypeGroup1.Id & _contentType2.NodeId
            Assert.That(all3.Length, Is.EqualTo(2), "GetPropertyTypesByGroup_2 Test3 failed");  // there is one property saved for _propertyTypeGroup1.Id & {_contentType1.NodeId & _contentType2.NodeId}

        }

        [Test(Description = "Test 'public static IEnumerable<PropertyType> GetByDataTypeDefinition(int dataTypeDefId)' method")]
        public void Test_PropertyType_GetByDataTypeDefinition()
        {
            var all = PropertyType.GetByDataTypeDefinition(TEST_DATA_TYPE_ID1).ToArray();
            int count = TRAL.Property.CountPropertyTypesByDataTypeId(TEST_DATA_TYPE_ID1);

            Assert.That(all.Length , Is.EqualTo(count)); // this test suite adds just one PropertyType for TEST_DATA_TYPE_ID1
        }


        //
        // not tested
        //
        //public virtual void Save()
        //public void FlushCacheBasedOnTab()
        //public IDataType GetEditControl(object value, bool isPostBack)

    }
}
