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
using umbraco.cms.businesslogic.property;
using umbraco.interfaces;

namespace Umbraco.Tests.BusinessLogic
{

    [TestFixture]
    public class cms_businesslogic_Property_Tests : BaseDatabaseFactoryTestWithContext
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

                //_propertyData1 = insertPropertyTypeData(_propertyType1.Id, _contentType1.NodeId);
                //_propertyData2 = insertPropertyTypeData(_propertyType1.Id, _contentType2.NodeId);
                //_propertyData3 = insertPropertyTypeData(_propertyType2.Id, _contentType1.NodeId);
                _propertyData1 = insertPropertyTypeData(_propertyType1.Id, _contentType1.NodeId);
                _propertyData2 = insertPropertyTypeData(_propertyType2.Id, _contentType1.NodeId);
                _propertyData3 = insertPropertyTypeData(_propertyType1.Id, _contentType2.NodeId);
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
        public void Test_Property_Constructors()
        {
            // public Property(int Id, propertytype.PropertyType pt) - no db - not tested here
            // public Property(int Id)
            // public Guid VersionId . get

            // ! constuctor fails
            // !!!!!!!!!!!!!
            //  Property class constructor is failing because of unclear reasons. 
            //  Could be that last part of its code is obsolete. 
            //  Suppressed by try {} catch {}. Should be carefully investigated and solved later.
            var property = new Property(_propertyData1.Id);
            var savedPropertyDto = getTestPropertyDataDto(_propertyData1.Id);

            Assert.That(property.Id, Is.EqualTo(savedPropertyDto.Id), "Id test failed");
            Assert.That(property.VersionId, Is.EqualTo(savedPropertyDto.VersionId), "Version test failed");
            Assert.That(property.PropertyType.Id, Is.EqualTo(savedPropertyDto.PropertyTypeId), "PropertyTypeId test failed");
        }

        [Test(Description = "Test 'public static Property MakeNew(propertytype.PropertyType pt, Content c, Guid versionId)' method")]
        public void Test_Property_MakeNew()
        {
            // public static Property MakeNew(propertytype.PropertyType pt, Content c, Guid versionId)

            var propertyType = new PropertyType(_propertyType1.Id);
            var content = new Content(_node1.Id);

            // ! recursively called constructor fails
            var property = Property.MakeNew(propertyType, content, Guid.NewGuid());
            var savedPropertyDto = getTestPropertyDataDto(property.Id);

            Assert.That(property.Id, Is.EqualTo(savedPropertyDto.Id), "Id test failed");
            Assert.That(property.VersionId, Is.EqualTo(savedPropertyDto.VersionId), "Version test failed");
            Assert.That(property.PropertyType.Id, Is.EqualTo(savedPropertyDto.PropertyTypeId), "PropertyTypeId test failed");
        }

        [Test(Description = "Test 'public void delete()' method")]
        public void Test_Property_Delete()
        {
            // public void delete()

            var property = new Property(_propertyData1.Id);  
            var savedPropertyDto = getTestPropertyDataDto(property.Id);

            Assert.That(property.Id, Is.EqualTo(savedPropertyDto.Id), "Id test failed");

            property.delete();

            var savedPropertyDto2 = getTestPropertyDataDto(property.Id);
            Assert.That(savedPropertyDto2, Is.Null);   

            EnsureAllTestRecordsAreDeleted(); 
        }


        #endregion
    }
}
