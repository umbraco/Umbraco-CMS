using System;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using System.Runtime.CompilerServices;
using umbraco.cms.businesslogic.packager;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using Umbraco.Core;
using umbraco.cms.businesslogic.media;
using Umbraco.Tests.BusinessLogic;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.ORM
{
    public abstract partial class BaseORMTest : BaseDatabaseFactoryTest
    {
        protected abstract void EnsureData();

        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get
            {
                //return DatabaseBehavior.NoDatabasePerFixture;
                return DatabaseBehavior.NewSchemaPerFixture; 
            }
        }

        protected TestRepositoryAbstractionLayer TRAL { get; private set; }
        protected bool initialized;
        public override void Initialize()
        {
            base.Initialize();
            if (!initialized)
            {
                this.TRAL = new TestRepositoryAbstractionLayer(GetUmbracoContext("http://localhost", 0));
            }
            EnsureData();
        }

        public const int ADMIN_USER_ID = 0;

        internal User _user;

        internal CMSNode _node1;
        internal CMSNode _node2;
        internal CMSNode _node3;
        internal CMSNode _node4;
        internal CMSNode _node5;

        internal MacroDto _macro1;
        internal MacroDto _macro2;
        internal MacroPropertyTypeDto _macroPropertyType1;
        internal MacroPropertyTypeDto _macroPropertyType2;
        internal MacroPropertyDto _macroProperty1;

        internal InstalledPackageDto _package1;

        internal DataTypeDefinition _dataTypeDefinition1;
        internal int _dataTypeDefinition1_PrevaluesTestCount;
        internal DataTypeDefinition _dataTypeDefinition2;
        internal int _dataTypeDefinition2_PrevaluesTestCount;

        internal umbraco.cms.businesslogic.datatype.PreValue.PreValueDto _preValue;

        internal ContentTypeDto _contentType1;
        internal ContentTypeDto _contentType2;

        internal PropertyTypeGroupDto _propertyTypeGroup1;
        internal PropertyTypeGroupDto _propertyTypeGroup2;
        internal PropertyTypeGroupDto _propertyTypeGroup3;

        internal PropertyTypeDto _propertyType1;
        internal PropertyTypeDto _propertyType2;
        internal PropertyTypeDto _propertyType3;

        internal PropertyDataDto _propertyData1;
        internal PropertyDataDto _propertyData2;
        internal PropertyDataDto _propertyData3;

        internal CMSNode _recycleBinNode1;
        internal CMSNode _recycleBinNode2;
        internal CMSNode _recycleBinNode3;
        internal CMSNode _recycleBinNode4;
        internal CMSNode _recycleBinNode5;

        internal RelationTypeDto _relationType1;
        internal RelationTypeDto _relationType2;

        internal RelationDto _relation1;
        internal RelationDto _relation2;
        internal RelationDto _relation3;


        internal TagDto _tag1;
        internal TagDto _tag2;
        internal TagDto _tag3;
        internal TagDto _tag4;
        internal TagDto _tag5;

        internal TagRelationshipDto _tagRel1;
        internal TagRelationshipDto _tagRel2;
        internal TagRelationshipDto _tagRel3;
        internal TagRelationshipDto _tagRel4;
        internal TagRelationshipDto _tagRel5;

        internal TaskTypeDto _taskType1;
        internal TaskTypeDto _taskType2;
        internal TaskTypeDto _taskType3;

        internal TaskDto _task1;
        internal TaskDto _task2;
        internal TaskDto _task3;
        internal TaskDto _task4;
        internal TaskDto _task5;

        internal TemplateDto _template1; // master
        internal TemplateDto _template2;
        internal TemplateDto _template3;
        internal TemplateDto _template4;
        internal TemplateDto _template5;

        protected const int NON_EXISTENT_TEST_ID_VALUE = 12345;
        protected int TEST_ITEMS_MAX_COUNT = 7;
        protected const string TEST_GROUP_NAME = "my test group";

        #region Ensure_Macro_TestData
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_Macro_TestData()
        {
            if (!initialized)
            {
                const string TEST_MACRO_PROPERTY_TYPE_ALIAS1 = "testAlias1";
                const string TEST_MACRO_PROPERTY_TYPE_ALIAS2 = "testAlias2";

                _macroPropertyType1 = TRAL.Macro.CreateMacroPropertyType(TEST_MACRO_PROPERTY_TYPE_ALIAS1 + uniqueAliasSuffix);
                _macroPropertyType2 = TRAL.Macro.CreateMacroPropertyType(TEST_MACRO_PROPERTY_TYPE_ALIAS2 + uniqueAliasSuffix);

                _macro1 = TRAL.Macro.CreateMacro("Twitter Ads" + uniqueNameSuffix, "twitterAds" + uniqueAliasSuffix);
                _macro2 = TRAL.Macro.CreateMacro("Yahoo Ads" + uniqueNameSuffix, "yahooAds" + uniqueAliasSuffix);

                _macroProperty1 = TRAL.Macro.CreateTestMacroProperty(_macro1.Id, _macroPropertyType1.Id); 
            }

            Assert.That(_macroPropertyType1, !Is.Null);
            Assert.That(_macroPropertyType2, !Is.Null);
            Assert.That(_macro1, !Is.Null);
            Assert.That(_macro2, !Is.Null);
            Assert.That(_macroProperty1, !Is.Null);

            initialized = true;
        }

        protected void EnsureAll_Macro_TestRecordsAreDeleted()
        {
            TRAL.Macro.DeleteMacroProperty(_macroProperty1.Id);
            TRAL.Macro.DeleteMacro(_macro1.Id);
            TRAL.Macro.DeleteMacro(_macro2.Id);
            TRAL.Macro.DeleteMacroPropertyType(_macroPropertyType1.Id);
            TRAL.Macro.DeleteMacroPropertyType(_macroPropertyType2.Id);
            
            initialized = false;
        }

        #endregion

        #region Ensure MacroProperty Test Data

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_MacroProperty_TestData()
        {
            Ensure_Macro_TestData();
        }

        protected void EnsureAll_MacroProperty_TestRecordsAreDeleted()
        {
            EnsureAll_Macro_TestRecordsAreDeleted();
        }
        #endregion

        #region Ensure MacroPropertyType Test Data
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_MacroPropertyType_TestData()
        {
            if (!initialized)
            {
                _macroPropertyType1 = TRAL.Macro.CreateMacroPropertyType("testAlias" + uniqueAliasSuffix);  
            }

            Assert.That(_macroPropertyType1, !Is.Null);

            initialized = true;
        }

 
        protected void EnsureAll_MacroPropertyType_TestRecordsAreDeleted()
        {
            TRAL.Macro.DeleteMacroPropertyType(_macroPropertyType1.Id);
            initialized = false;
        }

        #endregion

        #region Ensure Package Test Data
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_Package_TestData()
        {
            if (!initialized)
            {
                // ******************************************************************************************************************
                //
                //  [umbracoInstalledPackages] table doesn't exist (yet?)
                // 
                // failed: System.Data.SqlServerCe.SqlCeException : The specified table does not exist. [ umbracoInstalledPackages ]
                //
                // ******************************************************************************************************************
            }

            initialized = true;
        }

        protected void EnsureAll_Package_TestRecordsAreDeleted()
        {
            initialized = false;
        }

        #endregion

        #region Ensure PreValue Test Data
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_PreValue_TestData()
        {
            if (!initialized)
            {
                _user = TRAL.Users.GetUser(ADMIN_USER_ID);
                _dataTypeDefinition1 = TRAL.PreValue.CreateDataTypeDefinition(_user, "Nvarchar");
                _dataTypeDefinition2 = TRAL.PreValue.CreateDataTypeDefinition(_user, "Ntext");
            }

            var list = TRAL.PreValue.CreateDataTypePrevalues(_dataTypeDefinition1.Id, 3);
             if (list.Count > 0)  _preValue = list[0];
            _dataTypeDefinition1_PrevaluesTestCount = TRAL.PreValue.CountPreValuesByDataTypeId(_dataTypeDefinition1.Id);

            var list2 = TRAL.PreValue.CreateDataTypePrevalues(_dataTypeDefinition2.Id, 1);
            _dataTypeDefinition2_PrevaluesTestCount = TRAL.PreValue.CountPreValuesByDataTypeId(_dataTypeDefinition2.Id);

            Assert.That(_user, !Is.Null);
            Assert.That(_dataTypeDefinition1, !Is.Null);
            Assert.That(_dataTypeDefinition2, !Is.Null);
            Assert.That(_preValue, !Is.Null);

            initialized = true;
        }

        protected void EnsureAll_PreValue_TestRecordsAreDeleted()
        {
            TRAL.PreValue.DeletePreValuesByDataTypeDefinitionId(_dataTypeDefinition1.Id);
            TRAL.PreValue.DeletePreValuesByDataTypeDefinitionId(_dataTypeDefinition2.Id);

            TRAL.PreValue.DeleteDataTypeByDataTypeDefinitionId(_dataTypeDefinition1.Id);
            TRAL.PreValue.DeleteNodeByDataTypeDefinitionId(_dataTypeDefinition1.Id);

            TRAL.PreValue.DeleteDataTypeByDataTypeDefinitionId(_dataTypeDefinition2.Id);
            TRAL.PreValue.DeleteNodeByDataTypeDefinitionId(_dataTypeDefinition2.Id);    
 
            initialized = false;
        }
        #endregion

        #region Ensure PreValue Test Data
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_PreValues_TestData()
        {
            Ensure_PreValue_TestData();
        }

        protected void EnsureAll_PreValues_TestRecordsAreDeleted()
        {
            EnsureAll_PreValue_TestRecordsAreDeleted();
        }
        #endregion

        #region Ensure Property Test Data
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_Property_TestData()
        {
            // cmsDataType records are expected to be in the test database - 
            // otherwise it'd bee too much data to setup for this test suite

            const int TEST_DATA_TYPE_ID1 = -90;
            const int TEST_DATA_TYPE_ID2 = -36;
            const int TEST_DATA_TYPE_ID3 = -41;

            if (!initialized)
            {
                _node1 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 1", Document._objectType);
                _node2 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 2", Document._objectType);
                _node3 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 3", Document._objectType);
                _node4 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 4", Document._objectType);
                _node5 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 5", Document._objectType);

                _contentType1 = TRAL.Property.CreateContentType(1, _node1.Id, "testMedia1");
                _contentType2 = TRAL.Property.CreateContentType(1, _node2.Id, "testMedia2");

                _propertyTypeGroup1 = TRAL.Property.CreatePropertyTypeGroup(_contentType1.NodeId, (int?)null, "book");
                _propertyTypeGroup2 = TRAL.Property.CreatePropertyTypeGroup(_contentType2.NodeId, (int?)_propertyTypeGroup1.Id, "CD");
                _propertyTypeGroup3 = TRAL.Property.CreatePropertyTypeGroup(_contentType1.NodeId, (int?)_propertyTypeGroup1.Id, "DVD");

                _propertyType1 = TRAL.Property.CreatePropertyType(TEST_DATA_TYPE_ID1, _contentType1.NodeId, _propertyTypeGroup1.Id, "Test Property Type 1");
                _propertyType2 = TRAL.Property.CreatePropertyType(TEST_DATA_TYPE_ID2, _contentType2.NodeId, _propertyTypeGroup2.Id, "Test Property Type 2");
                _propertyType3 = TRAL.Property.CreatePropertyType(TEST_DATA_TYPE_ID3, _contentType2.NodeId, _propertyTypeGroup1.Id, "Test Property Type 3");

                _propertyData1 = TRAL.Property.CreatePropertyTypeData(_propertyType1.Id, _contentType1.NodeId);
                _propertyData2 = TRAL.Property.CreatePropertyTypeData(_propertyType2.Id, _contentType1.NodeId);
                _propertyData3 = TRAL.Property.CreatePropertyTypeData(_propertyType1.Id, _contentType2.NodeId);
            }

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

            initialized = true;
        }

 
        protected void EnsureAll_Property_TestRecordsAreDeleted()
        {
            TRAL.Property.DeletePropertyDataById(_propertyData3.Id);
            TRAL.Property.DeletePropertyDataById(_propertyData2.Id);
            TRAL.Property.DeletePropertyDataById(_propertyData1.Id);

            TRAL.Property.DeletePropertyTypeById(_propertyType3.Id);
            TRAL.Property.DeletePropertyTypeById(_propertyType2.Id);
            TRAL.Property.DeletePropertyTypeById(_propertyType1.Id);

            TRAL.Property.DeletePropertyTypeGroupById(_propertyTypeGroup3.Id);
            TRAL.Property.DeletePropertyTypeGroupById(_propertyTypeGroup2.Id);
            TRAL.Property.DeletePropertyTypeGroupById(_propertyTypeGroup1.Id);

            TRAL.Property.DeleteContentTypeByPrimaryKey(_contentType2.PrimaryKey);
            TRAL.Property.DeleteContentTypeByPrimaryKey(_contentType1.PrimaryKey);

            // nodes deletion recursively deletes all relations...
            if (_node1 != null) _node1.delete();
            if (_node2 != null) _node2.delete();
            if (_node3 != null) _node3.delete();
            if (_node4 != null) _node4.delete();
            if (_node5 != null) _node5.delete();

            initialized = false;
        }

        #endregion

        #region Ensure Property Type Test Data
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_PropertyType_TestData()
        {
            Ensure_Property_TestData();
        }
        protected void EnsureAll_PropertyType_TestRecordsAreDeleted()
        {
            EnsureAll_Property_TestRecordsAreDeleted();
        }
        #endregion

        #region Ensure Property Type Group Test Data
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_PropertyTypeGroup_TestData()
        {
            Ensure_Property_TestData();
        }
        protected void EnsureAll_PropertyTypeGroup_TestRecordsAreDeleted()
        {
            EnsureAll_Property_TestRecordsAreDeleted();
        }
        #endregion


        #region Ensure RecycleBin Test Data
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_RecycleBin_TestData()
        {
            _recycleBinNode1 = TRAL.Nodes.MakeNew(Constants.System.RecycleBinMedia, 2, "Test Media Content 1", Media._objectType);
            _recycleBinNode2 = TRAL.Nodes.MakeNew(Constants.System.RecycleBinMedia, 2, "Test Media Content 2", Media._objectType);
            _recycleBinNode3 = TRAL.Nodes.MakeNew(Constants.System.RecycleBinMedia, 2, "Test Media Content 3", Media._objectType);
            _recycleBinNode4 = TRAL.Nodes.MakeNew(Constants.System.RecycleBinContent, 2, "Test Content 1", Document._objectType);
            _recycleBinNode5 = TRAL.Nodes.MakeNew(Constants.System.RecycleBinContent, 2, "Test Content 2", Document._objectType);

            initialized = true;

            Assert.That(_recycleBinNode1, !Is.Null);
            Assert.That(_recycleBinNode2, !Is.Null);
            Assert.That(_recycleBinNode3, !Is.Null);
            Assert.That(_recycleBinNode4, !Is.Null);
            Assert.That(_recycleBinNode5, !Is.Null);

        }

        protected void EnsureAll_RecycleBin_TestRecordsAreDeleted()
        {
            _recycleBinNode5.delete();
            _recycleBinNode4.delete();
            _recycleBinNode3.delete();
            _recycleBinNode2.delete();
            _recycleBinNode1.delete();

            initialized = false;
        }
        #endregion

        #region Ensure Relation Test Data()

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_Relation_TestData()
        {
            if (!initialized)
            {
                _node1 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 1", Document._objectType);
                _node2 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 2", Document._objectType);
                _node3 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 3", Document._objectType);
                _node4 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 4", Document._objectType);
                _node5 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 5", Document._objectType);

                _relationType1 = TRAL.Relation.CreateTestRelationType();
                _relationType2 = TRAL.Relation.CreateTestRelationType();

                _relation1 = TRAL.Relation.CreateTestRelation(_relationType1.Id, _node1.Id, _node2.Id, "node1(parent) <-> node2(child)");
                _relation2 = TRAL.Relation.CreateTestRelation(_relationType1.Id, _node2.Id, _node3.Id, "node2(parent) <-> node3(child)");
                _relation3 = TRAL.Relation.CreateTestRelation(_relationType2.Id, _node1.Id, _node3.Id, "node1(parent) <-> node3(child)");

            }

            Assert.That(_node1, !Is.Null);
            Assert.That(_node2, !Is.Null);
            Assert.That(_node3, !Is.Null);
            Assert.That(_node4, !Is.Null);
            Assert.That(_node5, !Is.Null);

            Guid objectType = Document._objectType;
            int expectedValue = TRAL.Nodes.CountNodesByObjectTypeGuid( objectType);
            Assert.That(CMSNode.CountByObjectType(Document._objectType), Is.EqualTo(expectedValue));

            Assert.That(_relationType1, !Is.Null);
            Assert.That(_relationType2, !Is.Null);
            Assert.That(_relation1, !Is.Null);
            Assert.That(_relation2, !Is.Null);
            Assert.That(_relation3, !Is.Null);

            initialized = true;
        }

        protected void EnsureAll_Relation_TestRecordsAreDeleted()
        {
            // nodes deletion recursively deletes all relations...
            if (_node1 != null) _node1.delete();
            if (_node2 != null) _node2.delete();
            if (_node3 != null) _node3.delete();
            if (_node4 != null) _node4.delete();
            if (_node5 != null) _node5.delete();

            //... bit not relation types
            if (_relationType1 != null) TRAL.Relation.DeleteRelationType(_relationType1.Id);
            if (_relationType2 != null) TRAL.Relation.DeleteRelationType(_relationType2.Id);

            initialized = false;
        }
        #endregion

        #region Ensure RelationType Data
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_RelationType_TestData()
        {
            if (!initialized)
            {
                _relationType1 = TRAL.Relation.CreateTestRelationType();
                _relationType2 = TRAL.Relation.CreateTestRelationType();
            }

            Assert.That(_relationType1, !Is.Null);
            Assert.That(_relationType2, !Is.Null);

            initialized = true;
        }


        protected void EnsureAll_RelationType_TestRecordsAreDeleted()
        {
            if (_relationType1 != null) TRAL.Relation.DeleteRelationType(_relationType1.Id);
            if (_relationType2 != null) TRAL.Relation.DeleteRelationType(_relationType2.Id);

            initialized = false;
        }

        #endregion

        #region Ensure Tag Test Data

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_Tag_TestData()
        {
            if (!initialized)
            {
                _node1 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 1", Document._objectType);
                _node2 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 2", Document._objectType);
                _node3 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 3", Document._objectType);
                _node4 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 4", Document._objectType);
                _node5 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 5", Document._objectType);

                _tag1 = TRAL.Tag.CreateTestTag("Tag11", "1");
                _tag2 = TRAL.Tag.CreateTestTag("Tag12", "1");
                _tag3 = TRAL.Tag.CreateTestTag("Tag13", "1");
                _tag4 = TRAL.Tag.CreateTestTag("Tag21", "2");
                _tag5 = TRAL.Tag.CreateTestTag("Tag22", "2");

                _tagRel1 = TRAL.Tag.CreateTestTagRelationship(_node1.Id, _tag1.Id);
                _tagRel2 = TRAL.Tag.CreateTestTagRelationship(_node1.Id, _tag2.Id);
                _tagRel3 = TRAL.Tag.CreateTestTagRelationship(_node1.Id, _tag3.Id);
                _tagRel4 = TRAL.Tag.CreateTestTagRelationship(_node2.Id, _tag4.Id);
                _tagRel5 = TRAL.Tag.CreateTestTagRelationship(_node3.Id, _tag5.Id);
            }

            Assert.That(_tag1, !Is.Null);
            Assert.That(_tag2, !Is.Null);
            Assert.That(_tag3, !Is.Null);
            Assert.That(_tag4, !Is.Null);
            Assert.That(_tag5, !Is.Null);

            Assert.That(_tagRel1, !Is.Null);
            Assert.That(_tagRel2, !Is.Null);
            Assert.That(_tagRel3, !Is.Null);
            Assert.That(_tagRel4, !Is.Null);
            Assert.That(_tagRel5, !Is.Null);

            Assert.That(_node1, !Is.Null);
            Assert.That(_node2, !Is.Null);
            Assert.That(_node3, !Is.Null);
            Assert.That(_node4, !Is.Null);
            Assert.That(_node5, !Is.Null);

            initialized = true;
        }

        protected void EnsureAll_Tag_TestRecordsAreDeleted()
        {
            TRAL.Tag.DeleteTagRelationShip(_tagRel1);
            TRAL.Tag.DeleteTagRelationShip(_tagRel2);
            TRAL.Tag.DeleteTagRelationShip(_tagRel3);
            TRAL.Tag.DeleteTagRelationShip(_tagRel4);
            TRAL.Tag.DeleteTagRelationShip(_tagRel5);

            TRAL.Tag.Delete(_tag1);
            TRAL.Tag.Delete(_tag2);
            TRAL.Tag.Delete(_tag3);
            TRAL.Tag.Delete(_tag4);
            TRAL.Tag.Delete(_tag5);

            // nodes deletion recursively deletes all relations...
            if (_node1 != null) _node1.delete();
            if (_node2 != null) _node2.delete();
            if (_node3 != null) _node3.delete();
            if (_node4 != null) _node4.delete();
            if (_node5 != null) _node5.delete();

 
            initialized = false;
        }
 
        #endregion


       #region Ensure Task Test Data
       [MethodImpl(MethodImplOptions.Synchronized)]
       protected void Ensure_Task_TestData()
       {
           if (!initialized)
           {
               // results in runtime error for DatabaseBehavior.NewSchemaPerFixture; 
               // see - https://groups.google.com/forum/#!topic/umbraco-dev/vzIg6XbBsSU
               //var t = new TaskType()
               //{
               //    Alias = "Test Task Type"
               //};
               //t.Save();

               //  works well for DatabaseBehavior.NewSchemaPerFixture; 
               //var relationType = new RelationType()
               //{
               //    Name = Guid.NewGuid().ToString(),
               //    Alias = Guid.NewGuid().ToString(),
               //    Dual = false,
               //};
               //relationType.Save(); 

               // OK - database.Execute("insert into [cmsMacroPropertyType] (macroPropertyTypeAlias) VALUES (@0)", "TEST");
               // run-time error - database.Execute("insert into [cmsTaskType] (alias) VALUES (@0)", "TEST");

               _node1 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 1", Document._objectType);
               _node2 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 2", Document._objectType);
               _node3 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 3", Document._objectType);
               _node4 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 4", Document._objectType);
               _node5 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 5", Document._objectType);

               _user = new User(0);

               _taskType1 = TRAL.Task.CreateTestTaskType(TRAL.Task.NewTaskTypeAlias);
               _task1 = TRAL.Task.CreateTestTask(_taskType1, _node1, _user, _user, "TODO - #1");
               _task2 = TRAL.Task.CreateTestTask(_taskType1, _node1, _user, _user, "TODO - #2", closed: true);
               _task3 = TRAL.Task.CreateTestTask(_taskType1, _node2, _user, _user, "TODO - #3");

               _taskType2 = TRAL.Task.CreateTestTaskType(TRAL.Task.NewTaskTypeAlias);
               _task4 = TRAL.Task.CreateTestTask(_taskType1, _node1, _user, _user, "TODO - #4");
               _task5 = TRAL.Task.CreateTestTask(_taskType1, _node3, _user, _user, "TODO - #5", closed: true);
               _taskType3 = TRAL.Task.CreateTestTaskType(TRAL.Task.NewTaskTypeAlias);
           }

           Assert.That(_user, !Is.Null);

           Assert.That(_task1, !Is.Null);
           Assert.That(_task2, !Is.Null);
           Assert.That(_task3, !Is.Null);
           Assert.That(_task4, !Is.Null);
           Assert.That(_task5, !Is.Null);

           Assert.That(_taskType1, !Is.Null);
           Assert.That(_taskType2, !Is.Null);
           Assert.That(_taskType3, !Is.Null);

           Assert.That(_node1, !Is.Null);
           Assert.That(_node2, !Is.Null);
           Assert.That(_node3, !Is.Null);
           Assert.That(_node4, !Is.Null);
           Assert.That(_node5, !Is.Null);

           initialized = true;
       }

       protected void EnsureAll_Task_TestRecordsAreDeleted()
       {
           TRAL.Task.DeleteTask(_task1);
           TRAL.Task.DeleteTask(_task2);
           TRAL.Task.DeleteTask(_task3);
           TRAL.Task.DeleteTask(_task4);
           TRAL.Task.DeleteTask(_task5);

           TRAL.Task.DeleteTaskType(_taskType1);
           TRAL.Task.DeleteTaskType(_taskType2);
           TRAL.Task.DeleteTaskType(_taskType3);

           initialized = false;
       }

       #endregion



       #region Ensure Task Type Data
       [MethodImpl(MethodImplOptions.Synchronized)]
       protected void Ensure_TaskType_TestData()
       {
           if (!initialized)
           {
               // results in runtime error for DatabaseBehavior.NewSchemaPerFixture; 
               // see - https://groups.google.com/forum/#!topic/umbraco-dev/vzIg6XbBsSU
               //var t = new TaskType()
               //{
               //    Alias = "Test Task Type"
               //};
               //t.Save();

               //  works well for DatabaseBehavior.NewSchemaPerFixture; 
               //var relationType = new RelationType()
               //{
               //    Name = Guid.NewGuid().ToString(),
               //    Alias = Guid.NewGuid().ToString(),
               //    Dual = false,
               //};
               //relationType.Save(); 

               // OK - database.Execute("insert into [cmsMacroPropertyType] (macroPropertyTypeAlias) VALUES (@0)", "TEST");
               // run-time error - database.Execute("insert into [cmsTaskType] (alias) VALUES (@0)", "TEST");

               _node1 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 1", umbraco.cms.businesslogic.web.Document._objectType);
               _node2 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 2", Document._objectType);
               _node3 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 3", Document._objectType);
               _node4 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 4", Document._objectType);
               _node5 = TRAL.Nodes.MakeNew(-1, 1, "TestContent 5", Document._objectType);

               _user = new User(ADMIN_USER_ID);

               _taskType1 = TRAL.Task.CreateTestTaskType(TRAL.Task.NewTaskTypeAlias);
               _task1 = TRAL.Task.CreateTestTask(_taskType1, _node1, _user, _user, "TODO - #1");
               _task2 = TRAL.Task.CreateTestTask(_taskType1, _node1, _user, _user, "TODO - #2");
               _task3 = TRAL.Task.CreateTestTask(_taskType1, _node1, _user, _user, "TODO - #3");

               _taskType2 = TRAL.Task.CreateTestTaskType(TRAL.Task.NewTaskTypeAlias);
               _task4 = TRAL.Task.CreateTestTask(_taskType1, _node1, _user, _user, "TODO - #4");
               _task5 = TRAL.Task.CreateTestTask(_taskType1, _node1, _user, _user, "TODO - #5");
               _taskType3 = TRAL.Task.CreateTestTaskType(TRAL.Task.NewTaskTypeAlias);

           }

           Assert.That(_task1, !Is.Null);
           Assert.That(_task2, !Is.Null);
           Assert.That(_task3, !Is.Null);
           Assert.That(_task4, !Is.Null);
           Assert.That(_task5, !Is.Null);

           Assert.That(_taskType1, !Is.Null);
           Assert.That(_taskType2, !Is.Null);
           Assert.That(_taskType3, !Is.Null);

           Assert.That(_user, !Is.Null);

           Assert.That(_node1, !Is.Null);
           Assert.That(CMSNode.IsNode(_node1.Id), Is.True);
           Assert.That(_node2, !Is.Null);
           Assert.That(CMSNode.IsNode(_node2.Id), Is.True);
           Assert.That(_node3, !Is.Null);
           Assert.That(CMSNode.IsNode(_node3.Id), Is.True);
           Assert.That(_node4, !Is.Null);
           Assert.That(CMSNode.IsNode(_node4.Id), Is.True);
           Assert.That(_node5, !Is.Null);
           Assert.That(CMSNode.IsNode(_node5.Id), Is.True);

           initialized = true;
       }


       protected void EnsureAll_TaskType_TestRecordsAreDeleted()
       {
           TRAL.Task.DeleteTask(_task1);
           TRAL.Task.DeleteTask(_task2);
           TRAL.Task.DeleteTask(_task3);
           TRAL.Task.DeleteTask(_task4);
           TRAL.Task.DeleteTask(_task5);

           TRAL.Task.DeleteTaskType(_taskType1);
           TRAL.Task.DeleteTaskType(_taskType2);
           TRAL.Task.DeleteTaskType(_taskType3);

           initialized = false;
       }

       #endregion

       #region EnsureData


       [MethodImpl(MethodImplOptions.Synchronized)]
       protected void Ensure_Template_TestData()
       {
           if (!initialized)
           {
               _user = TRAL.Users.GetUser(ADMIN_USER_ID);    

               _template1 = TRAL.Template.CreateTemplate(null, TestRepositoryAbstractionLayer.TemplateRepository.TemplatesText.Master, _user.Id, 1);
               _template2 = TRAL.Template.CreateTemplate(_template1.NodeId, TestRepositoryAbstractionLayer.TemplateRepository.TemplatesText.NewsArticle, _user.Id, 2);
               _template3 = TRAL.Template.CreateTemplate(_template1.NodeId, TestRepositoryAbstractionLayer.TemplateRepository.TemplatesText.HomePage, _user.Id, 2);
               _template4 = TRAL.Template.CreateTemplate(null, TestRepositoryAbstractionLayer.TemplateRepository.TemplatesText.CommentRSS, _user.Id, 1);
               _template5 = TRAL.Template.CreateTemplate(null, TestRepositoryAbstractionLayer.TemplateRepository.TemplatesText.RSS, _user.Id, 1);
           }

           Assert.That(_user, !Is.Null);

           Assert.That(_template1, !Is.Null);
           Assert.That(_template2, !Is.Null);
           Assert.That(_template3, !Is.Null);
           Assert.That(_template4, !Is.Null);
           Assert.That(_template5, !Is.Null);

           initialized = true;
       }

       protected void EnsureAll_Template_TestRecordsAreDeleted()
       {
           TRAL.Template.Delete(_template2);
           TRAL.Template.Delete(_template3);
           TRAL.Template.Delete(_template4);
           TRAL.Template.Delete(_template5);
           TRAL.Template.Delete(_template1); // master

           initialized = false;
       }

       #endregion


       #region 'One-Liners'
       protected void l(string format, params object[] args)
       {
           System.Console.WriteLine(format, args);
       }

       protected string uniqueLabel
       {
           get
           {
               return string.Format("* {0} *", uniqueValue);
           }
       }
       protected string uniqueNameSuffix
       {
           get
           {
               return string.Format(" - {0}", uniqueValue);
           }
       }
       protected string uniqueAliasSuffix
       {
           get
           {
               return uniqueValue;
           }
       }
       protected string uniqueValue
       {
           get
           {
               return Guid.NewGuid().ToString();
           }
       }
       #endregion



    }

 
}
