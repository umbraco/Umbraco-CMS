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
    public class cms_businesslogic_MacroPropertyType_Tests : BaseDatabaseFactoryTestWithContext
    {
        #region EnsureData
        const string TEST_ALIAS = "testAlias";
        private MacroPropertyTypeDto _macroPropertyType1;
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void EnsureData()
        {
            if (!initialized)
            {
                independentDatabase.Execute("insert into [cmsMacroPropertyType] (macroPropertyTypeAlias, macroPropertyTypeRenderAssembly, macroPropertyTypeRenderType, macroPropertyTypeBaseType) " +  
                                           " values (@macroPropertyTypeAlias, @macroPropertyTypeRenderAssembly, @macroPropertyTypeRenderType, @macroPropertyTypeBaseType) ",
                                           new { macroPropertyTypeAlias = TEST_ALIAS, macroPropertyTypeRenderAssembly = "umbraco.macroRenderings",
                                                 macroPropertyTypeRenderType = "context", macroPropertyTypeBaseType = "string" });
                int id = independentDatabase.ExecuteScalar<int>("select MAX(id) from [cmsMacroPropertyType]");
                _macroPropertyType1 = getTestMacroPropertyType(id);  
            }

            initialized = true;
        }

        private MacroPropertyTypeDto getTestMacroPropertyType(int id)
        {
            return getPersistedTestDto<MacroPropertyTypeDto>(id);
        }

        private void EnsureAllTestRecordsAreDeleted()
        {
            independentDatabase.Execute("delete from  [cmsMacroPropertyType] where macroPropertyTypeAlias = @0", TEST_ALIAS);
            initialized = false; 
        }

        #endregion

        #region Tests
        [Test(Description = "Test EnsureData()")]
        public void Test_EnsureData()
        {
            Assert.IsTrue(initialized);
            Assert.That(_macroPropertyType1, !Is.Null);

            EnsureAllTestRecordsAreDeleted();

            Assert.That(getTestMacroPropertyType(_macroPropertyType1.Id), Is.Null);
        }

        [Test(Description = "Test 'public MacroPropertyType(int Id)' method")]
        public void Test_MacroPropertyType_Constructor_1_and_Setup()
        {
            var testMacroPropertyType = new MacroPropertyType(_macroPropertyType1.Id);

            Assert.That(testMacroPropertyType.Id, Is.EqualTo(_macroPropertyType1.Id));
            Assert.That(testMacroPropertyType.Alias, Is.EqualTo(_macroPropertyType1.Alias));
            Assert.That(testMacroPropertyType.Assembly, Is.EqualTo(_macroPropertyType1.RenderAssembly));
            Assert.That(testMacroPropertyType.Type, Is.EqualTo(_macroPropertyType1.RenderType));
            Assert.That(testMacroPropertyType.BaseType, Is.EqualTo(_macroPropertyType1.BaseType));
        }

        [Test(Description = "Test 'public MacroPropertyType(string Alias)' method")]
        public void Test_MacroPropertyType_Constructor_2_and_Setup()
        {
            var testMacroPropertyType = new MacroPropertyType(_macroPropertyType1.Alias);

            Assert.That(testMacroPropertyType.Id, Is.EqualTo(_macroPropertyType1.Id));
            Assert.That(testMacroPropertyType.Alias, Is.EqualTo(_macroPropertyType1.Alias));
            Assert.That(testMacroPropertyType.Assembly, Is.EqualTo(_macroPropertyType1.RenderAssembly));
            Assert.That(testMacroPropertyType.Type, Is.EqualTo(_macroPropertyType1.RenderType));
            Assert.That(testMacroPropertyType.BaseType, Is.EqualTo(_macroPropertyType1.BaseType));
        }

        [Test(Description = "Test 'public static List<MacroPropertyType> GetAll' method")]
        public void Test_MacroPropertyType_GetAll()
        {
            var all = MacroPropertyType.GetAll;
            //l("Count = {0}", all.Count);  // 16 - default

            Assert.That(all.Count, Is.EqualTo(17));   // 16 + 1
        }
        #endregion
    }
}
