using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.relation;
using umbraco.cms.businesslogic.macro;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_MacroProperty_Tests : BaseDatabaseFactoryTestWithContext
    {
        #region EnsureData

        const string TEST_MACRO_PROPERTY_TYPE_ALIAS1 = "testAlias1";
        const string TEST_MACRO_PROPERTY_TYPE_ALIAS2 = "testAlias2";

        const string TEST_MACRO_PROPERTY_NAME = "Your Web Site";
        const string TEST_MACRO_PROPERTY_ALIAS = "yourWebSite";

        private MacroPropertyTypeDto _macroPropertyType1;
        private MacroPropertyTypeDto _macroPropertyType2;
        private MacroDto _macro1;
        private MacroDto _macro2;
        private MacroPropertyDto _macroProperty1;

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void EnsureData()
        {
            if (!initialized)
            {
                _macroPropertyType1 = insertMacroPropertyType(TEST_MACRO_PROPERTY_TYPE_ALIAS1);
                _macroPropertyType2 = insertMacroPropertyType(TEST_MACRO_PROPERTY_TYPE_ALIAS2);

                _macro1 = insertMacro("Twitter Ads", "twitterAds");
                _macro2 = insertMacro("Yahoo Ads", "yahooAds");


                independentDatabase.Execute("insert into [cmsMacroProperty] (macroPropertyHidden, macroPropertyType, macro, macroPropertySortOrder, macroPropertyAlias, macroPropertyName) " +
                                           " values (@macroPropertyHidden, @macroPropertyType, @macro, @macroPropertySortOrder, @macroPropertyAlias, @macroPropertyName) ",
                                           new { macroPropertyHidden = true, macroPropertyType = _macroPropertyType1.Id, macro = _macro1.Id,
                                                 macroPropertySortOrder = 0,
                                                 macroPropertyAlias = TEST_MACRO_PROPERTY_ALIAS,
                                                 macroPropertyName = TEST_MACRO_PROPERTY_NAME
                                           });
               int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsMacroProperty]");
               _macroProperty1 = getTestMacroProperty(id);  
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
            return getTestMacro(id);
        }

        private MacroPropertyTypeDto insertMacroPropertyType(string alias)
        {
            independentDatabase.Execute("insert into [cmsMacroPropertyType] (macroPropertyTypeAlias, macroPropertyTypeRenderAssembly, macroPropertyTypeRenderType, macroPropertyTypeBaseType) " +
                                      " values (@macroPropertyTypeAlias, @macroPropertyTypeRenderAssembly, @macroPropertyTypeRenderType, @macroPropertyTypeBaseType) ",
                                      new
                                      {
                                          macroPropertyTypeAlias = alias, //TEST_MACRO_PROPERTY_TYPE_ALIAS,
                                          macroPropertyTypeRenderAssembly = "umbraco.macroRenderings",
                                          macroPropertyTypeRenderType = "context",
                                          macroPropertyTypeBaseType = "string"
                                      });
            int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsMacroPropertyType]");
            return getTestMacroPropertyType(id);
        }

        private MacroPropertyTypeDto getTestMacroPropertyType(int id)
        {
            return getPersistedTestDto<MacroPropertyTypeDto>(id);
        }
        private MacroPropertyDto getTestMacroProperty(int id)
        {
            return getPersistedTestDto<MacroPropertyDto>(id);
        }
        private MacroDto getTestMacro(int id)
        {
            return getPersistedTestDto<MacroDto>(id);
        }

        private void EnsureAllTestRecordsAreDeleted()
        {
            independentDatabase.Execute("delete from  [cmsMacroProperty] where id = @0", _macroProperty1.Id);
            independentDatabase.Execute("delete from  [cmsMacro] where id = @0", _macro1.Id);
            independentDatabase.Execute("delete from  [cmsMacro] where id = @0", _macro2.Id);
            independentDatabase.Execute("delete from  [cmsMacroPropertyType] where macroPropertyTypeAlias = @0", TEST_MACRO_PROPERTY_TYPE_ALIAS1);
            independentDatabase.Execute("delete from  [cmsMacroPropertyType] where macroPropertyTypeAlias = @0", TEST_MACRO_PROPERTY_TYPE_ALIAS2);
            initialized = false; 
        }

        #endregion

        #region Tests
        [Test(Description = "Test EnsureData()")]
        public void Test_EnsureData()
        {
            Assert.IsTrue(initialized);
            Assert.That(_macroPropertyType1, !Is.Null);
            Assert.That(_macroPropertyType2, !Is.Null);
            Assert.That(_macro1, !Is.Null);
            Assert.That(_macro2, !Is.Null);
            Assert.That(_macroProperty1, !Is.Null);

            EnsureAllTestRecordsAreDeleted();


            Assert.That(getTestMacroProperty(_macroProperty1.Id), Is.Null);
            Assert.That(getTestMacro(_macro1.Id), Is.Null);
            Assert.That(getTestMacro(_macro2.Id), Is.Null);
            Assert.That(getTestMacroPropertyType(_macroPropertyType1.Id), Is.Null);
            Assert.That(getTestMacroPropertyType(_macroPropertyType2.Id), Is.Null);
        }

        [Test(Description = "Test 'public MacroProperty(int Id)' method")]
        public void Test_MacroProperty_Constructor_1_and_Setup()
        {
            // public MacroProperty(int Id) &
            // private void setup() & 
            // private void PopulateMacroPropertyFromDto(MacroPropertyDto dto)
            MacroProperty testMacroProperty = null;
            Assert.Throws<ArgumentException>(() => { new MacroProperty(12345); });

            testMacroProperty = new MacroProperty(_macroProperty1.Id);

            // public int Id
            // public Macro Macro
            // public bool Public
            // public string Alias
            // public string Name
            // public MacroPropertyType Type
            Assert.That(testMacroProperty.Id, Is.EqualTo(_macroProperty1.Id));
            Assert.That(testMacroProperty.Macro.Id, Is.EqualTo(_macroProperty1.Macro));
            Assert.That(testMacroProperty.Public, Is.EqualTo(_macroProperty1.Hidden));
            Assert.That(testMacroProperty.Alias, Is.EqualTo(_macroProperty1.Alias));
            Assert.That(testMacroProperty.Name, Is.EqualTo(_macroProperty1.Name));
            Assert.That(testMacroProperty.Type.Id, Is.EqualTo(_macroProperty1.Type));
        }


        [Test(Description = "Test 'public static MacroProperty MakeNew(...)' method")]
        public void Test_MacroProperty_MakeNew()
        {
            var macro = new Macro(_macro1.Id);
            var macroPropertyType = new MacroPropertyType(_macroPropertyType1.Id);
            var testMacroProperty = MacroProperty.MakeNew(macro, true, "testAlias", "Test Name", macroPropertyType);  

            Assert.That(testMacroProperty, !Is.Null);

            int id = testMacroProperty.Id;
 
            var savedMacroProperty = getTestMacroProperty(id);

            Assert.That(testMacroProperty.Id, Is.EqualTo(savedMacroProperty.Id));
            Assert.That(testMacroProperty.Macro.Id, Is.EqualTo(savedMacroProperty.Macro));
            Assert.That(testMacroProperty.Public, Is.EqualTo(savedMacroProperty.Hidden));
            Assert.That(testMacroProperty.Alias, Is.EqualTo(savedMacroProperty.Alias));
            Assert.That(testMacroProperty.Name, Is.EqualTo(savedMacroProperty.Name));
            Assert.That(testMacroProperty.Type.Id, Is.EqualTo(savedMacroProperty.Type));

            independentDatabase.Execute("delete from [cmsMacroProperty] where id=@0", id);
            savedMacroProperty = getTestMacroProperty(id);
            Assert.That(savedMacroProperty, Is.Null);
        }

        [Test(Description = "Test 'public void Save()' method")]
        public void Test_MacroProperty_Save_MakeNew()
        {
            var testMacroProperty = new MacroProperty();
            //MacroProperty mp = MakeNew(m_macro, Public, Alias, Name, Type);
            testMacroProperty.Macro = new Macro(_macro1.Id);
            testMacroProperty.Public = true;
            testMacroProperty.Alias = "testAppMessage";
            testMacroProperty.Name = "Test App Message";
            testMacroProperty.Type = new MacroPropertyType(_macroPropertyType1.Id);
            testMacroProperty.Save();

            int id = testMacroProperty.Id;

            var savedMacroProperty = getTestMacroProperty(id);

            Assert.That(testMacroProperty.Id, Is.EqualTo(savedMacroProperty.Id));
            Assert.That(testMacroProperty.Macro.Id, Is.EqualTo(savedMacroProperty.Macro));
            Assert.That(testMacroProperty.Public, Is.EqualTo(savedMacroProperty.Hidden));
            Assert.That(testMacroProperty.Alias, Is.EqualTo(savedMacroProperty.Alias));
            Assert.That(testMacroProperty.Name, Is.EqualTo(savedMacroProperty.Name));
            Assert.That(testMacroProperty.Type.Id, Is.EqualTo(savedMacroProperty.Type));

            independentDatabase.Execute("delete from [cmsMacroProperty] where id=@0", id);
            savedMacroProperty = getTestMacroProperty(id);
            Assert.That(savedMacroProperty, Is.Null);
        }

        [Test(Description = "Test 'public void Save()' method")]
        public void Test_MacroProperty_Save_Update()
        {
            var testMacroProperty = new MacroProperty(_macroProperty1.Id);

            testMacroProperty.Macro = new Macro(_macro2.Id);
            testMacroProperty.Public = false;
            testMacroProperty.Alias = "testAppMessage 1";
            testMacroProperty.Name = "Test App Message 1";
            testMacroProperty.Type = new MacroPropertyType(_macroPropertyType2.Id);
            testMacroProperty.Save();

            int id = testMacroProperty.Id;

            var savedMacroProperty = getTestMacroProperty(id);

            Assert.That(testMacroProperty.Id, Is.EqualTo(savedMacroProperty.Id));
            Assert.That(testMacroProperty.Macro.Id, Is.EqualTo(savedMacroProperty.Macro));
            Assert.That(testMacroProperty.Public, Is.EqualTo(savedMacroProperty.Hidden));
            Assert.That(testMacroProperty.Alias, Is.EqualTo(savedMacroProperty.Alias));
            Assert.That(testMacroProperty.Name, Is.EqualTo(savedMacroProperty.Name));
            Assert.That(testMacroProperty.Type.Id, Is.EqualTo(savedMacroProperty.Type));

            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static MacroProperty[] GetProperties(int MacroId)' method")]
        public void Test_MacroProperty_GetProperties()
        {
            var all = MacroProperty.GetProperties(_macro1.Id);
            Assert.That(all.Length, Is.EqualTo(1));

            var testMacroProperty = getTestMacroProperty(all[0].Id);

            Assert.That(testMacroProperty.Id, Is.EqualTo(all[0].Id));
            Assert.That(testMacroProperty.Hidden, Is.EqualTo(all[0].Public));
            Assert.That(testMacroProperty.Type , Is.EqualTo(all[0].Type.Id));
            Assert.That(testMacroProperty.Macro, Is.EqualTo(all[0].Macro.Id));
            Assert.That(testMacroProperty.SortOrder, Is.EqualTo(all[0].SortOrder));
            Assert.That(testMacroProperty.Alias, Is.EqualTo(all[0].Alias));
            Assert.That(testMacroProperty.Name, Is.EqualTo(all[0].Name));
        }        
        
        #endregion
    }
}
