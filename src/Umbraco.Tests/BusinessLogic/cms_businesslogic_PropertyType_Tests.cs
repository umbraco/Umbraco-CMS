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

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_PropertyType_Tests : BaseDatabaseFactoryTestWithContext
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

        private PropertyDataDto _propertyData1;
        private PropertyDataDto _propertyData2;
        private PropertyDataDto _propertyData3;

        // cmsDataType records are expected to be in the test database - 
        // otherwise it'd bee too much data to setup for this test suite
        const int TEST_DATA_TYPE_ID1 = -90;
        const int TEST_DATA_TYPE_ID2 = -36;
        const int TEST_DATA_TYPE_ID3 = -41;

        //const int TEST_CONTENT_NODE_ID1 = 1063;
        //const int TEST_CONTENT_NODE_ID2 = 1065;
        //const int TEST_CONTENT_NODE_ID3 = 1068;

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

                _propertyData1 = insertPropertyTypeData(_propertyType1.Id, _contentType1.NodeId);
                _propertyData2 = insertPropertyTypeData(_propertyType1.Id, _contentType2.NodeId);
                _propertyData3 = insertPropertyTypeData(_propertyType2.Id, _contentType1.NodeId);
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
        private PropertyDataDto getTestPropertyDataDto(int id)
        {
            return getPersistedTestDto<PropertyDataDto>(id);
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

        private PropertyDataDto insertPropertyTypeData(int propertyTypeId, int contentNodeId)
        {
            independentDatabase.Execute("insert into [cmsPropertyData] " +
                       " (contentNodeId, versionId, propertytypeid, dataInt, dataDate, dataNvarchar, dataNtext) " +
                       "   VALUES " +
                       " (@contentNodeId, @versionId, @propertytypeid, @dataInt, @dataDate, @dataNvarchar, @dataNtext)",
                       new
                       {
                           contentNodeId = contentNodeId, 
                           versionId = Guid.NewGuid(), 
                           propertytypeid = propertyTypeId, 
                           dataInt = (int?)null, 
                           dataDate = (DateTime?)null,
                           dataNvarchar = (string)null, 
                           dataNtext = (string)null
                       });
            int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsPropertyData]");
            return getTestPropertyDataDto(id);
        }


        private void EnsureAllTestRecordsAreDeleted()
        {
            independentDatabase.Execute("delete from [cmsPropertyData] where id = @0", _propertyData3.Id);
            independentDatabase.Execute("delete from [cmsPropertyData] where id = @0", _propertyData2.Id);
            independentDatabase.Execute("delete from [cmsPropertyData] where id = @0", _propertyData1.Id);

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
            Assert.That(_propertyData1, !Is.Null);
            Assert.That(_propertyData2, !Is.Null);
            Assert.That(_propertyData3, !Is.Null);
            Assert.That(_node1, !Is.Null);
            Assert.That(_node2, !Is.Null);
            Assert.That(_node3, !Is.Null);
            Assert.That(_node4, !Is.Null);
            Assert.That(_node5, !Is.Null);

            EnsureAllTestRecordsAreDeleted();

            Assert.That(getTestPropertyDataDto(_propertyData1.Id), Is.Null);
            Assert.That(getTestPropertyDataDto(_propertyData2.Id), Is.Null);
            Assert.That(getTestPropertyDataDto(_propertyData3.Id), Is.Null);
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

        [Test(Description = "Constructors")]
        public void Test_PropertyType_Constructors()
        {
            //public PropertyType(int id)
            var propertyType = new PropertyType(_propertyType1.Id);
            var savedPropertyTypeDto = getTestPropertyTypeDto(_propertyType1.Id);

            Assert.That(propertyType.Id, Is.EqualTo(savedPropertyTypeDto.Id), "Id test failed");

            Assert.That(propertyType.DataTypeDefinition.Id, Is.EqualTo(savedPropertyTypeDto.DataTypeId), "DataTypeId test failed");
            Assert.That(propertyType.TabId, Is.EqualTo(savedPropertyTypeDto.PropertyTypeGroupId), "PropertyTypeGroupId test failed");
            Assert.That(propertyType.PropertyTypeGroup, Is.EqualTo(savedPropertyTypeDto.PropertyTypeGroupId), "PropertyTypeGroup test failed");
            Assert.That(propertyType.Mandatory, Is.EqualTo(savedPropertyTypeDto.Mandatory), "Mandatory property test failed");
            Assert.That(propertyType.ValidationRegExp, Is.EqualTo(savedPropertyTypeDto.ValidationRegExp), "ValidationRegExp test failed");
            Assert.That(propertyType.Description, Is.EqualTo(savedPropertyTypeDto.Description), "Description test failed");
            Assert.That(propertyType.SortOrder, Is.EqualTo(savedPropertyTypeDto.SortOrder), "SortOrder test failed");
            Assert.That(propertyType.Alias, Is.EqualTo(savedPropertyTypeDto.Alias), "Alias test failed");
            Assert.That(propertyType.Name, Is.EqualTo(savedPropertyTypeDto.Name), "Name test failed");

        }

        [Test(Description = "Test 'public static PropertyType MakeNew(DataTypeDefinition dt, ContentType ct, string name, string alias)' method")]
        public void Test_PropertyType_MakeNew()
        {
            //public static PropertyType MakeNew(DataTypeDefinition dt, ContentType ct, string name, string alias)
            PropertyType propertyType = null;
            try
            {

                var dataType = new DataTypeDefinition(TEST_DATA_TYPE_ID2);
                var contentType = new ContentType(_contentType1.NodeId);

                Assert.Throws<ArgumentNullException>(() => { PropertyType.MakeNew(dataType, contentType, "Test Property Type 123", string.Empty); }, "MakeNew: Alias can't be empty");
                Assert.Throws<ArgumentNullException>(() => { PropertyType.MakeNew(dataType, contentType, string.Empty, "testPropertyType123"); }, "MakeNew: Name can't be empty");
                Assert.Throws<ArgumentException>(() => { PropertyType.MakeNew(dataType, contentType, "Test Property Type 123", "123 testPropertyType123"); }, "MakeNew: Alias must start with a letter");  

                propertyType = PropertyType.MakeNew(dataType, contentType, "Test Property Type 123", "testPropertyType123");

                Assert.That(propertyType, !Is.Null, "Object instance !Is.Null test failed");

                var savedPropertyTypeDto = getTestPropertyTypeDto(propertyType.Id);

                Assert.That(propertyType.Id, Is.EqualTo(savedPropertyTypeDto.Id), "Id test failed");

                Assert.That(propertyType.DataTypeDefinition.Id, Is.EqualTo(savedPropertyTypeDto.DataTypeId), "DataTypeId test failed");
                //Assert.That(propertyType.TabId, Is.EqualTo(savedPropertyTypeDto.PropertyTypeGroupId), "TabId test failed");
                Assert.That(propertyType.TabId, Is.EqualTo(0), "TabId test failed");
                //Assert.That(propertyType.PropertyTypeGroup, Is.EqualTo(savedPropertyTypeDto.PropertyTypeGroupId), "PropertyTypeGroup test failed");
                Assert.That(propertyType.PropertyTypeGroup, Is.EqualTo(0) , "MakeNew doesn't set PropertyTypeGroup");
                Assert.That(propertyType.Mandatory, Is.EqualTo(savedPropertyTypeDto.Mandatory), "Mandatory property test failed");
                Assert.That(propertyType.ValidationRegExp, Is.EqualTo(savedPropertyTypeDto.ValidationRegExp), "ValidationRegExp test failed");
                Assert.That(propertyType.Description, Is.EqualTo(savedPropertyTypeDto.Description), "Description test failed");
                Assert.That(propertyType.SortOrder, Is.EqualTo(savedPropertyTypeDto.SortOrder), "SortOrder test failed");
                Assert.That(propertyType.Alias, Is.EqualTo(savedPropertyTypeDto.Alias), "Alias test failed");
                Assert.That(propertyType.Name, Is.EqualTo(savedPropertyTypeDto.Name), "Name test failed");
            }
            finally
            {
                // clean-up
                if (propertyType != null)
                    independentDatabase.Execute("delete from [cmsPropertyType] where Id = @0", propertyType.Id);
            }
        }

        [Test(Description = "Test 'public void delete()' method")]
        public void Test_PropertyType_Delete()
        {
            //public void delete()
            // TODO - see https://groups.google.com/d/msg/umbraco-dev/9qLYrQrTQ8o/Uljx446Bv1YJ
            //private void CleanPropertiesOnDeletion(int contentTypeId)
            //
            //private void CleanAllPropertiesOnDeletion()
            PropertyType propertyType = null;
            int id = -1;
            try
            {
                propertyType = new PropertyType(_propertyType1.Id);  

                id = propertyType.Id; 
                propertyType.delete();  

                var savedPropertyTypeDto = getTestPropertyTypeDto(id);

                Assert.That(savedPropertyTypeDto, Is.Null, "Delete test failed");
            }
            finally
            {
                EnsureAllTestRecordsAreDeleted(); // force test data reset for the follow-up tests 
            }
        }

        [Test(Description = "Test 'DataTypeDefinition DataTypeDefinition .set' property")]
        public void Test_PropertyType_DataTypeDefinition_Set()
        {
            //public DataTypeDefinition DataTypeDefinition .set
            var propertyType = new PropertyType(_propertyType1.Id);
            var oldValue = propertyType.DataTypeDefinition;
            try
            {
                var newValue = new DataTypeDefinition(TEST_DATA_TYPE_ID3);
 
                propertyType.DataTypeDefinition = newValue;

                var dtoWithNewValue = getTestPropertyTypeDto(propertyType.Id);
                Assert.That(dtoWithNewValue.DataTypeId, Is.EqualTo(newValue.Id), "DataTypeDefinition .set test(1) failed");

                var PropertyTypeWithNewValue = new PropertyType(_propertyType1.Id);
                Assert.That(PropertyTypeWithNewValue.DataTypeDefinition.Id, Is.EqualTo(propertyType.DataTypeDefinition.Id), "DataTypeDefinition .set test(2) failed");
            }
            finally
            {
                propertyType.DataTypeDefinition = oldValue; 
            }
        }

        [Test(Description = "Test 'public int TabId .set => PropertyTypeGroup' property")]
        public void Test_PropertyType_TabId_Set()
        {
            //public int TabId .set => PropertyTypeGroup
            var propertyType = new PropertyType(_propertyType1.Id);
            var oldValue = propertyType.TabId;
            try
            {
                var newValue = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup3.Id);

                propertyType.TabId = newValue.Id;

                var dtoWithNewValue = getTestPropertyTypeDto(propertyType.Id);
                Assert.That(dtoWithNewValue.PropertyTypeGroupId, Is.EqualTo(newValue.Id), "TabId .set test(1) failed");

                var PropertyTypeWithNewValue = new PropertyType(_propertyType1.Id);
                Assert.That(PropertyTypeWithNewValue.TabId, Is.EqualTo(propertyType.TabId), "TabId .set test(2) failed");
            }
            finally
            {
                propertyType.TabId = oldValue;
            }
        }

        [Test(Description = "Test 'public int PropertyTypeGroup .set' property")]
        public void Test_PropertyType_PropertyTypeGroup_Set()
        {
            //public int PropertyTypeGroup .set
            var propertyType = new PropertyType(_propertyType1.Id);
            var oldValue = propertyType.PropertyTypeGroup;
            try
            {
                var newValue = PropertyTypeGroup.GetPropertyTypeGroup(_propertyTypeGroup3.Id);

                propertyType.PropertyTypeGroup = newValue.Id;

                var dtoWithNewValue = getTestPropertyTypeDto(propertyType.Id);
                Assert.That(dtoWithNewValue.PropertyTypeGroupId, Is.EqualTo(newValue.Id), "PropertyTypeGroup .set test(1) failed");

                var PropertyTypeWithNewValue = new PropertyType(_propertyType1.Id);
                Assert.That(PropertyTypeWithNewValue.PropertyTypeGroup, Is.EqualTo(propertyType.PropertyTypeGroup), "PropertyTypeGroup .set test(2) failed");
            }
            finally
            {
                propertyType.PropertyTypeGroup = oldValue;
            }
        }
        
        [Test(Description = "Test 'public bool Mandatory .set' property")]
        public void Test_PropertyType_Mandatory_Set()
        {
            //public bool Mandatory .set
            var propertyType = new PropertyType(_propertyType1.Id);
            var oldValue = propertyType.Mandatory;
            try
            {
                var newValue = !oldValue; 

                propertyType.Mandatory = newValue;

                var dtoWithNewValue = getTestPropertyTypeDto(propertyType.Id);
                Assert.That(dtoWithNewValue.Mandatory, Is.EqualTo(newValue), "Mandatory .set test(1) failed");

                var PropertyTypeWithNewValue = new PropertyType(_propertyType1.Id);
                Assert.That(PropertyTypeWithNewValue.Mandatory, Is.EqualTo(propertyType.Mandatory), "Mandatory .set test(2) failed");
            }
            finally
            {
                propertyType.Mandatory = oldValue;
            }
        }

        [Test(Description = "Test 'public string ValidationRegExp .set' property")]
        public void Test_PropertyType_ValidationRegExp_Set()
        {
            // public string ValidationRegExp .set
            var propertyType = new PropertyType(_propertyType1.Id);
            var oldValue = propertyType.ValidationRegExp;
            try
            {
                var newValue = @"[a-b]\w[1-9]";

                propertyType.ValidationRegExp = newValue;

                var dtoWithNewValue = getTestPropertyTypeDto(propertyType.Id);
                Assert.That(dtoWithNewValue.ValidationRegExp, Is.EqualTo(newValue), "ValidationRegExp .set test(1) failed");

                var PropertyTypeWithNewValue = new PropertyType(_propertyType1.Id);
                Assert.That(PropertyTypeWithNewValue.ValidationRegExp, Is.EqualTo(propertyType.ValidationRegExp), "ValidationRegExp .set test(2) failed");
            }
            finally
            {
                propertyType.ValidationRegExp = oldValue;
            }
        }

        [Test(Description = "Test 'public string Description .set' property")]
        public void Test_PropertyType_Description_Set()
        {
            // public string Description .set
            var propertyType = new PropertyType(_propertyType1.Id);
            var oldValue = propertyType.Description;
            try
            {
                var newValue = @"New description";

                propertyType.Description = newValue;

                var dtoWithNewValue = getTestPropertyTypeDto(propertyType.Id);
                Assert.That(dtoWithNewValue.Description, Is.EqualTo(newValue), "Description .set test(1) failed");

                var PropertyTypeWithNewValue = new PropertyType(_propertyType1.Id);
                Assert.That(PropertyTypeWithNewValue.Description, Is.EqualTo(propertyType.Description), "Description .set test(2) failed");
            }
            finally
            {
                propertyType.Description = oldValue;
            }
        }

        [Test(Description = "Test 'public string SortOrder .set' property")]
        public void Test_PropertyType_SortOrder_Set()
        {
            // public string SortOrder .set
            var propertyType = new PropertyType(_propertyType1.Id);
            var oldValue = propertyType.SortOrder;
            try
            {
                var newValue = 7;

                propertyType.SortOrder = newValue;

                var dtoWithNewValue = getTestPropertyTypeDto(propertyType.Id);
                Assert.That(dtoWithNewValue.SortOrder, Is.EqualTo(newValue), "SortOrder .set test(1) failed");

                var PropertyTypeWithNewValue = new PropertyType(_propertyType1.Id);
                Assert.That(PropertyTypeWithNewValue.SortOrder, Is.EqualTo(propertyType.SortOrder), "SortOrder .set test(2) failed");
            }
            finally
            {
                propertyType.SortOrder = oldValue;
            }
        }

        [Test(Description = "Test 'public string Alias .set' property")]
        public void Test_PropertyType_Alias_Set()
        {
            // public string Alias .set
            var propertyType = new PropertyType(_propertyType1.Id);
            var oldValue = propertyType.Alias;
            try
            {
                var newValue = "theNewAliasValue";

                propertyType.Alias = newValue;

                var dtoWithNewValue = getTestPropertyTypeDto(propertyType.Id);
                Assert.That(dtoWithNewValue.Alias, Is.EqualTo(newValue), "Alias .set test(1) failed");

                var PropertyTypeWithNewValue = new PropertyType(_propertyType1.Id);
                Assert.That(PropertyTypeWithNewValue.Alias, Is.EqualTo(propertyType.Alias), "Alias .set test(2) failed");
            }
            finally
            {
                propertyType.Alias = oldValue;
            }
        }

        [Test(Description = "Test 'public string Name .set' property")]
        public void Test_PropertyType_Name_Set()
        {
            // public string Name .set
            var propertyType = new PropertyType(_propertyType1.Id);
            var oldValue = propertyType.Name;
            try
            {
                var newValue = "The New Name Value";

                propertyType.Name = newValue;

                var dtoWithNewValue = getTestPropertyTypeDto(propertyType.Id);
                Assert.That(dtoWithNewValue.Name, Is.EqualTo(newValue), "Name .set test(1) failed");

                var PropertyTypeWithNewValue = new PropertyType(_propertyType1.Id);
                Assert.That(PropertyTypeWithNewValue.Name, Is.EqualTo(propertyType.Name), "Name .set test(2) failed");
            }
            finally
            {
                propertyType.Name = oldValue;
            }
        }

        [Test(Description = "Test 'public static PropertyType[] GetAll()' method")]
        public void Test_PropertyType_GetAll()
        {
            // public static PropertyType[] GetAll()
            var all1 = PropertyType.GetAll();

            EnsureAllTestRecordsAreDeleted(); // force reset test data 

            var all2 = PropertyType.GetAll();

            Assert.That(all2.Length + 3, Is.EqualTo(all1.Length), "GetAll test failed");

        }

        [Test(Description = "Test 'public static IEnumerable<PropertyType> GetPropertyTypes()' method")]
        public void Test_PropertyType_GetPropertyTypes()
        {
            // public static IEnumerable<PropertyType> GetPropertyTypes()
            var all1 = PropertyType.GetPropertyTypes().ToArray() ; 

            EnsureAllTestRecordsAreDeleted(); // force reset test data 

            var all2 = PropertyType.GetPropertyTypes().ToArray();

            Assert.That(all2.Length + 3, Is.EqualTo(all1.Length), "GetPropertyTypes test failed");

        }

        [Test(Description = "Test 'public static PropertyType GetPropertyType(int id)' method")]
        public void Test_PropertyType_GetPropertyType()
        {
            // public static PropertyType GetPropertyType(int id)
            var propertyType = PropertyType.GetPropertyType(_propertyType1.Id);

            Assert.That(propertyType, !Is.Null, "Object instance !Is.Null test failed");

            var savedPropertyTypeDto = getTestPropertyTypeDto(propertyType.Id);

            Assert.That(propertyType.Id, Is.EqualTo(savedPropertyTypeDto.Id), "Id test failed");

            Assert.That(propertyType.DataTypeDefinition.Id, Is.EqualTo(savedPropertyTypeDto.DataTypeId), "DataTypeId test failed");
            Assert.That(propertyType.TabId, Is.EqualTo(savedPropertyTypeDto.PropertyTypeGroupId), "PropertyTypeGroupId test failed");
            Assert.That(propertyType.PropertyTypeGroup, Is.EqualTo(savedPropertyTypeDto.PropertyTypeGroupId), "PropertyTypeGroup test failed");
            Assert.That(propertyType.Mandatory, Is.EqualTo(savedPropertyTypeDto.Mandatory), "Mandatory property test failed");
            Assert.That(propertyType.ValidationRegExp, Is.EqualTo(savedPropertyTypeDto.ValidationRegExp), "ValidationRegExp test failed");
            Assert.That(propertyType.Description, Is.EqualTo(savedPropertyTypeDto.Description), "Description test failed");
            Assert.That(propertyType.SortOrder, Is.EqualTo(savedPropertyTypeDto.SortOrder), "SortOrder test failed");
            Assert.That(propertyType.Alias, Is.EqualTo(savedPropertyTypeDto.Alias), "Alias test failed");
            Assert.That(propertyType.Name, Is.EqualTo(savedPropertyTypeDto.Name), "Name test failed");

        } 

        [Test(Description = "Test 'public static IEnumerable<PropertyType> GetPropertyTypesByGroup(int groupId)' method")]
        public void Test_PropertyType_GetPropertyTypesByGroup_1()
        {
            // public static IEnumerable<PropertyType> GetPropertyTypesByGroup(int groupId)
            var all = PropertyType.GetPropertyTypesByGroup(_propertyTypeGroup1.Id).ToArray() ;

            Assert.That(all.Length, Is.EqualTo(2));  // there are two properties saved for _propertyTypeGroup1.Id

        }

        [Test(Description = "Test 'public static IEnumerable<PropertyType> GetPropertyTypesByGroup(int groupId, List<int> contentTypeIds)' method")]
        public void Test_PropertyType_GetPropertyTypesByGroup_2()
        {
            // public static IEnumerable<PropertyType> GetPropertyTypesByGroup(int groupId, List<int> contentTypeIds)
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
            // public static IEnumerable<PropertyType> GetByDataTypeDefinition(int dataTypeDefId)
            var all1 = PropertyType.GetByDataTypeDefinition(TEST_DATA_TYPE_ID1).ToArray();

            EnsureAllTestRecordsAreDeleted(); // force reset test data 

            var all2 = PropertyType.GetByDataTypeDefinition(TEST_DATA_TYPE_ID1).ToArray();

            Assert.That(all1.Length, Is.EqualTo(all2.Length + 1)); // this test suite adds just one PropertyType for TEST_DATA_TYPE_ID1

        }

        //
        // not tested
        //
        //public virtual void Save()
        //public void FlushCacheBasedOnTab()
        //public IDataType GetEditControl(object value, bool isPostBack)


        #endregion
    }
}
