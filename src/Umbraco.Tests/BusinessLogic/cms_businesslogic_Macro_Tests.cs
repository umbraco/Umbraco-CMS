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
    public class cms_businesslogic_Macro_Tests : BaseDatabaseFactoryTestWithContext
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
                                           new
                                           {
                                               macroPropertyHidden = true,
                                               macroPropertyType = _macroPropertyType1.Id,
                                               macro = _macro1.Id,
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

        [Test(Description = "Test 'public Macro(int Id)' constructor")]
        [TestCase(1)] // .ctor
        [TestCase(2)] // static Macro.GetById(...)
        public void Test_Macro_Constructor_1_and_Setup(int testCase)
        {
            // public Macro() {}
            // public Macro(int Id)
            // private void setup()
            // private void PopulateMacroFromDto(MacroDto dto)       
            Macro testMacro = new Macro();

            Assert.That(testMacro.Id, Is.EqualTo(0));  
            Assert.Throws<ArgumentException>(() => { new Macro(12345); });

            testMacro = testCase == 1 ?
                new Macro(_macro1.Id) :
                Macro.GetById(_macro1.Id);  

            // public bool UseInEditor .set
            // public int RefreshRate .set
            // public string Alias .set
            // public string Name .set
            // public string Assembly .set
            // public string Type .set
            // public string Xslt .set
            // public string ScriptingFile .set
            // public bool RenderContent .set
            // public bool CachePersonalized  .set
            // public bool CacheByPage { .set
            Assert.That(testMacro.Id, Is.EqualTo(_macro1.Id));
            Assert.That(testMacro.UseInEditor, Is.EqualTo(_macro1.UseInEditor));
            Assert.That(testMacro.RefreshRate, Is.EqualTo(_macro1.RefreshRate));
            Assert.That(testMacro.Alias, Is.EqualTo(_macro1.Alias));
            Assert.That(testMacro.Name, Is.EqualTo(_macro1.Name));
            Assert.That(testMacro.Assembly, Is.EqualTo(_macro1.ScriptAssembly));
            Assert.That(testMacro.Type, Is.EqualTo(_macro1.ScriptType));
            Assert.That(testMacro.Xslt, Is.EqualTo(_macro1.Xslt));
            Assert.That(testMacro.ScriptingFile, Is.EqualTo(_macro1.Python));
            Assert.That(testMacro.RenderContent, Is.EqualTo(_macro1.DontRender));
            Assert.That(testMacro.CachePersonalized, Is.EqualTo(_macro1.CachePersonalized));
            Assert.That(testMacro.CacheByPage, Is.EqualTo(_macro1.CacheByPage));
        }

        [Test(Description = "public Macro(string alias)' constructor")]
        [TestCase(1)] // .ctor
        [TestCase(2)] // static Macro.GetByAlias(...)
        public void Test_Macro_Constructor_2_and_Setup(int testCase)
        {
            // public Macro(string alias)
            // private void setup()
            // private void PopulateMacroFromDto(MacroDto dto)       
            Macro testMacro = testCase == 1 ?
                new Macro(_macro1.Alias) :
                Macro.GetByAlias(_macro1.Alias);  

            // public bool UseInEditor .set
            // public int RefreshRate .set
            // public string Alias .set
            // public string Name .set
            // public string Assembly .set
            // public string Type .set
            // public string Xslt .set
            // public string ScriptingFile .set
            // public bool RenderContent .set
            // public bool CachePersonalized  .set
            // public bool CacheByPage { .set
            Assert.That(testMacro.Id, Is.EqualTo(_macro1.Id));
            Assert.That(testMacro.UseInEditor, Is.EqualTo(_macro1.UseInEditor));
            Assert.That(testMacro.RefreshRate, Is.EqualTo(_macro1.RefreshRate));
            Assert.That(testMacro.Alias, Is.EqualTo(_macro1.Alias));
            Assert.That(testMacro.Name, Is.EqualTo(_macro1.Name));
            Assert.That(testMacro.Assembly, Is.EqualTo(_macro1.ScriptAssembly));
            Assert.That(testMacro.Type, Is.EqualTo(_macro1.ScriptType));
            Assert.That(testMacro.Xslt, Is.EqualTo(_macro1.Xslt));
            Assert.That(testMacro.ScriptingFile, Is.EqualTo(_macro1.Python));
            Assert.That(testMacro.RenderContent, Is.EqualTo(_macro1.DontRender));
            Assert.That(testMacro.CachePersonalized, Is.EqualTo(_macro1.CachePersonalized));
            Assert.That(testMacro.CacheByPage, Is.EqualTo(_macro1.CacheByPage));
        }

        [Test(Description = "Test 'public bool UseInEditor .set' property")]
        public void Test_UseEditor_set()
        {
            var valueToSet = !_macro1.UseInEditor;

            // before new value is set
            var testMacro1 = new Macro(_macro1.Id);
            Assert.That(testMacro1.UseInEditor, !Is.EqualTo(valueToSet));

            testMacro1.UseInEditor = valueToSet;

            // after new value is set
            var testMacro2 = getTestMacro(_macro1.Id);
            Assert.That(testMacro2.UseInEditor, Is.EqualTo(valueToSet));
        }

        [Test(Description = "Test 'public int RefreshRate .set' property")]
        public void Test_RefreshRate_set()
        {
            var valueToSet = _macro1.RefreshRate + 101;

            // before new value is set
            var testMacro1 = new Macro(_macro1.Id);
            Assert.That(testMacro1.RefreshRate, !Is.EqualTo(valueToSet));

            testMacro1.RefreshRate = valueToSet;

            // after new value is set
            var testMacro2 = getTestMacro(_macro1.Id);
            Assert.That(testMacro2.RefreshRate, Is.EqualTo(valueToSet));
        }
        
        [Test(Description = "Test 'public string Alias .set' property")]
        public void Test_Alias_set()
        {
            var valueToSet = _macro1.Alias + "_SUFFIX" ;

            // before new value is set
            var testMacro1 = new Macro(_macro1.Id);
            Assert.That(testMacro1.Alias, !Is.EqualTo(valueToSet));

            testMacro1.Alias = valueToSet;

            // after new value is set
            var testMacro2 = getTestMacro(_macro1.Id);
            Assert.That(testMacro2.Alias, Is.EqualTo(valueToSet));

        }  
        
        [Test(Description = "Test 'public string Name .set' property")]
        public void Test_Name_set()
        {
            var valueToSet = "PREFIX_" + _macro1.Name;

            // before new value is set
            var testMacro1 = new Macro(_macro1.Id);
            Assert.That(testMacro1.Name, !Is.EqualTo(valueToSet));

            testMacro1.Name = valueToSet;

            // after new value is set
            var testMacro2 = getTestMacro(_macro1.Id);
            Assert.That(testMacro2.Name, Is.EqualTo(valueToSet));
        }        
        
        [Test(Description = "Test 'public string Assembly .set' property")]
        public void Test_Assembly_set()
        {
            var valueToSet = "NewAssembly.DLL";

            // before new value is set
            var testMacro1 = new Macro(_macro1.Id);
            Assert.That(testMacro1.Assembly, !Is.EqualTo(valueToSet));

            testMacro1.Assembly = valueToSet;

            // after new value is set
            var testMacro2 = getTestMacro(_macro1.Id);
            Assert.That(testMacro2.ScriptAssembly, Is.EqualTo(valueToSet));
        }        
        
        [Test(Description = "Test 'public string Type .set' property")]
        public void Test_Type_set()
        {
            var valueToSet = "test"; // _macro1.ScriptType;

            // before new value is set
            var testMacro1 = new Macro(_macro1.Id);
            Assert.That(testMacro1.UseInEditor, !Is.EqualTo(valueToSet));

            testMacro1.Type = valueToSet;

            // after new value is set
            var testMacro2 = getTestMacro(_macro1.Id);
            Assert.That(testMacro2.ScriptType, Is.EqualTo(valueToSet));
        }        
        
        [Test(Description = "Test 'public string Xslt .set' property")]
        public void Test_Xslt_set()
        {
            var valueToSet = "NewTest.xslt";

            // before new value is set
            var testMacro1 = new Macro(_macro1.Id);
            Assert.That(testMacro1.Xslt, !Is.EqualTo(valueToSet));

            testMacro1.Xslt = valueToSet;

            // after new value is set
            var testMacro2 = getTestMacro(_macro1.Id);
            Assert.That(testMacro2.Xslt, Is.EqualTo(valueToSet));
        }        
        
        [Test(Description = "Test 'public string ScriptingFile .set' property")]
        public void Test_ScriptingFile_set()
        {
            var valueToSet = "_macro1.Python";

            // before new value is set
            var testMacro1 = new Macro(_macro1.Id);
            Assert.That(testMacro1.ScriptingFile, !Is.EqualTo(valueToSet));

            testMacro1.ScriptingFile = valueToSet;

            // after new value is set
            var testMacro2 = getTestMacro(_macro1.Id);
            Assert.That(testMacro2.Python, Is.EqualTo(valueToSet));
        }    
        
        [Test(Description = "Test 'public bool RenderContent .set' property")]
        public void Test_RenderContent_set()
        {
            var valueToSet = !_macro1.DontRender;

            // before new value is set
            var testMacro1 = new Macro(_macro1.Id);
            Assert.That(testMacro1.RenderContent, !Is.EqualTo(valueToSet));

            testMacro1.RenderContent = valueToSet;

            // after new value is set
            var testMacro2 = getTestMacro(_macro1.Id);
            Assert.That(testMacro2.DontRender, Is.EqualTo(valueToSet));
        }        
        
        [Test(Description = "Test 'public bool CachePersonalized  .set' property")]
        public void Test_CachePersonalized_set()
        {
            var valueToSet = !_macro1.CachePersonalized;

            // before new value is set
            var testMacro1 = new Macro(_macro1.Id);
            Assert.That(testMacro1.CachePersonalized, !Is.EqualTo(valueToSet));

            testMacro1.CachePersonalized = valueToSet;

            // after new value is set
            var testMacro2 = getTestMacro(_macro1.Id);
            Assert.That(testMacro2.CachePersonalized, Is.EqualTo(valueToSet));
        }        
        
        [Test(Description = "Test 'public bool CacheByPage { .set' property")]
        public void Test_CacheByPage_set()
        {
            var valueToSet = !_macro1.CacheByPage;

            // before new value is set
            var testMacro1 = new Macro(_macro1.Id);
            Assert.That(testMacro1.CacheByPage, !Is.EqualTo(valueToSet));

            testMacro1.CacheByPage = valueToSet;

            // after new value is set
            var testMacro2 = getTestMacro(_macro1.Id);
            Assert.That(testMacro2.CacheByPage, Is.EqualTo(valueToSet));
        }

        [Test(Description = "public static Macro MakeNew(string Name)' method")]
        public void Test_MakeNew()
        {
            var testMacro1 = Macro.MakeNew("My new macro"); 
            Assert.That(testMacro1, !Is.Null, "testMacro1 is null");

            var testMacro2 = getTestMacro(testMacro1.Id);

            Assert.That(testMacro1.Id, Is.EqualTo(testMacro2.Id), "ID test failed");
            Assert.That(testMacro1.UseInEditor, Is.EqualTo(testMacro2.UseInEditor), "UseInEditor test  failed");
            Assert.That(testMacro1.RefreshRate, Is.EqualTo(testMacro2.RefreshRate), "RefreshRate test  failed");
            Assert.That(testMacro1.Alias, Is.EqualTo(testMacro2.Alias), "Alias test failed");
            Assert.That(testMacro1.Name, Is.EqualTo(testMacro2.Name), "Name test failed");
            Assert.That(testMacro1.Assembly, Is.EqualTo(testMacro2.ScriptAssembly), "ScriptAssembly test failed");
            Assert.That(testMacro1.Type, Is.EqualTo(testMacro2.ScriptType), "ScriptType test failed");
            Assert.That(testMacro1.Xslt, Is.EqualTo(testMacro2.Xslt), "Xslt test failed");
            Assert.That(testMacro1.ScriptingFile, Is.EqualTo(testMacro2.Python), "Python test failed");
            Assert.That(testMacro1.RenderContent, Is.EqualTo(testMacro2.DontRender), "DontRender test failed");
            Assert.That(testMacro1.CachePersonalized, Is.EqualTo(testMacro2.CachePersonalized), "CachePersonalized test failed");
            Assert.That(testMacro1.CacheByPage, Is.EqualTo(testMacro2.CacheByPage), "CacheByPage test failed");
        }


        [Test(Description = "public void Delete()' method")]
        public void Test_Delete()
        {
            var testMacro1 = new Macro(_macro1.Id);  
            Assert.That(testMacro1, !Is.Null, "testMacro1 is null");

            testMacro1.Delete();  

            var testMacro2 = getTestMacro(testMacro1.Id);
            Assert.That(testMacro2, Is.Null, "testMacro2 is not null");

            var testMacroProperty = getTestMacroProperty(_macroProperty1.Id);
            Assert.That(testMacroProperty, Is.Null, "testMacroProperty is not null");

            EnsureAllTestRecordsAreDeleted(); // reset test db
        }

        [Test(Description = "public static Macro[] GetAll()' method")]
        public void Test_GetAll()
        {
            var macros = Macro.GetAll();
            Assert.That(macros.Length, !Is.EqualTo(0), "There is no any macros saved in DB");
        }

        [Test(Description = "public MacroProperty[] Properties' method")]
        public void Test_Properties()
        {
            var macro = new Macro(_macro1.Id);  
            Assert.That(macro.Properties.Length, Is.EqualTo(1), "There is more than one test property saved for macro");
        }
        
        //
        // The following methods do not use PetaPOCO calls - skip testing for now
        //
        // public static Macro Import(XmlNode n)
        // public XmlNode ToXml(XmlDocument xd)
        // public static MacroTypes FindMacroType(string xslt, string scriptFile, string scriptType, string scriptAssembly)

        #endregion
    }
}
