using System.Diagnostics;
using System.Reflection;
using System;
using System.Linq; 
using Umbraco.Web;
using Umbraco.Core.Persistence;
using umbraco.cms.businesslogic;
using System.Xml;
using umbraco.cms.businesslogic.web;
using NUnit.Framework;
using System.Text;
using Umbraco.Core.Models.Rdbms;
using System.Runtime.CompilerServices;
using umbraco.cms.businesslogic.packager;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using System.Collections.Generic;
using Umbraco.Core;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.relation;
using umbraco.cms.businesslogic.task;

namespace Umbraco.Tests.TestHelpers
{
    public abstract partial class BaseORMTest : BaseDatabaseFactoryTest
    {
        protected abstract void EnsureData();

        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get
            {
                return DatabaseBehavior.NoDatabasePerFixture;
                //return DatabaseBehavior.NewSchemaPerFixture; 
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            if (!initialized) CreateContext();
            EnsureData();
        }

        protected bool initialized;

        protected UmbracoDatabase independentDatabase { get { return TRAL.Repository; } }
        protected TestRepositoryAbstractionLayer TRAL { get; private set; }

        protected void CreateContext()
        {
            this.TRAL = new TestRepositoryAbstractionLayer(GetUmbracoContext("http://localhost", 0));
        }

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

        #region Helper methods borrowed from umbraco
        protected class ORMTestCMSNode : CMSNode
        {
            public static CMSNode MakeNew(
                int parentId,
                int level,
                string text,
                Guid objectType)
            {
                return CMSNode.MakeNew(parentId, objectType, 0, level, text, Guid.NewGuid());
            }
        }

        protected string getSqlStringArray(string commaSeparatedArray)
        {
            // create array
            string[] array = commaSeparatedArray.Trim().Split(',');

            // build SQL array
            StringBuilder sqlArray = new StringBuilder();
            foreach (string item in array)
            {
                string trimmedItem = item.Trim();
                if (trimmedItem.Length > 0)
                {
                    sqlArray.Append("'").Append(escapeString(trimmedItem)).Append("',");
                }
            }

            // remove last comma
            if (sqlArray.Length > 0)
                sqlArray.Remove(sqlArray.Length - 1, 1);
            return sqlArray.ToString();
        }

        protected static string escapeString(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("'", "''");
        }

        #endregion


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
        const string TEST_VALUE_VALUE = "test value";
        private const string TEST_RELATION_TYPE_NAME = "Test Relation Type";
        private const string TEST_RELATION_TYPE_ALIAS = "testRelationTypeAlias";
        protected int TEST_ITEMS_MAX_COUNT = 7;
        protected const string TEST_GROUP_NAME = "my test group";
        const string FOLDER_NODE_GUID_STR = "f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d";
        const int FOLDER_NODE_ID = 1031;

        #region Ensure_Macro_TestData
        //
        //  back-end db tables' CrUD-ed in this test suite:
        //  
        //  lookup:     cmdMacroProperty 
        //  production: cmsMacro (anchor), cmsMacroProperty
        //
        // set for and used in this test suite DTO instances: 
        // _macroPropertyType1, _macroPropertyType2, _macro1, _macro2, _macroProperty1
        //

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_Macro_TestData()
        {
            if (!initialized)
            {
                const string TEST_MACRO_PROPERTY_TYPE_ALIAS1 = "testAlias1";
                const string TEST_MACRO_PROPERTY_TYPE_ALIAS2 = "testAlias2";

                const string TEST_MACRO_PROPERTY_NAME = "Your Web Site";
                const string TEST_MACRO_PROPERTY_ALIAS = "yourWebSite";

                _macroPropertyType1 = insertMacroPropertyType(TEST_MACRO_PROPERTY_TYPE_ALIAS1 + uniqueAliasSuffix);
                _macroPropertyType2 = insertMacroPropertyType(TEST_MACRO_PROPERTY_TYPE_ALIAS2 + uniqueAliasSuffix);

                _macro1 = insertMacro("Twitter Ads" + uniqueNameSuffix, "twitterAds" + uniqueAliasSuffix);
                _macro2 = insertMacro("Yahoo Ads" + uniqueNameSuffix, "yahooAds" + uniqueAliasSuffix);


                independentDatabase.Execute("insert into [cmsMacroProperty] (macroPropertyHidden, macroPropertyType, macro, macroPropertySortOrder, macroPropertyAlias, macroPropertyName) " +
                                           " values (@macroPropertyHidden, @macroPropertyType, @macro, @macroPropertySortOrder, @macroPropertyAlias, @macroPropertyName) ",
                                           new
                                           {
                                               macroPropertyHidden = true,
                                               macroPropertyType = _macroPropertyType1.Id,
                                               macro = _macro1.Id,
                                               macroPropertySortOrder = 0,
                                               macroPropertyAlias = TEST_MACRO_PROPERTY_ALIAS + uniqueAliasSuffix ,
                                               macroPropertyName = TEST_MACRO_PROPERTY_NAME + uniqueNameSuffix
                                           });
                int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsMacroProperty]");
                _macroProperty1 = TRAL.GetDto<MacroPropertyDto>(id);
            }

            Assert.That(_macroPropertyType1, !Is.Null);
            Assert.That(_macroPropertyType2, !Is.Null);
            Assert.That(_macro1, !Is.Null);
            Assert.That(_macro2, !Is.Null);
            Assert.That(_macroProperty1, !Is.Null);

            initialized = true;
        }

        private MacroDto insertMacro(string name, string alias)
        {
            independentDatabase.Execute("insert into [cmsMacro] " +
                 " (macroUseInEditor, macroRefreshRate, macroAlias, macroName, macroScriptType, " +
                 " macroScriptAssembly, macroXSLT, macroPython, macroDontRender, macroCacheByPage, macroCachePersonalized) " +
                 " VALUES  " +
                 " (@macroUseInEditor, @macroRefreshRate, @macroAlias, @macroName, @macroScriptType, " +
                 " @macroScriptAssembly, @macroXSLT, @macroPython, @macroDontRender, @macroCacheByPage, @macroCachePersonalized) ",
                 new
                 {
                     macroUseInEditor = true,
                     macroRefreshRate = 0,
                     macroAlias = alias, // "twitterAds",
                     macroName = name, //"Twitter Ads",
                     macroScriptType = "mst",
                     macroScriptAssembly = "",
                     macroXSLT = "some.xslt",
                     macroPython = "",
                     macroDontRender = false,
                     macroCacheByPage = true,
                     macroCachePersonalized = false
                 });
            int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsMacro]");
            return TRAL.GetDto<MacroDto>(id);
        }

        private MacroPropertyTypeDto insertMacroPropertyType(string alias)
        {
            independentDatabase.Execute("insert into [cmsMacroPropertyType] (macroPropertyTypeAlias, macroPropertyTypeRenderAssembly, macroPropertyTypeRenderType, macroPropertyTypeBaseType) " +
                                      " values (@macroPropertyTypeAlias, @macroPropertyTypeRenderAssembly, @macroPropertyTypeRenderType, @macroPropertyTypeBaseType) ",
                                      new
                                      {
                                          macroPropertyTypeAlias = alias,
                                          macroPropertyTypeRenderAssembly = "umbraco.macroRenderings",
                                          macroPropertyTypeRenderType = "context",
                                          macroPropertyTypeBaseType = "string"
                                      });
            int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsMacroPropertyType]");
            return TRAL.GetDto<MacroPropertyTypeDto>(id);
        }

        protected void EnsureAll_Macro_TestRecordsAreDeleted()
        {
            independentDatabase.Execute("delete from  [cmsMacroProperty] where id = @0", _macroProperty1.Id);
            independentDatabase.Execute("delete from  [cmsMacro] where id = @0", _macro1.Id);
            independentDatabase.Execute("delete from  [cmsMacro] where id = @0", _macro2.Id);
            independentDatabase.Execute("delete from  [cmsMacroPropertyType] where id = @0", _macroPropertyType1.Id);
            independentDatabase.Execute("delete from  [cmsMacroPropertyType] where id = @0", _macroPropertyType2.Id);
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
                independentDatabase.Execute("insert into [cmsMacroPropertyType] (macroPropertyTypeAlias, macroPropertyTypeRenderAssembly, macroPropertyTypeRenderType, macroPropertyTypeBaseType) " +
                                           " values (@macroPropertyTypeAlias, @macroPropertyTypeRenderAssembly, @macroPropertyTypeRenderType, @macroPropertyTypeBaseType) ",
                                           new
                                           {
                                               macroPropertyTypeAlias = "testAlias" + uniqueAliasSuffix,
                                               macroPropertyTypeRenderAssembly = "umbraco.macroRenderings",
                                               macroPropertyTypeRenderType = "context",
                                               macroPropertyTypeBaseType = "string"
                                           });
                int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsMacroPropertyType]");
                _macroPropertyType1 = TRAL.GetDto<MacroPropertyTypeDto>(id);
            }

            Assert.That(_macroPropertyType1, !Is.Null);

            initialized = true;
        }

        private static int _testRunNum;

        protected void EnsureAll_MacroPropertyType_TestRecordsAreDeleted()
        {
            independentDatabase.Execute("delete from  [cmsMacroPropertyType] where Id = @0", _macroPropertyType1.Id);
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
                _user = new User(ADMIN_USER_ID);
                _dataTypeDefinition1 = DataTypeDefinition.MakeNew(_user, "Nvarchar");
                _dataTypeDefinition2 = DataTypeDefinition.MakeNew(_user, "Ntext");
            }

            var values = new List<string> 
                {
                    "default",
                    ",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|",
                    "test"
                };

            if ((int)PreValues.Database.ExecuteScalar<int>("select count(*) from cmsDataTypePreValues where datatypenodeid = @0", _dataTypeDefinition1.Id) == 0)
            {
                initialized = false;

                // insert three test PreValue records
                int index = 1;
                values.ForEach(x =>
                {
                    PreValue.Database.Execute(
                        "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid, @value,0,'')",
                        new { dtdefid = _dataTypeDefinition1.Id, value = x });
                    if (index++ == 1)
                    {
                        var id = PreValue.Database.ExecuteScalar<int>("SELECT MAX(id) FROM cmsDataTypePreValues");
                        _preValue = TRAL.GetDto<umbraco.cms.businesslogic.datatype.PreValue.PreValueDto>(id);
                    }

                });

                _dataTypeDefinition1_PrevaluesTestCount = getPreValuesCount(_dataTypeDefinition1.Id);
            }

            if ((int)PreValues.Database.ExecuteScalar<int>("select count(*) from cmsDataTypePreValues where datatypenodeid = @0", _dataTypeDefinition2.Id) == 0)
            {
                initialized = false;

                // insert one test PreValueRecord
                PreValue.Database.Execute(
                    "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",
                    new { dtdefid = _dataTypeDefinition2.Id, value = values[0] });

                _dataTypeDefinition2_PrevaluesTestCount = getPreValuesCount(_dataTypeDefinition2.Id);
            }

            Assert.That(_user, !Is.Null);
            Assert.That(_dataTypeDefinition1, !Is.Null);
            Assert.That(_dataTypeDefinition2, !Is.Null);
            Assert.That(_preValue, !Is.Null);

            initialized = true;
        }

        protected int getPreValuesCount(int DataTypeId)
        {
            return independentDatabase.ExecuteScalar<int>("Select count(id) from cmsDataTypePreValues where DataTypeNodeId = @dataTypeId", new { dataTypeId = DataTypeId });
        }

        protected void EnsureAll_PreValue_TestRecordsAreDeleted()
        {
            independentDatabase.Execute("delete from cmsDataTypePreValues where datatypenodeid = @0", _dataTypeDefinition1.Id);
            independentDatabase.Execute("delete from cmsDataTypePreValues where datatypenodeid = @0", _dataTypeDefinition2.Id);

            independentDatabase.Execute("delete from cmsDataType where nodeid = @0", _dataTypeDefinition1.Id);
            independentDatabase.Execute("delete from umbracoNode where id = @0", _dataTypeDefinition1.Id);
            independentDatabase.Execute("delete from cmsDataType where nodeid = @0", _dataTypeDefinition2.Id);
            independentDatabase.Execute("delete from umbracoNode where id = @0", _dataTypeDefinition2.Id);
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
                _node1 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 1", Document._objectType);
                _node2 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 2", Document._objectType);
                _node3 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 3", Document._objectType);
                _node4 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 4", Document._objectType);
                _node5 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 5", Document._objectType);

                _contentType1 = insertContentType(1, _node1.Id, "testMedia1");
                _contentType2 = insertContentType(1, _node2.Id, "testMedia2");

                _propertyTypeGroup1 = insertPropertyTypeGroup(_contentType1.NodeId, (int?)null, "book");
                _propertyTypeGroup2 = insertPropertyTypeGroup(_contentType2.NodeId, (int?)_propertyTypeGroup1.Id, "CD");
                _propertyTypeGroup3 = insertPropertyTypeGroup(_contentType1.NodeId, (int?)_propertyTypeGroup1.Id, "DVD");

                _propertyType1 = insertPropertyType(TEST_DATA_TYPE_ID1, _contentType1.NodeId, _propertyTypeGroup1.Id, "Test Property Type 1");
                _propertyType2 = insertPropertyType(TEST_DATA_TYPE_ID2, _contentType2.NodeId, _propertyTypeGroup2.Id, "Test Property Type 2");
                _propertyType3 = insertPropertyType(TEST_DATA_TYPE_ID3, _contentType2.NodeId, _propertyTypeGroup1.Id, "Test Property Type 3");

                _propertyData1 = insertPropertyTypeData(_propertyType1.Id, _contentType1.NodeId);
                _propertyData2 = insertPropertyTypeData(_propertyType2.Id, _contentType1.NodeId);
                _propertyData3 = insertPropertyTypeData(_propertyType1.Id, _contentType2.NodeId);
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

        private ContentTypeDto insertContentType(int index, int nodeId, string alias)
        {
            independentDatabase.Execute("insert into [cmsContentType] " +
                  "([nodeId] ,[alias],[icon],[thumbnail],[description],[isContainer],[allowAtRoot]) values " +
                  "(@nodeId , @alias, @icon, @thumbnail, @description, @isContainer, @allowAtRoot)",
                  new
                  {
                      nodeId = nodeId,
                      alias = alias,
                      icon = string.Format("test{0}.gif", index),
                      thumbnail = string.Format("test{0}.png", index),
                      description = string.Format("test{0}", index),
                      isContainer = false,
                      allowAtRoot = false
                  });
            int id = independentDatabase.ExecuteScalar<int>("select MAX(pk) from [cmsContentType]");
            return TRAL.GetDto<ContentTypeDto>(id, idKeyName:"pk");
        }

        private PropertyTypeGroupDto insertPropertyTypeGroup(int contentNodeId, int? parentNodeId, string text)
        {
            independentDatabase.Execute("insert into [cmsPropertyTypeGroup] " +
                       " ([parentGroupId],[contenttypeNodeId],[text],[sortorder]) VALUES " +
                       " (@parentGroupId,@contenttypeNodeId,@text,@sortorder)",
                       new { parentGroupId = parentNodeId, contenttypeNodeId = contentNodeId, text = text, sortorder = 1 });
            int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsPropertyTypeGroup]");
            return TRAL.GetDto<PropertyTypeGroupDto>(id);
        }

        private PropertyTypeDto insertPropertyType(int dataTypeId, int contentNodeId, int? propertyGroupId, string text)
        {
            independentDatabase.Execute("insert into [cmsPropertyType] " +
                       " (dataTypeId, contentTypeId, propertyTypeGroupId, [Alias], [Name], helpText, sortOrder, " +
                       "  mandatory, validationRegExp, [Description]) VALUES " +
                       " (@dataTypeId, @contentTypeId, @propertyTypeGroupId, @alias, @name, @helpText, @sortOrder, @mandatory, @validationRegExp, @description)",
                       new
                       {
                           dataTypeId = dataTypeId,
                           contentTypeId = contentNodeId,
                           propertyTypeGroupId = propertyGroupId,
                           alias = text.Replace(" ", ""),
                           name = "text",
                           helpText = "",
                           sortOrder = 0,
                           mandatory = false,
                           validationRegExp = "",
                           description = ""
                       });
            int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsPropertyType]");
            return TRAL.GetDto<PropertyTypeDto>(id);
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
            return TRAL.GetDto<PropertyDataDto>(id);
        }


        protected void EnsureAll_Property_TestRecordsAreDeleted()
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
            _recycleBinNode1 = ORMTestCMSNode.MakeNew(Constants.System.RecycleBinMedia, 2, "Test Media Content 1", Media._objectType);
            _recycleBinNode2 = ORMTestCMSNode.MakeNew(Constants.System.RecycleBinMedia, 2, "Test Media Content 2", Media._objectType);
            _recycleBinNode3 = ORMTestCMSNode.MakeNew(Constants.System.RecycleBinMedia, 2, "Test Media Content 3", Media._objectType);
            _recycleBinNode4 = ORMTestCMSNode.MakeNew(Constants.System.RecycleBinContent, 2, "Test Content 1", Document._objectType);
            _recycleBinNode5 = ORMTestCMSNode.MakeNew(Constants.System.RecycleBinContent, 2, "Test Content 2", Document._objectType);

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
                _node1 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 1", Document._objectType);
                _node2 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 2", Document._objectType);
                _node3 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 3", Document._objectType);
                _node4 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 4", Document._objectType);
                _node5 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 5", Document._objectType);

                _relationType1 = insertTestRelationType(1);
                _relationType2 = insertTestRelationType(2);

                _relation1 = insertTestRelation(new RelationType(_relationType1.Id), _node1.Id, _node2.Id, "node1(parent) <-> node2(child)");
                _relation2 = insertTestRelation(new RelationType(_relationType1.Id), _node2.Id, _node3.Id, "node2(parent) <-> node3(child)");
                _relation3 = insertTestRelation(new RelationType(_relationType2.Id), _node1.Id, _node3.Id, "node1(parent) <-> node3(child)");

            }

            Assert.That(_node1, !Is.Null);
            Assert.That(_node2, !Is.Null);
            Assert.That(_node3, !Is.Null);
            Assert.That(_node4, !Is.Null);
            Assert.That(_node5, !Is.Null);

            Guid objectType = Document._objectType;
            int expectedValue = independentDatabase.ExecuteScalar<int>("SELECT COUNT(*) from umbracoNode WHERE nodeObjectType = @objectType", new { objectType });
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
            if (_relationType1 != null) independentDatabase.Execute("delete from [umbracoRelationType] where (Id = @0)", _relationType1.Id);
            if (_relationType2 != null) independentDatabase.Execute("delete from [umbracoRelationType] where (Id = @0)", _relationType2.Id);

            initialized = false;
        }

        private RelationTypeDto insertTestRelationType(int testRelationTypeNumber)
        {
            independentDatabase.Execute("insert into [umbracoRelationType] ([dual], [parentObjectType], [childObjectType], [name], [alias]) values " +
                            "(@dual, @parentObjectType, @childObjectType, @name, @alias)",
                            new
                            {
                                dual = 1,
                                parentObjectType = Guid.NewGuid(),
                                childObjectType = Guid.NewGuid(),
                                name = string.Format("{0}_{1}", TEST_RELATION_TYPE_NAME, testRelationTypeNumber),
                                alias = string.Format("{0}_{1}", TEST_RELATION_TYPE_ALIAS, testRelationTypeNumber),
                            });
            int relationTypeId = independentDatabase.ExecuteScalar<int>("select max(id) from [umbracoRelationType]");
            return TRAL.GetDto<RelationTypeDto>(relationTypeId);
        }

        private RelationDto insertTestRelation(RelationType relationType, int parentNodeId, int childNodeId, string comment)
        {
            independentDatabase.Execute("insert into [umbracoRelation] (parentId, childId, relType, datetime, comment) values (@parentId, @childId, @relType, @datetime, @comment)",
                             new { parentId = parentNodeId, childId = childNodeId, relType = relationType.Id, datetime = DateTime.Now, comment = comment });
            int relationId = independentDatabase.ExecuteScalar<int>("select max(id) from [umbracoRelation]");
            return TRAL.GetDto<RelationDto>(relationId);
        }


        #endregion

        #region Ensure RalationType Data
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_RelationType_TestData()
        {
            if (!initialized)
            {
                _relationType1 = insertTestRelationType(uniqueRelationName);
                _relationType2 = insertTestRelationType(uniqueRelationName);
            }

            Assert.That(_relationType1, !Is.Null);
            Assert.That(_relationType2, !Is.Null);

            initialized = true;
        }


        protected void EnsureAll_RelationType_TestRecordsAreDeleted()
        {
            if (_relationType1 != null) independentDatabase.Execute("delete from [umbracoRelationType] where (Id = @0)", _relationType1.Id);
            if (_relationType2 != null) independentDatabase.Execute("delete from [umbracoRelationType] where (Id = @0)", _relationType2.Id);

            initialized = false;
        }

        protected string uniqueRelationName
        {
            get
            {
                return string.Format("Relation Name {0}", Guid.NewGuid().ToString());
            }
        }

        private RelationTypeDto insertTestRelationType(string testRelationName)
        {
            independentDatabase.Execute("insert into [umbracoRelationType] ([dual], [parentObjectType], [childObjectType], [name], [alias]) values " +
                            "(@dual, @parentObjectType, @childObjectType, @name, @alias)",
                            new
                            {
                                dual = 1,
                                parentObjectType = Guid.NewGuid(),
                                childObjectType = Guid.NewGuid(),
                                name = testRelationName,
                                alias = testRelationName.Replace(" ", "")
                            });
            int relationTypeId = independentDatabase.ExecuteScalar<int>("select max(id) from [umbracoRelationType]");
            return TRAL.GetDto<RelationTypeDto>(relationTypeId);
        }


        #endregion

        #region Ensure Tag Test Data

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void Ensure_Tag_TestData()
        {
            if (!initialized)
            {
                _node1 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 1", Document._objectType);
                _node2 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 2", Document._objectType);
                _node3 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 3", Document._objectType);
                _node4 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 4", Document._objectType);
                _node5 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 5", Document._objectType);

                _tag1 = InsertTestTag("Tag11", "1");
                _tag2 = InsertTestTag("Tag12", "1");
                _tag3 = InsertTestTag("Tag13", "1");
                _tag4 = InsertTestTag("Tag21", "2");
                _tag5 = InsertTestTag("Tag22", "2");

                _tagRel1 = InsertTestTagRelationship(_node1.Id, _tag1.Id);
                _tagRel2 = InsertTestTagRelationship(_node1.Id, _tag2.Id);
                _tagRel3 = InsertTestTagRelationship(_node1.Id, _tag3.Id);
                _tagRel4 = InsertTestTagRelationship(_node2.Id, _tag4.Id);
                _tagRel5 = InsertTestTagRelationship(_node3.Id, _tag5.Id);
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
            delRel(_tagRel1);
            delRel(_tagRel2);
            delRel(_tagRel3);
            delRel(_tagRel4);
            delRel(_tagRel5);

            delTag(_tag1);
            delTag(_tag2);
            delTag(_tag3);
            delTag(_tag4);
            delTag(_tag5);

            // nodes deletion recursively deletes all relations...
            if (_node1 != null) _node1.delete();
            if (_node2 != null) _node2.delete();
            if (_node3 != null) _node3.delete();
            if (_node4 != null) _node4.delete();
            if (_node5 != null) _node5.delete();

            //deleteContent();

            initialized = false;
        }
        private TagDto InsertTestTag(string tag, string group)
        {
            independentDatabase.Execute("insert into [cmsTags] ([Tag], [Group]) values (@0, @1)", tag, group);
            int id = independentDatabase.ExecuteScalar<int>("select Max(id) from cmsTags");
            return TRAL.GetDto<TagDto>(id);
        }
        private TagRelationshipDto InsertTestTagRelationship(int nodeId, int tagId)
        {
            independentDatabase.Execute("insert into [cmsTagRelationship] ([nodeId], [tagId]) values (@0, @1)", nodeId, tagId);
            return getTestTagRelationshipDto(nodeId, tagId);
        }

        internal TagRelationshipDto getTestTagRelationshipDto(int nodeId, int tagId)
        {
            return independentDatabase.SingleOrDefault<TagRelationshipDto>(string.Format("where [nodeId] = @0 and [tagId] = @1"), nodeId, tagId);
        }

        //private NodeDto getTestNodeDto(int id)
        //{
        //    return TRAL.GetDto<NodeDto>(id);
        //}

        internal void delRel(TagRelationshipDto tagRel)
        {
            if (tagRel != null) independentDatabase.Execute("delete from [cmsTagRelationship] " +
                                                                      string.Format("where [nodeId] = @0 and [tagId] = @1"), tagRel.NodeId, tagRel.TagId);
        }
        internal void delTag(TagDto tag)
        {
            if (tag != null) independentDatabase.Execute("delete from [cmsTags] " +
                                                                      string.Format("where [Id] = @0"), tag.Id);
        }

        protected int addTestTags(int qty = 5, string groupName = TEST_GROUP_NAME, int? nodeId = null)
        {
            for (int i = 1; i <= qty; i++)
            {
                string tagName = string.Format("Tag #{0}", uniqueLabel);
                if (nodeId == null)
                    addTag(tagName, groupName);
                else
                    addTagToNode((int)nodeId, tagName, groupName);
            }
            return qty;
        }

        protected int addTagToNode(int nodeId, string tag, string group)
        {
            int id = getTagId(tag, group);
            if (id == 0) id = addTag(tag, group);

            //Perform a subselect insert into cmsTagRelationship using a left outer join to perform the if not exists check
            string sql = "insert into cmsTagRelationship (nodeId,tagId) select " + string.Format("{0}", nodeId) + ", " + string.Format("{0}", id) + " from cmsTags ";
            //sorry, gotta do this, @params not supported in join clause
            sql += "left outer join cmsTagRelationship on (cmsTags.id = cmsTagRelationship.TagId and cmsTagRelationship.nodeId = " + string.Format("{0}", nodeId) + ") ";
            sql += "where cmsTagRelationship.tagId is null and cmsTags.id = " + string.Format("{0}", id);

            independentDatabase.Execute(sql);

            return id;
        }

        protected int addTag(string tag, string group)
        {
            // independentDatabase.Insert returns System.Decimal - why?
            return (int)(decimal)independentDatabase.Insert(new TagDto() { Tag = tag, Group = group });
        }

        internal class TagDtoExt : TagDto
        {
            public int NodeCount { get; set; }
        }

        private IEnumerable<TagDto> convertSqlToTags(string sql, params object[] param)
        {
            //string testSql = sql;
            //int index = 0;
            //foreach (var p in param)
            //{
            //    testSql = testSql.Replace(string.Format("@{0}", index++), p.ToString());  
            //}

            //l("TEST SQL = '{0}'", testSql);

            foreach (var tagDto in independentDatabase.Query<TagDtoExt>(sql, param))
            {
                yield return tagDto;
                //new Tag(tagDto.Id, tagDto.Tag, tagDto.Group, tagDto.NodeCount);
            }
        }
        private int convertSqlToTagsCount(string sql, params object[] param)
        {
            return convertSqlToTags(sql, param).ToArray().Length;
        }

        protected int countTags(int? nodeId = null, string groupName = "")
        {
            var groupBy = " GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";
            var sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags " +
                        "INNER JOIN cmsTagRelationship ON cmsTagRelationShip.tagId = cmsTags.id";
            if (nodeId != null && !string.IsNullOrWhiteSpace(groupName))
                return convertSqlToTagsCount(sql + " WHERE cmsTags.[group] = @0 AND cmsTagRelationship.nodeid = @1" + groupBy, groupName, nodeId);
            else if (nodeId != null)
                return convertSqlToTagsCount(sql + " WHERE cmsTagRelationship.nodeid = @0" + groupBy, nodeId);
            else if (!string.IsNullOrWhiteSpace(groupName))
                return convertSqlToTagsCount(sql + " WHERE cmsTags.[group] = @0" + groupBy, groupName);
            else
                return convertSqlToTagsCount(sql.Replace("INNER JOIN", "LEFT JOIN") + groupBy);
        }


        private int getTagId(string tag, string group)
        {
            var tagDto = independentDatabase.FirstOrDefault<TagDto>("where tag=@0 AND [group]=@1", tag, group);
            if (tagDto == null) return 0;
            return tagDto.Id;
        }

        public void addTagsToNode(int nodeId, string tags, string group)
        {
            string[] allTags = tags.Split(",".ToCharArray());
            for (int i = 0; i < allTags.Length; i++)
            {
                //if not found we'll get zero and handle that onsave instead...
                int id = getTagId(allTags[i], group);
                if (id == 0)
                    id = addTag(allTags[i], group);

                //Perform a subselect insert into cmsTagRelationship using a left outer join to perform the if not exists check
                string sql = "insert into cmsTagRelationship (nodeId,tagId) select " + string.Format("{0}", nodeId) + ", " + string.Format("{0}", id) + " from cmsTags ";
                //sorry, gotta do this, @params not supported in join clause
                sql += "left outer join cmsTagRelationship on (cmsTags.id = cmsTagRelationship.TagId and cmsTagRelationship.nodeId = " + string.Format("{0}", nodeId) + ") ";
                sql += "where cmsTagRelationship.tagId is null and cmsTags.id = " + string.Format("{0}", id);

                independentDatabase.Execute(sql);
            }
        }

        protected int countDocumentsWithTags(string tags, bool publishedOnly = false)
        {
            int count = 0;
            var docs = new List<DocumentDto>();
            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id 
                            INNER JOIN umbracoNode ON cmsTagRelationShip.nodeId = umbracoNode.id
                            WHERE (cmsTags.tag IN ({0})) AND nodeObjectType=@nodeType";
            foreach (var id in independentDatabase.Query<int>(string.Format(sql, getSqlStringArray(tags)),
                                                   new { nodeType = Document._objectType }))
            {
                // temp comment - results in runtime error in abridged project
                //Document cnode = new Document(id);
                //if (cnode != null && (!publishedOnly || cnode.Published)) 
                    count++;
            }

            return count;
        }

       protected int countNodesWithTags(string tags)
        {
            int count = 0;

            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id WHERE (cmsTags.tag IN (" + getSqlStringArray(tags) + "))";
            foreach (var id in independentDatabase.Query<int>(sql)) count++;
            return count;
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

               _node1 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 1", Document._objectType);
               _node2 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 2", Document._objectType);
               _node3 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 3", Document._objectType);
               _node4 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 4", Document._objectType);
               _node5 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 5", Document._objectType);

               _user = new User(0);

               _taskType1 = insertTestTaskType(newTaskTypeAlias);
               _task1 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #1");
               _task2 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #2", closed: true);
               _task3 = insertTestTask(_taskType1, _node2, _user, _user, "TODO - #3");

               _taskType2 = insertTestTaskType(newTaskTypeAlias);
               _task4 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #4");
               _task5 = insertTestTask(_taskType1, _node3, _user, _user, "TODO - #5", closed: true);
               _taskType3 = insertTestTaskType(newTaskTypeAlias);
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
           delTask(_task1);
           delTask(_task2);
           delTask(_task3);
           delTask(_task4);
           delTask(_task5);

           delTaskType(_taskType1);
           delTaskType(_taskType2);
           delTaskType(_taskType3);

           initialized = false;
       }


       private UserDto getAdminUser()
       {
           return TRAL.GetDto<UserDto>(0);
       }

       protected string newTaskTypeAlias
       {
           get
           {
               return string.Format("Test TaskType, GUID = {0}", Guid.NewGuid());
           }
       }

       [MethodImpl(MethodImplOptions.Synchronized)]
       internal TaskTypeDto insertTestTaskType(string alias)
       {
           independentDatabase.Execute("insert into [cmsTaskType] ([Alias]) values (@0)", alias);
           int id = independentDatabase.ExecuteScalar<int>("select Max(id) from cmsTaskType");
           return TRAL.GetDto<TaskTypeDto>(id);
       }

       [MethodImpl(MethodImplOptions.Synchronized)]
       internal TaskDto insertTestTask(TaskTypeDto taskType, CMSNode node, User parentUser, User user, string comment, bool closed = false)
       {
           independentDatabase.Execute(
                 @"insert into [cmsTask] ([closed], taskTypeId, nodeId, parentUserId, userId, [DateTime], [Comment]) 
                   values (@closed, @taskTypeId, @nodeId, @parentUserId, @userId, @dateTime, @comment)",
                 new
                 {
                     closed = closed,
                     taskTypeId = taskType.Id,
                     nodeId = node.Id,
                     parentUserId = parentUser.Id,
                     userId = user.Id,
                     dateTime = DateTime.Now,
                     comment = comment
                 });
           int id = independentDatabase.ExecuteScalar<int>("select Max(id) from cmsTask");
           return TRAL.GetDto<TaskDto>(id);
       }

       internal int countTasksByTaskType(int taskTypeId)
       {
           return independentDatabase.ExecuteScalar<int>("select count(id) from cmsTask where taskTypeId = @0", taskTypeId);
       }

       internal int countTasksByNodeId(int id)
       {
           return independentDatabase.ExecuteScalar<int>("select count(id) from cmsTask where nodeId = @0", id);
       }

       internal int countTasksByUserAndIncludeClosedFlag(int userId, bool includeClosed)
       {
           string sql = "select count(id) from  cmsTask  where userId = @0";
           if (!includeClosed)
               sql += " and closed = 0";
           return independentDatabase.ExecuteScalar<int>(sql, userId);
       }

       internal int countOwnedTasksByUserAndIncludeClosedFlag(int userId, bool includeClosed)
       {
           string sql = "select count(id) from  cmsTask  where parentUserId = @0";
           if (!includeClosed)
               sql += " and closed = 0";
           return independentDatabase.ExecuteScalar<int>(sql, userId);
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

               _node1 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 1", umbraco.cms.businesslogic.web.Document._objectType);
               _node2 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 2", Document._objectType);
               _node3 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 3", Document._objectType);
               _node4 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 4", Document._objectType);
               _node5 = ORMTestCMSNode.MakeNew(-1, 1, "TestContent 5", Document._objectType);

               _user = new User(ADMIN_USER_ID);

               _taskType1 = insertTestTaskType(newTaskTypeAlias);
               _task1 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #1");
               _task2 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #2");
               _task3 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #3");

               _taskType2 = insertTestTaskType(newTaskTypeAlias);
               _task4 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #4");
               _task5 = insertTestTask(_taskType1, _node1, _user, _user, "TODO - #5");
               _taskType3 = insertTestTaskType(newTaskTypeAlias);

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
           delTask(_task1);
           delTask(_task2);
           delTask(_task3);
           delTask(_task4);
           delTask(_task5);

           delTaskType(_taskType1);
           delTaskType(_taskType2);
           delTaskType(_taskType3);

           initialized = false;
       }

       [MethodImpl(MethodImplOptions.Synchronized)]
       private TaskDto insertTestTask(TaskTypeDto taskType, CMSNode node, UserDto parentUser, UserDto user, string comment)
       {
           independentDatabase.Execute(
                 @"insert into [cmsTask] ([closed], taskTypeId, nodeId, parentUserId, userId, [DateTime], [Comment]) 
                   values (@closed, @taskTypeId, @nodeId, @parentUserId, @userId, @dateTime, @comment)",
                 new
                 {
                     closed = false,
                     taskTypeId = taskType.Id,
                     nodeId = node.Id,
                     parentUserId = parentUser.Id,
                     userId = user.Id,
                     dateTime = DateTime.Now,
                     comment = comment
                 });
           int id = independentDatabase.ExecuteScalar<int>("select Max(id) from cmsTask");
           return TRAL.GetDto<TaskDto>(id);
       }

       internal void delTaskType(TaskTypeDto dto)
       {
           if (dto != null) independentDatabase.Delete<TaskTypeDto>("where [Id] = @0", dto.Id);
       }
       internal void delTask(TaskDto dto)
       {
           if (dto != null) independentDatabase.Delete<TaskDto>("where [Id] = @0", dto.Id);
       }

       internal int getAllTaskTypesCount()
       {
           return independentDatabase.ExecuteScalar<int>("select count(*) from cmsTaskType");
       }

       internal int getGetTaskTypesTasks(int taskTypeId)
       {
           return independentDatabase.ExecuteScalar<int>("select count(*) from cmsTask where TaskTypeId = @0", taskTypeId);
       }

       #endregion

       #region EnsureData

       public static string SpaceCamelCasing(string text)
       {
           return text.Length < 2 ? text : text.SplitPascalCasing().ToFirstUpperInvariant();
       }

       private int GetNewDocumentSortOrder(int parentId)
       {
           // I'd let other node types than document get a sort order too, but that'll be a bugfix
           // I'd also let it start on 1, but then again, it didn't.
           var max = independentDatabase.SingleOrDefault<int?>(
               "SELECT MAX(sortOrder) FROM umbracoNode WHERE parentId = @parentId AND nodeObjectType = @ObjectTypeId",
               new { parentId, ObjectTypeId = Document._objectType }
               );
           return (max ?? -1) + 1;
       }

       [MethodImpl(MethodImplOptions.Synchronized)]
       private TemplateDto insertTemplate(int? masterNodeId, string design, int userId, int level, string alias = "")
       {
           if (string.IsNullOrWhiteSpace(alias)) alias = uniqueTemplateAlias;
           Guid ObjectType = new Guid(Constants.ObjectTypes.Template);

           CMSNode parent = null;
           string path = "";
           int sortOrder = 0;

           if (level > 0 && masterNodeId != null)
           {
               parent = new CMSNode((int)masterNodeId);
               sortOrder = GetNewDocumentSortOrder((int)masterNodeId);
               path = parent.Path;
           }
           else
               path = "-1";

           if (masterNodeId == null) masterNodeId = -1;

           independentDatabase.Execute(
               @"INSERT INTO umbracoNode(
                      trashed, parentID, nodeObjectType, nodeUser, level, 
                      path, sortOrder, uniqueID, text, createDate) 
                  VALUES (
                      @trashed, @parentID, @nodeObjectType, @nodeUser, @level, 
                      @path, @sortOrder, @uniqueID, @text, @createDate)",
               new
               {
                   trashed = false,
                   parentID = masterNodeId,
                   nodeObjectType = ObjectType,
                   nodeUser = userId,
                   level,
                   path,
                   sortOrder,
                   uniqueID = Guid.NewGuid(),
                   text = alias,
                   createDate = DateTime.Now
               });

           int nodeId = independentDatabase.ExecuteScalar<int>("Select max(id) from umbracoNode");

           independentDatabase.Execute(
                @"insert into [cmsTemplate] ([nodeId],[master],[alias],[design]) 
                     values (@nodeId, @master, @alias, @design)",
                    new { nodeId = nodeId, master = masterNodeId, design = design, alias = alias });
           int pk = independentDatabase.ExecuteScalar<int>("select Max(pk) from cmsTemplate");
           var dto = TRAL.GetDto<TemplateDto>(pk, idKeyName: "pk");
           dto.NodeDto = TRAL.GetDto<NodeDto>(nodeId);
           return dto;
       }

       protected string uniqueTemplateAlias
       {
           get
           {
               return string.Format("umbTemplateAlias-{0}", Guid.NewGuid().ToString());
           }
       }
       protected string uniqueTemplateName
       {
           get
           {
               return string.Format("Template Name {0}", Guid.NewGuid().ToString());
           }
       }

       [MethodImpl(MethodImplOptions.Synchronized)]
       protected void Ensure_Template_TestData()
       {
           if (!initialized)
           {
               //MakeNew_PersistsNewUmbracoNodeRow();

               _user = new User(0); // getAdminUser();

               _template1 = insertTemplate(null, TemplatesText.Master, _user.Id, 1);
               _template2 = insertTemplate(_template1.NodeId, TemplatesText.NewsArticle, _user.Id, 2);
               _template3 = insertTemplate(_template1.NodeId, TemplatesText.HomePage, _user.Id, 2);
               _template4 = insertTemplate(null, TemplatesText.CommentRSS, _user.Id, 1);
               _template5 = insertTemplate(null, TemplatesText.RSS, _user.Id, 1);
           }

           Assert.That(_user, !Is.Null);

           Assert.That(_template1, !Is.Null);
           Assert.That(_template2, !Is.Null);
           Assert.That(_template3, !Is.Null);
           Assert.That(_template4, !Is.Null);
           Assert.That(_template5, !Is.Null);

           initialized = true;
       }

       //private UserDto getAdminUser()
       //{
       //    return TRAL.GetDto<UserDto>(0);
       //}

       private string newTemplateName
       {
           get
           {
               return string.Format("Template With GUID = {0}", Guid.NewGuid());
           }
       }

       private void delTemplate(TemplateDto dto)
       {
           if (dto != null) independentDatabase.Delete<TemplateDto>("where [pk] = @0", dto.PrimaryKey);
       }

       private int countTasksByMasterNodeId(int nodeId)
       {
           return independentDatabase.ExecuteScalar<int>("select count(pk) from cmsTemplate where master = @0", nodeId);
       }

       protected void EnsureAll_Template_TestRecordsAreDeleted()
       {
           delTemplate(_template2);
           delTemplate(_template3);
           delTemplate(_template4);
           delTemplate(_template5);
           delTemplate(_template1); // master

           initialized = false;
       }

       #endregion

       #region Templates Text

       class TemplatesText
       {
           public static string CommentRSS
           {
               get
               {
                   return @"
<%@ Master Language=""C#"" MasterPageFile=""~/umbraco/masterpages/default.master"" AutoEventWireup=""true"" %>
<asp:Content ContentPlaceHolderID=""ContentPlaceHolderDefault"" runat=""server""><umbraco:Macro iscommentfeed=""1"" Alias=""BlogRss"" runat=""server""></umbraco:Macro></asp:Content>";
               }
           }

           public static string NewsArticle
           {
               get
               {
                   return @"
<%@ Master Language=""C#"" MasterPageFile=""~/masterpages/umbMaster.master"" AutoEventWireup=""true"" %>

<asp:Content ContentPlaceHolderID=""cp_content"" runat=""server"">

<div id=""content"" class=""textpage"">
  
      <div id=""contentHeader"">  
          <h2><umbraco:Item runat=""server"" field=""pageName""/></h2>
      </div>
        
      <h4><umbraco:Item field=""introduction"" convertLineBreaks=""true"" runat=""server""></umbraco:Item></h4>
  
      <umbraco:Item runat=""server"" field=""bodyText"" />
</div>


</asp:Content>
";

               }
           }

           public static string HomePage
           {
               get
               {
                   return @"
<%@ Master Language=""C#"" MasterPageFile=""~/masterpages/umbMaster.master"" AutoEventWireup=""true"" %>
    <asp:Content ContentPlaceHolderID=""cp_content"" runat=""server"">
      <div id=""content"" class=""frontPage"">
        <umbraco:Item runat=""server"" field=""bodyText""/>
        

      </div>
    </asp:Content>";
               }
           }


           public static string Master
           {
               get
               {
                   return @"
<%@ Master Language=""C#"" MasterPageFile=""~/umbraco/masterpages/default.master"" AutoEventWireup=""true"" %>
<asp:Content ContentPlaceHolderID=""ContentPlaceHolderDefault"" runat=""server"">

<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""[]> 
<html xmlns=""http://www.w3.org/1999/xhtml"">
    <head id=""head"" runat=""server"">
    
  
    <meta http-equiv=""Content-Type"" content=""text/html; charset=iso-8859-1"" />
    <title><asp:placeholder runat=""server""><umbraco:Item runat=""server"" field=""pageName"" /> - <umbraco:Item runat=""server"" field=""siteName"" recursive=""true"" /></asp:placeholder></title>
    
    <link rel=""stylesheet"" type=""text/css"" href=""/css/Starterkit.css"" /> 
  
    <umbraco:Macro Alias=""BlogRssFeedLink"" runat=""server""></umbraco:Macro>
    
    <asp:contentplaceholder id=""cp_head"" runat=""server"" />
</head>
    <body>    
    <div id=""main"">
      
        <asp:contentplaceholder id=""cp_top"" runat=""server"">
        <div id=""top"">
            <h1 id=""siteName""><a href=""/""><umbraco:Item runat=""server"" field=""siteName"" recursive=""true"" /></a></h1>
            <h2 id=""siteDescription""><umbraco:Item runat=""server"" field=""siteDescription"" recursive=""true"" /></h2>
        
            <umbraco:Macro Alias=""umbTopNavigation"" runat=""server"" />
        </div>
        </asp:contentplaceholder>
            
        <div id=""body"" class=""clearfix"">
            <form id=""RunwayMasterForm"" runat=""server"">
            <asp:ContentPlaceHolder ID=""cp_content"" runat=""server""></asp:ContentPlaceHolder>
            </form>
        </div> 
      
        <asp:contentplaceholder id=""cp_footer"" runat=""server"">
        <div id=""footer""></div>
        </asp:contentplaceholder>
    </div>
    </body>
</html> 
</asp:content>";
               }
           }

           public static string RSS
           {
               get
               {
                   return
                       @"<%@ Master Language=""C#"" MasterPageFile=""~/umbraco/masterpages/default.master"" AutoEventWireup=""true"" %>
                            <asp:Content ContentPlaceHolderID=""ContentPlaceHolderDefault"" runat=""server"">
                            <umbraco:Macro Alias=""BlogRss"" runat=""server""></umbraco:Macro>
                            </asp:Content>";
               }
           }


       }
       #endregion


    }

 
}
