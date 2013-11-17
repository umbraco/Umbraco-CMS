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
    public class cms_businesslogic_MacroProperty_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_MacroProperty_TestData(); }

        [Test(Description = "Test EnsureData()")]
        public void _1st_Test_MacroProperty_EnsureTestData()
        {
            Assert.IsTrue(initialized);
            Assert.That(_macroPropertyType1, !Is.Null);
            Assert.That(_macroPropertyType2, !Is.Null);
            Assert.That(_macro1, !Is.Null);
            Assert.That(_macro2, !Is.Null);
            Assert.That(_macroProperty1, !Is.Null);

            EnsureAll_MacroProperty_TestRecordsAreDeleted();

            Assert.That(getPersistedTestDto <MacroPropertyDto>(_macroProperty1.Id), Is.Null);
            Assert.That(getPersistedTestDto <MacroDto>(_macro1.Id), Is.Null);
            Assert.That(getPersistedTestDto <MacroDto>(_macro2.Id), Is.Null);
            Assert.That(getPersistedTestDto <MacroPropertyTypeDto>(_macroPropertyType1.Id), Is.Null);
            Assert.That(getPersistedTestDto <MacroPropertyTypeDto>(_macroPropertyType2.Id), Is.Null);
        }

        [Test(Description = "Test 'public MacroProperty(int Id)' and indirectly private setup() method")]
        public void _2nd_Test_MacroProperty_Constructor_and_setup()
        {
            MacroProperty testMacroProperty = null;
            Assert.Throws<ArgumentException>(() => { new MacroProperty(12345); });  // test non-existent id

            testMacroProperty = new MacroProperty(_macroProperty1.Id);
            Assert.That(testMacroProperty, !Is.Null);
            assertMacroPropertySetup(testMacroProperty, _macroProperty1);    
        }

        private void assertMacroPropertySetup(MacroProperty testMacroProperty, MacroPropertyDto savedMacroProperty)
        {
            Assert.That(testMacroProperty.Id, Is.EqualTo(savedMacroProperty.Id), "Id test failed");
            Assert.That(testMacroProperty.Macro.Id, Is.EqualTo(savedMacroProperty.Macro), "Macro Id test failed");
            Assert.That(testMacroProperty.Public, Is.EqualTo(savedMacroProperty.Hidden), "Public flag test failed");
            Assert.That(testMacroProperty.Alias, Is.EqualTo(savedMacroProperty.Alias), "Alias test failed");
            Assert.That(testMacroProperty.Name, Is.EqualTo(savedMacroProperty.Name), "Name test failed");
            Assert.That(testMacroProperty.Type.Id, Is.EqualTo(savedMacroProperty.Type), "Type test failed");
        }

        [Test(Description = "Test 'public static MacroProperty MakeNew(...)' method")]
        public void Test_MacroProperty_MakeNew()
        {
            // prerequisites
            var macro = new Macro(_macro1.Id);
            Assert.That(macro, !Is.Null);

            var macroPropertyType = new MacroPropertyType(_macroPropertyType1.Id);
            Assert.That(macroPropertyType, !Is.Null);

            // body
            var testMacroProperty = MacroProperty.MakeNew(macro, true, "testAlias" + uniqueAliasSuffix, "Test Name" + uniqueNameSuffix, macroPropertyType);
            Assert.That(testMacroProperty, !Is.Null);
            assertMacroPropertySetup(testMacroProperty, getDto<MacroPropertyDto>(testMacroProperty.Id));    
        }

        [Test(Description = "Test 'public void Save()' method - create new MacroProperty")]
        public void Test_MacroProperty_Save_New()
        {
            // prerequisites
            var macro = new Macro(_macro1.Id);
            Assert.That(macro, !Is.Null);

            var macroPropertyType = new MacroPropertyType(_macroPropertyType1.Id);
            Assert.That(macroPropertyType, !Is.Null);

            // body
            var testMacroProperty = new MacroProperty();
            testMacroProperty.Macro = macro;
            testMacroProperty.Public = true;
            testMacroProperty.Alias = "testAppMessage" + uniqueAliasSuffix;
            testMacroProperty.Name = "Test App Message" + uniqueNameSuffix;
            testMacroProperty.Type = macroPropertyType;
            testMacroProperty.Save();

            assertMacroPropertySetup(testMacroProperty, getPersistedTestDto<MacroPropertyDto>(testMacroProperty.Id));    
        }

        [Test(Description = "Test 'public void Save()' method - update existing MacroProperty")]
        public void Test_MacroProperty_Save_Update()
        {
            // prerequisites
            var macro = new Macro(_macro2.Id);
            Assert.That(macro, !Is.Null);

            var macroPropertyType = new MacroPropertyType(_macroPropertyType2.Id);
            Assert.That(macroPropertyType, !Is.Null);

            // body
            var testMacroProperty = new MacroProperty(_macroProperty1.Id);
            testMacroProperty.Macro = macro;
            testMacroProperty.Public = false;
            testMacroProperty.Alias = "testAppMessage" + uniqueAliasSuffix;
            testMacroProperty.Name = "Test App Message" + uniqueNameSuffix;
            testMacroProperty.Type = macroPropertyType;
            testMacroProperty.Save();

            assertMacroPropertySetup(testMacroProperty, getPersistedTestDto<MacroPropertyDto>(testMacroProperty.Id));    

            initialized = true; // next test will get new test data generated
        }

        [Test(Description = "Test 'public static MacroProperty[] GetProperties(int MacroId)' method")]
        public void Test_MacroProperty_GetProperties()
        {
            var all = MacroProperty.GetProperties(_macro1.Id);

            int count = independentDatabase.ExecuteScalar<int>("select count(id) from cmsMacroProperty where macro = @0", _macro1.Id);
            Assert.That(all.Length, Is.EqualTo(count));

            foreach (var testMacroProperty in all)
                assertMacroPropertySetup(testMacroProperty, getPersistedTestDto<MacroPropertyDto>(testMacroProperty.Id));
        }        

    }
}
