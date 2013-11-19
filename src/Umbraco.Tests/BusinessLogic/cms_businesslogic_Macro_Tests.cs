//#define TRACE_EXECUTION_SPEED
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.relation;
using umbraco.cms.businesslogic.macro;
using Umbraco.Tests.BusinessLogic;

namespace Umbraco.Tests.ORM
{
    [TestFixture]
    public class cms_businesslogic_Macro_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_Macro_TestData(); }

        [Test(Description = "Test Ensure_Macro_TestData()")]
        public void _1st_Test_Macro_EnsureTestData()
        {
            Assert.IsTrue(initialized);
            Assert.That(_macroPropertyType1, !Is.Null);
            Assert.That(_macroPropertyType2, !Is.Null);
            Assert.That(_macro1, !Is.Null);
            Assert.That(_macro2, !Is.Null);
            Assert.That(_macroProperty1, !Is.Null);

            EnsureAll_Macro_TestRecordsAreDeleted();

            Assert.That(TRAL.GetDto<MacroPropertyDto>(_macroProperty1.Id), Is.Null);
            Assert.That(TRAL.GetDto<MacroDto>(_macro1.Id), Is.Null);
            Assert.That(TRAL.GetDto<MacroDto>(_macro2.Id), Is.Null);
            Assert.That(TRAL.GetDto<MacroPropertyTypeDto>(_macroPropertyType1.Id), Is.Null);
            Assert.That(TRAL.GetDto<MacroPropertyTypeDto>(_macroPropertyType2.Id), Is.Null);
        }

        [Test(Description = "Test 'public Macro()' constructor")]
        public void _2nd_Test_Macro_I_Constructor()
        {
            var testMacro = new Macro();
            Assert.That(testMacro.Id, Is.EqualTo(0));  

            // test Macro constructor with non-existent Id
            Assert.Throws<ArgumentException>(() => { new Macro(12345); });
        }

        [Test(Description = "Test 'public Macro(int Id) and public Macro(string alias)' constructors")]
        [TestCase(1)] // public Macro(int Id) .ctor
        [TestCase(2)] // public Macro(string alias) .ctor
        public void _3rd_Test_Macro_II_Constructors(int testCase)
        {
            var testMacro = testCase == 1 ? new Macro(_macro1.Id) : new Macro (_macro1.Alias);
            Assert.That(testMacro, !Is.Null); 
            assertMacroSetup(testMacro, TRAL.GetDto<MacroDto>(testMacro.Id));    
        }

        private void assertMacroSetup(Macro testMacro, MacroDto savedMacro)
        {
            Assert.That(testMacro.Id, Is.EqualTo(savedMacro.Id), "ID test failed");
            Assert.That(testMacro.UseInEditor, Is.EqualTo(savedMacro.UseInEditor), "UseInEditor test  failed");
            Assert.That(testMacro.RefreshRate, Is.EqualTo(savedMacro.RefreshRate), "RefreshRate test  failed");
            Assert.That(testMacro.Alias, Is.EqualTo(savedMacro.Alias), "Alias test failed");
            Assert.That(testMacro.Name, Is.EqualTo(savedMacro.Name), "Name test failed");
            Assert.That(testMacro.Assembly, Is.EqualTo(savedMacro.ScriptAssembly), "ScriptAssembly test failed");
            Assert.That(testMacro.Type, Is.EqualTo(savedMacro.ScriptType), "ScriptType test failed");
            Assert.That(testMacro.Xslt, Is.EqualTo(savedMacro.Xslt), "Xslt test failed");
            Assert.That(testMacro.ScriptingFile, Is.EqualTo(savedMacro.Python), "Python test failed");
            Assert.That(testMacro.RenderContent, Is.EqualTo(savedMacro.DontRender), "DontRender test failed");
            Assert.That(testMacro.CachePersonalized, Is.EqualTo(savedMacro.CachePersonalized), "CachePersonalized test failed");
            Assert.That(testMacro.CacheByPage, Is.EqualTo(savedMacro.CacheByPage), "CacheByPage test failed");
        }

        [Test(Description = "Test 'public static Macro.GetById(int id) and public static Macro.GetByAlias(string alias)' methods")]
        [TestCase(1, Description="Test 'public static Macro.GetById(int id)' method")]
        [TestCase(2, Description="Test 'public static Macro.GetByAlias(string alias)' method")]
        public void Test_Macro_GetById_and_GetByAlias(int testCase)
        {
            var testMacro = testCase == 1 ?
                Macro.GetById(_macro1.Id)   :
                Macro.GetByAlias(_macro1.Alias);
            Assert.That(testMacro, !Is.Null);
            assertMacroSetup(testMacro, TRAL.GetDto<MacroDto>(testMacro.Id));    
        }

        [Test(Description = "Test 'public bool UseInEditor' property.set")]
        public void Test_Macro_UseEditor_set()
        {
            var newValue =  !_macro1.UseInEditor;
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.macro.Macro, bool, bool>(
                    n => n.UseInEditor,
                    n => n.UseInEditor = newValue,
                    "cmsMacro",
                    "macroUseInEditor",
                    expectedValue,
                    "Id",
                    _macro1.Id
                );
        }

        [Test(Description = "Test 'public int RefreshRate' property.set")]
        public void Test_Macro_RefreshRate_set()
        {
            var newValue = _macro1.RefreshRate + 101;
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.macro.Macro, int, int>(
                    n => n.RefreshRate,
                    n => n.RefreshRate = newValue,
                    "cmsMacro",
                    "macroRefreshRate",
                    expectedValue,
                    "Id",
                    _macro1.Id
                );
        }
        
        [Test(Description = "Test 'public string Alias' property.set")]
        public void Test_Macro_Alias_set()
        {
            var newValue = _macro1.Alias + "_a_Test_Suffix";
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.macro.Macro, string, string>(
                    n => n.Alias,
                    n => n.Alias = newValue,
                    "cmsMacro",
                    "macroAlias",
                    expectedValue,
                    "Id",
                    _macro1.Id
                );

        }  
        
        [Test(Description = "Test 'public string Name' property.set")]
        public void Test_Macro_Name_set()
        {
            var newValue = "A_Name_Prefix_" + _macro1.Name;
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.macro.Macro, string, string>(
                    n => n.Name,
                    n => n.Name = newValue,
                    "cmsMacro",
                    "macroName",
                    expectedValue,
                    "Id",
                    _macro1.Id
                );
        }        
        
        [Test(Description = "Test 'public string Assembly' property.set")]
        public void Test_Macro_Assembly_set()
        {
            var newValue = "NewAssembly.DLL";
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.macro.Macro, string, string>(
                    n => n.Assembly,
                    n => n.Assembly = newValue,
                    "cmsMacro",
                    "macroScriptAssembly",
                    expectedValue,
                    "Id",
                    _macro1.Id
                );
        }        
        
        [Test(Description = "Test 'public string Type' property.set")]
        public void Test_Macro_Type_set()
        {
            var newValue = "Python";
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.macro.Macro, string, string>(
                    n => n.Type,
                    n => n.Type = newValue,
                    "cmsMacro",
                    "macroScriptType",
                    expectedValue,
                    "Id",
                    _macro1.Id
                );
        }        
        
        [Test(Description = "Test 'public string Xslt' property.set")]
        public void Test_Macro_Xslt_set()
        {
            var newValue = "NewTest.xslt";
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.macro.Macro, string, string>(
                    n => n.Xslt,
                    n => n.Xslt = newValue,
                    "cmsMacro",
                    "macroXslt",
                    expectedValue,
                    "Id",
                    _macro1.Id
                );
        }        
        
        [Test(Description = "Test 'public string ScriptingFile' property.set")]
        public void Test_Macro_ScriptingFile_set()
        {
            var newValue = "macro1.pt";
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.macro.Macro, string, string>(
                    n => n.ScriptingFile,
                    n => n.ScriptingFile = newValue,
                    "cmsMacro",
                    "macroPython",
                    expectedValue,
                    "Id",
                    _macro1.Id
                );
        }    
        
        [Test(Description = "Test 'public bool RenderContent' property.set")]
        public void Test_Macro_RenderContent_set()
        {
            var newValue = !_macro1.DontRender;
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.macro.Macro, bool, bool>(
                    n => n.RenderContent,
                    n => n.RenderContent = newValue,
                    "cmsMacro",
                    "macroDontRender",
                    expectedValue,
                    "Id",
                    _macro1.Id
                );
        }        
        
        [Test(Description = "Test 'public bool CachePersonalized' property.set")]
        public void Test_Macro_CachePersonalized_set()
        {
            var newValue = !_macro1.CachePersonalized;
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.macro.Macro, bool, bool>(
                    n => n.CachePersonalized,
                    n => n.CachePersonalized = newValue,
                    "cmsMacro",
                    "macroCachePersonalized",
                    expectedValue,
                    "Id",
                    _macro1.Id
                );
        }        
        
        [Test(Description = "Test 'public bool CacheByPage' property.set")]
        public void Test_Macro_CacheByPage_set()
        {
            var newValue = !_macro1.CacheByPage;
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.macro.Macro, bool, bool>(
                    n => n.CacheByPage,
                    n => n.CacheByPage = newValue,
                    "cmsMacro",
                    "macroCacheByPage",
                    expectedValue,
                    "Id",
                    _macro1.Id
                );
        }

        [Test(Description = "public static Macro MakeNew(string Name)' method")]
        public void Test_Macro_MakeNew()
        {
            var testMacro = Macro.MakeNew("My new macro" + uniqueNameSuffix); 
            Assert.That(testMacro, !Is.Null, "testMacro1 is null");
            assertMacroSetup(testMacro, TRAL.GetDto<MacroDto>(testMacro.Id));
        }


        [Test(Description = "public void Delete()' method")]
        public void Test_Macro_Delete()
        {
            var testMacro1 = new Macro(_macro1.Id);  
            Assert.That(testMacro1, !Is.Null, "testMacro1 is null");

            testMacro1.Delete();

            var testMacro2 = TRAL.GetDto<MacroDto>(testMacro1.Id);
            Assert.That(testMacro2, Is.Null, "Macro hasn't been deleted - testMacro2 is not null");

            var testMacroProperty = TRAL.GetDto<MacroPropertyDto>(_macroProperty1.Id);
            Assert.That(testMacroProperty, Is.Null, "Macro peroperty hasn't been deleted - testMacroProperty is not null");

            initialized = false; // reset test data on the next test run
        }

        // TRACE_EXECUTION_SPEED test results
        //1. 6:17:43 PM
        //2. 64 6:17:46 PM
        [Test(Description = "public static Macro[] GetAll()' method")]
        public void Test_Macro_GetAll()
        {
#if TRACE_EXECUTION_SPEED
            l("1. {0:T}", DateTime.Now);
#endif
            var macros = Macro.GetAll();
#if TRACE_EXECUTION_SPEED
            l("2. {0} {1:T}", macros.Length, DateTime.Now);
#endif
            int count = TRAL.Macro.CountAll;
            Assert.That(macros.Length, Is.EqualTo(count), "Macro.GetAll() test failed");
        }

        [Test(Description = "public MacroProperty[] Properties' property.get")]
        public void Test_Macro_Properties()
        {
            var macro = new Macro(_macro1.Id);
            int count = TRAL.Macro.CountAllProperties (_macro1.Id);
            Assert.That(macro.Properties.Length, Is.EqualTo(1), "There is more than one test property saved for macro");
        }
        
        //
        // The following methods do not use PetaPOCO calls - skip testing for now
        //
        // public static Macro Import(XmlNode n)
        // public XmlNode ToXml(XmlDocument xd)
        // public static MacroTypes FindMacroType(string xslt, string scriptFile, string scriptType, string scriptAssembly)
    }
}
