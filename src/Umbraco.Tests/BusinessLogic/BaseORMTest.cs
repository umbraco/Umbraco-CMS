using System.Diagnostics;
using System.Reflection;
using System;
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

namespace Umbraco.Tests.TestHelpers
{
    public abstract partial class BaseORMTest : BaseDatabaseFactoryTestWithContext
    {
        public const int ADMIN_USER_ID = 1;

        internal User _user;
        internal DataTypeDefinition _dataTypeDefinition;

        internal MacroDto _macro1;
        internal MacroDto _macro2;
        internal MacroPropertyTypeDto _macroPropertyType1;
        internal MacroPropertyTypeDto _macroPropertyType2;
        internal MacroPropertyDto _macroProperty1;

        internal InstalledPackageDto _package1;

        internal umbraco.cms.businesslogic.datatype.PreValue.PreValueDto _preValue;

        protected const int NON_EXISTENT_TEST_ID_VALUE = 12345;
        const string TEST_VALUE_VALUE = "test value";



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
                _macroProperty1 = getPersistedTestDto <MacroPropertyDto>(id);
            }

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
            return getPersistedTestDto <MacroDto>(id);
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
            return getPersistedTestDto <MacroPropertyTypeDto>(id);
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
                _macroPropertyType1 = getPersistedTestDto <MacroPropertyTypeDto>(id);
            }

            initialized = true;
        }

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
                _dataTypeDefinition = DataTypeDefinition.MakeNew(_user, "Nvarchar");
            }

            if ((int)independentDatabase.ExecuteScalar<int>("select count(*) from cmsDataTypePreValues where datatypenodeid = @0", _dataTypeDefinition.Id) == 0)
            {
                initialized = false;

                string value = ",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|";

                independentDatabase.Execute(
                    "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",
                    new { dtdefid = _dataTypeDefinition.Id, value = value });
                var id = PreValue.Database.ExecuteScalar<int>("SELECT MAX(id) FROM cmsDataTypePreValues");

                _preValue = getDto<umbraco.cms.businesslogic.datatype.PreValue.PreValueDto>(id);
            }

            initialized = true;
        }
        #endregion
    }

 
}
